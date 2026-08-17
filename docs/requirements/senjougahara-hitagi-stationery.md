# 功能需求卡片: senjougahara-hitagi-stationery

## 基本信息
- **功能名称**: 战场原黑仪 — 随机文具弹 + 压力连发/体型
- **所属模块**: Chess / Skill / Bullet / Buff / Weapon
- **需求类型**: 功能扩展（半成品做完）
- **优先级**: P1（打卡 8/20「战场原黑仪能种」）
- **预估复杂度**: L2
- **预估耗时**: 4–6 小时（含 Prefab 接线；美术占位可）
- **提出日期**: 2026-08-17

## 功能描述
### 详细描述
战场原黑仪为压力体系远程植物（阳光 225）。

**普攻**：每次攻击从四种文具中**等概率**随机发射 **1** 发；若因压力需要多发，则在首发之后按可配置**间隔**陆续再发（不一口气齐射）。

**被动**：额外弹数与体型随**当前压力**双向同步：
- `extraShots = floor(stress / 20)`
- `Size = 1 - floor(stress / 20)`（**允许负数**；仅改属性 Size，不改 Transform.scale）
- 压力上升 / 下降时，额外弹数与体型一并重算

**文具**（均为远程弹，伤害走攻击力；元素/效果如下）：

| 文具 | 伤害标记 | 特殊效果 |
|------|----------|----------|
| 铅笔 | `ElementType.Puncture`（穿刺） | 无；默认命中 1 次 |
| 尺子 | `ElementType.Cutting`（切割） | `MaxHitNum=3`（最多伤 3 个单位） |
| 订书机 | 常规弹 | 命中时对目标施加 `DizznessBuff` **0.5s** |
| 橡皮擦 | 常规弹 | 命中叠「擦除」Buff；**满 5 层**目标直接死亡 |

无主动技能。

### 用户故事
作为玩家，我希望种下黑仪后用随机文具输出，并靠压力叠连发与缩体型，以便融入压力体系构筑。

## 现有业务上下文

### 相关现有功能
| 功能 | 模块 | 关系 | 说明 |
|------|------|------|------|
| `ShootBullet` / 对象池弹 | Weapon/Bullet | 相似 | 单发模板；本需求扩展为随机池 + 间隔连发 |
| `Buff_StressBuff_Death` | Buff | 依赖 | 压力读写 / 叠压 |
| `DizznessBuff` | Buff | 复用 | 订书机停顿 |
| `PropertyController.ChangeSize` | Chess | 需扩展 | 当前下限 clamp 1，黑仪需允许负数 |
| 老仓育压力被动 | Skill | 相似 | 入场挂压、事件驱动 |

### 需求类型判定理由
已有 `战场原黑仪.asset` / Prefab 壳，属半成品收尾，非新模块。

### 已确认决策
| 项 | 结论 |
|----|------|
| Q1 | 「池子」→ **尺子** |
| Q2 | 四种**等概率** |
| Q3 | 累计 `floor(stress/20)`；**压力减少时弹数也减少** |
| Q4 | 额外弹与普攻同一随机池 |
| Q5 | 初始 Size=1，可减到**负数** |
| Q6 | **只改属性 Size**，不缩 Scale |
| Q7 | 停顿 = `DizznessBuff` 0.5s |
| Q8 | **无主动** |
| Q9 | 脚本 + Prefab/Asset 接线；弹图可占位 |
| 连发 | **有间隔**，非齐射 |

### 集成点
- **调用**: `ObjectPool.Create`、`Bullet.InitBullet`、`TakeDamage`、压力 Buff、`DizznessBuff`
- **触发**: 普攻 `IAttackFunction.Attack`；压力 `BuffReset` / stress 变化回调
- **需扩展**: Size 写入允许负数（黑仪专用 API 或 `ChangeSize` 可选下限，避免误伤全局）

### 对现有功能的影响
- **接口变更**: `PropertyController` 增加「允许负 Size」写入路径（或黑仪旁路）
- **行为变更**: 负 Size 影响碾压判定（`GetSize` 比较）；需回归大体型交互
- **数据变更**: Asset 填攻/射程/名；Prefab 挂武器、被动、四弹、攻击模板 stateGraph

## 技术要求
- **依赖模块**: Skill, Bullet, Buff, Weapon, Chess
- **性能**: 连发用协程/Timer 间隔；单次攻击弹数 ≈ 1+stress/20（压力 100 → 6 发量级）
- **兼容性**: 向后兼容；不改全局弹基类行为（子类/effect 扩展）
- **约束**: 技能逻辑不新增运行时 MonoBehaviour（用 `ISkillEffect` + 事件 + `user.StartCoroutine`）

## 功能清单
### 核心
- [ ] 随机文具攻击（等概率 + 间隔连发）
- [ ] 四文具 Prefab/效果（穿刺 / 切割 MaxHit3 / 晕 0.5s / 擦除 5 层斩杀）
- [ ] 被动：stress↔extraShots↔Size 双向同步（Size 可负）
- [ ] Asset/Prefab 接线，测试关能种能打

### 可选
- [ ] 文具权重表（本期不做）
- [ ] 体型视觉缩放（本期不做）

## 验收标准
- [ ] 测试关能种、不缺关键引用
- [ ] 每次攻击随机文具；压力 0 只 1 发
- [ ] stress=20/40 时额外 +1/+2，发弹有间隔
- [ ] stress 从 40→15 时额外弹变回 0，Size 回到对应值
- [ ] 尺子最多伤 3 目标；订书机晕 0.5s；橡皮擦 5 层斩杀
- [ ] Size 可为负；Scale 不变

## 风险评估
| 风险点 | 概率 | 影响 | 应对 |
|--------|------|------|------|
| 全局 `ChangeSize` 下限 1 | 高 | 中 | 黑仪专用 SetSize，不改默认 clamp |
| 负 Size 碾压逻辑 | 中 | 中 | 回归 Timoris/碾压相关 |
| 高压连发过多 | 低 | 中 | 间隔可配；可选 maxExtra 上限（默认不设） |
| 齐射误实现 | 中 | 低 | 强制协程间隔 |

## 关联 Context
- 涉及模块: Chess, Skill, Buff
- 参考: `context/modules/Skill.md`, `docs/game-design/plant-roster.md`
