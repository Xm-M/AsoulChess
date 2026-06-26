# 功能需求卡片: 肉鸽 Run 结束结算（复合奖励面板）

## 基本信息
- **功能名称**: Roguelike Run End（通关 / 本局结束）
- **所属模块**: Roguelike / UI / LevelSystem
- **需求类型**: 功能扩展
- **优先级**: P0
- **提出日期**: 2026-06-11

## 功能描述
- 肉鸽战斗失败：`TextPanel` 保留 **重来** + 显示 **结算**；点结算 → `RoguelikeRewardPanel` 本局结束摘要 → **返回主菜单** → `LeaveLevel`
- 全 Run 通关：搜刮后同面板 **通关** 摘要 → 返回主菜单
- 不新增 Panel；局外养成 UI 本期不做

## 验收标准
- [x] 非肉鸽失败：仅重来，无结算按钮
- [x] 肉鸽失败：重来保留；结算 → 摘要 → 主菜单 → LeaveLevel
- [x] 最终 Boss 全胜：搜刮 → 通关摘要 → 主菜单
- [x] 中途战斗胜：仅搜刮 → 回地图

## 预制体接线
1. `文字显示面板`：`roguelikeSettleButton` → `TextPanel.RoguelikeSettle`
2. `RoguelikeRewardPanel`：`rewardsSectionRoot`、`runSummaryRoot`、`summaryText`
3. **返回主菜单**：优先绑 `returnToMenuButton`（可放在 `失败结算画面` 下）；未绑定时复用 `skipButton`（搜刮区底部「返回游戏」按钮，结算时文案改为「返回主菜单」）
