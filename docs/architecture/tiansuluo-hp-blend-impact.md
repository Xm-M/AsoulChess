# 影响面分析: 天素罗血量 Blend 档位

## 改动范围
| 文件 | 改动 | 程度 |
|------|------|------|
| `Property.cs` | Heal 分支：先 Heal 再 OnGetDamage | 低（时序） |
| `天素罗.prefab` | AnimatorController → Nut + 阈值/参数名 | 低 |
| （可选注释）`AnimatorController_Nut.cs` | 文档说明治疗依赖 GetDamage 时序 | 极低 |

**不改**: `PassiveSkillEffect_TianSuoLuo`、素世转化逻辑。

## 间接影响
- 所有经 `GetDamage(Heal)` 治疗的棋子：`OnGetDamage`（含闪白）在回血之后
- 所有 `AnimatorController_Nut`：治疗可回档（预期收益）

## 回归测试
1. 天素罗：打穿 66%/33% 阈值，Blend 0→1→2
2. 天素罗：治疗后比例回升，Blend 回退
3. 天素罗：到期仍生成素世
4. 任意 Nut 坚果（若场景有）：受伤破损 + 治疗回档
5. 治疗技能闪白仍出现、无报错

## 结论
影响面小；高风险 0；建议通过。
