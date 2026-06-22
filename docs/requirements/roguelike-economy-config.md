# 功能需求卡片：肉鸽 Run 经济配置（RoguelikeEconomyConfig）

## 基本信息

- **功能名称**：RoguelikeEconomyConfig — Run 内经济/奖励规则
- **所属模块**：Roguelike / LevelSystem
- **需求类型**：功能扩展
- **优先级**：P0（首版仅金币）
- **预估复杂度**：L2
- **提出日期**：2026-05-20

## 功能描述

### 详细描述

新增 `RoguelikeEconomyConfig` ScriptableObject，集中配置肉鸽 Run 内奖励规则。**首版仅实现战斗胜利金币**，按 `LevelData.roguelikeKind` 查表发钱；卡牌三选一、遗物、商店物价等后续在同 Config 扩展。

约定：**进战斗的关，`roguelikeKind` 与地图节点类型一致**；休息/商店不进战斗、不走 `LevelOutCome_Roguelike`。

### 用户故事

作为玩家，我希望打完肉鸽战斗关后获得与关卡类型匹配的金币，以便后续在商店消费；作为策划，我希望在 Inspector 里调各类型金币区间而不用改代码。

## 现有业务上下文

| 功能 | 模块 | 关系 |
|------|------|------|
| `LevelOutCome_Roguelike` | LevelSystem | 扩展：胜利时先走奖励再回地图 |
| `RoguelikeRunService` | Roguelike | 持有 `RunMapConfig`、`RoguelikeRunState` |
| `LevelData.roguelikeKind` | LevelSystem | 发奖主键 |
| `RunMapConfig` | Roguelike | 新增 `economyConfig` 引用 |
| `RoguelikeRunSaveSystem` | SaveSystem | 序列化 `runGold` |

### 集成点

- **触发**：`LevelOutCome_Roguelike` → `RoguelikeRewardPanel` → 点击条目时 `RoguelikeRewardFlow.TryClaimEntry`
- **数据源**：`LevelManage.instance.currentLevel.roguelikeKind`
- **规则表**：`RunMapConfig.economyConfig`（空则用尖塔参考默认值）
- **写入**：`RoguelikeRunState.runGold` → `SaveRun()`

### 流程

```text
战斗胜利
  → LevelOutCome_Roguelike
  → RoguelikeRewardFlow（LevelData.roguelikeKind + EconomyConfig）
  → runGold += 掷骰金币
  → OnCombatFinished → SaveRun
  → ReturnToMapUI
```

## 技术要求

- **Unity**：ScriptableObject + Odin Inspector
- **默认数值**：参考杀戮尖塔（普通 10～20，精英 25～35，Boss 95～105，事件 15～30）
- **随机**：同 Run 种子 + 关卡名 + 已通关数，可复现
- **兼容**：旧存档无 `runGold` 字段时默认为 0

## 功能清单

### 核心功能（P0 — 已实现）

- [x] `RoguelikeEconomyConfig` + `RoguelikeGoldRule`
- [x] `RoguelikeRewardFlow.TryGrantCombatGold`
- [x] `RoguelikeRunState.runGold` + 存档克隆
- [x] `RunMapConfig.economyConfig` 引用
- [x] `LevelOutCome_Roguelike` 接入
- [x] `RoguelikeRunInfoPanel` 可选 `runGoldText` / summary 显示金币

### 后续扩展（P1+）

- [x] 精英/Boss 三选一 plant — **EconomyConfig 筛选 + 条目生成**（`RollPlantPickOptions`）
- [ ] `RoguelikePlantPickPanel` UI + 领取 `AddPlant`
- [ ] 商店节点花金币、`RoguelikeShopPanel`
- [ ] 遗物 / 道具规则字段
- [ ] 胜利弹窗「+XX 金币」UI
- [ ] `roguelikeKind == None` 时 Editor 校验告警

## 验收标准

- [ ] 普通/精英/Boss 战斗胜利后 `runGold` 增加，数值落在 Config 区间内
- [ ] 继续冒险后金币与退出前一致
- [ ] 未挂 `economyConfig` 时使用默认尖塔参考值
- [ ] 休息/商店不触发本流程
- [ ] Console 可见 `[RoguelikeRewardFlow] +N 金币` 日志

## 配置步骤（Unity）

1. **Project** → `Create → Roguelike → Economy Config`（建议 `Assets/SO/rough/RoguelikeEconomyConfig.asset`）
2. Inspector 点 **重置为尖塔参考默认值**（或手调 `goldRules`）
3. 打开 `整局分配`（RunMapConfig）→ **经济 → Run 经济配置** 拖入上一步资产
4. 可选：在 `RoguelikeRunInfoPanel` prefab 拖 `runGoldText`

## 风险评估

| 风险 | 概率 | 影响 | 应对 |
|------|------|------|------|
| LevelData 未标 roguelikeKind | 中 | 无金币 | 配表规范 + Warning 日志 |
| 与商店系统重复定价 | 低 | 后期重构 | EconomyConfig 预留 shop 字段 |

## 关联 Context

- `context/index.md`
- `Assets/Script/Roguelike/Map/`
- `docs/requirements/roguelike-run-save.md`

---

*实现版本：2026-05-20，首版仅金币。*
