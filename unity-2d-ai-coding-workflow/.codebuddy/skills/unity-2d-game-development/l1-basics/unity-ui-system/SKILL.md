---
name: unity-ui-system
description: Unity UI系统，包括Canvas、TextMeshPro、Button等组件。强调Canvas分离策略（静态UI和动态UI分离提升5-10倍性能）、使用TextMeshPro替代Legacy Text、避免Layout Group频繁重建。提供完整的性能优化指南。
---

# Unity UI System Skill

## 技能描述
Unity UI系统是创建游戏界面的核心系统，包括Canvas、Button、Image、Text等组件。本技能提供UI开发的最佳实践、性能优化建议和API白名单。

---

## API白名单

### ✅ 推荐使用的API

**Canvas设置：**
- 使用Screen Space - Overlay用于简单UI
- 使用Screen Space - Camera用于3D UI效果
- 使用World Space用于游戏内UI

**性能优化：**
- 分离动态和静态UI到不同Canvas
- 使用对象池管理UI元素
- 使用ObjectPool替代Instantiate/Destroy

### ⚠️ 性能警告的API

**Layout重建：**
- 避免频繁启用/禁用LayoutGroup
- 避免在运行时动态添加/移除Layout元素

### ❌ 禁止使用的API

**性能陷阱：**
- ❌ 在Update中频繁修改UI属性
- ❌ 每帧调用LayoutRebuilder.ForceRebuildLayoutImmediate
- ❌ 大量嵌套LayoutGroup

---

## 功能边界 ⚠️ 强制说明

### 本Skill涵盖范围

- ✅ Canvas组件配置（Screen Space、World Space）
- ✅ TextMeshPro文本系统
- ✅ Button、Toggle、Slider等基础UI组件
- ✅ Image、RawImage图形组件
- ✅ ScrollRect滚动视图
- ✅ Layout Group布局系统
- ✅ Canvas分离策略（静态UI/动态UI分离）
- ✅ UI事件系统（EventSystem、EventTrigger）
- ✅ UI对象池管理

### 不在本Skill范围内

- ❌ UI Toolkit（USS/UXML）→ 本项目使用UGUI
- ❌ IMGUI（OnGUI）→ 仅用于编辑器开发
- ❌ 自定义UI Shader → 不涉及
- ❌ UI动画系统 → 见 unity-2d-animation Skill
- ❌ UI输入系统 → 见 unity-input-system Skill

### 跨Skill功能依赖

**完整UI系统需要**：
- unity-ui-system（UI系统）← 当前Skill
- unity-input-system（输入系统）
- unity-ui-panel（UI面板控制器）

**UI动画系统需要**：
- unity-ui-system（UI组件）← 当前Skill
- unity-2d-animation（动画系统）

### 性能限制

| 指标 | 建议值 | 说明 |
|------|--------|------|
| Canvas数量 | ≤ 10 | 静态/动态UI分离 |
| 活跃UI元素 | ≤ 500 | 减少渲染开销 |
| Layout嵌套层级 | ≤ 3 | 避免频繁重建 |
| TextMeshPro字体 | ≤ 5 | 控制字体资源数量 |

---

## 核心原则

1. **Canvas分离策略** - 动态UI和静态UI使用不同Canvas
2. **避免频繁Rebuild** - 减少Layout重建次数
3. **使用对象池** - 复用UI元素避免频繁创建销毁

---

## 版本说明
- 技能版本：1.0
- Unity版本：2021.3 LTS+
- 最后更新：2024年
