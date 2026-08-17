# 架构设计分析: survival-save-wave-hud

**日期**: 2026-08-14  
**复杂度**: L2  
**需求**: 生存存档修复 + ProgressBar 波次 HUD

## 1. 系统定位

| 项 | 归属 |
|----|------|
| 所属模块 | LevelSystem（主）+ SaveSystem + UI |
| 上游 | `LevelManage`、`MapManage_PVZ` 读档流 |
| 下游 | `PlantsShop` 选卡 UI、`ProgressBar` |
| 横向 | 冒险存档路径（须隔离，避免改门闸误伤） |

**模块归属**: 不新建系统；修补现有生存 Controller / 选卡插件 / ProgressBar。

## 2. 设计决策（推荐）

### 方案 A（推荐）：最小修补三处

1. **选卡缓存**  
   - `PreParePlugun_ShowPlantShop.ClearLockedHand()`  
   - `RestartLevel` / 无档进关时调用；`OverPlugin` 也可清，防止 SO 残留  

2. **植物持久化**  
   - 轮末：保持「先 `SaveCurrentLevel` 再 `GamePause`」（已有）  
   - 增补：生存模式下，轮间准备阶段离开关卡前，若场上仍有植物且内存档过期，允许一次 `SaveSurvivalSnapshot`（绕过或局部放宽 `IfGameStart`），**仅 SurvivalMode**  
   - 读档：确认 `EnterMap` Restore 在地图就绪后执行；必要时延迟一帧；避免二次 EnterMap 清场  

3. **波次 HUD**  
   - 在 `ProgressBar.Show` / `MoveBar`（或进波回调）中，若 `levelMode == SurvivalMode`，标题设为  
     `{levelName} · 第{cur}/{max}波`  
   - `cur`：本轮显示波（`currentWave+1`，未开战为 0 或 1 需约定：推荐开战前 `第0/Y` 或隐藏波次仅关卡名；开战用 `MoveBar` 的 cur）  
   - **约定**: 与现有 `MoveBar(cur, max)` 一致，显示 `第{cur}/{max}波`  

### 备选 B
单独 `SurvivalSaveService` — 过重，否决。

### 波次进度附带修复
`RunRoundEnter` 在读档时不要无条件 `currentWave = -1`：若 `IsLoadFromSave` 且存档 `currentWave >= 0`，保留存档波次后再决定是否重建池。

## 3. 影响面（架构视角）

| 文件 | 改动 |
|------|------|
| `PreParePlugun_ShowPlantShop.cs` | ClearLockedHand；无档不 Prefill |
| `LevelManage.cs` | Restart 清缓存 / 清 SaveLoadContext |
| `LevelController_Endless.cs` | 读档波次；可选轮间补存钩子 |
| `SaveSystem.cs` | 可选 `SaveSurvivalAllowPaused` |
| `ProgressBar.cs` | 生存标题格式 + 进波刷新 |

## 4. 风险

| 风险 | 概率 | 影响 | 措施 |
|------|------|------|------|
| 放宽存档门闸 | 中 | 高 | 仅 Survival + 显式 API |
| 读档双次 EnterMap | 低 | 中 | LoadFlowExecuted 已有，回归验证 |
| 标题刷新时机 | 低 | 低 | Show + MoveBar 双点刷新 |

**风险是否可接受**: 推荐方案 A，风险可控。
