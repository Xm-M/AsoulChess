# 需求开发审批报告

## 基本信息
- **需求名称**: 揭幕喵梦
- **所属模块**: Chess / Skill / Buff / UI
- **分析日期**: 2026-07-13

## 需求摘要
祐天寺若麦异形态灰烬棋子：种植后立刻对 3×3（含自己）友军各施加 25 压力并在友军位置产阳（可配，默认 25）；仅共享 AveMujica 身份；无武器；Death 由技能动画结束事件触发。

**需求类型**: 新增功能  
**与现有关系**: 种植进技能对齐史尔特尔；身份对齐压力希异形态模式；压力/产阳复用现有 API

## 分析结果汇总

### Context
✅ 已读 — Skill / Chess / Buff / AveMujica

### 现有业务
- 相关：史尔特尔 `EnterWarState=SkillState`、压力协议、产阳、`祐天寺若麦` Member
- 类型：新增功能
- 影响：不改若麦本体；不用 `ConsumablesSkill` 自动 Death

### 需求卡片
✅ `docs/requirements/unveil-nyamu.md`

### Skills 白名单
- ✅ 预制体 `@unity-prefab-system`
- ✅ SO 配置 `@unity-scriptableobject-config`
- ✅ 状态机 `@unity-state-machine`（现有 SkillState）
- ✅ UI（产阳走现有 ItemPanel，非新 UI 面板）

**未覆盖**: 无关键 Unity API  
**结论**: 通过

### 架构
✅ `docs/architecture/unveil-nyamu.md`  
- `ColdSkill` + `ISkillFireUseSkillOnEnter` + `SkillEffect` 3×3  
- 风险：低～中

### 影响面
✅ `docs/architecture/unveil-nyamu-impact.md`  
- 新增为主；回归约 7 项

## 风险总评

| 维度 | 评级 | 说明 |
|------|------|------|
| 技术可行性 | 高 | 全可复用 |
| 改动范围 | 小 | 1～2 脚本 + Prefab |
| 性能风险 | 低 | 一次性扫 ≤9 格 |
| 时间评估 | 2–4 小时 | 动画/立绘可后补 |

## 建议 Skills
- `@unity-prefab-system` / `@unity-scriptableobject-config`
- 完成后可选 `@unity-code-review`

## 开发优先级
- P0: SkillEffect（3×3 加压+产阳）+ 进态立刻 UseSkill（不自动 Death）
- P0: PropertyCreator 身份字段 + Prefab 接线
- P1: allChess / 动画 Death 事件（用户）/ 特效

## 决策审批
✅ 通过 - 可以开始开发

审批意见: 用户确认通过  
日期: 2026-07-13

## 开发状态
✅ 已实现（2026-07-13）
- `ColdSkill_FireOnEnter`：进 SkillState 立刻 UseSkill；`SkillOver` 不 Death
- `SkillEffect_UnveilNyamu`：3×3（含己）友军加压 + 产阳（`baseDamage[0]`，默认 25）
- PropertyCreator：`chessName=揭幕喵梦（祐天寺若麦）`，`fetterMemberId=祐天寺若麦`，tag AveMujica
- Prefab / SkillConfig / allChess 已接线
- **待用户**：技能动画结束帧挂 Death 事件；立绘/专属动画可后补（当前暂用若麦 controller）
