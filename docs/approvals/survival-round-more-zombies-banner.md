# 需求开发审批报告

## 基本信息
- **需求名称**: survival-round-more-zombies-banner
- **分析日期**: 2026-08-14
- **审批**: 通过 — 横幅 + 通关掉落（Prefab/动画由人工挂接）

## 需求摘要
生存轮末（非通关）调用 `TextPanel.MoreZombiesComing()`；通关轮不播。通关改为 `SpawnVictoryReward` 掉落奖杯，点击后再 `GameOver(true)`。

## 实现备注
- Prefab：在 `文字显示面板` 上绑定 `moreZombiesComing` GO（布局/动画自理）
- 代码已接：`LevelController_Endless.OnRoundCompleteCoroutine`

## 决策审批
✅ 通过 - 横幅 + 顺便修通关掉落

日期: 2026-08-14
