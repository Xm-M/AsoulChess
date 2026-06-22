# 功能需求卡片：肉鸽战斗奖励面板（RoguelikeRewardPanel）

## 基本信息

- **功能名称**：RoguelikeRewardPanel（尖塔式搜刮面板）
- **所属模块**：Roguelike / UI / LevelSystem
- **需求类型**：功能扩展
- **优先级**：P0（首版仅金币条目）
- **提出日期**：2026-05-20

## 功能描述

战斗胜利后不立刻回地图，先弹出奖励面板。每条奖励是一个按钮：

- **金币**：点击 → `runGold += N` → 条目消失
- **跳过**：未领取条目直接放弃（与尖塔一致）
- 全部领完或点跳过 → `OnCombatFinished` → 打开地图

首版只做 **Gold**；PlantPick / Item 预留枚举与条目类型。

### 用户确认（2026-05-20）

1. 未领取条目可直接放弃（跳过）
2. 第一版只做金币

## 流程

```text
最后一波僵尸全灭（SpawnVictoryReward）
  → LevelOutCome_Roguelike（仍在战斗场景）
  → RoguelikeRewardPanel 弹出
  → 逐条领取 / 跳过
  → OnCombatFinished + ReturnToMapUI（离开关卡 → 开始场景 → 地图）
```

失败仍：OnCombatFinished(false) → 结束 Run，不弹面板。

## 预制体（手动）

路径：`Resources/UIPrefab/RoguelikeRewardPanel.prefab`，根物体 name = `RoguelikeRewardPanel`，挂 `RoguelikeRewardPanel : View`。

| 字段 | 说明 |
|------|------|
| titleText | 标题，如「搜刮！」 |
| totalGoldText | 当前持有金币 |
| entriesRoot | 条目列表父节点（Vertical 布局区域） |
| entryPrefab | 可选；挂 `RoguelikeRewardEntryWidget` 的按钮预制体 |
| skipButton | 跳过/继续 |
| skipButtonLabel | 按钮文案（跳过 / 继续） |
| panelAnimator | 面板 Animator；关闭时 Play `close` |
| closeAnimStateName | 关闭状态名，默认 `close` |

`close` 动画最后一帧添加 **Animation Event**，调用 `RoguelikeRewardPanel.OnCloseAnimationFinished`。

`entryPrefab` 留空时运行时会代码生成简单按钮行。

## 验收标准

- [ ] 普通/精英/Boss 战斗胜利后弹出面板，显示金币条目
- [ ] 点击金币条目：金币增加、条目消失、存档更新
- [ ] 点「跳过」：未领条目放弃，进入地图
- [ ] 领完所有条目后自动继续（或按钮变「继续」）
- [ ] 无奖励条目时直接进入地图
- [ ] 失败不弹面板

## 关联

- `docs/requirements/roguelike-economy-config.md`
- `RoguelikeRewardFlow` / `RoguelikeEconomyConfig`
