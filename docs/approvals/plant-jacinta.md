# 审批报告: 嘉辛塔（plant-jacinta）

## 基本信息
- **需求 ID**: plant-jacinta
- **日期**: 2026-08-14
- **类型**: 新增角色（Skill 组合）

## 需求摘要
嘉辛塔：造价 50，常态不攻；持续技 25s / 结束后 CD 15s；技能期动画产阳（建议单次 20）+ 薇薇安同范围挡弹。

## Context
✅ `context/index.md`；撑伞 / 桃金娘 / 薇薇安已有实现可复用。

## Skills 白名单
✅ ScriptableObject 配置  
✅ 现有 Skill / PlantUmbrella / SunLight（无新 Unity 子系统）  
结论：通过

## 架构
✅ 单一 `ISkillEffect` + ColdSkill Duration；无运行时技能 Mono  
风险：低

## 影响面
- 文件数：约 4（新增为主）
- 高风险点：0
- 回归项：桃金娘、薇薇安、持续 ColdSkill

## 风险总评
| 维度 | 评级 | 说明 |
|------|------|------|
| 可行性 | 高 | 两套路已验证 |
| 改动范围 | 小 | 新角色 |
| 性能 | 低 | 挡弹 Tick 仅技能窗 |
| 时间 | 4–6h | 美术另计 |

## 开发优先级
- P0: SkillEffect + Config + SO/Prefab 接线  
- P1: 文案/标签/验收  
- P2: 正式立绘与动画事件调参

## 实现状态
- **审批**: ✅ 通过（2026-08-14）
- **代码**: 已落地（`SkillEffect_Jacinta` + SO/Prefab 接线）
- **待 Editor**: 立绘、Animator、skill 动画事件 `UseSkill`

## 决策审批
✅ 通过 - 可以开始开发（已实现）  
⬜ 修改后通过（例如单次阳光不用 20）  
⬜ 驳回  

审批意见: 单次阳光 20；用户确认通过  
日期: 2026-08-14
