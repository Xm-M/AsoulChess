# 架构设计：芭菲猫（parfait-cat）

## 系统定位
- **所属模块**: Skill（被动）/ Chess（MyGO 异形态单位）/ Buff
- **上游依赖**: TimerManage、IGridFindTarget、BuffController、MatchaParfaitBuff
- **下游影响**: MyGO fetter 计数（`fetterMemberId=要乐奈`）
- **横向关联**: 天素罗（异形态先例）、要乐奈/抹茶芭菲

## 设计决策
**采用**: `ISkillEffect` + `TimerManage` 循环计时 + `IGridFindTarget` 九宫索敌（无技能专用 MonoBehaviour）

| 方案 | 说明 | 结论 |
|------|------|------|
| A. ISkillEffect + Timer | 对齐天素罗/莴苣伞，符合技能规范 | **采用** |
| B. 运行时 MonoBehaviour | 初雪旧模式，规则禁止 | 否 |
| C. 武器普攻挂 Buff | 需求为无普攻光环 | 否 |

### 数据流
```
种植 → PassiveSkill.UseSkill → PassiveSkillEffect_ParfaitCat
  → AttackAble=false
  → 立即 OnPulse + AddTimer(interval, loop)
  → FindTarget 3×3 → AddBuff(ColdBuff | FreezyBuff)
  → 自身 buffDic 含「抹茶芭菲」时切 FreezyBuff
离场 OnRemove → Stop Timer
```

## 风险
| 风险 | 等级 | 应对 |
|------|------|------|
| 周期冰冻过强 | 中 | 间隔/持续可配 |
| Prefab 无武器 NRE | 低 | 判空再关 AttackAble |
| FreezyBuff 浅拷贝嵌套 Cold | 低 | PrepareFreeze 深拷贝嵌套 |

## 风险总评
**低** — L1，局部新增 + Prefab 接线。
