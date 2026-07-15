# 功能需求卡片: 肉鸽搜刮金币 — 复用 Item_Coin 飞向 Run 金币条

## 基本信息
- **功能名称**: Roguelike 战斗奖励金币视觉反馈（RG-P03）
- **所属模块**: Roguelike / UI / ItemPanel
- **需求类型**: 功能扩展（复用主线 `Item_Coin` 表现，接入 `runGold`）
- **优先级**: P2
- **预估复杂度**: L2
- **提出日期**: 2026-05-20
- **关联**: `docs/roguelike-known-issues.md` RG-P03

## 功能描述

### 背景（用户原话摘要）

- 肉鸽**进战斗**时会 `RoguelikeRunInfoPanel.HideForCombat()`，战斗场景里**看不到 Run 金币增长**。
- 肉鸽 **`runGold`** 与主线 **`PlayerSaveData.coins`**（戴夫车商店）**完全切割**。
- 希望在 **搜刮面板** 点击领取金币时，**复用打僵尸掉钱那套 UI**（`Item_Coin`）：在点击处生成若干金币/银币图标，停留后**飞向左下角计数条**；左下角条在肉鸽奖励场景下显示 **Run 金币总数**，而非主线 coins。

### 用户故事

作为玩家，我在战斗胜利搜刮里点「130 金币」时，希望看到硬币从按钮旁飞出并汇入左下角 Run 金币条，像主线捡硬币一样有反馈，且不会和外面商店金币混淆。

### 期望流程（确认版）

```text
战斗胜利 → RoguelikeRewardPanel（仍在战斗场景，RunInfo HUD 已隐藏）
  → 玩家点击金币条目（如 130）
  → TryClaimEntry：runGold += 130（逻辑不变）
  → 在条目按钮中心生成 N 枚 Item_Coin（面额拆分）
  → 每枚从【按钮中心】SmoothStep 外散 0.5s 到周围（非瞬间闪现）
  → 全部外散完成后停留最多 3s
  → 3s 到期 或 玩家点击任意硬币 → 全部飞向「左下角 Run 金币条」
  → 汇入后更新条上数字为当前 runGold；播 Coin 音效
  → 金币条目从列表移除（与现有一致）
```

### 与主线 Item_Coin 的差异

| 项 | 主线 `Item_Coin` | 肉鸽搜刮版 |
|----|------------------|------------|
| 触发 | 僵尸死亡 Buff | 搜刮面板点金币条目 |
| 出生位置 | 僵尸世界坐标 → 屏幕 | **条目按钮中心附近**（屏幕/UI 坐标） |
| 加钱目标 | `PlayerSaveData.coins` | **`RoguelikeRunState.runGold`**（已在 TryClaimEntry 加过，视觉仅反馈） |
| 回收目标 | 左下角 `(80,80)` 或 `RecycleTarget` | **Run 金币条** Transform |
| 待机 | 等点击/移入 | **最多 3s** 或点击提前收束 |
| 掉落动画 | 抛物线落地 | **从按钮中心外散 0.5s**（SmoothStep），再停留 |

## 现有业务上下文

### 相关现有功能

| 功能 | 关系 |
|------|------|
| `Item_Coin` | **复用** prefab、图标、飞行、音效；需 **Roguelike 模式** 分支 |
| `ParsePanel.coinDisplayObject` + `ShowCoinDisplay` | 主线捡币后左下角显示 **总 coins**；肉鸽可 **同 UI 槽位改显 runGold** 或平行 HUD |
| `RoguelikeRewardPanel.OnEntryClicked` → `TryClaimEntry` | 集成点 |
| `RoguelikeRunInfoPanel` | 地图/商店显示 runGold；战斗隐藏 |
| `Buff_DeathCoin` / `GameStartPlugin_ZombieDropCoin` | 参考表现，**不共用**触发链 |

### 集成点

- `RoguelikeRewardPanel.AfterEntryClaimed` / `OnEntryClicked`（Gold 分支）
- `Item_Coin.InitCoin` 或新 overload：`InitRoguelikeRewardVisual(...)`
- 左下角条：`ParsePanel.ShowCoinDisplay` 扩展，或 `RoguelikeRunGoldHud` 挂同一 prefab 节点

### 对现有功能的影响

- **主线捡币**：行为不变；仅当 `RoguelikeRunService.HasActiveRun && 奖励面板可见` 时走新分支
- **`Item_Coin.AddCoinsAndRecycle`**：须避免肉鸽视觉硬币再次 `coins +=`（**只飞、不加主线钱**）
- **ParsePanel 金币条**：肉鸽奖励期间显示 runGold；离开奖励/非 Run 时恢复隐藏或显示主线逻辑

## 硬币数量拆分规则

三档面额贪心（来自 `RoguelikeEconomyConfig`）：

| 图标 | 面额 | 字段 |
|------|------|------|
| 银币 | **10** | `coinVisualUnitSmall` |
| 金币 | **50** | `coinVisualUnitLarge` |
| 钻石 | **1000** | `coinVisualUnitDiamond` |

从大到小贪心拆分，最多 8 枚视觉硬币；余数不足时补 1 枚银币（纯展示）。

| 领取 runGold | 示例拆分 |
|--------------|----------|
| 130 | 50×2 + 10×3 |
| 1300 | 1000×1 + 50×6 |
| 85 | 50×1 + 10×4（含余数补银） |

## 可调参数（2026-05-20 增补）

### 散开范围

`RoguelikeRewardPanel` Inspector → **金币领取视觉**：

| 字段 | 默认 | 说明 |
|------|------|------|
| `rewardCoinScatterRadiusMin` | 56 | 相对按钮中心最小半径（屏幕像素） |
| `rewardCoinScatterRadiusMax` | 140 | 最大半径 |
| `rewardCoinScatterDuration` | **0.5** | 从按钮中心向外散开的时长（秒） |

3s 待机计时在**全部硬币外散完成后**才开始。

### PvZ 货币尺度（×10）

`RoguelikeEconomyConfig`：

| 字段 | 默认 | 说明 |
|------|------|------|
| `coinVisualUnitDiamond` | 1000 | 钻石图标 |
| `coinVisualUnitLarge` | 50 | 金币图标 |
| `coinVisualUnitSmall` | 10 | 银币图标 |
| `runGoldRewardMultiplier` | 1 | PvZ 建议 **10**（表内 goldMin/Max 基数×倍率） |

一键：`RoguelikeEconomyConfig` Inspector → **重置为 PvZ 尺度（奖励×10）**；视觉面额仍为 1000/50/10。

**注意**：只影响肉鸽 `runGold` 发放与搜刮视觉；主线 `PlayerSaveData.coins` / 僵尸掉币 **不自动×10**（另开需求）。

## 实现状态（2026-05-20）

- [x] `RoguelikeRewardCoinVisual` + `RoguelikeRewardCoinBatch`
- [x] `Item_Coin.InitRoguelikeRewardScatter` + 外散 0.5s（`RoguelikeScatterThenIdle`）
- [x] `RoguelikeRewardPanel` 金币条目领取集成 + scatter Inspector
- [x] `ParsePanel.ShowRunGoldDisplay`
- [x] `RoguelikeEconomyConfig.runGoldRewardMultiplier` + 视觉面额 + PvZ 预设按钮


- **Skills**: `@unity-ui-system`（RectTransform、屏幕坐标）、`@unity-coroutine-system`、`@unity-object-pool`（ItemPanel 池）
- **不复用** `DamagePanel` 飘字
- **性能**: 单次领取 ≤8 个 Item_Coin 实例

## 功能清单

### 核心（P0 本需求）

- [ ] 点击 Gold 条目：spawn 拆分后的 Item_Coin（UI 位置）
- [ ] 3s 待机或点击收束 → 飞向 Run 金币条
- [ ] 左下角条显示 **runGold**（非 PlayerSaveData.coins）
- [ ] 肉鸽视觉硬币 **不写入** 主线 coins
- [ ] Coin 音效（与 Item_Coin 一致）

### 可选（P2）

- [ ] 条目缩小消失与飞币并行
- [ ] 多条金币条目连点队列（不重叠销毁）

## 验收标准

- [x] 肉鸽战斗胜利搜刮：硬币从按钮中心外散 0.5s，停留后飞向左下角 Run 金币条
- [x] 3s 无操作自动收束；点击硬币提前收束
- [x] 领取后 runGold 数值正确（不重复加钱）
- [x] PlantPick / 道具条目无硬币动画
- [x] 主线 Item_Coin 行为不变（仍加 PlayerSaveData.coins）

## 风险评估

| 风险 | 概率 | 影响 | 应对 |
|------|------|------|------|
| Item_Coin 误加主线 coins | 中 | 高 | `InitRoguelikeRewardVisual` + 跳过 AddCoins |
| ParsePanel 与 Run 状态耦合 | 中 | 中 | 显式 `ShowRunGoldDisplay(int)` |
| 战斗场景无 RecycleTarget | 低 | 中 | 打开奖励面板时设置 `Item_Coin.RecycleTarget` |

## 关联 Context

- `context/modules/UI.md`
- `docs/requirements/roguelike-reward-panel.md`
- `docs/requirements/roguelike-economy-config.md`

## 待用户确认

1. **拆分规则**：✅ 钻石 1000 / 金币 50 / 银币 10 三档贪心
2. **左下角 UI**：✅ A — 复用 `ParsePanel.coinDisplayObject`，`ShowRunGoldDisplay(runGold)`
3. **出生动画**：✅ 从按钮中心外散 **0.5s**，再停留后飞左下角

## 实现状态（2026-05-20）

- [x] `RoguelikeRewardCoinVisual` + `RoguelikeRewardCoinBatch`
- [x] `Item_Coin.InitRoguelikeRewardScatter`
- [x] `RoguelikeRewardPanel` 金币条目领取集成
- [x] `ParsePanel.ShowRunGoldDisplay`
