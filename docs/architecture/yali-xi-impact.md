# 影响面分析: 压力希

## 改动范围
- **类型**: 新增功能（新被动、新子弹类、配置/Prefab）
- **不修改**: 立希本体、`Buff_StressBuff_Death` 核心、MyGO 羁绊 SO 名单（靠同 `fetterMemberId`）

## 预估文件
| 文件 | 动作 |
|------|------|
| `PassiveSkillEffect_YaliXi.cs` | 新增 |
| `Bullet_LingerDot.cs`（名可定） | 新增 |
| `压力希.asset` / `压力希.prefab` / 驻留弹 Prefab | 修改/新增 |
| `开始.unity` allChess | 可选 |

## 依赖
- 复用：`OkuwakiStressSpread`、`ShootBullet`、压力 Buff
- 被依赖：无

## 影响评估
| 项 | 程度 |
|----|------|
| 立希战斗 | 无 |
| MyGO 点亮 | 低（同成员位互斥计数，预期） |
| 性能 | 低（邻格≤4 订阅；驻留弹 Timer） |

## 回归测试
1. 邻格友军攻击 → 该友军压力 +1；对角/敌方不触发  
2. 压力希自身不挂压力  
3. 驻留弹：首击 + 每秒跳伤 + N 秒消失  
4. 目标死后换下一碰撞敌人  
5. `fetterMemberId=椎名立希` 可点亮 MyGO；与常服立希同额  
6. 无恐惧波、无四人加射程  
7. 离场后邻格攻击不再加压  

## 风险总评
改动范围中小；高风险 0；中风险 2（订阅、对象池）。
