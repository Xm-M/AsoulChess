# 功能需求卡片: 天素罗

## 基本信息
- **功能名称**: 天素罗
- **所属模块**: Chess / Skill（被动计时转化）/ Map（占格）
- **需求类型**: 新增功能（新单位）
- **优先级**: P1
- **预估复杂度**: L1–L2
- **预估耗时**: 2–4 小时（Prefab/asset 已有骨架，补配置 + 被动）
- **提出日期**: 2026-07-15
- **来源**: `docs/待做-单位想法.md` §4

## 功能描述
### 详细描述
新增棋子「天素罗」：长崎素世的**另一形态**。种植后表现为**小坚果同款**高防挡路单位（不攻击）；入场后启动**可配置倒计时**（默认 180s，跟关卡 `TimerManage` / 游戏时间）。到期后本体**自灭**，并在**同一格子**生成完整配置的**长崎素世**（现有 `长崎素世.asset` / Prefab）。同格南瓜罩（Support）可继续共存，不视为「格子被占」。

### 用户故事
作为玩家，我希望用低价（50）先种一堵墙挡线，等够时间后自动换成正式素世，用时间换高价值前排。

## 现有业务上下文

### 相关现有功能
| 功能 | 模块 | 关系 | 说明 |
|------|------|------|------|
| 长崎素世 | Chess | 转化目标 | 到期后 `CreateChess` 用现有 Creator |
| 压力希 / 揭幕喵梦 | Chess | 异形态先例 | `chessName` 不同，`fetterMemberId` 对齐本体 |
| 气球僵尸落地 | Skill | 相似（死→同格生成） | `PassiveSkillEffect_BalloonZombiePop`：`OnRemove` + `CreateChess` |
| 南瓜罩 | Map/Skill | 同格 Support | `PlantType.SupportPlant`；`Tile.stander` 仅 Main |
| `TimerManage.AddTimer` | Manage | 计时 | 非循环 delay；受 `timeScale`/关卡时间影响 |

### 需求类型判定理由
全新可种植单位 + 一条被动「到期自灭并生成素世」；不改核心种植规则，复用 `CreateChess` 与异形态身份字段。

### 命名与身份（已确认）
| 字段 | 建议值 | 说明 |
|------|--------|------|
| 显示名 `chessName` | **天素罗（长崎素世）** | 与常服素世区分 |
| `fetterMemberId` | **长崎素世**（必须完全一致） | MyGO Member 名单；同场与素世互斥计 1 |
| `plantTags` | **Mygo**（+ 可选贝斯） | 对齐素世羁绊 tag |
| `plantType` | **MainPlant** | 占 `Tile.stander` |
| 阳光 `price` | **50** | 草稿已定；asset 已写 50 |

### 集成点
- **调用的现有接口**:
  - `GameManage.instance.timerManage.AddTimer(onFinish, delay, false)` — 到期回调
  - `Chess.Death` / 离场链路 — 自灭
  - `ChessTeamManage.CreateChess(素世Creator, tile, "Player")` — 同格生成
  - `Tile.stander` / `PlantChess` — Main 占格切换
- **触发的现有事件**: 种植 `OnPlant`、死亡 `OnRemove`（南瓜罩等监听者随之换绑）
- **需要的新接口**: `PassiveSkillEffect_TianSuoLuo`（或等价命名）——入场开表、到期转化、离场停表

### 对现有功能的影响
- **接口变更**: 无破坏性核心改动
- **行为变更**: 无；仅新单位
- **数据变更**: 补全 `天素罗.asset` / Prefab 被动与数值；引用 `长崎素世` Creator

## 已确认规则

| 项 | 结论 |
|----|------|
| 生成目标 | **完整**现有「长崎素世」配置（非简化版） |
| 同格南瓜罩 | **允许**；Support 不挡 Main 转化。流程：天素罗自灭 → 同格 Create 素世；罩留在格上 |
| 倒计时 | Prefab/被动字段**可配置**，默认 180s |
| 时间基准 | **关卡/游戏时间**（`TimerManage`，受暂停/`timeScale`） |
| 资源范围 | 机制开发；**Prefab 已生成**（`Assets/Prefab/ChessPrefab/Mygo/天素罗/`），补接线与数值 |
| 提前战死 | **不生成素世**（仅到期转化；若需「死也变」另议） |

## 技术要求
- **Unity / 依赖**: Chess、Skill（`ISkillEffect`）、Map/Tile、TimerManage、ChessTeamManage
- **规范**: 被动用 `ISkillEffect`，禁止技能专用运行时 `AddComponent` MonoBehaviour；`OnRemove` 必须 `Stop` Timer
- **性能**: 单 Timer / 单位，可忽略
- **兼容性**: 向后兼容；不改素世本体
- **平台**: 与现项目一致

## 功能清单
### 核心功能（必须）
- [ ] 补全 `天素罗.asset`：名称、描述、`fetterMemberId`、tags、MainPlant、高防无攻数值、Prefab 引用
- [ ] Prefab 挂被动：可配置 delay → 到期自灭 → 同格生成长崎素世
- [ ] 离场/提前死亡：停 Timer，且不生成素世
- [ ] 同格南瓜罩回归：转化后罩仍护新 Main（依赖现有南瓜逻辑）

### 扩展功能（可选）
- [ ] UI/进度提示（剩余时间）
- [ ] 转化特效/音效
- [ ] 写入图鉴 / allChess 池 / 构筑池（若当前流程需要）

## 验收标准
- [ ] 种植后为挡路高防、不攻击；阳光 50
- [ ] 到期（可配秒数）后本体消失，同格出现完整长崎素世
- [ ] 有南瓜罩时转化后罩仍在、且护素世
- [ ] 被僵尸咬死 / 铲除：不刷素世；Timer 无泄漏
- [ ] 暂停游戏时倒计时不空转（与 TimerManage 行为一致）
- [ ] MyGO 羁绊：`fetterMemberId=长崎素世` 点亮正确；与常服素世同场只计 1 人
- [ ] 代码通过审查；需求文档完整

## 风险评估
| 风险点 | 概率 | 影响 | 应对措施 |
|-------|------|------|---------|
| 自灭与 Create 同帧顺序导致 stander/南瓜订阅错乱 | 中 | 中 | 先 Death/Leave 再 Create；测南瓜重绑 |
| asset 骨架字段为空（当前 `chessName`/`plantType` 未填） | 高 | 中 | 开发首步对齐压力希/素世 asset 填全 |
| 「小坚果」无现成同名单位，数值靠猜 | 中 | 低 | 以高 Hp、0 攻、挡路为准，策划再调（当前草稿 asset Hp=300） |
| 提前死亡误刷素世 | 低 | 高 | 仅 Timer 回调置「允许转化」标志 |

## 关联 Context
- 涉及模块: Chess, Skill, Map, Manage
- 参考文档: `context/modules/Chess.md`, `context/modules/Skill.md`
- 草稿来源: `docs/待做-单位想法.md`
- 制作流程: `docs/单位制作.md`
- 异形态先例: `docs/requirements/yali-xi.md`, `docs/requirements/unveil-nyamu.md`
