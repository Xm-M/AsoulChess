# 需求开发审批报告：初雪冰道踩入伤害

## 基本信息
- **需求名称**: 初雪冰道踩入伤害
- **需求 ID**: ICE-HATSUYUKI-001
- **所属模块**: Effect / Map / Skill（圣聆初雪）
- **分析日期**: 2026-07-06

## 需求摘要
仅**圣聆初雪**铺出的冰格在**铺冰初雪存活**期间，对**该初雪的敌对阵营**造成**踩入一次**伤害：`攻击力 × 系数`。初雪死亡后冰不伤人；多只初雪不叠加；与冰格 Player/Enemy 归属无关；P0 无减速、无 Boss 特例。

**需求类型**: 功能扩展  
**与现有功能的关系**: 基于 `Effect_Snow` + `PassiveSkillEffect_Hatsuyuki` 扩展进格结算

## 用户定稿记录（2026-07-06）

| 项 | 决定 |
|----|------|
| 生效冰 | 仅初雪铺的冰 |
| 阵营 | 与冰格 ownerTag 无关；目标 = `GetEnemyTeam(铺冰初雪.tag)` |
| 触发 | 进入冰格一次伤害（`OnReachTile` + 原地铺冰 `tile.stander` 补刀） |
| 初雪离场 | 死亡后冰不伤人 |
| 多只初雪 | 每格一次伤害，不叠加 |
| 伤害 | `GetAttack() × iceEnterDamageCoeff` |
| 减速 | P0 不做 |
| Boss | 无额外配置 |
| 启动 | 初雪在场（Register/Unregister），无关卡插件开关 |
| 实现 | `Effect_Snow` 集中 `WhenChessEnterWar` + `OnReachTile` 监听 |

## 分析结果汇总

### Context 复用
- ✅ 已读取 `context/index.md`
- 涉及模块: Map、Skill、Manage、Buff
- 参考: `context/modules/Map.md`、`context/modules/Skill.md`

### 现有业务分析
- **相关功能**: `Effect_Snow`、`IceCell`、`PassiveSkillEffect_Hatsuyuki`、冰车被动、开局雪插件
- **集成点**: `OnReachTile`、`WhenChessEnterWar`、`GetEnemyTeam`、`GetAttack`、`GetDamage`

### 需求卡片
- ✅ 已生成 → `docs/requirements/ice-hatsuyuki-enter-damage.md`

### Skills 白名单检查

**已覆盖**:
- ✅ 事件驱动 → `@unity-2d-game-development` / `EventController`、`UnityEvent`
- ✅ 对象池 / 关卡回收 → `@unity-object-pool`（`WeatherManage.RecycleAll`）
- ✅ 预制体配置 → `@unity-prefab-system`（`Effect_Snow` 预制体系数可在 Skill 序列化字段）

**未覆盖**: 无（本需求不涉及新 Unity API）

**结论**: ✅ 通过

### 架构分析
- ✅ 已生成 → `docs/architecture/ice-hatsuyuki-enter-damage.md`
- **设计模式**: 观察者（进格事件）+ 格子上来源标记
- **风险等级**: 低

### 影响面分析

| 类别 | 内容 |
|------|------|
| 直接影响文件 | 3（`Effect_Snow.cs`、`IceCell.cs`、`PassiveSkillEffect_Hatsuyuki.cs`） |
| 间接影响 | 初雪 Prefab/Asset 被动系数；敌方初雪若存在则受益于 `ownerTag` 修复 |
| 无影响 | 冰车、开局雪、融化、种植 UI |
| 高风险点 | 监听泄漏（有缓解方案） |
| 回归测试 | ① 初雪在场僵尸进冰 ② 初雪死后进冰 ③ 冰车冰无伤 ④ 双初雪同格不叠伤 ⑤ 冰铺脚下 ⑥ 关卡结束无泄漏 |

## 风险总评

| 维度 | 评级 | 说明 |
|------|------|------|
| 技术可行性 | 高 | 与现有 `OnReachTile` / `WhenChessEnterWar` 模式一致 |
| 改动范围 | 小 | 约 3 个脚本，~120 行量级 |
| 性能风险 | 低 | 进格 O(1)，无周期扫场 |
| 时间评估 | 2–4h | L1 |

## 建议的 Skills 使用清单
- `@unity-2d-game-development` — 事件与棋子移动
- `@unity-design-patterns` — 观察者模式参考
- 开发后 `@unity-code-review`

## 开发优先级建议
- **P0**: `IceCell` 来源、`Effect_Snow` 监听与结算、初雪被动 Register + 系数 + `ownerTag` 修复
- **P1**: 减速 Buff、种植到初雪冰上的补结算（若验收需要）
- **P2**: 初雪 `chessEffect` 文案更新

## 决策审批
- ✅ **通过** — 可以开始开发
- ⬜ 修改后通过 — 需要调整方案
- ⬜ 驳回 — 暂不开发

**审批意见**: 用户确认通过（2026-07-06）  
**日期**: 2026-07-06
