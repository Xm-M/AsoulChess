---
name: unity-ui-panel
description: 用于快速生成Unity UI面板控制器，支持弹出窗口、常驻面板、全屏界面，包含动画系统和事件绑定。
---

# Unity UI面板生成器

## 目的

快速生成Unity UI面板控制器，支持多种面板类型、动画效果和事件系统，提高UI开发效率。

## 使用场景

适用于以下情况：
- 创建菜单、背包、设置、商店等UI界面
- 需要UI面板的打开/关闭动画
- 需要UI元素的自动事件绑定
- 需要统一的UI管理架构

## 工作流程

### 步骤1：确认面板类型

**操作**：询问用户面板类型：
1. **popup**（弹出窗口）- 点击外部可关闭，有遮罩层
   - 适用：提示框、确认框、简单设置
2. **persistent**（常驻面板）- 一直显示，无遮罩
   - 适用：HUD、主菜单
3. **fullscreen**（全屏界面）- 占满屏幕，独占显示
   - 适用：背包、技能树、地图

**确认点**：等待用户选择面板类型。

### 步骤2：确认动画方案

**操作**：询问动画方案：
- **none** - 无动画
- **dotween** - DOTween动画（推荐，需安装DOTween插件）
- **animation** - Unity Animation组件

**确认点**：等待用户选择。

### 步骤3：确认UI元素

**操作**：让用户提供UI元素列表（JSON格式）：
```
[
  {
    "name": "elementName",
    "type": "Button/Toggle/InputField/Text/Image/Slider/Scrollbar",
    "action": "事件类型",
    "tooltip": "元素说明"
  }
]
```

**示例**：
```
[
  {
    "name": "closeButton",
    "type": "Button",
    "action": "Close"
  },
  {
    "name": "confirmButton",
    "type": "Button",
    "action": "OnConfirm"
  }
]
```

**确认点**：等待用户提供元素列表。

### 步骤4：生成面板类

**操作**：生成完整的UI面板控制器，包含：
- ✅ 继承BasePanel基类
- ✅ UI元素引用
- ✅ 事件绑定逻辑
- ✅ 动画控制（如启用）
- ✅ 遮罩层处理（popup类型）

## 生成内容

### BasePanel基类（首次使用生成）
- Open()方法：打开面板
- Close()方法：关闭面板
- Refresh()方法：刷新数据
- 事件：OnOpen, OnClose

### 具体面板类
- 继承BasePanel
- UI元素引用（[SerializeField]）
- 事件绑定（Start中绑定）
- 事件注销（OnDestroy中注销）
- 动画逻辑（如启用）

## 代码规范

生成的代码遵循：
- 使用`[SerializeField] private`声明UI元素
- 在Awake中获取组件引用
- 在Start中绑定事件
- 在OnDestroy中注销事件
- 使用`#region`划分代码区域
- 包含XML注释

## 验收标准

- [ ] 编译无错误无警告
- [ ] UI元素引用正确
- [ ] 事件绑定正常
- [ ] 动画流畅（如启用）
- [ ] 遮罩显示正确（popup类型）
- [ ] 内存无泄漏
- [ ] 包含完整注释

## 使用示例

### 示例1：主菜单面板

```
用户：我需要生成主菜单UI

AI：好的！请选择面板类型：
1. popup - 弹出窗口
2. persistent - 常驻面板
3. fullscreen - 全屏界面

[用户选择：persistent]

AI：收到！常驻面板。

请选择动画方案：
- none - 无动画
- dotween - DOTween动画
- animation - Animation组件

[用户选择：dotween]

AI：请提供UI元素列表（JSON格式）：

[用户提供元素列表]

AI：正在生成MainMenuPanel.cs...
[生成完整代码]
```

### 示例2：设置面板

```
用户：我需要设置面板

AI：请选择面板类型...

[用户选择：popup]

AI：请选择动画方案...

[用户选择：dotween]

AI：请提供UI元素：

[用户提供：音乐滑块、音效滑块、语言下拉框、关闭按钮]

AI：正在生成SettingsPanel.cs...
[生成完整代码]
```

## 注意事项

- DOTween需要提前安装并导入命名空间
- popup类型会自动生成遮罩层和点击关闭逻辑
- 事件绑定采用安全的null检查模式
- 生成的代码需要配合Unity UI系统使用
