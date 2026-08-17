# 架构/影响面: survival-round-more-zombies-banner

**日期**: 2026-08-14

## 方案
```
OnRoundComplete:
  ForceClear / ClearSun / ClearTime
  if !CheckSurvivalWin即将通关:
    TextPanel.MoreZombiesComing()   // 新 API，独立 GO
  wait roundTransitionDelay
  ...
  if CheckSurvivalWin: GameOver(true); break  // 不播横幅
  else: Timeline / 选卡
```

注意：`CheckSurvivalWin` 目前在 delay **之后**调用。应按 Q2/Q3：要么提前判断「本轮结束后是否通关」再决定是否播横幅，要么把横幅放在 `if (!CheckSurvivalWin)` 分支内、delay 之前。

推荐顺序：
```
Clear...
bool willWin = WouldSurvivalWinAfterThisRound(); // 或先 totalWavesCleared += count 再判断
if (!willWin) MoreZombiesComing();
wait delay
RoundOverPlugins / Capture / ...
if (willWin) { GameOver(true); ... }
else { Timeline... }
```
（实现时注意 `totalWavesCleared` 累加与现逻辑一致，避免双加。）

## 文件
- `TextPanel.cs` + `文字显示面板.prefab`
- `LevelController_Endless.cs`

## 通关奖励缺口（知情）
`GameOver(true)` 胜利用途不含 `SpawnVictoryReward`；生存通关当前无奖杯掉落。本需求默认不修，审批时可勾选「一并修」。
