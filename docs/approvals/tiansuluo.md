# 需求开发审批报告

## 基本信息
- **需求名称**: 天素罗
- **所属模块**: Chess / Skill / Map
- **分析日期**: 2026-07-15

## 需求摘要
长崎素世异形态：50 阳光高防挡路；可配置倒计时（默认 180s，跟关卡时间）到期后自灭，同格生成完整长崎素世；南瓜罩同格可共存。

**需求类型**: 新增功能（新单位）  
**与现有关系**: 异形态身份对齐压力希；转化链路对齐气球僵尸落地；南瓜经 `Tile.OnPlant` 换绑

## 分析结果汇总

### Context
✅ 已读（2026-03-30）— Chess / Skill / Map / Manage

### 现有业务
- 相关：长崎素世、压力希、气球落地、南瓜罩、TimerManage
- 类型：新增单位 + 被动计时转化
- 影响：无破坏性核心改动；asset/Prefab 骨架已存在需补全

### 需求卡片
✅ `docs/requirements/tiansuluo.md`（用户已确认通过）

### Skills 白名单检查

已覆盖：
- ✅ 预制体 → `@unity-prefab-system`（已有 Prefab 接线）
- ✅ 数据配置 → `@unity-scriptableobject-config`（`PropertyCreator` asset）
- ✅ 计时 → 项目 `TimerManage`（非新 Unity API；协程 Skill 有覆盖时可参考）
- ✅ 状态机/棋子生命周期 → 现有 Chess Death / CreateChess

未覆盖：
- （无）本需求不引入拖拽/新物理/新 UI 系统

**结论**: 全部覆盖 / 通过

### 架构分析
✅ `docs/architecture/tiansuluo.md`  
- 模式：`ISkillEffect` 被动 + 一次性 Timer + OnRemove 条件生成（方案 A）  
- 风险等级：低～中

### 影响面分析
✅ `docs/architecture/tiansuluo-impact.md`  
- 影响文件：新增 1 + 改 asset/prefab；核心系统 0 改  
- 高风险点：0  
- 回归测试项：7

## 风险总评

| 维度 | 评级 | 说明 |
|------|------|------|
| 技术可行性 | 高 | 有落地先例 |
| 改动范围 | 小 | 单被动 + 配置 |
| 性能风险 | 低 | 单 Timer/单位 |
| 时间评估 | 2–4 小时 | Prefab 已生成 |

## 建议的 Skills 使用清单
- `@unity-prefab-system` — Prefab 被动接线
- `@unity-scriptableobject-config` — asset 字段补全
- 完成后可选 `@unity-code-review`

## 开发优先级建议
- **P0**: `PassiveSkillEffect_TianSuoLuo`（Timer + 成熟转化 + 停表）
- **P0**: `天素罗.asset` 身份/MainPlant/数值 + Prefab 挂被动与素世引用
- **P0**: 回归：到期转化、早死不刷、南瓜罩、暂停计时
- **P1**: 图鉴 / allChess / 转化特效

## 决策审批
✅ 通过 - 可以开始开发

审批意见: 用户确认通过  
日期: 2026-07-15

## 开发状态
✅ 已实现（2026-07-15）
- `PassiveSkillEffect_TianSuoLuo`：关卡时间倒计时 → 到期自灭 → 同格生成完整长崎素世；早死不转化；关闭 AttackAble
- `天素罗.asset`：`chessName=天素罗（长崎素世）`，`fetterMemberId=长崎素世`，tag Mygo+贝斯，MainPlant，price 50，Hp 1500
- Prefab：被动接线 + `soyorinCreator` → 长崎素世，`delaySeconds=180`，stateGraph 标准植物

**待你本地确认**：
1. 将 `天素罗.asset` 拖入场景 `GameManage.allChess`（若要进商店/选卡）
2. 测试时可将 Prefab 上 `delaySeconds` 临时改短
3. Hp 1500 为墙体占位，可按「小坚果」手感再调
