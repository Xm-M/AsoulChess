# 需求开发审批报告

## 基本信息
- **需求名称**: hanekawa-tsubasa-black-form（羽川翼黑羽川）
- **所属模块**: Chess / Skill / Buff / Move / Weapon
- **分析日期**: 2026-08-17
- **审批**: ✅ 通过（已实现）

## 需求摘要
羽川翼常态产阳（向日葵）；压力 ≥90 自动变黑羽川（HP+500% 基数、30% 吸血、大范围最近敌近战移动、压力 −2/s、原格预订）；压力归 0 回原位，无法回去则 Death。

## 实现摘要
- `PassiveSkillEffect_Hanekawa`：挂压、自动变身/回位、HP/吸血、降压 Timer、索敌移动、stander 原格预订
- `SkillReady_HanekawaCanProduceSun`：黑形态禁止产阳
- Prefab：`复合模版` + ColdSkill（虹夏阳光配置）+ 近战 Weapon + 被动；Asset 价 150、攻 60、移速 4
- 动画：`transform` / `transform_back`；末帧 `Chess.AnimCallback` + 超时兜底；结束后再切 `Blend`

## 文档
- 需求：`docs/requirements/hanekawa-tsubasa-black-form.md`
- 架构：`docs/architecture/hanekawa-tsubasa-black-form.md`
- 影响面：`docs/architecture/hanekawa-tsubasa-black-form-impact.md`

## 决策审批
✅ 通过（含变身动画扩展）

日期: 2026-08-17
