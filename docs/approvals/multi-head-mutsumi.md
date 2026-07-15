# 需求开发审批报告

## 基本信息
- **需求名称**: 多首的怪物（黄瓜睦第二条升级线）
- **所属模块**: Skill / Chess / Weapon
- **分析日期**: 2026-07-10

## 需求摘要
黄瓜睦并列升级形态：压力满 50 清零并加无实体分身头；攻击时按行序多发射击，分身弹从各自 Sprite 出发；**跨行弹先斜飞入目标行再直线**（需新增弹道，现有库无）；有分身时可 Mortis 式吞头复活。

**需求类型**: 新增功能  
**与现有功能**: LevelUp + 压力 + ShootBullet 模式扩展；并列 Mortis

## 分析结果汇总

### Context
✅ 已读 `context/index.md` — Skill / Chess / Buff

### 需求卡片
✅ `docs/requirements/multi-head-mutsumi.md`

### Skills白名单
| 技术点 | 结论 |
|--------|------|
| ISkillEffect / 被动 | ✅ 项目规范 + Skill 模块 |
| IAttackFunction / 子弹 | ✅ 现有 ShootBullet 模式 |
| 协程/Timer（Resume） | ✅ |
| 对象池子弹 | ✅ `@unity-object-pool` |
| UI | 非必须 |

**结论**: 通过

### 架构 / 影响面
✅ `docs/architecture/multi-head-mutsumi.md`  
✅ `docs/architecture/multi-head-mutsumi-impact.md`  
- 风险: 中  
- 主要新增文件，少改旧逻辑

## 风险总评

| 维度 | 评级 |
|------|------|
| 技术可行性 | 高 |
| 改动范围 | 小–中 |
| 性能 | 低 |
| 时间 | 1–2 天 |

## 决策审批
✅ **通过** - 可以开始开发  
⬜ 修改后通过  
⬜ 驳回  

审批意见: 用户确认通过（含分身各自发射点 + 跨行先斜后直弹道）  
日期: 2026-07-10

## 开发备注（2026-07-10 已落地脚本）

代码已进工程（Prefab / PropertyCreator 需在 Editor 配置）：

| 文件 | 说明 |
|------|------|
| `PassiveSkillEffect_MultiHeadMutsumi.cs` | 压力清零+分身头、吞头 Resume |
| `MultiHeadMutsumiKeys.cs` | 行序 / Context 键 |
| `MultiHeadShootAttack.cs` | 分身各自起点发射 |
| `BulletMove_DiagonalThenLane.cs` | 先斜后直 |

**Editor 待办**：新建「多首的怪物」PropertyCreator（LevelUp→黄瓜睦）、Prefab 挂被动与 `MultiHeadShootAttack`、分身头 Prefab、跨行弹 Prefab（`bulletMove = DiagonalThenLane`）。

**上限规则补丁**：每 +1 分身 `stressLimit -= 5`（入场默认 145 → 20 头后 45，再涨压紫砂）。
