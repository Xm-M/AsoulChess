# 影响面分析: senjougahara-hitagi-stationery

**改动类型**: 新增功能（半成品接线）+ 小幅 Property API  
**日期**: 2026-08-17

## 涉及文件（预估）
| 文件 | 改动 |
|------|------|
| `Assets/Script/Weapon/IAttackFunction/HitagiStationeryAttack.cs` | 新增 |
| `Assets/Script/Skill/ISkillPassive/Story/Hitagi/...` | 被动 + EraseBuff |
| `Assets/Script/Chess/Property.cs` | `SetSizeRaw` |
| `Assets/Resources/ChessData/Player/战场原黑仪.asset` | 属性/引用 |
| `Assets/Prefab/ChessPrefab/物语/战场原黑仪/*` | 接线 + 4 弹 |

## 依赖
- 上游: ObjectPool, Bullet, StressBuff, DizznessBuff, 攻击状态机
- 下游: 无强制依赖；压力队友可加快其 tier

## 风险
| 点 | 等级 | 说明 |
|----|------|------|
| 负 Size | 中 | 碾压/恐惧体型判定 |
| 连发协程 | 中 | 死亡/铲除泄漏 |
| 全局 ChangeSize | 低 | 不改默认下限 |

## 回归测试
1. 黑仪能种、普攻出弹  
2. stress 0/20/40 弹数 1/2/3，有间隔  
3. stress 下降弹数回落、Size 回升  
4. Size 可为负；Scale 不变  
5. 尺子 3 穿；钉书机晕；橡皮 5 层死  
6. 友希那/老仓压力、仙人掌穿刺、其他植物 Size 不受影响  
