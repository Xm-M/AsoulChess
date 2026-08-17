# 架构设计: 橄榄球僵尸 VisualTier 一体外观

## 系统定位
- **所属模块**: Chess（Animator + Armor）
- **上游**: `PropertyController.GetDamage` → `onSetDamage` / 本体扣血 → `OnGetDamage`
- **下游**: Animator `VisualTier` Blend Tree → 一体 Sprite 序列
- **横向**: 对齐 `IceCarArmor`「护甲写档 + SampleZombie Skip」模式

## 设计决策（已锁定）

### 推荐方案：护甲写头盔档 + SampleZombie Skip
```
onSetDamage
  └─ HeadArmor_VisualTier.GetDamage
        ├─ 扣甲 → SetVisualTier(3/4/5)
        └─ 破甲 → SetVisualTier(0) + FX，卸监听

本体 damage > 0 时
  └─ SampleZombie.OnGetDamage / EnterWar
        └─ if IceCar || (HeadArmor_VisualTier && !IsBroken) → Skip Sync
        └─ else → TierFromDamagePhaseRatio(HP) → 0/1/2
```

### 否决方案
| 方案 | 否决原因 |
|------|----------|
| 仅 `damage<=0` 不改档 | `damage==0` 本就不进 `OnGetDamage`；爆炸扣甲后仍打本体会误改档 |
| 复用/改 `HeadArmor` | 破坏路障分体帽；职责混杂 |

## 档位表
| VisualTier | 含义 | 写入方 |
|------------|------|--------|
| 3 | 帽满 | HeadArmor_VisualTier |
| 4 | 帽损1 | 同上（甲 < 2/3） |
| 5 | 帽损2 | 同上（甲 < 1/3） |
| 0 | 无帽完好 | 破甲瞬间护甲；此后 SampleZombie |
| 1 | 断手 | SampleZombie（HP） |
| 2 | 断头 | SampleZombie（HP） |

## 文件改动预估
| 文件 | 动作 |
|------|------|
| `Assets/Script/Weapon/Armor/HeadArmor_VisualTier.cs` | 新增 |
| `Assets/Script/Chess/AnimtorControllier/Zombie/AnimatorController_SampleZombie.cs` | 扩展 Skip |
| `Assets/Prefab/ChessPrefab/Zombie/橄榄球僵尸/*` | Prefab + Controller |

## 风险
- 低：逻辑局部；中：Animator 资源需美术/Animation 配齐 0–5
