# 影响面分析: 天素罗

## 改动类型
新增功能（新被动类 + 补全已有 Prefab/asset）；**不修改**核心 `Tile` / `CreateChess` / 素世本体逻辑。

## 改动范围

| 文件 | 改动 | 影响程度 |
|------|------|----------|
| `PassiveSkillEffect_TianSuoLuo.cs`（新） | 计时 / 转化 | — |
| `天素罗.asset` | 身份、数值、PlantType | 低（仅本单位） |
| `天素罗.prefab` | 被动序列化 | 低 |
| 可选：图鉴 / allChess 列表 | 注册 | 低 |

**不改**：`长崎素世` Prefab/asset、`Tile.cs`、`Chess.Death`、南瓜罩脚本（仅运行时经 `OnPlant` 间接受益）。

## 依赖关系
- **依赖**：`TimerManage`、`ChessTeamManage.CreateChess`、`PropertyCreator`（素世）、`Chess.Death` / `OnRemove`
- **被依赖**：无（新单位）
- **运行时关联**：同格 `PassiveSkillEffect_PumpkinShell`（`Tile.OnPlant`）

## 影响面评估
| 类型 | 项 | 程度 |
|------|----|------|
| 直接 | 天素罗自身生命周期 | 高（预期行为） |
| 间接 | 同格南瓜罩换绑新 Main | 中（需回归） |
| 间接 | MyGO 羁绊 Member 计数（与素世同 id） | 低 |
| 潜在 | 转化瞬间站位/碰撞空窗 | 低 |

## 风险
| 风险 | 概率 | 影响 | 说明 |
|------|------|------|------|
| 南瓜 `ProtectChess` 未卸旧监听 | 中 | 低～中 | 旧 Main 已回收则通常无害；回归转化后罩是否挡伤 |
| 转化时 tile 已空/无效 | 低 | 中 | OnRemove 前缓存 tile；无效则只自灭 |
| 池化棋子脏状态 | 低 | 中 | 被动字段在 SkillEffect 入口重置标志与 Timer |

## 回归测试建议
1. 天素罗种植：50 阳、高防、不攻击、占 Main
2. 到期（可改短 delay 测）：自灭 + 同格完整素世（技能/数值与直接种素世一致）
3. 提前咬死 / 铲子：不刷素世；无 Timer 残留（反复种多只）
4. 暂停后再继续：倒计时不空跑（与 TimerManage 一致）
5. 同格南瓜罩：转化后罩仍在，素世受伤由罩挡
6. MyGO：场上仅天素罗点亮素世位；天素罗+素世同场只计 1 Member
7. 波次/回收：转化出的素世可正常死亡回收

## 结论
影响面**小**；高风险点 0；建议审批通过后按架构方案 A 实现。
