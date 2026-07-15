# 功能需求卡片: 多首的怪物（黄瓜睦第二条升级线）

## 基本信息
- **功能名称**: 多首的怪物
- **所属模块**: Chess / Skill / Weapon（攻击） / Buff（压力）
- **需求类型**: 新增功能
- **优先级**: P1
- **预估复杂度**: L2
- **预估耗时**: 1–2 天（含 Prefab/动画联调）
- **提出日期**: 2026-07-10

## 功能描述
### 详细描述
「多首的怪物」为**黄瓜睦的第二条升级路线**（与 Mortis 并列）。被动：压力每达到 50 → **清零压力并增加 1 个分身**（有上限）。分身无实体：本行随机左右位置的 Sprite+动画。本体攻击时，本体 + 全部分身各射一发；分身弹道按「本行 → 邻行 → 更远行」分配，行盖满后循环。受致命伤时若有分身则消耗 1 个分身并进入 Resume 不可选中后复活（对齐 Mortis）。

### 用户故事
作为玩家，我希望把黄瓜睦升成多首形态，用外部压力叠分身、像三线射手一样多行输出，并在有分身时获得类似 Mortis 的免死机会。

## 现有业务上下文

### 相关现有功能
| 功能 | 模块 | 关系 | 说明 |
|------|------|------|------|
| 黄瓜睦 | Chess | 升级基底 | `LevelUpPlant.basePlant` |
| Mortis / `Passive_Mujica_Mortis` | Skill | 并列升级 / 复活参考 | 濒死 Resume；多首不可种在 Mortis 上（basePlant 校验自然互斥） |
| `Buff_StressBuff_Death` + `stress` | Buff | 依赖 | 外部压力来源 |
| `ShootBullet` / `StraightFindTarget` | Weapon | 扩展 | 多首用自定义 `IAttackFunction` 多发行弹 |
| 维什戴尔 Shadow | Skill | **不采用** | 真棋子占格，与「无实体」不符 |

### 需求类型判定理由
全新棋子机制 + 新被动/攻击函数；复用升级种植、压力、Resume，不改全局压力规则。

### 集成点
- **调用**: `LevelUpPlant`、`Chess.UnSelectable`/`ResumeState`、`ObjectPool` 子弹、压力 Buff
- **触发**: 压力变更、`onGetDamage`、武器 `Attack`
- **新接口**: `PassiveSkillEffect_*`、`MultiHeadShootAttack`（名可定）、分身视觉 helper

### 对现有功能的影响
- **接口**: 无破坏性变更
- **行为**: 新增可种植升级卡；Mortis / 黄瓜睦原逻辑不变
- **数据**: 新 PropertyCreator + Prefab + 分身头 Prefab

## 已确认规则

| 项 | 结论 |
|----|------|
| 升级关系 | 与 Mortis **并列**；只能种在黄瓜睦上，不能种在 Mortis 上 |
| 压力 | `stress >= 50` → **只加 1 分身** + **stress = 0** |
| 分身表现 | 本行；单 Sprite；左右范围随机；有动画；无 Collider/Chess |
| 攻击 | 本体攻击时全部分身同步攻击（三线射手式） |
| 弹道行 | **按行序**：本行 → ±1 → ±2…，盖满后循环 |
| 发射点 | **分身弹从各自 Sprite 世界坐标发射**，不得全部共用本体 `weaponPos` / 单一 shootPos；本体弹仍用原 shootPos |
| 跨行轨迹 | **PVZ 三线射手式**：从本格（分身所在左右位置）出发 → **斜飞进入目标行** → 再沿该行直线前进。项目现无此 `IBulletMove`，需新增 |
| 复活 | 对齐 Mortis：`onGetDamage` 拦截 + `ResumeState` + 消耗 1 分身 |
| 上限 | 每成功 +1 分身 → 「压力」Buff.`stressLimit` **-5**；入场阈值默认 **145** → 约 **20** 头后为 **45**；再涨到 50 时 `stress > limit` 紫砂（第 21 次不再出头） |
| 压力来源 | 仅外部，不自涨压 |

## 技术要求
- **依赖模块**: Skill、Weapon、Buff、Chess、ObjectPool
- **性能**: 分身数有上限；攻击时额外射线/弹道次数 = cloneCount
- **兼容性**: 向后兼容；不影响 Mortis
- **规范**: `ISkillEffect`，禁止技能专用运行时 `AddComponent` MonoBehaviour（视觉可用普通子物体/池化 Prefab）

## 功能清单
### 核心功能（必须）
- [ ] PropertyCreator + LevelUp 配置（base=黄瓜睦）
- [ ] 被动：压力阈值、清零、cloneCount、上限
- [ ] 分身头视觉生成/销毁/离场清理
- [ ] `MultiHeadShootAttack`：1+N 发，行序索敌；**第 i 发分身弹 `InitBullet` 起点 = 第 i 个分身头 Transform.position**
- [ ] **跨行弹道** `BulletMove_DiagonalThenLane`（名可定）：斜入目标行 Y 后再直线；同行分身弹可用普通直线
- [ ] 致命伤吞分身 + Resume 复活
- [ ] Prefab 挂被动与攻击函数

### 扩展功能（可选）
- [ ] 分身头独立开火口位置
- [ ] 该行无敌人时分身空射开关
- [ ] UI 显示当前分身数

## 验收标准
- [ ] 只能种在黄瓜睦上；Mortis 上不可种
- [ ] 压力到 50：分身 +1，压力变 0；100 一次也只 +1 并清零
- [ ] 每 +1 头 `stressLimit -= 5`；20 头后 limit=45，再满 50 压力紫砂且不再出头
- [ ] 分身仅本行随机 X，有动画，无实体碰撞
- [ ] 攻击时弹道行序正确，盖满循环
- [ ] 多分身同时开火时，子弹起点分别落在各分身 Sprite 处（非同一 shootPos）
- [ ] 打邻行/更远行时：先斜飞到目标行高度，再直线前进（非出生即在邻行、也非纯抛物线乱飞）
- [ ] 有分身时致死 → 掉 1 头并 Resume；无分身正常死
- [ ] 铲除/离场分身全清

## 风险评估
| 风险点 | 概率 | 影响 | 应对 |
|-------|------|------|------|
| 上限满后压力仍清零导致「浪费」压力 | 中 | 低 | 验收时确认；可改为满上限不清零 |
| 多行索敌与地图行边界 | 低 | 中 | helper 夹紧 map Y |
| 跨行弹道缺失 | 高（已确认） | 中 | 新增两阶段 `IBulletMove`；勿误用纯 `IBulletMoveRight` |

## 关联 Context
- 涉及模块: Skill, Chess, Weapon, Buff
- 参考: Mortis 被动、压力 Buff、ShootBullet
