# 影响面分析: football-zombie-visual-tier

## 改动类型
功能扩展（新护甲类 + SampleZombie 门禁扩展 + 橄榄球 Prefab）

## 影响范围
| 范围 | 说明 |
|------|------|
| 直接改 | `AnimatorController_SampleZombie`、新 `HeadArmor_VisualTier`、橄榄球 Prefab/Controller |
| 间接 | 所有使用 SampleZombie 的僵尸：仅多一次 `GetComponentInChildren<HeadArmor_VisualTier>`（无组件时快速返回） |
| 不改 | `HeadArmor`（路障）、IceCar 语义保持（Skip 条件 OR 合并） |

## 回归测试建议
1. 普通僵尸：受伤 0→1→2、断手/断头 FX
2. 路障：分体帽三态与破甲掉落
3. 冰车：护甲档仍由 IceCar 驱动，不被血量覆盖
4. 橄榄球：3→4→5；爆炸打本体外观仍在头盔档；破甲→0→1→2
5. 多只橄榄球重叠：无帽子层乱序

## 风险评级
- 改动范围：小
- 回归面：中（SampleZombie 公共路径）
- 总体：可接受
