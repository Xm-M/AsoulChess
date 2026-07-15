# 需求开发审批报告

## 基本信息
- **需求名称**: 多首的怪物 · 压力阈值三档动画
- **所属模块**: Skill / Chess
- **分析日期**: 2026-07-14

## 需求摘要
每次分身降低 `stressLimit` 时，按初始 max 与 min=50 算出的两档分界，用 `ChangeFloat(0/1/2)`（`Blend`）切换三套动画。**驱动量是当前 stressLimit，不是当前压力值。**

**需求类型**: 功能扩展

## 分析结果汇总

### Context
✅ 已读 — Skill / Chess

### 需求卡片
✅ `docs/requirements/multi-head-mutsumi-stress-limit-anim.md`

### Skills 白名单
- ✅ 2D 动画 `@unity-2d-animation`（现有 AnimatorController API）
- **结论**: 通过

### 架构 / 影响面
✅ `docs/architecture/multi-head-mutsumi-stress-limit-anim.md`  
✅ `docs/architecture/multi-head-mutsumi-stress-limit-anim-impact.md`  
- 风险：低

## 风险总评

| 维度 | 评级 |
|------|------|
| 技术可行性 | 高 |
| 改动范围 | 小 |
| 性能风险 | 低 |
| 时间评估 | &lt; 1 小时 |

## 开发优先级
- P0: limit 变化 → ChangeFloat(Blend) 映射
- P1: Animator 配 `Blend`（资源）

## 决策审批
✅ 通过 - 可以开始开发

审批意见: 用户确认通过  
日期: 2026-07-14

## 开发状态
✅ 已实现（2026-07-14）
- `PassiveSkillEffect_MultiHeadMutsumi.RefreshAnimTierByStressLimit`
- 入场与每次 `stressLimit` 递减后按高/低档线调 `ChangeFloat(0/1/2)`（Animator `Blend`）
- Animator 需具备 `Blend` Float 参数与三套形态（资源侧）
