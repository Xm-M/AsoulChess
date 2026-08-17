# 影响面分析：芭菲猫（parfait-cat）

## 改动范围
| 文件 | 类型 |
|------|------|
| `Assets/Script/Skill/ISkillPassive/Mygo/PassiveSkillEffect_ParfaitCat.cs` | 新增 |
| `Assets/Resources/ChessData/Player/芭菲猫.asset` | 配置接线 |
| `Assets/Prefab/ChessPrefab/Mygo/芭菲猫/芭菲猫.prefab` | 被动接线 |
| `docs/requirements/parfait-cat.md` 等 | 文档 |

**不修改** `MatchaParfaitBuff`（被动侧读 buffName，无需改 ReturnCD）。

## 依赖
- 代码：TimerManage、IGridFindTarget、ColdBuff、FreezyBuff、PassiveSkill
- 资源：芭菲猫 Prefab / PropertyCreator；可选复用要乐奈冷气/冻结特效 GUID

## 回归建议
- [ ] 种植后不普攻，约 2s 对 3×3 减速（持续约 5s）
- [ ] 抹茶芭菲叠在芭菲猫上 → 脉冲变冰冻；Buff 结束后恢复减速
- [ ] 铲除后无 Timer 泄漏
- [ ] MyGO 乐奈槽计数（与要乐奈互斥计 1）
- [ ] 要乐奈吃抹茶芭菲行为不变
