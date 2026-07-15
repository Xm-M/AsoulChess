# 架构设计: 多首 · 压力阈值三档动画

## 系统定位
- **模块**: Skill（`PassiveSkillEffect_MultiHeadMutsumi`）
- **上游**: `stressLimit` 递减、`initialStressLimit` / `stressPerClone`
- **下游**: `AnimatorController.ChangeFloat` → Animator `Blend`

## 设计决策（推荐）
在 `PassiveSkillEffect_MultiHeadMutsumi` 内：
1. 缓存 `maxLimit = initialStressLimit`、`minLimit = stressPerClone`（或固定 50）
2. `high = (max−min)×2/3 + min`，`low = (max−min)×1/3 + min`
3. `RefreshAnimTier()`：读当前 `Buff_StressBuff_Death.stressLimit`，映射 0/1/2，调用 `ChangeFloat`
4. 调用点：入场挂 Buff 后；`ReduceStressDeathLimit` 之后

**不**订阅压力计数做切档。

## 影响面
- 仅改一个被动脚本；无 Fetter/压力协议变更
- 回归：出头、limit−5、紫砂、Resume、射击

## 风险
低；Animator 未配 `Blend` 时静默无表现。

## 复杂度
L1。
