# 需求开发审批报告：战斗框架竖切

## 基本信息
- **需求名称**: 数值–战斗–移动框架竖切（Board + Entity 加厚）
- **分析日期**: 2026-07-28
- **需求 ID**: `combat-framework-vertical-slice`

## 需求摘要

在 AVZ 仓库 Packages 中新建 Board、加厚 Entity，交付空项目可运行的「格子双单位互殴」Sample；不迁移 AVZ；不搬具体内容。

**需求类型**: 新增 + Entity 扩展  
**选型**: 竖切 Demo / Monorepo / 不迁 AVZ（用户 1/1/A）

## 分析结果汇总

### Context
✅ Chess / Map / State；已有 Core + Entity 0.1（空项目已测通）

### 需求卡片
✅ `docs/requirements/combat-framework-vertical-slice.md`

### Skills白名单

已覆盖:
✅ 状态机 → `@unity-state-machine`  
✅ 对象池（Core）→ `@unity-object-pool`  
✅ 设计模式 → `@unity-design-patterns`  
✅ 2D/格子概念可参考 → `@unity-tilemap`（本包用自研格，非必须 Unity Tilemap）

未覆盖:
⚠️ UPM 多包依赖 — 官方文档  
⚠️ 自研 Grid（非 Tilemap）— 以架构报告为准  

结论: **通过**

### 架构
✅ `docs/architecture/analysis/combat-framework-vertical-slice.md`  
风险: 中（范围控制）

### 影响面
✅ 玩法脚本 **0**；仅 Packages  

## 风险总评

| 维度 | 评级 |
|------|------|
| 可行性 | 中高 |
| 对 AVZ 改动 | 无 |
| 工期 | 5～10 天 |

## 开发优先级

- **P0**: Board 占位 + Move + Attack + 状态衔接 + Sample 互殴  
- **P1**: IAnimBridge、README、Entity 0.2 版本号  
- **P2**: 邻格索敌优化、更多状态  

## 建议 Skills
`@unity-state-machine`、`@unity-design-patterns`、Core/Entity README

## 决策审批

✅ **通过** - 可以开始开发  
⬜ 修改后通过  
⬜ 驳回  

审批意见: 用户确认竖切 Demo / Monorepo / 不迁 AVZ  
日期: 2026-07-28
