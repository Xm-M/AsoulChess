# 架构分析: survival-round-replant-cleartime

**日期**: 2026-08-14  
**复杂度**: L2  
**模块**: LevelSystem / Endless / SaveSystem

## 1. 系统定位
- **归属**: `LevelController_Endless` 驱动轮界；`SaveSystem` 提供植物快照；`TimerManage.ClearTime` 清 Timer；`ChessTeamManage` 创建/回收。
- **上游**: Timeline 信号、选卡 `PlantsShop`、GameStart 插件。
- **下游**: 场上植物实例、Buff/被动、羁绊、ProgressBar/存档。

## 2. 推荐方案
**双次种植 + 轮末 ClearTime（生存专用）**

```
轮末:
  ClearFieldSunLights (已有)
  ClearTime()
  CaptureSurvivalPlants() // creatorId, tile, hp — 无 buffs
  Save

进轮 / 读档 EnterMap:
  若场空 → PlantForDisplay(snapshot)  // CreateChess + Plant，EnterWar 可弱化/不挂战斗 Timer 或照常但随后会灭
  （同会话若仍有旧实例：先静默清空再 PlantForDisplay）

选卡 → 玩家开战 → GameStart():
  跑 GameStartPlugin
  ClearPlayerPlantsSilent()
  PlantForCombat(snapshot)  // 完整 WhenChessEnterWar，重挂 Buff/Timer
  LevelManage.GameStart() / Resume
```

**备选（未采纳）**: 只 ClearTime 不重种 → 植物 Timer 丢失；或完整序列化 `buffFrom` → 成本高且与「模拟重载」目标不符。

## 3. 关键设计点
| 点 | 决策 |
|----|------|
| 静默灭植物 | 避免走完整 `Death()` 掉落/统计；优先 LeaveWar + Recycle / 专用 Remove |
| 展示种 vs 进战种 | 可用同一 `RestorePlayerPlants(restoreBuffs:false)`；进战种再调一次完整进战 |
| 事件重复 | 展示种建议 `WhenChessEnterWar(triggerEvents:false)`；插件若依赖事件则展示种仍触发 EnterMap 相关、进战事件留给重种 |
| 冒险模式 | **不改** Buff 存读；仅 `LevelMode.SurvivalMode` 跳过 buffs |

## 4. 风险
- 中：插件假设植物已有被动 Buff → 用展示种跑插件可能偏弱；需测羁绊插件。
- 中：双次种植性能与闪烁 → 可接受；必要时同帧无动画。
- 低：旧档 buffs 残留 → Restore 忽略。

## 5. 结论
方案与「重载关卡模拟」一致，可行。风险可控，建议进入开发。
