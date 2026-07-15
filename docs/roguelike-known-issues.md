# 肉鸽模式 — 已知问题与待办记录

> **用途**：集中记录当前已发现、尚未关闭的问题（Bug、功能缺口、配置风险、文档偏差）。  
> **维护**：发现新问题请追加到对应章节；修复后改状态并注明日期/PR。  
> **最后更新**：2026-05-20（Prefab 验收、RG-P03、GP-001/002 closed）
> **关联**：[制作进度](./roguelike-progress.md) · [策划总览](../肉鸽.md) · [需求目录](./requirements/)

---

## 如何使用本文档

| 字段 | 说明 |
|------|------|
| **ID** | `RG-xxx` 肉鸽系统 · `GP-xxx` 影响肉鸽关卡的通用战斗问题 |
| **状态** | `open` 未处理 · `investigating` 分析中 · `blocked` 依赖外部 · `fixed` 已修待验 · `closed` 已关闭 |
| **优先级** | P0 阻塞竖切 / P1 影响体验 / P2 可延后 |
| **类型** | `bug` · `gap` 功能缺口 · `config` 配置/预制体 · `doc` 文档 · `polish` 体验优化 |

**新增条目模板**（复制到对应章节）：

```markdown
### RG-xxx · [简短标题]

- **状态**：open
- **优先级**：P1
- **类型**：bug
- **发现**：2026-xx-xx
- **现象**：
- **复现**：
- **分析/根因**：
- **建议修复**：
- **关联文件**：
```

---

## 一、活跃 Bug（影响可玩性）

_当前无 open 的 P1 战斗 Bug。GP-001 / GP-002 已于 2026-05-20 关闭，见 §七。_

---

## 二、肉鸽功能缺口（非 Bug，但影响完整度）

### RG-001 · 休息房选项无实际收益

- **状态**：closed（2026-05-20）
- **优先级**：P1
- **类型**：gap
- **说明**：休息（小推车 +2）、扩容（携带格 +1）已接入；选项内容改 `RoguelikeEconomyConfig.restOptionCatalog` + 配图。
- **关联**：`RoguelikeRestFlow.cs`、`RoguelikeRestOptionCatalog`、`docs/requirements/roguelike-rest-option-cards.md`

---

### RG-002 · 事件节点无独立事件 UI

- **状态**：open
- **优先级**：P1
- **类型**：gap
- **现象**：地图上有 Event 节点，但进入后走 `eventLevelPool` 进战斗，无叙事/选项分支。
- **建议**：二选一并在策划文档写明——(A) 做 `RoguelikeEventPanel` + EventData；(B) 明确 Event = 特殊战斗并配足关卡池。
- **关联**：`RoguelikeRunService.ResolveLevelForPendingCombat`、`ActMapConfig.eventLevelPool`

---

### RG-003 · 道具奖励与 Run 道具栏未闭环

- **状态**：open
- **优先级**：P1
- **类型**：gap
- **现象**：`RoguelikeRewardEntryKind.Item` 与 `RoguelikeRunPropPool` 已有代码，但经济配置道具池可能为空；奖励流对 Item 类型提示「走专用流程」。
- **建议**：配 `RoguelikeEconomyConfig` 道具候选池；在 `RoguelikeRewardPanel` 接 Item 领取 UI；战斗前 `ApplyRunPropsToGame` 回归测试。
- **关联**：`RoguelikeEconomyConfig.cs`、`RoguelikeRewardFlow.cs`、`RoguelikeRunPropPool.cs`

---

### RG-004 · 遗物系统未实现

- **状态**：open
- **优先级**：P2
- **类型**：gap
- **现象**：无 Relic Config / State / UI；构筑深度依赖 PlantPick + 商店。
- **关联**：`docs/roguelike-progress.md` 未完成项

---

### RG-005 · 战斗场景 Run 上下文展示

- **状态**：fixed（2026-05-20，待 Play 验收）
- **优先级**：P1
- **类型**：gap
- **方案**：不整板显示 `RoguelikeRunInfoPanel`；扩展 `ProgressBar.stadgeName` 为肉鸽关 **`Act · Ln · 关卡名`**。
- **剩余**：战斗内 Run 金币/卡组快捷入口仍无（非本期范围）。
- **关联**：`ProgressBar.cs`、`RoguelikeRunInfoFormatter.FormatProgressBarStageName`

---

### RG-006 · Meta 进度无可视化 UI

- **状态**：open
- **优先级**：P2
- **类型**：gap
- **现象**：`RoguelikeMetaProgress` 写入 `PlayerSaveData`，主菜单/图鉴侧无「最高 Act/层数」展示与解锁反馈。
- **关联**：`RoguelikeMetaProgress.cs`、`StartUI.cs`

---

### RG-007 · 「肉鸽通用面板」规范

- **状态**：partial（2026-05-20）
- **优先级**：P1
- **类型**：gap
- **已完成**：Act、Run 金币、小推车、携带格、**地图层 Ln**（Prefab 已绑）、卡组、设置。
- **未完成**：Run **道具栏**。
- **关联**：`docs/requirements/roguelike-hud-map-layer.md`

---

### RG-008 · 本关可携带格子增减

- **状态**：closed（2026-05-20）
- **优先级**：P1
- **类型**：gap
- **说明**：`runLoadoutSlotCount`（7~15）、休息 +1、`PlantsShop.maxCount` + 顶栏中间格 `SetActive` 已落地。
- **关联**：`PlantsShop.cs`、`docs/requirements/roguelike-loadout-slot-ui.md`

---

### RG-009 · 肉鸽模式「提前进入下一波」按钮

- **状态**：done（2026-05-20）
- **优先级**：P1
- **类型**：gap → 已实现
- **实现**：`LevelController` 手动进波 API + `ProgressBar.EarlyNextWaveButton`；仅肉鸽普关；mintime 后显隐；保留自动进波
- **关联**：`LevelController.cs`、`ProgressBar.cs`、`docs/requirements/roguelike-early-next-wave.md`

---

### RG-013 · 通用道具未制作（与道具奖励闭环）

- **状态**：open
- **优先级**：P1
- **类型**：gap
- **发现**：2026-05-20
- **现象**：`RoguelikeRunPropPool` / `RoguelikeRewardEntryKind.Item` 有代码骨架，但缺少 **可复用的通用 Run 道具** 设计与 Prefab/SO 实现（非植物、非遗物）。
- **期望**：
  - 定义通用道具数据结构与效果接口（一次性 / 持久 / 战斗前触发等）。
  - 实现若干 MVP 道具并在搜刮/商店/休息等节点可获取。
  - 与 RG-003 道具奖励领取 UI、`ApplyRunPropsToGame` 联调。
- **关联**：`RoguelikeRunPropPool.cs`、`RoguelikeEconomyConfig.cs`、RG-003

---

## 三、体验 / Polish 待办

| ID | 问题 | 优先级 | 状态 |
|----|------|--------|------|
| RG-P01 | 乐队选择「开始挑战」leave 离场 + 观众欢呼 | P1 | fixed（2026-06-11） |
| RG-P02 | 首次进地图 Boss→起点 intro 滚动 | P1 | fixed（2026-05-20，`mapIntroEnabled`） |
| RG-P03 | 胜利金币飞散 / 飞 Run 金币条 | P2 | closed（2026-05-20 验收） |
| ~~RG-P04~~ | ~~地图连线 BloodLine~~ | — | **cancelled** |
| ~~RG-P05~~ | ~~乐队 BGM~~ | — | **closed** |
| ~~RG-P06~~ | ~~独立「查看当前卡牌」页~~ | — | **closed** |
| RG-P07 | 地图节点视觉 | P1 | closed（2026-05-20；✔ Sprite 已绑） |

**RG-P07 备注**：`clearedCheckmarkSprite` 已配置于 MapPanel → Node State Style。

---

## 四、配置与工程风险

### RG-C01 · Test 模式不写 Run 存档

- **状态**：open（设计如此，易误用）
- **优先级**：P2
- **类型**：config
- **现象**：`GameMode.Test` 时 `RoguelikeRunSaveSystem.SkipSaveLoad == true`，`active.json` 不写入；暂停返回主菜单会打 Warning。
- **影响**：编辑器 Test 模式下「继续冒险」、存档相关验收无效。
- **建议**：Test 跑存档流程时切非 Test 模式，或文档/Inspector 明确提示。
- **关联**：`RoguelikeRunSaveSystem.cs`、`RoguelikeRunService.SaveAndExitToMainMenu`

---

### RG-C02 · RunMapConfig / Act 关卡池可能未配全

- **状态**：open
- **优先级**：P0（竖切前）
- **类型**：config
- **现象**：点击节点时 Console 可能出现 `[RoguelikeMapPanel] 无法解析关卡` / `[Roguelike] 无法解析战斗关卡`。
- **检查项**：
  - `StartUI.roguelikeRunConfig` 是否绑定有效 `RunMapConfig`
  - 各 `ActMapConfig` 的 `normalLevelPool` / `eliteLevelPool` / `bossLevel`
  - `Resources/LevelData/RogueMode/` 下关卡 `LevelData.roguelikeKind` 是否正确
- **资源现状**：前院 / Ring / 学校有关卡；**剧院** 目录仅有 `.meta`，无 `.asset` 关卡。

---

### RG-C03 · 预制体缺失时运行时 Error

- **状态**：open
- **优先级**：P1
- **类型**：config
- **现象**：以下面板若未放在 `Resources/UIPrefab/` 且根物体名匹配，会 `Debug.LogError` 并降级：
  - `RoguelikeBandSelectPanel`
  - `RoguelikeMapPanel`
  - `RoguelikeRunInfoPanel`
  - `RoguelikeRewardPanel`
  - `RoguelikeShopPanel`
  - `RoguelikeRestPanel`
- **建议**：竖切前做 Prefab 清单核对（见各 `docs/requirements/roguelike-*.md` 预制体章节）。

---

### RG-C04 · 继续冒险依赖 runConfigName 解析

- **状态**：open
- **优先级**：P1
- **类型**：config
- **现象**：存档内 `runConfigName` 与当前 `StartUI.roguelikeRunConfig.name` 不一致时，`TryContinueRun` 失败。
- **建议**：Run 配置改名时做迁移或固定 Resources 路径解析（`RoguelikeRunSaveSystem.ResolveRunConfig`）。

---

### RG-C05 · Editor 缺少 roguelikeKind 校验

- **状态**：open
- **优先级**：P2
- **类型**：config
- **现象**：关卡池混入 `roguelikeKind == None` 的 LevelData 时，运行时才发现 Pick 异常。
- **建议**：ActMapConfig / RunMapConfig 的 Editor `OnValidate` 告警。

---

### RG-C06 · 植物与僵尸属性 / 简介等配置未补全

- **状态**：open
- **优先级**：P1
- **类型**：config
- **发现**：2026-05-20
- **现象**：项目中已有大量 Chess Prefab / SO，但图鉴、商店、奖励展示所需的 **属性、简介、定位文案** 等字段未系统填写或 SSOT 分散。
- **期望**：
  - 植物：价格、占格、肉鸽档位、简介、标签等（对齐 `plant-deckbuilding-design.md` 字段表）。
  - 僵尸：血量/速度等展示用摘要、简介、威胁描述。
  - 统一配置入口或 Editor 校验缺失项。
- **关联**：Chess Property SO、`PlantsShop` / 图鉴 UI、`docs/game-design/plant-deckbuilding-design.md`

---

### RG-C07 · 羁绊简介与效果配置未补全

- **状态**：open
- **优先级**：P1
- **类型**：config
- **发现**：2026-05-20
- **现象**：乐队 / 乐器羁绊有设计文档与 `Fetter` / `plantTags` 机制，但 **各羁绊的简介文案与效果数值** 未作为可交付配置落盘。
- **期望**：
  - 每条羁绊：名称、简介、2/3/5 档（或策划定稿档数）效果描述与数值。
  - 冒险 vs 肉鸽强度分化（见 `plant-band-design-evolution.md`）。
  - 与 Run 内 aura 触发条件（loadout 含 N 人）对齐。
- **关联**：`Fetter`、`plantTags`、`docs/game-design/band-instrument-bonds-reference.md`

---

## 五、内容审计（跨模式）

### RG-A01 · 植物实现状态清单未建立

- **状态**：open
- **优先级**：P1
- **类型**：gap
- **发现**：2026-05-20
- **现象**：项目内已有大量植物 Prefab / 技能脚本，但缺少 **哪些已实现、哪些占位、哪些有已知 Bug** 的集中清单，影响 PlantPick 池配置与 QA。
- **期望**：
  - 扫描 `Assets/Prefab/ChessPrefab/`、`Assets/Script/Skill/` 等，输出表格：名称 / 实现状态 / 已知问题 / 是否进肉鸽池。
  - 与 GP-xxx 战斗 Bug、RG-C06 配置缺口交叉引用。
  - 可沉淀为 `docs/game-design/plant-implementation-status.md` 或表格资产。
- **关联**：`RoguelikeRunPlantPool`、`RoguelikeEconomyConfig` PlantPick 池、RG-C06

---

## 六、文档偏差（需同步）

| ID | 问题 | 正确状态 | 关联 |
|----|------|----------|------|
| DOC-01 | `肉鸽.md` §9「待实现」仍写 MapGenerator / MapView / RunState 未做 | 已实现 | 应更新 §9 或标注「已归档至 progress」 |
| DOC-02 | `docs/roguelike-progress.md` 写 Run 通关/失败结算页未完成 | `RoguelikeRewardPanel.ShowRunEnd` + `roguelike-run-end-in-reward-panel.md` 已实现 | 更新 progress 勾选 |
| DOC-03 | `docs/requirements/roguelike-pause-return-menu-save.md` 功能清单仍为 `[ ]` | 代码已实现（2026-05-30） | 勾选验收项 |
| DOC-04 | `docs/requirements/roguelike-band-select.md` 验收标准未勾选 | 功能已实现，BGM/动画 polish 见 RG-P01/P05 | 部分勾选 + 注明 polish |
| DOC-05 | `docs/game-design/plant-deckbuilding-design.md` §9 数值表 TODO | 策划未填 | 影响 PlantPick 分档平衡 |

---

## 七、近期已修复（供回归参考）

| ID | 问题 | 修复摘要 | 日期 |
|----|------|----------|------|
| RG-F01 | 肉鸽暂停「返回主菜单」不存档、不关地图 | `SaveAndExitToMainMenu` + `ParsePanel` 分支 + `TryResumePendingCombatNode` | 2026-05-30 |
| RG-F02 | Run 失败/全通无结算页 | `RoguelikeRunEndSummary` + `RoguelikeRewardPanel.ShowRunEnd` | 2026-06-11 |
| RG-F04 | 休息选项 Catalog 未绑 EconomyConfig | `restOptionCatalog` 指向 `RoguelikeRestOptionCatalog` | 2026-05-20 |
| RG-F05 | 地图通关 ✔ 需选下一格才显示 | Cleared 优先于 Current | 2026-05-20 |
| GP-F01 | GP-001 霸凌者几乎只放一次技能 | Skill CD / 动画 / FindTarget | 2026-05-20 |
| GP-F02 | GP-002 撑杆跳进场往后退 | 敌方朝向在 EnterWar 前设置 | 2026-05-20 |
| RG-F03 | 乐队切换无动画 / BGM NRE | BandSelect + AudioSource | 2026-05 |

## 八、建议处理顺序（竖切 Act1）

1. **P0 配置**：RG-C02 配全 Act1 关卡池
2. **P1 玩法**：RG-002 事件节点策略
3. **P1 道具**：RG-013 + RG-003（RG-007 道具栏）
4. **P1 内容**：RG-A01 植物审计；RG-C06 / RG-C07
5. **文档**：DOC-01～03 与 progress 对齐

~~已完成~~：RG-P03 金币飞散；RG-P07 ✔ / HUD 层数 Prefab；GP-001 / GP-002

---

## 变更日志

| 日期 | 变更 |
|------|------|
| 2026-05-20 | RG-P03 closed；GP-001/002 closed；RG-P07/RG-007 Prefab 验收（✔ Sprite、层数 icon） |
| 2026-05-20 | RG-001/008 closed；RG-005 fixed；RG-007 partial；RG-P02/P07 fixed |
| 2026-05-20 | 新增 RG-008/009/013、RG-P07、RG-A01、RG-C06/C07 |
| 2026-05-30 | 初版 |
