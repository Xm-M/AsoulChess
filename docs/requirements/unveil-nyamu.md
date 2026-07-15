# 功能需求卡片: 揭幕喵梦

## 基本信息
- **功能名称**: 揭幕喵梦
- **所属模块**: Chess / Skill / Buff（压力）/ UI（阳光）
- **需求类型**: 新增功能
- **优先级**: P1
- **预估复杂度**: L1～L2
- **预估耗时**: 2–4 小时
- **提出日期**: 2026-07-13

## 功能描述
### 详细描述
新增棋子「揭幕喵梦」：祐天寺若麦的**另一形态**（仅共享 AveMujica **身份**，不继承若麦战斗技能）。无武器/普攻。种植后立即进入技能态并释放技能：对自身所在格为中心的 **3×3**（含自己）内所有**友军**各施加 **25** 点压力，并在每名友军位置生成阳光（数量可配，默认 **25**）。空格不产阳。技能动画结束由动画 `Death` 事件回收自身（灰烬型）。

### 用户故事
作为玩家，我希望种下揭幕喵梦后立刻给 3×3 友军叠高压并掉阳，作为若麦形态位的一次性爆发/支援。

## 现有业务上下文

### 相关现有功能
| 功能 | 模块 | 关系 | 说明 |
|------|------|------|------|
| 史尔特尔 | Chess/Skill | 相似 | `EnterWarState=SkillState`，种植进技能；效果靠 `UseSkill`；动画收尾 |
| 喜多 | Chess/Skill | 相似 | 灰烬型种植放技能 |
| `ConsumablesSkill` | Skill | 参考但不用其 Death | `ISkillFireUseSkillOnEnter`；`SkillOver` 会 `Death()`——本需求 Death 由动画事件负责，避免双死亡 |
| `OkuwakiStressSpread.ApplyStressDelta` | Skill | 复用 | Ensure + BuffReset 加压 |
| `SkillEffect_CreateSunLight` / `SkillEffect_AnonHide` | Skill | 复用思路 | `ItemPanel.Create<SunLight>()` + `InitSunLight` |
| `OkuwakiGridHelper.CollectNeighbor8Tiles` | Skill | 部分复用 | 现有实现**不含中心格**；本需求需 3×3 **含自己** |
| 祐天寺若麦 | Chess/Fetter | 同成员位 | AveMujica Member 名单已有「祐天寺若麦」 |

### 需求类型判定理由
全新棋子 + 新主动效果；种植进技能态、压力协议、产阳管线均复用现有实现。

### 命名与身份（已确认）
| 字段 | 值 | 说明 |
|------|-----|------|
| 显示名 `chessName` | **揭幕喵梦（祐天寺若麦）** | 与常服若麦区分 |
| `fetterMemberId` | **祐天寺若麦** | 与 Member 名单完全一致；同场与若麦互斥计 1 |
| `plantTags` | **AveMujica**（+ 可选鼓手等，自填） | Fever / 羁绊效果依赖 tag |

### 集成点
- **状态**: `EnterWarState = SkillState`（种植后立刻播技能）
- **主动**: `ColdSkill`（或薄封装）+ `ISkillFireUseSkillOnEnter` → 进技能态立刻 `UseSkill`
- **效果**: 新 `ISkillEffect`：扫 3×3（含己）友军 → 加压 + 脚下产阳
- **死亡**: Prefab 技能动画结束帧挂 `Death`（用户负责）；代码**不**在 `SkillOver` 再 `Death()`

### 对现有功能的影响
- **接口**: 无破坏性改动；新增 SkillEffect（+ 可选薄 ColdSkill）
- **行为**: 不改若麦本体；AveMujica Member 因同 `fetterMemberId` 与若麦互斥计数
- **数据**: 揭幕喵梦 Prefab / `.asset`；可选 allChess

## 已确认规则

| 项 | 结论 |
|----|------|
| 技能后自身 | 消失；动画结束 `Death` 事件（用户加） |
| 身份 | `chessName=揭幕喵梦（祐天寺若麦）`，`fetterMemberId=祐天寺若麦`，tag AveMujica；不继承若麦战斗技能 |
| 3×3 | **包含自己**（保底至少自身 25 阳光） |
| 阳光量 | 可配置，默认 25 |
| 压力量 | 25（建议 Effect 字段可配，默认 25） |
| 空格 | 只对格子上实际友军生效；空格不产阳 |
| 武器 | **不需要** |

## 技术要求
- **依赖**: Skill、Buff、Chess、Map、ItemPanel/SunLight
- **规范**: `ISkillEffect`，禁止技能专用运行时 MonoBehaviour
- **兼容**: 向后兼容若麦与现有压力 Buff

## 功能清单
### 核心功能（必须）
- [ ] `SkillEffect_*`：3×3（含己）友军 → 压力 + 阳光
- [ ] 种植进 `SkillState` + 进态立刻 `UseSkill`（不 `SkillOver` 自动 Death）
- [ ] PropertyCreator：显示名 / `fetterMemberId` / AveMujica tag
- [ ] Prefab 基本配置（动画 Death 事件用户后补）

### 扩展功能（可选）
- [ ] allChess 注册
- [ ] 技能特效 Prefab

## 验收标准
- [ ] 种植后立刻释放技能（无需手动点技能）
- [ ] 3×3 含自身：自身必得压力 25 + 阳光（默认 25）
- [ ] 范围内其他友军各得压力 25 + 各一颗阳光；空格无阳光
- [ ] 阳光数量可在 Prefab/配置改；默认 25
- [ ] 无武器/普攻
- [ ] AveMujica Member：`fetterMemberId=祐天寺若麦` 可点亮；与常服若麦同名额
- [ ] 不拥有若麦原战斗技能
- [ ] 动画结束 Death 后正常回收，无双 Death / 残留不可选

## 风险评估
| 风险点 | 概率 | 影响 | 应对 |
|-------|------|------|------|
| 与 ConsumablesSkill 双 Death | 中 | 中 | 不用其 `SkillOver→Death`；Death 仅动画 |
| 3×3 漏中心格 | 中 | 中 | 自写含中心枚举，勿直接用 CollectNeighbor8 |
| 压力 Buff 模板未挂 | 低 | 中 | Prefab 挂 guestStressBuff 模板 |
| 动画未挂 Death | 低 | 中 | 验收检查；文档注明用户补事件 |

## 关联 Context
- Skill / Buff / Chess：`context/modules/`
- 参考：`docs/requirements/yali-xi.md`（异形态身份）、史尔特尔种植进技能
