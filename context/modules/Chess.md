# Chess 模块

## 定位

**棋子**唯一根对象（`Chess` MonoBehaviour），通过多个 **Controller** 组合属性、技能、状态、Buff、移动、攻击与动画。

## 核心类型

| 类型 | 职责 |
|------|------|
| `Chess` | `InitChess`、`WhenChessEnterWar`、`Death`、事件 `WhenEnterGame` / `DeathEvent` / `OnRemove` |
| `PropertyController` | 属性、`PropertyCreator`、承伤与结算 |
| `SkillController` | `ISkill` 主动/被动、`SkillContext`、`UseSkill` / `SkillOver` |
| `StateController` | 状态图驱动（Idle / Attack / Skill / Death…） |
| `BuffController` | Buff 列表与 Tick |
| `MoveController` | 格子、路径、`standTile` |
| `AttackController` | 武器、索敌、普攻 |
| `AnimatorController` | 动画；子类如 `AnimatorController_Zombieking` 重写 `PlayIdle` / `PlaySkill` / `PlayDeath` |

## 生命周期（简）

1. `InitChess()` — 各 Controller `InitController(this)`  
2. `WhenChessEnterWar(triggerEvents)` — 入场与事件  
3. `Update` — `StateController.StateUpdate()`  
4. `Death` / 离场 — 清事件、回池或销毁  

## 僵王动画（与 Context 对齐）

- 待机形态：`SkillContext` 键 **`stand`**（bool），由 `AnimatorController_Zombieking.PlayIdle()` 与 `anim_idle` / `anim_head_*` 同步。  
- 技能片段：进入 `SkillState` 前写入 **`ZombieKingContextKeys`** + **`ZombieKingSkillAnimKind`**，由 `PlaySkill()` 分发 `anim_spawn_*`、`anim_stomp_*`、`anim_RV_1`、`anim_bungee_*`、`anim_head_attack_*`。  

## 扩展方式

- 新棋子类型：换 `PropertyCreator` 预制数据；动画继承 `AnimatorController`。  
- 新能力：优先 **`ISkill` / `ISkillEffect`** 与 Buff，而非继承 `Chess` 除非必要。
