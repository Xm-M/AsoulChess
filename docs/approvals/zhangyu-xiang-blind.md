# 审批：章鱼祥（致盲被动）

✅ 通过（用户确认 1.1 / 2.1 / 3.1 / 4.1 / Miss 飘字）— 已实现。

## 实现摘要
- `PropertyController.onBeforeTakeDamage` + `GetDamage` Miss 飘字早退
- `Buff_Blind`：输出非治疗 → Miss
- `PassiveSkillEffect_ZhangyuXiang`：命中 25% 挂致盲 2s
- `章鱼祥.asset` / Prefab 对齐八九寺武器与状态图
- 棒球喂球补 `suppressFloatingDamage`，避免误飘 Miss
