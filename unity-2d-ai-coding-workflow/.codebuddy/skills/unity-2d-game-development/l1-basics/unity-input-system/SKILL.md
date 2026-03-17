---
name: unity-input-system
description: Unity新Input System输入系统，采用事件驱动架构。支持多设备（键盘、鼠标、手柄、触摸屏）。强制禁止旧Input系统，使用事件订阅节省99%性能。提供完整的API白名单和迁移指南。
---

# Unity Input System Skill

## 技能描述

Unity Input System是Unity的新一代输入系统，采用事件驱动架构，支持多种输入设备（键盘、鼠标、游戏手柄、触摸屏等）。本技能提供新Input System的完整使用指南，包括API白名单、事件订阅模式和性能优化建议。

---

## 核心原则

### 1. 强制使用新Input System

**❌ 禁止使用旧Input系统：**
```csharp
// ❌ 禁止：旧Input系统
if (Input.GetKeyDown(KeyCode.Space)) { }
if (Input.GetAxis("Horizontal") > 0) { }
if (Input.GetMouseButton(0)) { }
```

**✅ 使用新Input System：**
```csharp
// ✅ 推荐：新Input System
if (Keyboard.current.spaceKey.wasPressedThisFrame) { }
if (Gamepad.current.leftStick.ReadValue().x > 0) { }
if (Mouse.current.leftButton.wasPressedThisFrame) { }
```

### 2. 事件订阅模式优先

**❌ 禁止在Update中轮询：**
```csharp
// ❌ 禁止：每帧轮询，性能差
void Update() {
    if (Keyboard.current.spaceKey.wasPressedThisFrame) {
        Jump();
    }
}
```

**✅ 使用事件订阅：**
```csharp
// ✅ 推荐：事件驱动，性能优
void OnEnable() {
    Keyboard.current.spaceKey.performed += OnJumpPerformed;
}

void OnDisable() {
    Keyboard.current.spaceKey.performed -= OnJumpPerformed;
}

void OnJumpPerformed(InputAction.CallbackContext context) {
    Jump();
}
```

---

## API白名单

### ✅ 推荐使用的API（高效、规范）

#### InputAction和InputActionAsset
```csharp
// ✅ 推荐：使用InputAction Asset配置输入
[SerializeField] private InputActionAsset inputActions;
private InputAction moveAction;
private InputAction jumpAction;

void Awake() {
    moveAction = inputActions.FindAction("Move");
    jumpAction = inputActions.FindAction("Jump");
}

// ✅ 推荐：事件订阅
void OnEnable() {
    moveAction.Enable();
    jumpAction.Enable();
    
    jumpAction.performed += OnJump;
    moveAction.performed += OnMove;
    moveAction.canceled += OnMoveCanceled;
}

void OnDisable() {
    moveAction.Disable();
    jumpAction.Disable();
    
    jumpAction.performed -= OnJump;
    moveAction.performed -= OnMove;
    moveAction.canceled -= OnMoveCanceled;
}
```

#### PlayerInput组件
```csharp
// ✅ 推荐：使用PlayerInput组件简化多玩家输入
[SerializeField] private PlayerInput playerInput;

void OnEnable() {
    playerInput.onActionTriggered += OnActionTriggered;
}

void OnDisable() {
    playerInput.onActionTriggered -= OnActionTriggered;
}

void OnActionTriggered(InputAction.CallbackContext context) {
    if (context.action.name == "Jump" && context.performed) {
        Jump();
    }
}
```

#### InputAction.performed / started / cancelled
```csharp
// ✅ 推荐：使用三个阶段处理输入
jumpAction.started += context => Debug.Log("开始按下");
jumpAction.performed += context => Debug.Log("完成按下");
jumpAction.canceled += context => Debug.Log("释放按键");
```

#### ReadValue<T>
```csharp
// ✅ 推荐：类型安全的值读取
Vector2 moveValue = moveAction.ReadValue<Vector2>();
float triggerValue = triggerAction.ReadValue<float>();
```

---

### ⚠️ 性能警告的API（需谨慎使用）

#### 直接访问InputDevice.current
```csharp
// ⚠️ 警告：频繁访问有性能开销
void Update() {
    if (Keyboard.current.spaceKey.wasPressedThisFrame) { } // 每帧访问
}

// ✅ 正确：缓存引用
private Keyboard keyboard;

void Awake() {
    keyboard = Keyboard.current;
}

void Update() {
    if (keyboard.spaceKey.wasPressedThisFrame) { }
}
```

#### wasPressedThisFrame / wasReleasedThisFrame
```csharp
// ⚠️ 警告：仅用于简单场景，推荐使用事件
void Update() {
    if (Keyboard.current.spaceKey.wasPressedThisFrame) {
        Jump(); // 简单场景可用
    }
}

// ✅ 更好：使用事件订阅
void OnEnable() {
    Keyboard.current.spaceKey.performed += OnJump;
}
```

---

### ❌ 禁止使用的API

#### 旧Input系统（Input Manager）
```csharp
// ❌ 禁止：使用旧的Input系统
Input.GetKeyDown(KeyCode.Space);        // ❌ 禁止
Input.GetKey(KeyCode.Space);            // ❌ 禁止
Input.GetKeyUp(KeyCode.Space);          // ❌ 禁止
Input.GetAxis("Horizontal");            // ❌ 禁止
Input.GetAxisRaw("Vertical");           // ❌ 禁止
Input.GetButton("Jump");                // ❌ 禁止
Input.GetMouseButton(0);                // ❌ 禁止
Input.mousePosition;                    // ❌ 禁止
```

**禁用原因：**
- 旧系统性能差，每帧轮询
- 不支持多设备热插拔
- 不支持重映射和自定义布局
- 无法处理复杂输入组合

#### 在Update中频繁ReadValue
```csharp
// ❌ 禁止：每帧读取大量输入
void Update() {
    Vector2 move = moveAction.ReadValue<Vector2>();  // 每帧读取
    float fire = fireAction.ReadValue<float>();      // 每帧读取
    Vector2 look = lookAction.ReadValue<Vector2>();  // 每帧读取
}

// ✅ 正确：使用事件驱动
void OnEnable() {
    moveAction.performed += OnMove;
}

void OnMove(InputAction.CallbackContext context) {
    Vector2 moveValue = context.ReadValue<Vector2>();
    // 只在输入变化时处理
}
```

---

## 功能边界 ⚠️ 强制说明

### 本Skill涵盖范围

- ✅ Input System包核心API（InputAction、InputActionAsset）
- ✅ 键盘输入（Keyboard.current）
- ✅ 鼠标输入（Mouse.current）
- ✅ 游戏手柄输入（Gamepad.current）
- ✅ 触摸屏输入（Touchscreen.current）
- ✅ 事件订阅模式（performed、canceled）
- ✅ PlayerInput组件
- ✅ 输入映射配置

### 不在本Skill范围内

- ❌ 旧Input系统（Input.GetAxis、Input.GetKeyDown等）→ **强制禁止使用**
- ❌ 自定义输入设备驱动 → 不涉及
- ❌ SteamVR/OpenXR输入 → 本项目不涉及
- ❌ Input System底层API → 不涉及
- ❌ 输入录制与回放 → 不涉及

### 跨Skill功能依赖

**角色控制系统需要**：
- unity-input-system（输入系统）← 当前Skill
- unity-2d-character-controller（角色移动）
- unity-2d-animation（动画控制）

**UI交互系统需要**：
- unity-input-system（输入系统）← 当前Skill
- unity-ui-system（UI系统）
- unity-ui-panel（UI面板）

### 性能限制

| 指标 | 建议值 | 说明 |
|------|--------|------|
| InputAction数量 | ≤ 50 | 控制输入动作数量 |
| 事件订阅对象 | ≤ 100 | 避免过多订阅者 |
| 轮询检查频率 | 禁止 | 必须使用事件模式 |

---

## Input System工作流程

### 标准配置流程

```
1. 安装Input System Package
   Window → Package Manager → Input System
   ↓
2. 创建Input Actions Asset
   Project右键 → Create → Input Actions
   ↓
3. 配置Action Maps和Actions
   定义动作名称、绑定类型、控制方案
   ↓
4. 生成C# Class（推荐）
   Input Actions Asset → Generate C# Class
   ↓
5. 使用事件订阅处理输入
   performed / started / cancelled
```

### Input Actions配置示例

```json
{
  "name": "PlayerControls",
  "maps": [
    {
      "name": "Gameplay",
      "actions": [
        {
          "name": "Move",
          "type": "Value",
          "expectedControlType": "Vector2",
          "bindings": [
            {
              "path": "<Gamepad>/leftStick",
              "groups": "Gamepad"
            },
            {
              "path": "<Keyboard>/w",
              "groups": "KeyboardMouse",
              "composite": "Dpad",
              "part": "up"
            },
            {
              "path": "<Keyboard>/s",
              "groups": "KeyboardMouse",
              "composite": "Dpad",
              "part": "down"
            },
            {
              "path": "<Keyboard>/a",
              "groups": "KeyboardMouse",
              "composite": "Dpad",
              "part": "left"
            },
            {
              "path": "<Keyboard>/d",
              "groups": "KeyboardMouse",
              "composite": "Dpad",
              "part": "right"
            }
          ]
        },
        {
          "name": "Jump",
          "type": "Button",
          "expectedControlType": "Button",
          "bindings": [
            {
              "path": "<Gamepad>/buttonSouth",
              "groups": "Gamepad"
            },
            {
              "path": "<Keyboard>/space",
              "groups": "KeyboardMouse"
            }
          ]
        }
      ]
    }
  ],
  "controlSchemes": [
    {
      "name": "KeyboardMouse",
      "bindingGroup": "KeyboardMouse"
    },
    {
      "name": "Gamepad",
      "bindingGroup": "Gamepad"
    }
  ]
}
```

---

## 性能优化要点

### 1. 使用事件订阅替代轮询

**性能对比：**
- 轮询模式：每帧检查所有输入（60次/秒）
- 事件模式：仅在输入发生时触发（节省99%性能）

```csharp
// ❌ 错误：轮询模式
void Update() {
    if (moveAction.triggered) { }
    if (jumpAction.triggered) { }
    if (fireAction.triggered) { }
    // 每帧检查，浪费性能
}

// ✅ 正确：事件模式
void OnEnable() {
    moveAction.performed += OnMove;
    jumpAction.performed += OnJump;
    fireAction.performed += OnFire;
}
// 仅在输入时触发
```

### 2. 缓存InputDevice引用

```csharp
// ⚠️ 警告：每次访问.current都有开销
void Update() {
    if (Keyboard.current.spaceKey.wasPressedThisFrame) { }
    if (Mouse.current.leftButton.wasPressedThisFrame) { }
}

// ✅ 推荐：缓存设备引用
private Keyboard keyboard;
private Mouse mouse;

void Awake() {
    keyboard = Keyboard.current;
    mouse = Mouse.current;
}
```

### 3. 合理使用InputAction phases

```csharp
// ✅ 理解三个阶段
jumpAction.started += context => {
    // 按键开始按下（适用于长按检测）
};

jumpAction.performed += context => {
    // 按键完成按下（适用于单次触发）
};

jumpAction.canceled += context => {
    // 按键释放（适用于蓄力、充能等）
};
```

### 4. 使用Control Schemes优化多设备支持

```csharp
// ✅ 根据当前设备自动切换
[SerializeField] private InputActionAsset inputActions;

void Start() {
    // 检测当前控制方案
    var gamepadScheme = inputActions.controlSchemes.First(s => s.name == "Gamepad");
    var keyboardScheme = inputActions.controlSchemes.First(s => s.name == "KeyboardMouse");
    
    // 根据连接的设备启用对应方案
    if (Gamepad.current != null) {
        inputActions.bindingMask = InputBinding.MaskByGroup(gamepadScheme.bindingGroup);
    } else {
        inputActions.bindingMask = InputBinding.MaskByGroup(keyboardScheme.bindingGroup);
    }
}
```

---

## 常见用例

### 1. 角色移动

```csharp
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private float moveSpeed = 5f;
    
    private InputAction moveAction;
    private Vector2 moveInput;
    private Rigidbody2D rb;
    
    void Awake() {
        rb = GetComponent<Rigidbody2D>();
        
        // 获取Move Action
        var gameplayMap = inputActions.FindActionMap("Gameplay");
        moveAction = gameplayMap.FindAction("Move");
    }
    
    void OnEnable() {
        moveAction.Enable();
        moveAction.performed += OnMove;
        moveAction.canceled += OnMoveCanceled;
    }
    
    void OnDisable() {
        moveAction.Disable();
        moveAction.performed -= OnMove;
        moveAction.canceled -= OnMoveCanceled;
    }
    
    void OnMove(InputAction.CallbackContext context) {
        moveInput = context.ReadValue<Vector2>();
    }
    
    void OnMoveCanceled(InputAction.CallbackContext context) {
        moveInput = Vector2.zero;
    }
    
    void FixedUpdate() {
        // 应用移动
        Vector2 velocity = moveInput * moveSpeed;
        rb.velocity = velocity;
    }
}
```

### 2. 多玩家输入

```csharp
using UnityEngine;
using UnityEngine.InputSystem;

public class MultiplayerManager : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private PlayerInputManager playerInputManager;
    
    void Start() {
        // 监听玩家加入
        playerInputManager.onPlayerJoined += OnPlayerJoined;
        playerInputManager.onPlayerLeft += OnPlayerLeft;
    }
    
    void OnPlayerJoined(PlayerInput playerInput) {
        Debug.Log($"玩家 {playerInput.playerIndex} 加入游戏");
        
        // 配置玩家
        var player = playerInput.GetComponent<PlayerController>();
        player.playerId = playerInput.playerIndex;
    }
    
    void OnPlayerLeft(PlayerInput playerInput) {
        Debug.Log($"玩家 {playerInput.playerIndex} 离开游戏");
    }
}
```

### 3. 设备热插拔

```csharp
using UnityEngine;
using UnityEngine.InputSystem;

public class DeviceHotSwap : MonoBehaviour
{
    void OnEnable() {
        // 监听设备连接
        InputSystem.onDeviceChange += OnDeviceChange;
    }
    
    void OnDisable() {
        InputSystem.onDeviceChange -= OnDeviceChange;
    }
    
    void OnDeviceChange(InputDevice device, InputDeviceChange change) {
        switch (change) {
            case InputDeviceChange.Added:
                Debug.Log($"设备连接: {device.name}");
                // 切换到新手柄控制方案
                break;
                
            case InputDeviceChange.Removed:
                Debug.Log($"设备断开: {device.name}");
                // 切换回键鼠控制方案
                break;
                
            case InputDeviceChange.Disconnected:
                Debug.Log($"设备暂时断开: {device.name}");
                break;
                
            case InputDeviceChange.Reconnected:
                Debug.Log($"设备重新连接: {device.name}");
                break;
        }
    }
}
```

---

## 迁移指南

### 从旧Input系统迁移到新系统

| 旧Input API | 新Input System API | 说明 |
|------------|-------------------|------|
| `Input.GetKeyDown(KeyCode.Space)` | `Keyboard.current.spaceKey.wasPressedThisFrame` 或 事件订阅 | 推荐使用事件 |
| `Input.GetKey(KeyCode.Space)` | `Keyboard.current.spaceKey.isPressed` 或 事件订阅 | 推荐使用事件 |
| `Input.GetKeyUp(KeyCode.Space)` | `Keyboard.current.spaceKey.wasReleasedThisFrame` 或 事件订阅 | 推荐使用事件 |
| `Input.GetAxis("Horizontal")` | `moveAction.ReadValue<Vector2>().x` | 使用InputAction |
| `Input.GetAxisRaw("Vertical")` | `moveAction.ReadValue<Vector2>().y` | 使用InputAction |
| `Input.GetButton("Jump")` | `jumpAction.triggered` 或 事件订阅 | 推荐使用事件 |
| `Input.GetMouseButton(0)` | `Mouse.current.leftButton.isPressed` 或 事件订阅 | 推荐使用事件 |
| `Input.mousePosition` | `Mouse.current.position.ReadValue()` | 使用新API |

---

## 相关资源

- [Unity Input System官方文档](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/manual/index.html)
- [Input System脚本API](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/api/UnityEngine.InputSystem.html)
- [Input System迁移指南](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/manual/Migration.html)

---

## 版本说明

- 技能版本：1.0
- Unity版本：2021.3 LTS+
- Input System版本：1.0+
- 最后更新：2024年
