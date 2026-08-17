# 架构 / 影响面：粉色奶龙（pink-nailong）

## 设计
- 被动 `PassiveSkillEffect_PinkNailong`：关 `AttackAble`
- 主动 `ColdSkill` + `SkillReady_IfTargetInRange(IGridFindTarget)` + `SkillEffect_PinkNailongConvert`
- 转化：`Death` → `CreateChess(nailongZombieCreator, tile, user.tag)`；Creator 可热替换

## 文件
| 路径 | 说明 |
|------|------|
| `PassiveSkillEffect_PinkNailong.cs` | 新增 |
| `SkillEffect_PinkNailongConvert.cs` | 新增 |
| `Assets/SO/Skill/Mygo/粉色奶龙.asset` | CD 15 |
| `粉色奶龙.asset` / `.prefab` | 接线 |

## 风险
- 占位普通僵尸外形/数值非最终奶龙僵尸（预期）
- 无技能动画时用短 DurationFinish 退出技能态

## 回归
- [ ] 不普攻；15s 转化前方最近敌
- [ ] 生成单位为我方；换 Creator 生效
- [ ] MyGO 爱音槽计数
