# Unity Input System API 完整参考文档

## 概述

Unity Input System是新一代输入处理系统，采用事件驱动架构，支持多种输入设备。本文档提供完整的API参考，包括性能优化建议和白名单分类。

---

## 核心类

### 1. InputSystem（静态类）

Input System的核心管理类，提供全局配置和设备管理。

#### ✅ 推荐使用的API

##### onDeviceChange - 设备变化事件
```csharp
// ✅ 推荐：监听设备热插拔
InputSystem.onDeviceChange += OnDeviceChange;

void OnDeviceChange(InputDevice device, InputDeviceChange change) {
    switch (change) {
        case InputDeviceChange.Added:
            Debug.Log($"新设备连接: {device.name}");
            break;
        case InputDeviceChange.Removed:
            Debug.Log($"设备断开: {device.name}");
            break;
    }
}
```

##### UpdateTiming - 更新时机
```csharp
// ✅ 推荐：配置更新时机
InputSystem.settings.updateMode = InputSettings.UpdateMode.ProcessEventsInDynamicUpdate;
// ProcessEventsInDynamicUpdate - 在Update前处理（默认）
// ProcessEventsInFixedUpdate - 在FixedUpdate前处理
// ProcessEventsInBoth - 两者都处理
```

#### ⚠️ 性能警告的API

##### devices - 设备列表
```csharp
// ⚠️ 警告：频繁访问有性能开销
foreach (var device in InputSystem.devices) {
    // 遍历所有设备
}

// ✅ 推荐：缓存需要的设备
private Gamepad gamepad;
void Awake() {
    gamepad = Gamepad.current;
}
```

---

### 2. InputAction（核心类）

InputAction是输入动作的核心类，代表一个可触发的输入操作。

#### ✅ 推荐使用的API

##### Enable / Disable
```csharp
// ✅ 推荐：显式启用/禁用Action
[SerializeField] private InputAction moveAction;

void OnEnable() {
    moveAction.Enable();
}

void OnDisable() {
    moveAction.Disable();
}
```

**重要：** 所有InputAction必须显式启用才能工作！

##### performed / started / cancelled 事件
```csharp
// ✅ 推荐：使用事件订阅处理输入
[SerializeField] private InputAction jumpAction;

void OnEnable() {
    jumpAction.Enable();
    jumpAction.started += OnJumpStarted;
    jumpAction.performed += OnJumpPerformed;
    jumpAction.canceled += OnJumpCanceled;
}

void OnDisable() {
    jumpAction.Disable();
    jumpAction.started -= OnJumpStarted;
    jumpAction.performed -= OnJumpPerformed;
    jumpAction.canceled -= OnJumpCanceled;
}

void OnJumpStarted(InputAction.CallbackContext context) {
    // 按键开始按下
    Debug.Log("Jump started");
}

void OnJumpPerformed(InputAction.CallbackContext context) {
    // 按键完成按下（触发动作）
    Debug.Log("Jump performed");
    Jump();
}

void OnJumpCanceled(InputAction.CallbackContext context) {
    // 按键释放
    Debug.Log("Jump canceled");
}
```

**三个阶段的区别：**
- `started` - 输入开始（适用于检测长按开始）
- `performed` - 输入完成（适用于触发动作）
- `canceled` - 输入取消（适用于检测释放）

##### ReadValue<T> - 读取输入值
```csharp
// ✅ 推荐：类型安全的值读取
[SerializeField] private InputAction moveAction;

void OnMove(InputAction.CallbackContext context) {
    Vector2 moveValue = context.ReadValue<Vector2>();
    // 或者
    Vector2 moveValue = moveAction.ReadValue<Vector2>();
    
    // 应用移动
    transform.position += new Vector3(moveValue.x, moveValue.y, 0) * speed * Time.deltaTime;
}
```

**常用类型：**
- `Vector2` - 移动、瞄准（摇杆、WASD）
- `float` - 触发器、压力感应
- `bool` - 按钮状态

##### triggered - 是否触发
```csharp
// ⚠️ 适用于简单场景（推荐使用事件）
if (jumpAction.triggered) {
    Jump();
}
```

##### WasPressedThisFrame / WasReleasedThisFrame
```csharp
// ⚠️ 适用于Button类型Action
if (jumpAction.WasPressedThisFrame()) {
    Debug.Log("按键按下这一帧");
}

if (jumpAction.WasReleasedThisFrame()) {
    Debug.Log("按键释放这一帧");
}
```

#### ❌ 禁止使用的API

##### 在Update中频繁ReadValue
```csharp
// ❌ 禁止：每帧读取，性能差
void Update() {
    Vector2 move = moveAction.ReadValue<Vector2>();
    float trigger = triggerAction.ReadValue<float>();
    // 每帧调用，浪费性能
}

// ✅ 正确：使用事件驱动
void OnEnable() {
    moveAction.performed += OnMove;
}

void OnMove(InputAction.CallbackContext context) {
    Vector2 move = context.ReadValue<Vector2>();
    // 只在输入变化时处理
}
```

---

### 3. InputActionAsset

InputActionAsset用于组织多个Action Maps和Actions。

#### ✅ 推荐使用的API

```csharp
// ✅ 推荐：使用InputActionAsset组织输入
[SerializeField] private InputActionAsset inputActions;

private InputAction moveAction;
private InputAction jumpAction;

void Awake() {
    // 获取Action Map
    var gameplayMap = inputActions.FindActionMap("Gameplay");
    
    // 获取Actions
    moveAction = gameplayMap.FindAction("Move");
    jumpAction = gameplayMap.FindAction("Jump");
}

void OnEnable() {
    // 启用整个Action Map
    inputActions.FindActionMap("Gameplay").Enable();
    
    // 或启用单个Action
    moveAction.Enable();
    jumpAction.Enable();
}

void OnDisable() {
    inputActions.FindActionMap("Gameplay").Disable();
}
```

---

### 4. PlayerInput组件

PlayerInput是高层封装，简化单玩家和多玩家输入处理。

#### ✅ 推荐使用的API

```csharp
// ✅ 推荐：使用PlayerInput组件
[SerializeField] private PlayerInput playerInput;

void Start() {
    // 获取当前控制方案
    string controlScheme = playerInput.currentControlScheme;
    Debug.Log($"当前控制方案: {controlScheme}");
    
    // 获取玩家索引
    int playerIndex = playerInput.playerIndex;
}

// 方式1：使用Unity Events（Inspector配置）
public void OnMove(InputAction.CallbackContext context) {
    Vector2 move = context.ReadValue<Vector2>();
    // 处理移动
}

public void OnJump(InputAction.CallbackContext context) {
    if (context.performed) {
        Jump();
    }
}

// 方式2：使用代码订阅
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

#### 多玩家支持

```csharp
// ✅ 推荐：使用PlayerInputManager处理多玩家
[SerializeField] private PlayerInputManager playerInputManager;

void Start() {
    playerInputManager.onPlayerJoined += OnPlayerJoined;
    playerInputManager.onPlayerLeft += OnPlayerLeft;
}

void OnPlayerJoined(PlayerInput playerInput) {
    Debug.Log($"玩家 {playerInput.playerIndex} 加入");
    // 配置玩家
}

void OnPlayerLeft(PlayerInput playerInput) {
    Debug.Log($"玩家 {playerInput.playerIndex} 离开");
}
```

---

### 5. InputDevice（设备基类）

所有输入设备的基类，包括Keyboard、Mouse、Gamepad等。

#### ✅ 推荐使用的API

##### current - 当前活跃设备
```csharp
// ✅ 推荐：获取当前设备
Keyboard keyboard = Keyboard.current;
Mouse mouse = Mouse.current;
Gamepad gamepad = Gamepad.current;

// 检查设备是否存在
if (keyboard != null) {
    // 键盘可用
}
```

#### ⚠️ 性能警告的API

##### wasPressedThisFrame / wasReleasedThisFrame
```csharp
// ⚠️ 警告：直接访问设备按键（适用于简单场景）
if (Keyboard.current.spaceKey.wasPressedThisFrame) {
    Jump();
}

// ✅ 推荐：使用InputAction和事件
void OnEnable() {
    jumpAction.performed += OnJump;
}
```

---

### 6. Keyboard（键盘设备）

#### ✅ 推荐使用的API

```csharp
// ✅ 推荐：访问特定按键
Keyboard keyboard = Keyboard.current;

// 检查按键状态
if (keyboard.spaceKey.isPressed) { }      // 是否按下
if (keyboard.spaceKey.wasPressedThisFrame) { }  // 这一帧按下
if (keyboard.spaceKey.wasReleasedThisFrame) { } // 这一帧释放

// 常用按键
keyboard.spaceKey
keyboard.enterKey
keyboard.escapeKey
keyboard.tabKey
keyboard.backspaceKey
keyboard.upArrowKey
keyboard.downArrowKey
keyboard.leftArrowKey
keyboard.rightArrowKey

// 字母和数字
keyboard.aKey
keyboard.digit0Key
```

#### ⚠️ 性能警告的API

```csharp
// ⚠️ 警告：遍历所有按键（性能差）
foreach (var key in Keyboard.current.allKeys) {
    if (key.wasPressedThisFrame) {
        Debug.Log($"按键按下: {key.displayName}");
    }
}

// ✅ 推荐：只检测特定按键
if (Keyboard.current.spaceKey.wasPressedThisFrame) {
    // 处理空格键
}
```

---

### 7. Mouse（鼠标设备）

#### ✅ 推荐使用的API

```csharp
Mouse mouse = Mouse.current;

// ✅ 推荐：鼠标位置
Vector2 position = mouse.position.ReadValue();
Debug.Log($"鼠标位置: {position}");

// ✅ 推荐：鼠标按钮
if (mouse.leftButton.wasPressedThisFrame) {
    Debug.Log("左键按下");
}

if (mouse.rightButton.wasPressedThisFrame) {
    Debug.Log("右键按下");
}

if (mouse.middleButton.wasPressedThisFrame) {
    Debug.Log("中键按下");
}

// ✅ 推荐：鼠标滚轮
Vector2 scroll = mouse.scroll.ReadValue();
if (scroll.y > 0) {
    Debug.Log("向上滚动");
} else if (scroll.y < 0) {
    Debug.Log("向下滚动");
}

// ✅ 推荐：鼠标移动增量
Vector2 delta = mouse.delta.ReadValue();
if (delta.magnitude > 0) {
    Debug.Log($"鼠标移动: {delta}");
}
```

---

### 8. Gamepad（游戏手柄）

#### ✅ 推荐使用的API

```csharp
Gamepad gamepad = Gamepad.current;

// ✅ 推荐：左摇杆
Vector2 leftStick = gamepad.leftStick.ReadValue();
Debug.Log($"左摇杆: {leftStick}");

// ✅ 推荐：右摇杆
Vector2 rightStick = gamepad.rightStick.ReadValue();

// ✅ 推荐：扳机
float leftTrigger = gamepad.leftTrigger.ReadValue();
float rightTrigger = gamepad.rightTrigger.ReadValue();

// ✅ 推荐：肩键
if (gamepad.leftShoulder.wasPressedThisFrame) {
    Debug.Log("左肩键按下");
}

// ✅ 推荐：ABXY按钮
if (gamepad.buttonSouth.wasPressedThisFrame) { } // A (Xbox) / Cross (PS)
if (gamepad.buttonEast.wasPressedThisFrame) { }  // B (Xbox) / Circle (PS)
if (gamepad.buttonWest.wasPressedThisFrame) { }  // X (Xbox) / Square (PS)
if (gamepad.buttonNorth.wasPressedThisFrame) { } // Y (Xbox) / Triangle (PS)

// ✅ 推荐：方向键
Vector2 dpad = gamepad.dpad.ReadValue();
if (dpad.y > 0) Debug.Log("上");
if (dpad.y < 0) Debug.Log("下");
if (dpad.x > 0) Debug.Log("右");
if (dpad.x < 0) Debug.Log("左");

// ✅ 推荐：Start和Select
if (gamepad.startButton.wasPressedThisFrame) { }
if (gamepad.selectButton.wasPressedThisFrame) { }

// ✅ 推荐：震动
gamepad.SetMotorSpeeds(leftMotor: 0.5f, rightMotor: 0.8f);
// leftMotor: 低频震动（手柄左侧）
// rightMotor: 高频震动（手柄右侧）

// 停止震动
gamepad.SetMotorSpeeds(0f, 0f);
```

---

### 9. Touchscreen（触摸屏）

#### ✅ 推荐使用的API

```csharp
Touchscreen touchscreen = Touchscreen.current;

// ✅ 推荐：触摸数量
int touchCount = touchscreen.touches.Count;
Debug.Log($"触摸数量: {touchCount}");

// ✅ 推荐：获取特定触摸
for (int i = 0; i < touchCount; i++) {
    TouchControl touch = touchscreen.touches[i];
    
    // 触摸位置
    Vector2 position = touch.position.ReadValue();
    
    // 触摸阶段
    TouchPhase phase = touch.phase.ReadValue();
    
    switch (phase) {
        case TouchPhase.Began:
            Debug.Log($"触摸开始: {position}");
            break;
        case TouchPhase.Moved:
            Debug.Log($"触摸移动: {position}");
            break;
        case TouchPhase.Ended:
            Debug.Log($"触摸结束: {position}");
            break;
    }
}

// ✅ 推荐：主触摸（第一个触摸）
TouchControl primaryTouch = touchscreen.primaryTouch;
Vector2 primaryPosition = primaryTouch.position.ReadValue();
```

---

## InputAction.CallbackContext

在事件处理中传递的上下文对象。

### ✅ 推荐使用的属性和方法

```csharp
void OnJump(InputAction.CallbackContext context) {
    // ✅ 获取Action引用
    InputAction action = context.action;
    
    // ✅ 检查Action名称
    string actionName = context.action.name;
    
    // ✅ 读取输入值
    Vector2 value = context.ReadValue<Vector2>();
    
    // ✅ 检查阶段
    if (context.started) {
        Debug.Log("开始");
    }
    if (context.performed) {
        Debug.Log("完成");
    }
    if (context.canceled) {
        Debug.Log("取消");
    }
    
    // ✅ 获取持续时间
    double duration = context.duration;
    Debug.Log($"持续时间: {duration}秒");
    
    // ✅ 获取时间戳
    double time = context.time;
}
```

---

## InputBinding（输入绑定）

### ✅ 推荐使用的API

```csharp
// ✅ 推荐：获取绑定信息
InputAction action = moveAction;
InputBinding binding = action.bindings[0];

Debug.Log($"绑定路径: {binding.path}");
Debug.Log($"绑定组: {binding.groups}");
Debug.Log($"显示名称: {binding.name}");

// ✅ 推荐：重新绑定
void StartRebind() {
    moveAction.PerformInteractiveRebinding()
        .WithCancelingThrough("<Keyboard>/escape")
        .OnComplete(operation => {
            Debug.Log("重新绑定完成");
            operation.Dispose();
        })
        .Start();
}
```

---

## InputControl（控件基类）

所有输入控件的基类（按键、摇杆、触发器等）。

### ✅ 推荐使用的属性

```csharp
InputControl control = Keyboard.current.spaceKey;

// ✅ 推荐：获取控件信息
string name = control.name;              // 控件名称
string displayName = control.displayName; // 显示名称
string path = control.path;              // 控件路径
string layout = control.layout;          // 控件布局

// ✅ 推荐：读取值
bool isPressed = control.IsPressed();
float magnitude = control.EvaluateMagnitude();
```

---

## 性能优化清单

### ✅ 推荐做法

1. **使用事件订阅替代轮询**
   - 使用 `performed` / `started` / `cancelled` 事件
   - 避免在Update中频繁ReadValue

2. **缓存设备引用**
   ```csharp
   private Keyboard keyboard;
   void Awake() { keyboard = Keyboard.current; }
   ```

3. **合理启用/禁用Actions**
   - 在OnEnable中启用
   - 在OnDisable中禁用

4. **使用InputActionAsset组织输入**
   - 统一管理所有Actions
   - 支持Control Schemes切换

5. **使用PlayerInput简化多玩家**
   - 自动处理玩家分配
   - 支持本地多人游戏

### ❌ 禁止做法

1. **禁止使用旧Input系统**
   ```csharp
   Input.GetKeyDown(KeyCode.Space); // ❌ 禁止
   ```

2. **禁止在Update中频繁ReadValue**
   ```csharp
   void Update() {
       Vector2 move = moveAction.ReadValue<Vector2>(); // ❌ 禁止
   }
   ```

3. **禁止忘记启用Actions**
   ```csharp
   // ❌ 错误：Action未启用
   void Start() {
       jumpAction.performed += OnJump; // 没有Enable，不会触发
   }
   
   // ✅ 正确：显式启用
   void OnEnable() {
       jumpAction.Enable();
       jumpAction.performed += OnJump;
   }
   ```

---

## 常见问题

### Q1: InputAction不触发？
**A:** 检查是否调用了Enable()
```csharp
void OnEnable() {
    moveAction.Enable(); // 必须启用
}
```

### Q2: 如何检测长按？
**A:** 使用started和canceled事件
```csharp
bool isHolding = false;

void OnEnable() {
    action.started += context => isHolding = true;
    action.canceled += context => isHolding = false;
}
```

### Q3: 如何支持多个设备？
**A:** 使用Control Schemes
```csharp
// 在Input Actions Asset中定义多个Control Schemes
// Gamepad、KeyboardMouse等
```

---

## 相关资源

- [Input System Scripting API](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/api/UnityEngine.InputSystem.html)
- [Input System Manual](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/manual/index.html)

---

## 版本说明

- 文档版本：1.0
- Unity版本：2021.3 LTS+
- Input System版本：1.0+
- 最后更新：2024年
