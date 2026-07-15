# 功能需求卡片: 天素罗血量 Blend 档位

## 基本信息
- **功能名称**: 天素罗血量 Blend 档位（含坚果治疗同步）
- **所属模块**: Chess / AnimatorController
- **需求类型**: 功能扩展
- **优先级**: P1
- **预估复杂度**: L1
- **预估耗时**: 0.5–1 小时
- **提出日期**: 2026-07-15
- **关联**: `docs/requirements/tiansuluo.md`

## 功能描述
### 详细描述
天素罗根据当前生命比例切换 Animator **`Blend`**：
| 血量比例 | Blend |
|----------|-------|
| **> 66%**（含满血） | **0** |
| **≤ 66% 且 > 33%** | **1** |
| **≤ 33%** | **2** |

复用现有 **`AnimatorController_Nut`**（参数名配置为 `Blend`，阈值 0.66 / 0.33）。  
**治疗同步（方案 B）**：经 `GetDamage(Heal)` 回血后也刷新档位；同时修正坚果类（`AnimatorController_Nut`）一并支持治疗回档。

### 用户故事
作为玩家，我希望天素罗（及坚果类）破损外观随血量升降变化，治疗回血后外观也能恢复。

## 现有业务上下文

### 相关现有功能
| 功能 | 模块 | 关系 | 说明 |
|------|------|------|------|
| `AnimatorController_Nut` | Chess | 复用 | 受伤时 `SyncMorphFromHp`；默认参 `VisualTier`、阈值 0.6/0.25 |
| `Property.GetDamage` | Chess | 需微调 | Heal 分支当前先 `OnGetDamage` 再 `Heal`，导致同步读到旧血量 |
| 天素罗 Prefab | Chess | 改配置 | 现用基类 `AnimatorController` |
| `ChangeFloat("Blend")` | Chess | 同参名 | 多首等用 Blend；本需求用 Nut 写 Float，非 ChangeFloat 主动调用 |

### 需求类型判定理由
在已有坚果血量档位机制上，为天素罗接线并修正治疗时序；非新系统。

### 集成点
- Prefab：脚本改为 `AnimatorController_Nut`；`visualTierParameterName=Blend`；`hpTier0Above=0.66`；`hpTier1Above=0.33`
- `Property.GetDamage` Heal：先 `Heal` 再 `animatorController.OnGetDamage`（使 Nut 读到新比例）
- Animator：idle 等状态 Blend Tree 消费 `Blend`（美术/Override 侧）

### 对现有功能的影响
- **接口**: 无破坏性 API 变更
- **行为**: 所有走 `GetDamage(Heal)` 的单位，`OnGetDamage` 时机改为治疗后；基类闪白仍在治疗后触发（可接受）
- **坚果**: 凡挂 `AnimatorController_Nut` 的单位治疗可回档
- **数据**: 仅天素罗 Prefab 序列化字段

## 已确认规则
| 项 | 结论 |
|----|------|
| 阈值 | >66%→0；≤66%且>33%→1；≤33%→2 |
| 参数名 | **Blend**（非默认 VisualTier） |
| 治疗 | **B**：同步；改动小则坚果一并修 |
| 直接 `Heal(float)`（如吸血） | 本迭代可不覆盖（无 OnGetDamage）；文档注明 |

## 技术要求
- **依赖**: AnimatorController_Nut、Property、天素罗 Prefab
- **Skills**: `@unity-2d-animation`、`@unity-prefab-system`
- **兼容**: 向后兼容；未挂 Nut 的单位不受阈值影响；Heal 时序微调影响面见影响面报告

## 功能清单
### 核心功能（必须）
- [ ] `Property.GetDamage` Heal：先治疗再 `OnGetDamage`
- [ ] 天素罗 Prefab → `AnimatorController_Nut` + Blend + 0.66/0.33
- [ ] 确认 Animator 存在 Float `Blend`

### 扩展（可选）
- [ ] 直接 `Heal(float)` 也回调外观同步
- [ ] 僵尸 Sample 治疗回档（非本需求）

## 验收标准
- [ ] 满血 Blend=0；打到 ≤66% 变为 1；≤33% 变为 2
- [ ] 治疗使比例回到更高档时 Blend 回升
- [ ] 到期转化素世逻辑不受影响
- [ ] 其他挂 Nut 的坚果：受伤破损 + 治疗回档

## 风险评估
| 风险点 | 概率 | 影响 | 应对 |
|-------|------|------|------|
| Animator 无 Blend 参数 | 中 | 低 | `HasParameter` 静默跳过；需配 Override |
| Heal 时序影响依赖旧顺序的逻辑 | 低 | 中 | 影响面回归：治疗闪白、Heal 类技能 |

## 关联 Context
- Chess / 天素罗需求 `docs/requirements/tiansuluo.md`
- `AnimatorController_Nut.cs`
