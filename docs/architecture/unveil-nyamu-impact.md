# 影响面分析: 揭幕喵梦

## 改动范围
| 类型 | 内容 |
|------|------|
| 新增 | `SkillEffect_UnveilNyamu`（及可选 `ColdSkill_FireOnEnter`） |
| 新增 | Prefab / ChessData asset / 可选 allChess 条目 |
| 修改现有逻辑 | **无**（不改若麦、不改 ConsumablesSkill、不改压力协议本体） |

## 依赖关系
- **调用**: `OkuwakiStressSpread.ApplyStressDelta`、`UIManage.GetView<ItemPanel>().Create<SunLight>()`、`MapManage` 格子
- **被依赖**: 无（新棋子）
- **身份**: AveMujica `requiredMemberIds` 已含「祐天寺若麦」，无需改 Fetter SO

## 影响评估
| 项 | 程度 | 说明 |
|----|------|------|
| 若麦本体 | 无 | 仅共享 fetterMemberId |
| 压力 Buff | 低 | 复用现有 Ensure+Reset；瞬时多目标 +25 |
| 阳光经济 | 低～中 | 最多 9 颗×25（满员九宫格）；属设计预期 |
| 史尔特尔/喜多 | 无 | 仅模式参考 |

## 风险
| 风险 | 等级 | 应对 |
|------|------|------|
| 动画未挂 Death 导致残留 | 中 | 验收；可临时文档提醒 |
| 同场若麦+揭幕喵梦 Member 只算 1 | 低 | 与压力希同规则，文档已知 |

## 回归测试建议
1. 单种揭幕喵梦、周围无友军：自身压力 25 + 1 颗阳光（默认 25），随后动画 Death 消失
2. 3×3 内 2～3 名友军：每人压力 +25、各一阳光；空格无阳光
3. 阳光量改为非默认值（如 10）后验证
4. AveMujica：仅揭幕喵梦上场可点亮「祐天寺若麦」名额
5. 与常服若麦不同时/同时上场：Member 计数与 Fever 正常
6. 无普攻/无武器；种植后无需手动点技能
7. 史尔特尔/喜多回归：种植放技能、Death 流程未受影响

## 结论
新增为主，现有核心无改；影响面小，可开发。
