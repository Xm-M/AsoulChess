# Skill 模块

## 定位

棋子 **主动/被动技能**：冷却、选目标、效果执行、与 **状态机**、**动画**、**SkillContext** 协作。

## 结构要点

| 概念 | 说明 |
|------|------|
| `ISkill` | 主动技能接口；`InitSkill` / `UseSkill` / `SkillOver` / `IfSkillReady` 等 |
| `ISkillEffect` | 效果片段，配置在技能 SO 上，`SkillEffect(user, config, targets)` |
| `SkillController` | 持有 `activeSkill` / `passiveSkill`，`context` 字典，`UseSkill()` 由动画事件或立即类触发 |
| `SkillContext` | `Set` / `TryGet`；部分键不参与存档（`ShouldSkipKey`） |
| `SkillState` | 进入时 `PlaySkill()`；`IsSkillFinished` 由技能自身实现 |

## 项目规则（必读）

- **`Assets/Script/Skill/`** 下：**禁止**为技能再挂运行时专用 MonoBehaviour；用 `ISkillEffect` + `Chess` 事件 / `WhenEnterGame` / `OnRemove`。  
- 协程：**`user.StartCoroutine`**（Chess 为 MonoBehaviour）。  

## 僵王技能动画约定

进入技能状态前设置：

- `ZombieKingContextKeys.SkillAnimKind` → `ZombieKingSkillAnimKind` 整型  
- 按需：`Row`、`StompBand`、`BallVisual`（换皮用，Animator 不读）  

详见 `Assets/Script/Chess/AnimtorControllier/Zombie/AnimatorController_Zombieking.cs`。

## 扩展

- 新效果：实现 `ISkillEffect`，在配置中 `[SerializeReference]` 选型。  
- 复合技能：用组合 `ISkill` 或多段 `ISkillEffect`，避免在 `SkillController` 堆 List 破坏单主动技能假设（除非已有复合类型如 `MultySkill`）。
