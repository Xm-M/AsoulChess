# 需求开发审批报告

## 基本信息
- **需求名称**: 多首的怪物 · 幽灵召唤主动技
- **所属模块**: Skill / Chess / Weapon / Buff
- **分析日期**: 2026-07-15

## 需求摘要
多首主动技：清压、灭头、按头数召唤无占格不可选中轻量幽灵近战追击；开技后每秒加压直至死亡；锁出头与 limit 递减。

**需求类型**: 功能扩展  
**与现有功能关系**: 扩展 `PassiveSkillEffect_MultiHeadMutsumi`；幽灵不做子弹、不占格

## 分析结果汇总

### Context
✅ 已读 — Skill / Chess

### 需求卡片
✅ `docs/requirements/multi-head-mutsumi-ghost-skill.md`

### Skills 白名单
| 技术点 | Skill | 结论 |
|--------|-------|------|
| 技能效果 / Timer | `@unity-coroutine-system`（Timer） | ✅ |
| Prefab 召唤 | `@unity-prefab-system` | ✅ |
| 2D 索敌 Overlap | `@unity-2d-physics` | ✅ |
| SO 配置 | `@unity-scriptableobject-config` | ✅ |
| 动画（复用） | `@unity-2d-animation` | ✅ |

**结论**: 通过

### 架构 / 影响面
✅ `docs/architecture/multi-head-mutsumi-ghost-skill.md`  
✅ `docs/architecture/multi-head-mutsumi-ghost-skill-impact.md`  
- 设计: 轻量 Chess + Context 终局锁  
- 风险: 中

## 风险总评

| 维度 | 评级 | 说明 |
|------|------|------|
| 技术可行性 | 高 | 有压力/Timer/UnSelectable 先例 |
| 改动范围 | 中 | 新文件 + 被动锁 + Prefab |
| 性能风险 | 低～中 | 幽灵数 ≤ 头上限（约 20） |
| 时间评估 | 1–2 天 | 含联调 |

## 开发优先级
- P0: Burst 效果、锁被动、幽灵生成/追击近战、每秒加压、清场
- P1: Prefab/Creator/多首主动技接线、CD 与点击 Ready
- P2: 美术/分散索敌

## 决策审批
✅ **通过** — 计划已确认，进入开发

审批意见: 用户确认计划并要求实现全部 to-do  
日期: 2026-07-15

## 开发状态
✅ 已实现（2026-07-15）
- `SkillEffect_MultiHeadGhostBurst` / `SkillReady_MultiHeadHasClones` / `ColdSkill_FireSkillOnEnter`
- `PassiveSkillEffect_MultiHeadGhost` + `FindTarget_NearestEnemyInRange`
- Keys：终局锁 / 幽灵列表 / 加压 Timer；被动出头与降 limit 跳过
- 资源：`多首幽灵` Prefab+Creator、`多首幽灵召唤` SkillConfig；多首 Prefab 挂主动技并改用 `AveMujica_复合模版`
