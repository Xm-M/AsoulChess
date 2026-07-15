# 影响面分析: 多首 · 幽灵召唤主动技

## 改动范围

| 类型 | 文件 |
|------|------|
| 修改 | `PassiveSkillEffect_MultiHeadMutsumi.cs`、`MultiHeadMutsumiKeys.cs` |
| 修改 | `多首的怪物.prefab`（挂主动技） |
| 新增 | Burst / Ghost 被动 / FindTarget / Ready、幽灵 Creator+Prefab、SkillConfig |
| 文档 | requirements / architecture / approvals |

## 依赖
- 上游: SkillController、ChessFactory、ChessTeamManage、Timer、压力 Buff
- 下游: 多首战斗表现；关卡清场依赖 `Death`/`OnRemove`

## 风险
| 风险 | 等级 | 缓解 |
|------|------|------|
| 幽灵入队影响「清场判定」 | 中 | 确认 Level 以 Enemy 清场为准；幽灵为 Player |
| 无 standTile 导致部分插件 NRE | 中 | 幽灵逻辑不依赖 standTile；禁 PlantAreaBounce |
| 池化回收名 | 低 | chessName 固定「多首幽灵」 |

## 回归建议
- 多首未开技：出头、射击、Resume、紫砂照旧
- 开技：头清、幽灵数正确、加压、紫砂、睦死清幽灵
- 铲除睦：幽灵消失
- 0 头无法开技

## 风险等级
**中**（新召唤路径，需 Play 验收）
