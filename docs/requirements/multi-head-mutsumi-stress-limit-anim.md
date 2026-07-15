# 功能需求卡片: 多首的怪物 · 压力阈值三档动画

## 基本信息
- **功能名称**: 多首的怪物 · 压力阈值三档动画
- **所属模块**: Skill（多首被动）/ Chess（AnimatorController）
- **需求类型**: 功能扩展
- **优先级**: P1
- **预估复杂度**: L1
- **预估耗时**: 0.5–1 小时
- **提出日期**: 2026-07-14

## 功能描述
### 详细描述
多首的怪物已有三套动画（idle/attack ×3）。每次出分身会降低「压力」Buff 的 `stressLimit`。当 **当前 `stressLimit`** 落入由初始 max 与 min 算出的两档分界时，调用 `AnimatorController.ChangeFloat(0/1/2)`（`Blend`）切换动画形态。

分界（`max = initialStressLimit`，`min = 50` / `stressPerClone`）：
- **高档线** = `(max − min) × 2/3 + min`（默认约 113.33）
- **低档线** = `(max − min) × 1/3 + min`（默认约 81.67）

### 用户故事
作为玩家，我希望多首随着分身越多、死亡阈值越低，外观越来越「狂」，以表现逼近紫砂。

## 现有业务上下文

### 相关现有功能
| 功能 | 模块 | 关系 | 说明 |
|------|------|------|------|
| `PassiveSkillEffect_MultiHeadMutsumi` | Skill | 扩展 | 出头时 `stressLimit -= 5` |
| `AnimatorController.ChangeFloat` | Chess | 复用 | `SetFloat("Blend", value)` |
| 三套 anim 资源 | Prefab | 依赖 | idle/2/3、attack/2/3；Controller 需用 `Blend` 切形态 |

### 需求类型判定理由
不改出头/复活规则，仅在 limit 变化时驱动动画档位。

### 集成点
- **触发**: `ReduceStressDeathLimit` 之后（及入场初始化）刷新档位
- **调用**: `_user.animatorController.ChangeFloat(tier)`

### 对现有功能的影响
- **行为**: 仅多首外观；出头/压力/射击不变
- **数据**: 无新 asset；Animator 侧需已有/将配 `Blend` 参数与三态

## 已确认规则

| 项 | 结论 |
|----|------|
| 驱动量 | **当前 `stressLimit`**（每次分身递减），**不是**当前压力值 |
| max | `initialStressLimit`（入场固定，默认 145） |
| min | `50`（`stressPerClone`；limit 低于此后再出头会紫砂） |
| 高档线 | `(max−min)×2/3 + min` |
| 低档线 | `(max−min)×1/3 + min` |
| ChangeFloat / Blend | `0 / 1 / 2` |
| 档位映射（limit 从高到低） | `limit > 高` → 0；`低 < limit ≤ 高` → 1；`limit ≤ 低` → 2 |
| 挂点 | 现有 `PassiveSkillEffect_MultiHeadMutsumi` |

默认数值示例（max=145, min=50）：
- limit ∈ (113.33, 145] → 0
- limit ∈ (81.67, 113.33] → 1
- limit ≤ 81.67 → 2

（约每头 -5：从 145 起，大约第 7 头附近进 1 档，第 13 头附近进 2 档。）

## 技术要求
- **依赖**: Skill、Buff、AnimatorController
- **规范**: 不新增运行时 MonoBehaviour
- **兼容**: 向后兼容多首出头逻辑

## 功能清单
### 核心功能（必须）
- [ ] 由 `initialStressLimit` + `stressPerClone` 计算两档分界
- [ ] 入场 / 每次降低 limit 后按当前 limit 调 `ChangeFloat`
- [ ] 离场无需特殊处理（棋子销毁即可）

### 扩展功能（可选）
- [ ] Animator Controller 配好 `Blend` 与三套切换（资源侧）

## 验收标准
- [ ] 入场 limit=145 → `ChangeFloat(0)`
- [ ] limit 降到 ≤高档线且 >低档线 → `ChangeFloat(1)`
- [ ] limit 降到 ≤低档线 → `ChangeFloat(2)`
- [ ] 出头/清压/复活/射击逻辑与改前一致
- [ ] 无分身时不因压力计数变化乱切档（只跟 limit）

## 风险评估
| 风险点 | 概率 | 影响 | 应对 |
|-------|------|------|------|
| Animator 无 `Blend` 参数 | 中 | 中 | 文档注明；ChangeFloat 无效果直至资源配好 |
| 边界用 `<=` 浮点 | 低 | 低 | 用 float 比较或先算再 int 比较 limit |

## 关联 Context
- Skill / Chess：`context/modules/`
- 参考：`docs/requirements/multi-head-mutsumi.md`
