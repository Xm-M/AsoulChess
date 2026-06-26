# 功能需求卡片：肉鸽暂停面板 — 返回主菜单（存档并关闭地图）

## 基本信息

- **功能名称**：肉鸽模式下暂停「返回主菜单」保存 Run 并关闭地图
- **所属模块**：Roguelike / UI（ParsePanel）/ LevelSystem
- **需求类型**：功能扩展
- **优先级**：P0
- **预估复杂度**：L2
- **提出日期**：2026-05-30

## 功能描述

### 详细描述

在**肉鸽 Run 进行中**（地图界面或肉鸽战斗关内），玩家通过暂停面板（`ParsePanel`）点击「返回主菜单」时：

1. **保存**当前肉鸽 Run 进度（`RoguelikeRunService.SaveRun()` → `RoguelikeRuns/active.json`）
2. **关闭**肉鸽地图及相关 HUD（`RoguelikeMapPanel`、`RoguelikeRunInfoPanel` 等）
3. **回到**主菜单 `StartUI`，保留「继续冒险」入口（**不**调用 `AbandonRun`）

与主线关卡「返回主菜单」行为区分：主线仍走 `LevelManage.ReturnMenu()`；肉鸽走专用分支。

### 用户故事

作为肉鸽玩家，我希望在暂停里返回主菜单时自动保存冒险进度并关掉地图界面，以便下次从主菜单「继续冒险」接着玩，而不是丢失 Run 或误删存档。

## 现有业务上下文

### 相关现有功能

| 功能 | 模块 | 关系 |
|------|------|------|
| `ParsePanel.ReturnMenu()` | UI | 当前统一调用 `LevelManage.ReturnMenu()`，不区分肉鸽 |
| `RoguelikeRunService.SaveRun()` | Roguelike | 已有 Run 存档写入 |
| `RoguelikeRunService.ReturnToMapUI()` | Roguelike | 战斗胜利后回地图（相反方向） |
| `RoguelikeRunService.AbandonRun()` | Roguelike | 放弃 Run 并删档，**本需求不得调用** |
| `StartUI.ContinueRoguelikeRun()` | UI | 依赖 `HasContinuableRunSave` |
| `RoguelikeRunInfoPanel.OpenPausePanel()` | Roguelike UI | 地图/HUD 上打开同一 `ParsePanel` |
| `SaveSystem.SaveCurrentLevel()` | SaveSystem | 肉鸽 Run 进行中已跳过关卡中途快照 |

### 需求类型判定理由

在现有 `ParsePanel` + `RoguelikeRunService` 存档体系上**扩展返回主菜单分支**，不新建面板。

### 集成点

- **调用**：`RoguelikeRunService.SaveRun()`、`RoguelikeRunPlantPool.RestoreMainlinePlantsFromPlayerSave()`（若当前在战斗关内曾 `ApplyRunPlantsToGame`）
- **触发**：`ParsePanel.ReturnMenu()` 内检测 `RoguelikeRunService.HasActiveRun`
- **UI**：隐藏 `RoguelikeMapPanel`、`RoguelikeRunInfoPanel`、（若在）`RoguelikeShopPanel` / `RoguelikeRestPanel` / `RoguelikeRewardPanel`
- **场景**：复用 `LevelManage.ReturnMenu()` 或等价加载「开始」场景 + `StartUI.Show()`

### 对现有功能的影响

- **ParsePanel.ReturnMenu**：增加肉鸽分支（主线逻辑不变）
- **战斗中途返回**：不保存关卡内棋子/Buff 快照（与 `roguelike-run-save.md` 一致）；`pendingNodeId` 保持未通关，继续冒险时可再打该节点
- **内存态**：`State` / `ActiveRunConfig` 可在回主菜单后保留或仅依赖磁盘档；继续冒险时以 `TryContinueRun` 为准

## 技术要求

- **Unity 版本**：与项目一致
- **依赖模块**：Roguelike、LevelSystem、SaveSystem、UIManage
- **性能要求**：单次 JSON 写入，无额外开销
- **兼容性**：无 Run 时行为与现有一致

## 功能清单

### 核心功能（P0）

- [ ] `HasActiveRun` 时：`SaveRun()` 后再离开
- [ ] 关闭肉鸽地图与 Run HUD 及可能打开的子面板
- [ ] 恢复主线 `playerOwnedCreators`（`RestoreMainlinePlantsFromPlayerSave`）
- [ ] 回到 `StartUI`，`HasContinuableRoguelikeRun == true`
- [ ] 非肉鸽时：`ReturnMenu` 行为不变

### 扩展功能（P1，可选）

- [ ] 返回前二次确认（「将保存进度并返回主菜单」）
- [ ] 从地图暂停与从战斗暂停的文案区分

## 验收标准

- [ ] 地图界面暂停 → 返回主菜单 → 再点「继续冒险」→ 地图/金币/节点与离开前一致
- [ ] 肉鸽战斗关内暂停 → 返回主菜单 → **继续冒险** → 直接进入该战斗关卡（节点未通关）
- [ ] 返回主菜单后无 `RoguelikeMapPanel` / Run HUD 残留显示
- [ ] 主线关卡暂停返回主菜单行为无回归
- [ ] 未误调用 `AbandonRun`，`active.json` 仍存在

## 风险评估

| 风险点 | 概率 | 影响 | 应对措施 |
|--------|------|------|----------|
| 与 `ReturnToMapUI` 场景切换顺序冲突 | 中 | 中 | 统一封装 `SaveAndExitToMainMenu()` |
| 战斗内返回后植物池未恢复 | 中 | 高 | 离开战斗时必调 `RestoreMainlinePlantsFromPlayerSave` |
| 多面板未 Hide 导致 StartUI 叠层 | 中 | 中 | 集中 `RoguelikeUiFlow.HideAllRunPanels()` |

## 关联 Context

- `context/index.md`
- `docs/requirements/roguelike-run-save.md`
- `Assets/Script/UI/View/ParsePanel.cs`
- `Assets/Script/Roguelike/Map/RoguelikeRunService.cs`
