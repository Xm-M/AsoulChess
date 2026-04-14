# Buff 模块

## 定位

棋子可叠加的 **增益/减益**：持续时间、Tick、属性修正、与 `PropertyController` / `TimerManage` 交互。

## 核心类型

| 类型 | 职责 |
|------|------|
| `BuffController` | 添加、移除、遍历 Buff |
| `Buff` | 生命周期、`BuffOver`、与 `Chess` 引用 |
| 各类 `*Buff` | 继承或组合实现具体效果 |

## 依赖

- 强依赖 **Chess**；可能用 **Manage** 计时；少用直接 UI 引用。  

## 扩展

- 新 Buff：新类 + 在 Creator / 技能里挂载配置。  
- 注意离场时移除订阅与 Timer，避免空引用。
