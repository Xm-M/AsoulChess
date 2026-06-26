# 肉鸽模式 — 已知问题与待办记录

> **用途**：集中记录当前已发现、尚未关闭的问题（Bug、功能缺口、配置风险、文档偏差）。  
> **维护**：发现新问题请追加到对应章节；修复后改状态并注明日期/PR。  
> **最后更新**：2026-05-20  
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

### GP-001 · 霸凌者几乎只放一次技能

- **状态**：open
- **优先级**：P1
- **类型**：bug
- **发现**：2026-05（关卡/技能排查）
- **现象**：霸凌者入场后长时间不再放技能，或长时间卡在技能相关动画状态。
- **分析（待代码验证）**：
  1. `Assets/SO/Skill/僵尸/霸凌者.asset`：`baseCd: 50000`、`startCd: 49990` → 首次约 10s 后放技能，之后 CD 需再攒满 50000，整局可能只放一次。
  2. `jump.anim` / `AnimFinish` 时序偏长；`skill` 结束自动切 `jump`；若 `UseSkill` 动画事件未触发，可能长时间停在 `SkillState`。
  3. `Passive_Zombie_BullyMan.FindTarget` 射线命中无 `Chess` 组件时可能 NRE，导致被动逻辑中断。
- **建议修复**：
  - 调整 Skill SO 的 `baseCd` / `startCd` 至合理值。
  - 核对 `霸凌者.controller` 与 `skill.anim` 动画事件。
  - `FindTarget` 对无 `Chess` 碰撞体做 null 防护。
- **关联文件**：`Passive_Zombie_BullyMan.cs`、`ISkillEffect_Zombie_BullyMan.cs`、`霸凌者.prefab`、`霸凌者.controller`

---

### GP-002 · 撑杆跳僵尸出生后短暂往后退

- **状态**：open
- **优先级**：P1
- **类型**：bug
- **发现**：2026-05（进场/朝向排查）
- **现象**：撑杆跳僵尸出现在场地后，会先向错误方向（往后）移动几格，再恢复正常向左进攻。
- **分析（待代码验证）**：
  1. `EnemyManage.CreateChess` 在 `WhenChessEnterWar()` **之后**才设置 `transform.right = Vector2.left`。
  2. 撑杆跳 `EnterWarState: 1`（MoveState）进场即 `StartMoving()`；`HorMove.FindNextTile` 在朝向未设好时按默认 +X 算 `nextTile`。
  3. `ISkillEffect_Zombie_JumpToTagret` 会提前改 `standTile`，与错误朝向叠加更明显。
- **建议修复**：
  - 在 `WhenChessEnterWar` 之前设置敌方朝向；或设朝向后重算 `nextTile`。
  - 评估 `startCd` 与 `baseCd` 是否导致进场立刻跳技能。
- **关联文件**：`EnemyManage`（CreateChess）、`ISkillEffect_Zombie_JumpToTagret.cs`、撑杆跳 prefab / Skill SO

---

## 二、肉鸽功能缺口（非 Bug，但影响完整度）

### RG-001 · 休息房选项无实际收益（待接 v2 效果）

- **状态**：open（骨架已完成，效果待开发）
- **优先级**：P1
- **类型**：gap
- **现象**：休息房 UI / 流程已有，但两个常驻选项尚未接入 Run 数值。
- **策划定稿（2026-05-20）** — 每节点 **二选一**（扩容格下一期）：
  1. **休息（小推车）**：✅ `runLawnMowerCount += 2`（开局 6；spawn = Min(行数, owned)）
  2. **扩容（携带格）**：🔲 下一期 RG-008
- **建议**：优先做此条，顺带落地 RG-008 槽位系统与小推车肉鸽分支。
- **关联**：`RoguelikeRestFlow.cs`、`RoguelikeRunState.cs`、`EnterWarPlugin_CarCreate`、`PlantsShop.cs`、`docs/requirements/roguelike-rest-node.md` v2

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

### RG-005 · 战斗场景缺少统一肉鸽 HUD

- **状态**：open
- **优先级**：P1
- **类型**：gap
- **现象**：`RoguelikeRunInfoPanel` 主要在地图场景展示；战斗关内金币/层数/卡组入口不统一。
- **建议**：战斗场景复用 HUD 或抽「肉鸽通用条」规范（见 RG-007）。
- **关联**：`RoguelikeRunInfoPanel.cs`

---

### RG-006 · Meta 进度无可视化 UI

- **状态**：open
- **优先级**：P2
- **类型**：gap
- **现象**：`RoguelikeMetaProgress` 写入 `PlayerSaveData`，主菜单/图鉴侧无「最高 Act/层数」展示与解锁反馈。
- **关联**：`RoguelikeMetaProgress.cs`、`StartUI.cs`

---

### RG-007 · 「肉鸽通用面板」规范未落地

- **状态**：open
- **优先级**：P1
- **类型**：gap
- **现象**：缺少统一规范：层数、Run 金币、道具栏、查看卡组入口、设置/暂停入口的布局约定。
- **说明**：`RoguelikeRunInfoPanel` 已覆盖部分字段，但未定义为全模式 SSOT。
- **关联**：`docs/roguelike-progress.md` UI 未完成第 4 条

---

### RG-008 · 本关可携带格子固定为 10，无法增减

- **状态**：open
- **优先级**：P1
- **类型**：gap
- **发现**：2026-05-20
- **现象**：肉鸽战斗前选卡界面 `PlantsShop.maxCount` 默认 10，Run 内无机制增减「本关可携带植物数」。
- **期望**（2026-05-20 与 RG-001 对齐）：
  - Run 字段 `runLoadoutSlotCount`，Clamp **7~15**，开局默认待策划确认（建议 **10**）。
  - 休息房「扩容」+1；其它来源（Prop/奖励）可后续扩展。
  - 战斗前 `PlantsShop.maxCount` 读 Run 态，非肉鸽仍用 Inspector 默认。
- **关联**：`PlantsShop.cs`（`maxCount`）、`PreParePlugun_ShowPlantShop.cs`、`docs/game-design/plant-deckbuilding-design.md` §槽位

---

### RG-009 · 肉鸽模式缺少「提前进入下一波」按钮

- **状态**：open
- **优先级**：P1
- **类型**：gap
- **发现**：2026-05-20
- **现象**：波次推进仅依赖 `LevelController` 自动条件（清空 + mintime / maxtime 等），肉鸽局内无手动「下一波」入口。
- **期望**：肉鸽战斗 HUD 提供按钮，在波次可推进条件满足时允许玩家 **提前** 进入下一波（需与 `WaveCanAdvance` / `DoEnterNextWave` 对齐，避免最后一波误触）。
- **关联**：`LevelController.cs`、`LevelOutCome_Roguelike`、RG-005 战斗 HUD

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
| RG-P02 | 首次进地图无 Boss→起点 intro 滚动 | P1 | open |
| RG-P03 | 胜利金币无飘字/强反馈 | P2 | open（代码已有，待验收） |
| ~~RG-P04~~ | ~~地图连线 BloodLine~~ | — | **cancelled**（2026-05-20：维持 UI Image 折线，不改 Canvas / LineRenderer） |
| ~~RG-P05~~ | ~~乐队 BGM / AudioClip 未配齐~~ | — | **closed**（2026-05-20 资源已齐） |
| ~~RG-P06~~ | ~~独立「查看当前卡牌」页~~ | — | **closed**（2026-05-20：`RoguelikeRunInfoPanel` 卡组弹层） |
| RG-P07 | 地图节点视觉：已通关 ✔、不可达变暗、房间图标保持亮色 | P1 | open |

**RG-P07 细节**：
- **已通关（Cleared）**：叠加 ✔ 标记（当前仅 `clearedTint` 整节点变灰，无勾选图标）。
- **不可达（Locked）**：保持变暗（`lockedColor`），按钮不可点。
- **所有房间类型**：图标/底图应 **保持亮色** 以区分房间类型；状态差异通过 overlay（✔、边框、变暗层）表达，而非整颗节点发灰（当前 `Cleared`/`Visited` 对底图乘 `clearedTint`/`visitedTint`）。
- **关联**：`RoguelikeMapNodeWidget.cs`、`RoguelikeMapVisualSettings.cs`、`RoguelikeMapPanel.prefab`

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
| RG-F03 | 乐队切换无动画 / BGM NRE | `RoguelikeBandSelectPanel` change 动画 + `AudioPlayer.EnsureAudioSource` | 2026-05 |

---

## 八、建议处理顺序（竖切 Act1）

1. **P0 配置**：RG-C02 配全 Act1 关卡池；RG-C03 Prefab 清单
2. **P1 玩法**：**RG-001 休息房效果（小推车 +2 / 格 +1）+ RG-008 槽位**；RG-002 事件节点策略定稿
3. **P1 内容基线**：RG-A01 植物实现审计；RG-C06 / RG-C07 配置补全
4. **P1 Bug**：GP-001 / GP-002（学校/前院关卡常见僵尸）
5. **P1 体验**：RG-P02 地图 intro；RG-P07 节点视觉；RG-009 提前进波；RG-005 战斗 HUD
6. **P1 道具**：RG-013 通用道具 + RG-003 奖励闭环
7. **文档**：DOC-01～03 与 progress 对齐

---

## 变更日志

| 日期 | 变更 |
|------|------|
| 2026-05-20 | RG-P04 cancelled；RG-P05/P06 closed；RG-001 v2（小推车 +2 / 扩容格 +1）；需求卡 rest-node v2 |
| 2026-05-20 | 新增 RG-008/009/013、RG-P07、RG-A01、RG-C06/C07 |
| 2026-05-30 | 初版：汇总对话排查、progress、requirements 与代码 Warning 路径 |
