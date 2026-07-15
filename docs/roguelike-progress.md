# 肉鸽模式制作进度

> 最后更新：2026-05-20（Prefab 验收、RG-P03、GP-001/002 关闭）

整体判断：**核心 Run 闭环已可跑通**，UI/体验与常见战斗 Bug 已补一批，约占可玩原型的 **~82%**。

---

## 已完成

| 模块 | 说明 |
|------|------|
| Run 核心 | `RoguelikeRunService`：新开/继续、选路、进战斗、Boss 通关换 Act、全 Run 通关、放弃 Run |
| 地图生成 | `MapGenerator` + `ActMapConfig`：各房间类型配额与连线 |
| 地图 UI | `RoguelikeMapPanel`：横向 Scroll、节点状态、连线、Intro 卷动、节点 Prefab |
| 地图节点视觉 | RG-P07：overlay + ✔（MapPanel Sprite 已绑）；通关即打勾 |
| 胜利金币反馈 | RG-P03：`Item_Coin` 外散 → 飞 Run 金币条（已验收） |
| 开局乐队 | `RoguelikeBandSelectPanel` + `BandMes` / `RoguelikeBandCatalog` |
| Run 存档 | `RoguelikeRunSaveSystem` 单槽继续；地图、`runGold`、商店货单可恢复 |
| Meta 进度 | `RoguelikeMetaProgress` → `PlayerSaveData` |
| 局内 HUD | `RoguelikeRunInfoPanel`：Act、金币、小推车、携带格、地图层 Ln（Prefab 已绑）、卡组、设置 |
| 战斗进度条标题 | RG-005：`ProgressBar` 肉鸽关显示 `Act · Ln · 关卡名` |
| 战斗结算 | `LevelOutCome_Roguelike`：胜利搜刮、失败结束 Run |
| 战斗奖励 | `RoguelikeRewardPanel`：金币领取/跳过、硬币飞散（RG-P03）；PlantPick 三选一 |
| 经济配置 | `RoguelikeEconomyConfig`：金币、PlantPick、商店、**休息房选项 Catalog** |
| 休息房 | 选项卡 SO 配置 + `RoguelikeRestPanel`；小推车 +2 / 携带格 +1 |
| 地图商店 | `RoguelikeShopFlow` + `RoguelikeShopPanel` |
| 植物池 | `RoguelikeRunPlantPool` |
| 关卡池 | `RoguelikeLevelPicker` + `Resources/LevelData/RogueMode/` |
| 携带格 | RG-008：Run `runLoadoutSlotCount`（7~15）+ `PlantsShop` 顶栏中间格显隐 |
| 地图连线 | UI Image 折线（不采用 BloodLine） |
| 乐队 BGM | `BandMes.bgm` + `AudioPlayer` Clip 已配齐 |
| 查看当前卡牌 | `RoguelikeRunInfoPanel` 卡组弹层 |
| 地图 Intro | RG-P02：Boss 端 → 起点卷动（`mapIntroEnabled`） |

---

## 未完成

### UI / 体验

- [ ] **肉鸽通用面板 UI（剩余）** — Run **道具栏**（RG-007 / RG-003）
- [x] **肉鸽战斗「提前进下一波」** — RG-009（ProgressBar「下一波」按钮，mintime 后显示，仅肉鸽普关）

### 通用战斗 Bug（非肉鸽专属，影响肉鸽关卡池）

- [x] **GP-001 霸凌者技能 CD** — 已修复（2026-05-20）
- [x] **GP-002 撑杆跳进场朝向** — 已修复（2026-05-20）

### 非战斗房间

- [ ] **事件节点** — RG-002：叙事/选项 UI，或明确 Event = 特殊战斗

### 结算与 Meta

- [ ] **Meta 解锁 UI** — RG-006
- [ ] **遗物系统** — RG-004
- [ ] **道具奖励闭环** — RG-003 + RG-013

### 内容配置（策划 / 数据）

- [ ] **植物实现状态审计** — RG-A01
- [ ] **植物与僵尸配置补全** — RG-C06
- [ ] **羁绊配置补全** — RG-C07
- [ ] **Act1 关卡池配全** — RG-C02

### 配置与工程

- [ ] **需求文档勾选同步** — 部分已更，见各 `docs/requirements/roguelike-*.md`
- [ ] **Editor 校验** — 如 `roguelikeKind == None` 告警

---

## 部分完成

| 模块 | 现状 | 缺口 |
|------|------|------|
| RG-007 通用 HUD | 地图/商店/休息：金币、Act、层数、卡组、设置（Prefab 已绑） | 道具栏 |
| RG-005 战斗 HUD | ProgressBar 显示 Act·层·关卡名 | 战斗内 Run 金币/卡组入口仍无（若需要再扩展） |
| 事件节点 | 可走 `eventLevelPool` 进战斗 | 无事件选项 UI |
| 关卡内容 | 前院、Ring、学校等已有资产 | 剧院等文件夹可能仍偏空 |

---

## 关联文档

- **已知问题**：[roguelike-known-issues.md](./roguelike-known-issues.md) ⭐
- `docs/requirements/roguelike-map-node-visuals.md`（RG-P07）
- `docs/requirements/roguelike-hud-map-layer.md`（RG-005/007 层数）
- `docs/requirements/roguelike-map-intro-scroll.md`（RG-P02）
- `docs/requirements/roguelike-loadout-slot-ui.md`（RG-008）
- `docs/requirements/roguelike-rest-option-cards.md`
- `docs/requirements/roguelike-run-info-panel.md`
- `docs/game-design/plant-deckbuilding-design.md`
