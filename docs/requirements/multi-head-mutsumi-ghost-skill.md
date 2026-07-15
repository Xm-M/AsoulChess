# 功能需求卡片: 多首的怪物 · 幽灵召唤主动技

## 基本信息
- **功能名称**: 多首的怪物 · 幽灵召唤主动技
- **所属模块**: Skill / Chess / Weapon / Buff
- **需求类型**: 功能扩展
- **优先级**: P1
- **预估复杂度**: L2
- **预估耗时**: 1–2 天（含幽灵 Prefab 联调）
- **提出日期**: 2026-07-15

## 功能描述
### 详细描述
多首的怪物新增**主动技能**：开启后将当前压力清零，消灭全部分身头，并按开技前头数 N 召唤 N 个**无占格、不可选中、不可被攻击**的轻量幽灵棋子。幽灵全屏找最近敌人，追击至攻击范围后近战持续攻击。开技后睦每秒压力 +X（默认 1，可配置），直到死亡；期间不再出新头、不再降低 `stressLimit`（保持开技时 limit）。**终局后本体禁止普攻与再次使用主动技**（仅保留 Blend=3 待机外观 + 加压自毁）。

### 用户故事
作为玩家，我希望把叠好的分身头一次性化为幽灵军团爆发清场，并以自毁式加压作为终局代价。

## 现有业务上下文

### 相关现有功能
| 功能 | 模块 | 关系 | 说明 |
|------|------|------|------|
| `PassiveSkillEffect_MultiHeadMutsumi` | Skill | 扩展 | 出头/清头/紫砂/Resume |
| `MultiHeadShootAttack` | Weapon | 并存 | 开技灭头后分身射击自然失效 |
| `Buff_StressBuff_Death` | Buff | 依赖 | 清压、每秒加压、紫砂 |
| 魂灵之影 | Chess | **不采用占格** | 仅参考轻量召唤思路 |
| Tomo / GBC 加压 Timer | Skill | 复用 | `timerManage` + `BuffReset` |

### 需求类型判定理由
在已有多首被动上增加主动技与召唤物，不新开单位线。

### 集成点
- **调用**: `chessFactory` + `AddChess`（不占格）、`UnSelectable`、`CloseAttack`、压力 Buff
- **触发**: 主动 `ColdSkill`、主人 `OnRemove`/死亡清幽灵
- **新接口**: `SkillEffect_MultiHeadGhostBurst`、`PassiveSkillEffect_MultiHeadGhost`、`FindTarget_NearestEnemyInRange`、Keys 终局锁

### 对现有功能的影响
- **接口**: Keys / 被动增加终局锁；清头 API 可复用
- **行为**: 仅多首开技后改变出头与加压节奏
- **数据**: 幽灵 PropertyCreator + Prefab；多首挂主动技

## 已确认规则

| 项 | 结论 |
|----|------|
| 幽灵形态 | **轻量 Chess**（非子弹、非占格魂灵之影） |
| 0 头 | **不可开技** |
| 灭头后 | 不再出新头、不再减 limit；limit **保持** |
| 幽灵 AI | 各自全屏最近敌 → 追击 → 进距**近战**持续 |
| 加压 | 开技后每秒 +X，默认 **1**，可配置 |
| 清场 | 睦死亡/离场 → 全部幽灵销毁 |

## 技术要求
- **依赖**: Skill、Chess、Weapon、Buff、ChessFactory、Timer
- **规范**: `ISkillEffect`，禁止技能专用运行时 `AddComponent` MonoBehaviour
- **兼容**: 向后兼容未开技的多首行为

## 功能清单
### 核心功能（必须）
- [ ] 主动技：有头才可放；清压、灭头、锁被动、召唤 N 幽灵、启动每秒加压
- [ ] 幽灵：入队不占格、`UnSelectable`、追击近战
- [ ] 主人死亡清幽灵
- [ ] Prefab / Creator 接线

### 扩展功能（可选）
- [x] 幽灵软分离（追击/围殴时互推，避免叠模；不用 Rigidbody2D）
- [ ] 独立美术与音效

## 验收标准
- [ ] 0 头无法开技；≥1 头可开
- [ ] 开技后压力为 0、头全无、不再长头
- [ ] 幽灵数 = 开技前头数；不占格、不可铲、僵尸打不到
- [ ] 幽灵会追敌并近战
- [ ] 开技后压力按配置每秒上涨，可紫砂；limit 不回升
- [ ] 睦死幽灵全清

## 风险评估
| 风险点 | 概率 | 影响 | 应对 |
|-------|------|------|------|
| CreateChess 占地 | 高 | 中 | 走 factory + AddChess，不 ChessEnter |
| 多幽灵同追一人 | 中 | 低 | 一期接受 |
| Prefab 缺美术 | 中 | 低 | 临时复用占位 Sprite |

## 关联 Context
- 涉及模块: Skill, Chess, Weapon, Buff
- 参考: `docs/requirements/multi-head-mutsumi.md`
