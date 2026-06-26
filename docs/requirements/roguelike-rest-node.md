# 功能需求卡片：肉鸽休息节点（Roguelike Rest）

## 基本信息

- **功能名称**：Roguelike Rest — 地图休息房（可扩展选项）
- **所属模块**：Roguelike / UI / LevelSystem
- **需求类型**：功能扩展
- **优先级**：P0（地图节点真实性 + 竖切 Act1）
- **预估复杂度**：L2
- **提出日期**：2026-06-11

## 功能描述

### 详细描述

实现 `MapRoomType.Rest` 的非战斗流程，对齐杀戮尖塔「休息房」交互：**进入面板 → 从若干选项中选一项 → 离开并标记节点通关**。

与尖塔的差异与分期策略（用户确认 2026-06-11）：

| 尖塔 | 本游戏 v2 | 说明 |
|------|-----------|------|
| 休息（回血） | **休息（小推车 +2）** | 无 Run 血量；Run 态 `runLawnMowerCount` |
| 锻造（升级卡） | **扩容（携带格 +1）** | Run 态 `runLoadoutSlotCount`，7~15 |
| 孵化卡牌等 | **扩展选项** | `IRoguelikeRestOptionProvider` 注入 |

一期状态（2026-05-20 更新）：

1. ✅ 休息房 UI + 流程 + 可扩展选项 API
2. 🔲 **两个常驻选项**（v2 定稿）：
   - **休息**：小推车 +2（Run 态，开局 6）
   - **扩容**：携带格 +1（Run 态，7~15）
3. 事件等仍通过 `IRoguelikeRestOptionProvider` 注入第三选项+

### 用户故事

作为玩家，我希望点击地图「休」节点后进入休息房并选择一项收益，以便在不进战斗的情况下获得 Run 内资源，且未来事件能追加特殊选项。

## 现有业务上下文

### 相关现有功能

| 功能 | 模块 | 关系 | 说明 |
|------|------|------|------|
| `RoguelikeShopFlow` / `RoguelikeShopPanel` | Roguelike | 相似（高） | 非战斗房间范式 |
| `RoguelikeRunService.EnterShopNode` | Roguelike | 扩展模板 | Rest 对称实现 |
| `RoguelikeMapPanel.OnNodeClicked` | UI | 修改 | Rest 分支进休息房 |
| `RoguelikeEconomyConfig` | Roguelike | 扩展 | 休整数值规则 |
| `RoguelikeRunState` / Save | SaveSystem | 扩展 | 节点已用、休整效果状态 |

### 需求类型判定理由

地图已生成 Rest 节点但点击进战斗，属于 **功能扩展 + 体验断层修复**；在现有 Run 流程上增加非战斗房间，不新建独立 Run 子系统。

### 集成点

- **调用**：`RoguelikeRunService`、`RoguelikeRunPlantPool`（扩展选项可能用）、`RoguelikeEconomyConfig`
- **触发**：`OnNodeEntered` / `OnNodeResolved`（与商店一致）
- **新接口**：
  - `RoguelikeRestFlow` — 构建选项、执行选择、离开
  - `IRoguelikeRestOptionProvider`（或等价注册器）— 事件注入额外选项
  - `RoguelikeRunService.EnterRestNode` / `LeaveRestNode`
  - `TryResumePendingNonCombatNode` 增加 Rest 分支

### 对现有功能的影响

- **行为变更**：Rest 节点不再 `EnterCombatNode`
- **数据变更**：`RoguelikeRunState` 增加休息节点使用记录（每节点仅可选一次）；可选增加 `nextCombat*` 字段（若休整效果走战斗增益时）
- **接口变更**：无破坏性变更；EconomyConfig 增加 `restRule`

## 休整（v2 — 2026-05-20 策划定稿）

休息房提供 **两个常驻选项，每节点仅能选其一**（与尖塔一致）。骨架（`RoguelikeRestFlow` / `restUsedNodeIds`）已实现，效果待接。

| 选项 | Run 字段 | 效果 | 默认 / 上限 |
|------|----------|------|-------------|
| **休息（小推车）** | `runLawnMowerCount` | 本场 Run 拥有小推车 **+2** | 开局 **6** |
| **扩容（携带格）** | `runLoadoutSlotCount` | 本关 loadout 上限 **+1** | Clamp **7~15**（开局默认 **10**，待确认） |

### 小推车生成（肉鸽专用）

`EnterWarPlugin_CarCreate` 在肉鸽关卡 **不读** `DifficultyManager.GetCarCount()`（普通难度 `-1` = 每行一辆，会覆盖 Run 数值）。

建议生成数量（**方案 A，已定**）：

```csharp
spawnCount = Min(mapRows, runLawnMowerCount);
// RoguelikeRunState.ComputeLawnMowerSpawnCount
```

### 小推车战后损耗（2026-05-20）

**仅过关**计入损失（绑定在奖励面板出现前，非领完奖励离场时）：

```csharp
// LevelOutCome_Roguelike.HandleOutcome(win=true)
RoguelikeRunService.SettleCombatLawnMowerLosses();
// lost = SpawnedThisCombat - CountSurvivors()
// runLawnMowerCount -= lost
```

- 未上场的小推车（owned &gt; spawn）**不扣**
- 失败结束 Run：**不结算**
- 失败重开本关：重新进战会重新 spawn，不在失败离场扣数

### 携带格（与 RG-008 合并）

- `RoguelikeRunState.runLoadoutSlotCount` 序列化进 Run 存档；旧档缺字段时默认 10。
- 进战斗前 `PreParePlugun_ShowPlantShop` / `PlantsShop`：`maxCount = state.runLoadoutSlotCount`（仅肉鸽）。
- 若已选满格再扩容：下一战选卡 UI 多出一个空槽，**不自动**塞牌。

**离开**：可直接点「离开」通关节点，无需选择任一选项。

## 可扩展选项 API（一期必做骨架）

```text
RoguelikeRestFlow.BuildOptions(nodeId)
  → List<RoguelikeRestOption>
      1. 内置「休整」（若本节点未使用）
      2. [二期] 第二常驻位（一期不展示或占位）
      3. 各 IRoguelikeRestOptionProvider 返回的动态选项（事件等）

RoguelikeRestOption:
  - id（唯一）
  - title / description（UI）
  - enabled（是否可点）
  - Execute(state, nodeId) → bool
```

事件模块后续实现 `IRoguelikeRestOptionProvider`，在休息房打开时注入「孵化卡牌」等，**一期只提供接口 + 空注册列表**。

**约束**：

- 每个休息节点 **仅能成功执行一个选项**（与尖塔一致）；选后其余选项禁用，离开前不可再选
- 节点已 cleared 或已选过则不再打开休息房

## 流程

```text
地图点击 Rest
  → RoguelikeRunService.EnterRestNode()
  → RoguelikeRestFlow.BuildOptions(PendingNodeId)
  → RoguelikeRestPanel 展示选项按钮
  → 玩家点一项 → TryChooseOption(optionId)
  → SaveRun → 自动或点「离开」→ LeaveRestNode()
  → MarkCleared → 回地图

读档：Pending 未 cleared 的 Rest 节点 → TryResumePendingNonCombatNode 恢复面板
```

## 预制体（手动）

1. `Resources/UIPrefab/RoguelikeRestPanel.prefab`（可参考商店面板布局）
2. 根物体 name = `RoguelikeRestPanel`，挂 `RoguelikeRestPanel : View`
3. 字段建议：
   - `optionButtonRoot` + `optionButtonPrefab`（或固定 2+ 动态槽）
   - `secondPermanentSlot` — 二期锻造位，一期 `SetActive(false)`
   - `leaveButton` / `returnButton`
   - `titleText`、`hintText`
4. 选项按钮：标题 + 描述 TMP；disabled 态灰显但不必用 Button.interactable 变黑（参考 Shop SoldOut 经验）

## 技术要求

- **依赖模块**：RoguelikeRunService、UIManage、RoguelikeEconomyConfig、RoguelikeRunSaveSystem
- **性能要求**：与 ShopPanel 同级，打开时一次性构建选项列表
- **兼容性**：旧存档无 rest 相关字段时默认为空 / 0

## 功能清单

### 核心功能（P0）

- [x] `RoguelikeRestOption` / 选项 execute 回调
- [x] `IRoguelikeRestOptionProvider` + `RoguelikeRestOptionRegistry`
- [x] `RoguelikeRestFlow`：`BuildOptions`、`TryChooseOption`
- [x] `RoguelikeRunState.restUsedNodeIds`
- [x] `EnterRestNode` / `LeaveRestNode` / `TryResumePendingNonCombatNode`（Rest）
- [x] `RoguelikeMapPanel` Rest 分支
- [x] `RoguelikeRestPanel` UI
- [x] `RoguelikeRunState.runLawnMowerCount`（默认 6）+ 休息选项 +2
- [x] `EnterWarPlugin_CarCreate` 肉鸽分支（读 Run 小推车， bypass DifficultyManager）
- [x] `RoguelikeEconomyConfig` Rest：`initialLawnMowerCount` / `restLawnMowerBonus`
- [x] 过关奖励前：`SettleCombatLawnMowerLosses`（战中损失扣 Run 池）
- [x] `RoguelikeRunState.runLoadoutSlotCount`（7~15，默认 10）+ 扩容选项 +1（数值层；选卡顶栏 UI 布局后续）
- [ ] 休息面板第二常驻按钮：「扩容」文案与 disabled（达 15 时）

### 二期（本卡片范围外，仅预留）

- [ ] 第三选项位「锻造」类玩法
- [ ] 事件 Provider 实现「孵化卡牌」等

## 验收标准

- [x] 地图 Rest 节点点击不进战斗，打开休息面板
- [ ] 选「休息」：`runLawnMowerCount += 2`，下一战小推车 spawn = Min(行数, owned)
- [ ] 战中毁 N 台小推车 → 过关奖励出现时 Run 池 **-N**（未上场的不扣）
- [ ] 选「扩容」：`runLoadoutSlotCount += 1`（≤15），下一战 PlantsShop 多一槽（下一期）
- [x] 选「扩容」：`runLoadoutSlotCount += 1`（≤15），下一战 `PlantsShop.maxCount` 生效（顶栏格位 UI 后续）
- [x] HUD 显示携带格 `10/15`
- [ ] 达上限时对应按钮 disabled + 说明文案
- [ ] 选一个选项后不可再选第二项；离开后节点 cleared
- [ ] 继续冒险：停在未完成的 Rest 节点可恢复面板
- [ ] 商店 / 战斗 / Boss 通关流程回归正常

## 风险评估

| 风险点 | 概率 | 影响 | 应对措施 |
|--------|------|------|----------|
| 休整仅加金币体感偏弱 | 中 | 低 | EconomyConfig 调参；二期加战斗向休整 |
| 扩展 API 过早抽象 | 低 | 中 | 一期仅 id + Execute 回调，不做复杂 DSL |
| 与事件系统耦合 | 低 | 中 | Provider 接口隔离，事件二期实现 |

## 关联 Context

- `context/index.md`
- `context/modules/LevelSystem.md`
- `context/modules/UI.md`
- `docs/roguelike-progress.md`

## 关联需求

- `docs/requirements/roguelike-economy-config.md`
- 商店实现：`RoguelikeShopFlow` / `RoguelikeShopPanel`
