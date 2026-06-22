# 功能需求卡片：肉鸽植物三选一奖励（PlantPick）

## 基本信息

- **功能名称**：Roguelike PlantPick — 战斗胜利植物三选一
- **所属模块**：Roguelike / Economy / UI
- **需求类型**：功能扩展
- **优先级**：P1（本阶段：EconomyConfig 筛选 + 条目生成；UI 子面板后续）
- **预估复杂度**：L2
- **提出日期**：2026-05-20

## 功能描述

精英/Boss 战斗胜利后，搜刮面板除金币外增加 **PlantPick** 条目。点击后（后续）打开三选一面板；本阶段先实现 **杀戮尖塔式候选生成** 并写入 `RoguelikeRewardEntry.plantPickOptions`。

### 筛选逻辑（已确认）

1. **建池**：`GameManage.allChess`（或白名单）→ 有 `plantFunction` 或卡牌预制体 → 排除黑名单 / 已拥有 / `LevelUp_Limit` 升级卡
2. **每格两步**：先掷 price 档位（低/中/高），再从该档均匀随机 1 张
3. **同屏不重复**（比 STS 略友好）；Run 内不重复获得（沿用 `AddPlant`）
4. **种子固定**：与金币相同 runSeed + act + cleared + level + kind

### 默认发放

| 关卡类型 | PlantPick |
|----------|-----------|
| Normal | 否 |
| Elite | 是，权重 45/40/15 |
| Boss | 是，权重 20/35/45 |
| Event | 否 |

## 现有业务上下文

| 功能 | 关系 |
|------|------|
| `RoguelikeEconomyConfig` | 扩展 plantPickRules / priceBands / poolFilter |
| `RoguelikeRewardFlow` | BuildCombatRewardEntries 生成 PlantPick 条目 |
| `RoguelikeRunPlantPool.AddPlant` | 领取时写入（UI 阶段实现） |
| `RoguelikeRewardPanel` | 点击 PlantPick 开子面板（未实现） |

## 功能清单

### 本阶段（已实现）

- [x] `RoguelikePlantPickRule` / `RoguelikePlantPriceBands` / `RoguelikePlantPoolFilter`
- [x] `RollPlantPickOptions` / `ShouldGrantPlantPick`
- [x] `RoguelikeRewardEntry.plantPickOptions` + `CreatePlantPick`
- [x] `BuildCombatRewardEntries` 接入

### 后续（P1 UI）

- [ ] `RoguelikePlantPickPanel` 三选一 + 跳过
- [ ] `RoguelikeRewardPanel` PlantPick 点击流程
- [ ] `TryClaimEntry` PlantPick → `AddPlant`

## 验收标准

- [ ] Elite/Boss 胜利后搜刮列表出现 PlantPick 条目（池子非空时）
- [ ] Console / 调试可见 3 个 chessName，与重进同关一致（种子复现）
- [ ] Normal 关不出现 PlantPick
- [ ] 已拥有 plant 不再出现在候选（默认配置）
- [ ] Inspector「重置为尖塔参考默认值」含 PlantPick 规则

## 关联

- `docs/requirements/roguelike-economy-config.md`
- `docs/requirements/roguelike-reward-panel.md`
- `context/index.md`
