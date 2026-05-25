# 功能需求卡片：肉鸽 Run 存档

## 基本信息

- **功能名称**：肉鸽 Run 独立存档 + 玩家元进度
- **所属模块**：Roguelike / SaveSystem
- **需求类型**：功能扩展（新增 Run 存档子系统 + 扩展 PlayerSaveData）
- **优先级**：P1
- **预估复杂度**：L2
- **提出日期**：2026-05-20

## 功能描述

### 详细描述

为肉鸽模式增加**独立于关卡战斗快照**的 Run 存档：只保存地图选路进度（含当前地图各房间 grid 位置、当前/已访问/已通关节点），单槽「继续冒险」。同时把最高进度写入 `PlayerSaveData`，供日后解锁与统计。

不保存肉鸽战斗关卡中途状态（与 `GameSaveData` 分离）。

### 用户故事

作为玩家，我希望退出游戏后能继续上一次肉鸽冒险的地图与选路进度，并在账号存档里记录我打到过的最高层数，以便日后解锁内容。

## 现有业务上下文

### 相关现有功能

| 功能 | 模块 | 关系 |
|------|------|------|
| `PlayerSaveSystem` | SaveSystem | 写入肉鸽 Meta 进度 |
| `SaveSystem` / `GameSaveData` | SaveSystem | 肉鸽战斗关默认不写 |
| `RoguelikeRunService` | Roguelike | 扩展 Save/Load/Continue |
| `RoguelikeRunState` | Roguelike | 序列化主体 |

### 集成点

- **调用**：`RoguelikeRunSaveSystem.Save/Load/Delete`
- **触发 Save**：节点通关、换 Act、主动放弃前更新 Meta
- **触发 Delete**：战斗失败、放弃 Run、新开 Run 覆盖
- **继续游戏**：`StartUI` / `RoguelikeMapPanel` 读取 active Run

### 对现有功能的影响

- `SaveSystem.SaveCurrentLevel`：肉鸽 Run 进行中时跳过
- `PlayerSaveData`：新增若干 int 字段（版本迁移）

## 已确认决策（2026-05-20）

1. **粒度**：1.A — 仅地图选路；保存 `currentMap` 全节点（含 `layer`/`slot`）、`currentNodeId`、`visited`/`cleared`
2. **槽位**：2.A — 全局一个 `active` Run
3. **PlayerSaveData**：记录最高 Act/层等 Meta，未来解锁基于此
4. **分层**：Run 内数据 → `RoguelikeRunSaveData`；永久 Meta → `PlayerSaveData`

## 验收标准

- [ ] 选路推进后退出，再进可继续，地图拓扑与房间列位置一致
- [ ] 当前节点、已访问/已通关与退出前一致
- [ ] 战斗失败或放弃后无 active Run，但 PlayerSaveData 保留最高进度
- [ ] 肉鸽进战斗不写 `LevelSaves`
- [ ] 主菜单/开始界面可「继续冒险」（有档时）

## 关联 Context

- `context/index.md`
- `Assets/Script/SaveSystem/`
- `Assets/Script/Roguelike/Map/`
