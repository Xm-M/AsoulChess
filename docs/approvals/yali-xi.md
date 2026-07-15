# 需求开发审批报告

## 基本信息
- **需求名称**: 压力希
- **所属模块**: Chess / Skill / Bullet / Buff
- **分析日期**: 2026-07-13

## 需求摘要
椎名立希异形态远程棋子：邻格友军攻击时给该友军压力 +1；驻留 DoT 弹（首击后停点每秒伤，N 秒可配）；仅共享 MyGO 身份（`fetterMemberId=椎名立希`），不继承立希战斗技能。

**需求类型**: 新增功能  
**与现有关系**: 加压协议对齐友希那/老仓育；身份对齐灯异形态模式

## 分析结果汇总

### Context
✅ 已读（2026-03-30）— Skill / Buff / Chess

### 需求卡片
✅ `docs/requirements/yali-xi.md`（用户已确认）

### Skills 白名单
- ✅ 预制体 `@unity-prefab-system`
- ✅ SO 配置 `@unity-scriptableobject-config`
- ✅ 对象池 `@unity-object-pool`（子弹）
- ✅ 协程/计时（项目 TimerManage，非新增未知 API）

**结论**: 通过

### 架构
✅ `docs/architecture/yali-xi.md`  
- 被动邻格订阅 + `Bullet_LingerDot`  
- 风险：低～中

### 影响面
✅ `docs/architecture/yali-xi-impact.md`  
- 新增文件为主；立希无改动  
- 回归项 7

## 风险总评

| 维度 | 评级 |
|------|------|
| 技术可行性 | 高 |
| 改动范围 | 中（新子弹类） |
| 性能风险 | 低 |
| 时间评估 | 4–8 小时 |

## 建议 Skills
- `@unity-prefab-system` / `@unity-object-pool` / `@unity-scriptableobject-config`
- 完成后可选 `@unity-code-review`

## 开发优先级
- P0: 邻格加压被动（订阅安全）
- P0: 驻留 DoT 子弹
- P0: PropertyCreator 身份字段 + 远程接线
- P1: allChess / 资源抛光

## 决策审批
✅ 通过 - 可以开始开发

审批意见: 用户确认通过  
日期: 2026-07-13

## 开发状态
✅ 已实现（2026-07-13）
- `PassiveSkillEffect_YaliXi`：邻格友军 OnAttack → 加压 +1
- `Bullet_LingerDot`：首击驻留、每秒跳伤、换目标、时长可配（默认 3s）
- PropertyCreator：`chessName=压力希（椎名立希）`，`fetterMemberId=椎名立希`，tag Mygo
- Prefab / 驻留弹 / allChess 已接线
