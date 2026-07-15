# 架构设计: 多首 · 幽灵召唤主动技

## 基本信息
- **模块**: Skill（主动/被动）/ Chess（轻量召唤）/ Weapon（索敌近战）
- **复杂度**: L2
- **日期**: 2026-07-15

## 系统定位
在多首被动之上增加终局主动技；幽灵为轻量 Player 阵营 Chess，不占格。

## 设计决策

| 决策 | 选择 | 理由 |
|------|------|------|
| 幽灵载体 | 轻量 Chess | 复用状态机/武器/动画；追击近战是单位 AI |
| 生成 | `chessFactory` + `AddChess` | 避开 `tile.ChessEnter` |
| 无敌/不可选 | `UnSelectable` + 关 Collider | 层切换 + 物理不吃铲/撞 |
| 终局锁 | SkillContext 键 | 被动出头/降 limit 统一跳过 |
| 加压 | `timerManage` + `BuffReset` | 对齐 Tomo |

## 数据流

```
ColdSkill(ready: 有头 + 点击/条件)
  → SkillEffect_MultiHeadGhostBurst
      → stress=0
      → N=cloneCount; ClearHeads; SetFinaleLocked
      → Spawn N ghosts (off-tile)
      → Start stress tick timer on master
  → PassiveSkillEffect_MultiHeadMutsumi 见锁则不再出头/降 limit
  → PassiveSkillEffect_MultiHeadGhost 追击；近战 FindTarget+CloseAttack
  → master OnRemove → Destroy all ghosts + stop timer
```

## 新增类型（建议路径）

| 类型 | 路径 |
|------|------|
| `SkillEffect_MultiHeadGhostBurst` | `Assets/Script/Skill/ISkillEffect_Plant/AveMujica_Skill/` |
| `SkillReady_MultiHeadHasClones` | 同上或 `ISkillReady/` |
| `PassiveSkillEffect_MultiHeadGhost` | `ISkillPassive/AveMujica/` |
| `FindTarget_NearestEnemyInRange` | `Weapon/IFindTarget/` |
| Keys 扩展 | `MultiHeadMutsumiKeys` |
| 被动锁 | `PassiveSkillEffect_MultiHeadMutsumi` |

## 不采用
- 子弹改造
- 占格魂灵之影式 `CreateChess(tile)`
