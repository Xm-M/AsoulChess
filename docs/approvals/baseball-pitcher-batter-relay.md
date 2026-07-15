# 需求开发审批报告：棒球投手 × 挥棒手站位连招（BB-001）

## 基本信息

- **需求名称**：Baseball Pitcher–Batter Relay
- **需求 ID**：BB-001
- **所属模块**：Skill / Weapon / Chess
- **分析日期**：2026-07-07

## 需求摘要

两个 plant 被动：**投手**默认远程、**挥棒手**默认近战。当挥棒手落在投手攻击范围（**独立友方检测**）且投手自身索敌范围内有敌人时，投手**喂球**（零伤害）；挥棒手接球后**仅本次**远程出球并回近战。与羁绊无关；多投手可喂；挥棒手攻击中忽略新球；不排队（D1）。

**需求类型**：功能扩展  
**与现有功能的关系**：扩展 `ISkillEffect` + `IFindTarget` + 专用 `Bullet_PitchBall`，不改动 `Fetter`

## 分析结果汇总

### Context 复用

- 已读取 `context/index.md`（v2.0.0）
- 涉及模块：Skill、Chess、Weapon
- 参考：`context/modules/Skill.md`、`context/architecture/decisions.md`

### 用户确认决策

| 项 | 决策 |
|----|------|
| 远程切换 | 接球后仅本次攻击，打完回近战 |
| 无敌人 | B1：投手自身 enemy 索敌为 0 → 不攻击、不喂球 |
| 多投手 | 均可喂；挥棒手 AttackState 中忽略新球（C1：投手仍发，球丢弃） |
| 伤害 | 喂球零伤害 |
| 持续喂球 | 是 |
| 羁绊 | 无关 |
| 范围 | 攻击范围内（非固定 1 格）；**独立友方 FindBatterInRelayRange** |
| 多球 | D1：不排队，攻击结束后只响应下一颗 |

### 需求卡片

- 已生成 → `docs/requirements/baseball-pitcher-batter-relay.md`

### Skills 白名单检查

| 技术点 | Skill | 结论 |
|--------|-------|------|
| OverlapBox / Raycast 友方层 | unity-2d-physics | ✅ |
| ShootBullet / 对象池 | unity-object-pool | ✅ |
| 动画层切换 | unity-2d-animation | ✅ |
| SerializeReference 被动 | unity-scriptableobject-config | ✅ |
| AttackState FSM | unity-state-machine | ✅ |

**结论**：通过

### 架构分析（摘要）

| 决策 | 选择 | 理由 |
|------|------|------|
| 被动形态 | `ISkillEffect` + 事件 | 项目规范，参考 MinerZombie |
| 敌人门控 | 复用现有 `weapon.findTarget` | B1 与玩家直觉一致 |
| 挥棒手检测 | **独立友方通道** | StraightFindTarget 不扫友军（用户 A） |
| 格子投手 | 复用 `IGridFindTarget.relativeCells` + 友方层 | 与攻击范围一致 |
| 射线投手 | `GetAttackRange()` + 友方 RaycastAll/BoxCast | fallback |
| 喂球 | `Bullet_PitchBall` 零伤害 | 不改通用 Bullet 敌我逻辑 |
| 临时远程 | 内存换武器字段 + Attack 结束恢复 | 参考 WisadelBurstMode |
| 连招队列 | 无队列（D1） | 实现简单、避免 burst |

**设计模式**：策略（双 IFindTarget）+ 观察者（onGetDamage / WhenPlantChess）

**风险等级**：中（友方子弹 + 强制攻击时序）

### 影响面分析

**新增文件（约 5～7）**

| 路径 | 说明 |
|------|------|
| `BaseballRelayHelper.cs` | 标签、敌人门控、友方范围 |
| `FindTarget_BaseballPitcherRelay.cs` | 投手合并索敌 |
| `FindBatterInRelayRange.cs` | 友方挥棒手扫描（Grid / Ray 两路） |
| `PassiveSkillEffect_BaseballPitcher.cs` | 投手被动 |
| `PassiveSkillEffect_BaseballBatter.cs` | 挥棒手被动 |
| `Bullet_PitchBall.cs` | 喂球弹 |

**修改现有文件**：无必须修改；可选 `ElementType` 扩展。

**回归测试项**

- [ ] 普通远程 plant（StraightFindTarget）行为不变
- [ ] 普通近战 plant 行为不变
- [ ] 仅投手 / 仅挥棒手 / 连招 / 无敌人 / 攻击中喂球 / 多投手
- [ ] 读档后挥棒手不在半套远程状态（若 mid-attack 存档则按现有 SkillContext 策略）

**高风险点（2）**

1. 临时换 `attackFunction` 未在 Exit 恢复 → **缓解**：AttackState.Exit + OnRemove 双保险  
2. Grid 与 Straight 投手范围不一致 → **缓解**：Helper 内分支，Inspector 文档说明

## 风险总评

| 维度 | 评级 | 说明 |
|------|------|------|
| 技术可行性 | 高 | 有 Wisadel / MinerZombie / Grid 先例 |
| 改动范围 | 小～中 | 新增为主 |
| 性能风险 | 低 | 索敌频率与普攻一致 |
| 时间评估 | 1～2 天 | 含 Prefab 接线 |

## 建议的 Skills 使用清单

- `@unity-2d-physics` — 友方 OverlapBox / Raycast
- `@unity-object-pool` — 喂球弹
- `@unity-state-machine` — AttackState 强制切入
- `@unity-2d-animation` — 远程动画层（若需要）

## 开发优先级建议

**P0**

1. `BaseballRelayHelper` + `FindBatterInRelayRange`（Grid + Straight 两路）
2. `FindTarget_BaseballPitcherRelay` + 敌人门控
3. `Bullet_PitchBall` + 投手被动
4. 挥棒手被动（临时远程 + 恢复 + D1/C1）

**P1**

- 两 plant Prefab、tag、VFX

**P2**

- Editor Gizmo、击球倍率 SO 调优

## 决策审批

✅ **通过** — 可以开始开发  
⬜ **修改后通过** — 需调整方案  
⬜ **驳回** — 暂不开发  

**审批意见**：用户确认 A1/B1/C1/D1 + 独立友方索敌；2026-07-07 通过  
**日期**：2026-07-07
