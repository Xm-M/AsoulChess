# State 模块

## 定位

棋子 **有限状态机**：`StateController` 持有当前 `State`，通过 `Transition` 切换；与动画结束、技能结束、Buff 等条件联动。

## 核心类型

| 类型 | 职责 |
|------|------|
| `StateController` | 注册状态图、`StateUpdate`、切换 |
| `State` / `StateName` | Idle、Attack、Skill、Death、Dizzy 等 |
| `Transition` | `ifReach(Chess)`；如 `IfAnimPlayOverTransition`（实际常接 `IsSkillFinished`） |
| 自定义 | 如 `Transition_ZombieKing_ChangeIdleTransition` 写 `SkillContext` |

## 与动画

- `SkillState.Enter` → `animatorController.PlaySkill()`  
- 部分 Transition 依赖 `AnimatorController.IfAnimPlayOver()` 或指定状态名  

## 扩展

- 新状态：实现 `State` + 入出场逻辑；在状态图资源中连线。  
- 条件复杂时优先独立 `Transition` 类，避免在 `State` 内硬编码跨模块引用。
