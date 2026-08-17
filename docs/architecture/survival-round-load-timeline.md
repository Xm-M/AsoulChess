# 架构补丁: 生存读档 = 某轮「进关」正常 Timeline（2026-08-14）

## 决策
- 抛弃冒险式局内续波读档（`loadSkipToTime` 旁路）用于生存。
- 生存只在**轮末**存：轮次、植物、阳光、手牌；`currentWave=-1`。
- 读档：`SurvivalLoadPlayTimelineFlow` 从 `roundTimelineStartTime` **正常 Play**，信号驱动 EnterMap → Prepare → Start。
- 商店：读档/次轮不重置阳光，预勾手牌（`PreParePlugun_ShowPlantShop` 已有判断）。
- 植物：EnterMap 时场上已有则跳过 `RestorePlayerPlants`。

## 文件
- `MapManage_PVZ.cs` — 生存/冒险读档分流
- `LevelController_Endless.cs` — `PrepareSurvivalLoadFromSave`、进关不续波、有植物不重种
