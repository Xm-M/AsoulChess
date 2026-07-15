# 架构设计: 天素罗血量 Blend 档位

## 系统定位
- **模块**: Chess（`AnimatorController_Nut` + `Property`）
- **上游**: 伤害/治疗管线 `GetDamage`
- **下游**: Animator Float `Blend` → Blend Tree 外观
- **横向**: 天素罗单位；所有使用 `AnimatorController_Nut` 的坚果

## 设计决策（推荐）

### 方案 A：复用 Nut + 修 Heal 时序（采用）
1. Prefab 换 `AnimatorController_Nut`，`visualTierParameterName = "Blend"`，`hpTier0Above=0.66`，`hpTier1Above=0.33`
2. `Property.GetDamage` 在 `DamageType.Heal` 分支改为：
   ```
   Heal(mes.damage);
   chess.animatorController.OnGetDamage(mes);
   ```
   使 `SyncMorphFromHp` 读到治疗后比例。

**理由**：零新类；治疗同步改动约 2 行；坚果自然受益。

### 备选 B：仅天素罗被动监听 onHealDamage
- 缺点：重复逻辑、坚果仍不同步、仍受 Heal 时序坑影响；放弃。

### 备选 C：在 `Heal(float)` 末尾统一通知
- 可覆盖吸血；改动面略大；本需求不强制，可 P2。

## 风险
| 风险 | 等级 | 缓解 |
|------|------|------|
| Heal 后 OnGetDamage 改变闪白时机 | 低 | 治疗后闪白更合理 |
| 无 Blend 参数 | 低 | HasParameter 跳过 |

## 复杂度
L1。
