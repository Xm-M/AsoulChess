# Unity Input System 基础教程

## 快速开始

### 1. 安装Input System Package

```
Window → Package Manager → Unity Registry → Input System → Install
```

安装后需要重启Unity并启用新Input System：
```
Project Settings → Player → Other Settings → Active Input Handling → Input System Package (New)
```

### 2. 创建Input Actions Asset

```
Project窗口右键 → Create → Input Actions
命名为"PlayerControls.inputactions"
```

### 3. 配置Actions

双击打开Input Actions编辑器：

1. 创建Action Map：点击"+"添加"Gameplay"
2. 添加Action：点击"+"添加"Move"、"Jump"、"Fire"
3. 绑定按键：点击Action右侧的"+"添加Binding

### 4. 生成C#类（推荐）

在Input Actions Asset中：
```
勾选"Generate C# Class"
点击"Apply"
```

### 5. 使用示例

```csharp
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private PlayerControls controls;
    private Vector2 moveInput;

    void Awake()
    {
        controls = new PlayerControls();
    }

    void OnEnable()
    {
        controls.Gameplay.Enable();
        controls.Gameplay.Move.performed += OnMove;
        controls.Gameplay.Move.canceled += OnMoveCanceled;
        controls.Gameplay.Jump.performed += OnJump;
    }

    void OnDisable()
    {
        controls.Gameplay.Disable();
        controls.Gameplay.Move.performed -= OnMove;
        controls.Gameplay.Move.canceled -= OnMoveCanceled;
        controls.Gameplay.Jump.performed -= OnJump;
    }

    void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    void OnMoveCanceled(InputAction.CallbackContext context)
    {
        moveInput = Vector2.zero;
    }

    void OnJump(InputAction.CallbackContext context)
    {
        Debug.Log("Jump!");
    }

    void Update()
    {
        transform.position += new Vector3(moveInput.x, moveInput.y, 0) * Time.deltaTime * 5f;
    }
}
```

## 核心概念

### Action Maps
Action Maps将输入动作分组，适用于不同游戏状态：
- Gameplay - 游戏中的输入
- Menu - 菜单中的输入
- Vehicle - 载具控制

### Actions
每个Action代表一个输入动作：
- Move - 移动（Vector2类型）
- Jump - 跳跃（Button类型）
- Fire - 射击（Button类型）

### Bindings
Bindings将物理输入映射到Actions：
- `<Keyboard>/w` - 键盘W键
- `<Gamepad>/leftStick` - 手柄左摇杆
- `<Mouse>/leftButton` - 鼠标左键

### Control Schemes
Control Schemes定义不同的输入方案：
- KeyboardMouse - 键盘鼠标
- Gamepad - 游戏手柄
- Touch - 触摸屏

## 常见配置

### WASD移动配置

```
Action: Move (Vector2)
Bindings:
  - <Gamepad>/leftStick (Gamepad)
  - WASD Composite (KeyboardMouse)
    - Up: <Keyboard>/w
    - Down: <Keyboard>/s
    - Left: <Keyboard>/a
    - Right: <Keyboard>/d
```

### 跳跃配置

```
Action: Jump (Button)
Bindings:
  - <Gamepad>/buttonSouth (Gamepad) - A键
  - <Keyboard>/space (KeyboardMouse)
```

### 射击配置

```
Action: Fire (Button)
Bindings:
  - <Gamepad>/rightTrigger (Gamepad)
  - <Mouse>/leftButton (KeyboardMouse)
```

## 调试

### Input Debugger

```
Window → Analysis → Input Debugger
```

可以实时查看所有输入设备和输入事件。

### 打印输入信息

```csharp
void OnMove(InputAction.CallbackContext context)
{
    Vector2 value = context.ReadValue<Vector2>();
    Debug.Log($"Move: {value}, Phase: {context.phase}");
}
```

## 常见问题

### Q: Action不触发？
**A:** 检查是否调用了Enable()

### Q: 如何切换控制方案？
**A:** 使用PlayerInput组件的SwitchCurrentControlScheme方法

### Q: 如何重新绑定按键？
**A:** 使用PerformInteractiveRebinding API

---

## 版本说明
- 教程版本：1.0
- Unity版本：2021.3 LTS+
- 最后更新：2024年
