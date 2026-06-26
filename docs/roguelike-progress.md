# 肉鸽模式制作进度

> 最后更新：2026-05-20（休息房方案 v2、RG-P04/05/06 状态同步）

整体判断：**核心 Run 闭环已可跑通**（开局 → 地图选路 → 战斗 → 搜刮奖励 → 回地图 → 商店 → 多 Act → 存档续玩），约占可玩原型的 **~75%**。

---

## 已完成

| 模块 | 说明 |
|------|------|
| Run 核心 | `RoguelikeRunService`：新开/继续、选路、进战斗、Boss 通关换 Act、全 Run 通关、放弃 Run |
| 地图生成 | `MapGenerator` + `ActMapConfig`：各房间类型配额与连线 |
| 地图 UI | `RoguelikeMapPanel`：横向 Scroll、节点状态、连线（Overlay 下 UI Image 回退）、TestMode |
| 开局乐队 | `RoguelikeBandSelectPanel` + `BandMes` / `RoguelikeBandCatalog` |
| Run 存档 | `RoguelikeRunSaveSystem` 单槽继续；地图、`runGold`、商店货单可恢复 |
| Meta 进度 | `RoguelikeMetaProgress` → `PlayerSaveData` |
| 局内 HUD | `RoguelikeRunInfoPanel`：乐队、Act、节点、植物列表、`runGold` |
| 战斗结算 | `LevelOutCome_Roguelike`：胜利搜刮、失败结束 Run |
| 战斗奖励 | `RoguelikeRewardPanel`：金币领取/跳过；PlantPick 三选一子面板 |
| 经济配置 | `RoguelikeEconomyConfig`：金币、PlantPick、商店规则 |
| 地图商店 | `RoguelikeShopFlow` + `RoguelikeShopPanel` |
| 植物池 | `RoguelikeRunPlantPool` |
| 关卡池 | `RoguelikeLevelPicker` + `Resources/LevelData/RogueMode/` |
| 地图连线 | `RoguelikeMapPanel` UI Image 折线（**不采用** BloodLine / LineRenderer） |
| 乐队 BGM | `BandMes.bgm` + `AudioPlayer` Clip 已配齐 |
| 查看当前卡牌 | `RoguelikeRunInfoPanel` 卡组弹层（地图 HUD 内打开） |

---

## 未完成

### UI / 体验（新增）

- [ ] **首次进地图卷动动画** — 首次进入 `RoguelikeMapPanel` 时，地图从 Boss 端平滑滚动到起点（当前 `ScrollToFocus` 直接定位到当前节点，无 Boss→起点 intro 动画）
- [x] **选人界面 leave 离场** — `RoguelikeBandSelectPanel` 开始挑战播放 `leave` + 观众 `欢呼`，末帧进地图（2026-06-11）
- [ ] **地图节点按钮视觉** — 已通关节点显示 ✔；不可达/未解锁节点变暗；所有房间类型图标保持亮色底图（见 RG-P07）
- [x] **查看当前卡牌 UI** — `RoguelikeRunInfoPanel` 卡组弹层（2026-05-20 验收）
- [ ] **肉鸽通用面板 UI** — Rogue 场景常驻通用条/面板，包含：
  - ~~查看当前卡牌按钮~~（卡组弹层已有，并入 HUD 规范）
  - 当前道具展示
  - 当前层数
  - 当前金币数
- [ ] **肉鸽战斗「提前进下一波」** — 局内提供按钮，允许玩家在波次条件满足前手动进入下一波（见 RG-009）

### 非战斗房间

- [x] **休息节点小推车** — Run 态 `runLawnMowerCount` + 休息 +2 + 肉鸽进战 spawn（2026-05-20）
- [x] **休息节点扩容格** — `runLoadoutSlotCount` +1（RG-008，2026-05-20，数值层）
- [ ] **事件节点** — 叙事/选项事件 UI，或明确仅作特殊战斗并配足关卡池

### 构筑与槽位

- [x] **本关可携带格子增减** — Run 内 `runLoadoutSlotCount`（7~15），休息房 +1；`PlantsShop.maxCount` 读 Run 态（RG-008，顶栏 UI 后续）

### 结算与 Meta

- [ ] **Run 通关结算页** — 当前仅地图标题「通关！」
- [ ] **战斗失败结算页**
- [ ] **遗物系统** — Config / State / UI
- [ ] **道具奖励** — `RoguelikeRewardEntryKind.Item` 仅占位
- [ ] **通用道具制作** — 设计并实现可复用的 Run 道具（非植物），与道具奖励/道具栏闭环（见 RG-013）
- [ ] **Meta 解锁 UI** — 最高层/Act 等展示与解锁
- [ ] **战斗场景 HUD** — 局内信息面板主要在地图场景
- [ ] **胜利金币飘字** 等奖励反馈 UI

### 内容配置（策划 / 数据）

- [ ] **植物实现状态审计** — 盘点项目中已入库植物：已实现 / 占位 / 有 Bug，输出清单（见 RG-A01）
- [ ] **植物与僵尸配置补全** — 属性、简介、展示文案等 SO / 图鉴字段（见 RG-C06）
- [ ] **羁绊配置补全** — 各羁绊简介与效果数值、触发条件（见 RG-C07；参考 `band-instrument-bonds-reference.md`）

### 配置与工程

- [ ] **需求文档勾选同步** — `docs/requirements/roguelike-*.md` 与实现状态对齐
- [ ] **Editor 校验** — 如 `roguelikeKind == None` 告警

---

## 部分完成

| 模块 | 现状 | 缺口 |
|------|------|------|
| 休息节点 | UI + 选项 API 已有 | **效果未接**：小推车 +2 / 格 +1（RG-001） |
| 事件节点 | 可走 `eventLevelPool` 进战斗 | 无事件选项 UI |
| 局内信息 | `RoguelikeRunInfoPanel` 含卡组弹层 | 未形成全模式 HUD 规范（道具栏、战斗场景复用，见 RG-007） |
| 关卡内容 | 前院、Ring、学校等已有资产 | 剧院等文件夹可能仍偏空 |
| `RunMapConfig` | 挂于 `StartUI.roguelikeRunConfig` | 需在场景中确认 Act/关卡池配全 |

---

## 关联文档

- **已知问题**：[roguelike-known-issues.md](./roguelike-known-issues.md) ⭐
- `docs/requirements/roguelike-band-select.md`
- `docs/requirements/roguelike-map-horizontal-layout.md`
- `docs/requirements/roguelike-run-info-panel.md`
- `docs/requirements/roguelike-run-save.md`
- `docs/requirements/roguelike-reward-panel.md`
- `docs/requirements/roguelike-economy-config.md`
- `docs/requirements/roguelike-plant-pick-reward.md`
- `docs/game-design/plant-deckbuilding-design.md` — 植物构筑设计指南（策划 SSOT）
- `docs/requirements/roguelike-rest-node.md`
