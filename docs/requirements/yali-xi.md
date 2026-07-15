# 功能需求卡片: 压力希

## 基本信息
- **功能名称**: 压力希
- **所属模块**: Chess / Skill / Bullet / Buff（压力）
- **需求类型**: 新增功能
- **优先级**: P1
- **预估复杂度**: L2
- **预估耗时**: 4–8 小时（含驻留 DoT 弹）
- **提出日期**: 2026-07-13

## 功能描述
### 详细描述
新增棋子「压力希」：椎名立希的**另一形态**（仅共享 MyGO **身份**，不继承立希被动射程 / 恐惧波主动）。自身为**远程射击**；子弹命中后在命中点驻留，按可配置时长每秒造成伤害。被动：每当**上下左右邻格友军攻击**时，对该友军施加「压力」Buff（无则挂、有则 `BuffReset`，每次 +1）。自身不挂压力。

### 用户故事
作为玩家，我希望用立希形态位上的压力希远程输出，并在邻格队友开火时给他们叠压力，同时用驻留弹持续压线。

## 现有业务上下文

### 相关现有功能
| 功能 | 模块 | 关系 | 说明 |
|------|------|------|------|
| 凑友希那加压 | Skill | 相似（方向相反） | 己方攻击→邻格加压；压力希是邻格攻击→给攻击者加压 |
| `OkuwakiStressSpread.ApplyStressDelta` | Skill | 复用 | Ensure + BuffReset |
| `PassiveSkill_Mygo` / MyGO 羁绊 | Skill/Fetter | 身份集成 | Member 用 `fetterMemberId`；效果用 tag `Mygo` |
| 椎名立希 | Chess | 同成员位 | 不复制其被动/主动，仅 `fetterMemberId` 对齐 |
| 企鹅/宇宙高松灯 | Chess | 参考 | 异形态：`chessName` 不同，`fetterMemberId` 同为「高松灯」 |
| `Bullet` 命中回收 | Bullet | 需扩展 | 现有命中后常直接回收，需驻留 DoT 子弹 |

### 需求类型判定理由
全新棋子 + 新被动 + 新驻留弹行为；压力协议与射击管线复用现有实现。

### 命名与身份（已确认）
| 字段 | 建议值 | 说明 |
|------|--------|------|
| 显示名 `chessName` | **压力希（椎名立希）** | 形态展示；与常服立希区分 |
| `fetterMemberId` | **椎名立希**（必须完全一致） | MyGO `detectMode=Member` 名单匹配；与立希同场只算 1 人 |
| `plantTags` | **Mygo**（+ 可选鼓手，自填） | 羁绊 Buff / CD 减半依赖 tag |

> 是的：显示名用「压力希（椎名立希）」很合适；**千万不要**把 `fetterMemberId` 写成带括号的显示名，否则点不亮 Member 名单。

> 注意：`PassiveSkill_Mygo` 邻格去重用的是 `chessName`（灯有 Contains 归一，立希没有）。压力希与常服立希若同场邻接，会算两个不同 `chessName`；Member 点亮仍只算 1。本形态通常不同时上场，可接受。

### 集成点
- **被动**: 扫描四邻格友方，订阅其 `equipWeapon.OnAttack`；触发时对该友方 `ApplyStressDelta(+1)`；自身入场/离场维护订阅
- **攻击**: `Weapon_Sample` + `ShootBullet` + 直线/格子索敌
- **子弹**: 新 Bullet（或等价）：命中立即造成 1 次伤害并停移；之后每秒对当前锁定目标造成 ATK×rate；目标死亡/消失后等待下一碰撞敌人接锁；持续 `lingerDuration` 秒后回收

### 对现有功能的影响
- **接口**: 无破坏性核心改动；新增 Bullet/被动类
- **行为**: 不改立希本体；MyGO 名单因同 `fetterMemberId` 与立希互斥计数
- **数据**: `压力希.asset` + Prefab + 驻留弹 Prefab；可选 allChess 注册

## 已确认规则

| 项 | 结论 |
|----|------|
| 邻格 | 上下左右四格 |
| 被动 | 邻格友军攻击 → 给该友军压力 +1（Ensure + BuffReset） |
| 自身压力 | 不挂 |
| 立希战斗效果 | **不继承**（无 Taki 被动射程、无恐惧波） |
| 立希身份 | `fetterMemberId=椎名立希` + tag Mygo |
| 显示名 | `压力希（椎名立希）` |
| 远程 | 是 |
| 子弹首击 | 命中立刻 1 次伤害 |
| 驻留 | 停在命中位置；每秒 1 次伤；伤害 = ATK × 倍率（可配，默认 1） |
| 换目标 | 当前目标死亡/消失后，下一碰到的敌人接上 |
| 持续时长 N | Prefab 可配 |

## 技术要求
- **依赖**: Skill、Buff、Chess、Weapon、Bullet、Map、ObjectPool、Timer
- **规范**: `ISkillEffect`，禁止技能专用运行时 MonoBehaviour；离场卸邻格 `OnAttack` 订阅
- **兼容**: 向后兼容立希与现有压力 Buff 子类

## 功能清单
### 核心功能（必须）
- [ ] 被动：邻格友军 `OnAttack` → 加压 +1；订阅维护与清理
- [ ] 驻留 DoT 子弹（首击 + 每秒 + 换目标 + 时长可配）
- [ ] 远程武器接线
- [ ] PropertyCreator：`chessName` / `fetterMemberId` / Mygo tag
- [ ] Prefab 基本配置（动画/立绘可后补）

### 扩展功能（可选）
- [ ] 鼓手 tag
- [ ] allChess 注册
- [ ] 邻格去重改为读 `fetterMemberId`（若要与立希同场归一，另开需求）

## 验收标准
- [ ] 邻格友军每次攻击，该友军压力 +1；敌方/自身不受被动加压
- [ ] 对角格不触发；离场后不再加压
- [ ] 子弹命中立刻伤一次，驻留每秒伤，N 秒后消失
- [ ] 目标死后，下一碰撞敌人继续吃跳伤
- [ ] MyGO Member：`fetterMemberId=椎名立希` 可点亮；与常服立希同名额
- [ ] 不拥有立希恐惧波 / 四人加射程被动

## 风险评估
| 风险点 | 概率 | 影响 | 应对 |
|-------|------|------|------|
| 邻格 OnAttack 订阅泄漏 | 中 | 中 | OnRemove / 邻格离场成对卸监听 |
| 驻留弹与对象池状态残留 | 中 | 中 | Init 时重置计时/目标/移动态 |
| 同场立希+压力希邻格计数异常 | 低 | 低 | 文档注明；通常不同时上场 |

## 关联 Context
- Skill / Buff / Chess：`context/modules/`
- 参考：`docs/requirements/minato-yukina.md`（加压协议）
