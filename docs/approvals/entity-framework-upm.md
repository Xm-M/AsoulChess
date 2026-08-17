# 需求开发审批报告：Entity Framework UPM

## 基本信息
- **需求名称**: Entity Framework UPM（Chess 体系骨架）
- **所属模块**: `com.asoulchess.game.entity`
- **分析日期**: 2026-07-27
- **需求 ID**: `entity-framework-upm`

## 需求摘要

在 Core 之上新建 Entity Framework 包：实体生命周期 + Controller 组件化 + 属性/技能/Buff 骨架；Sample 演示最小战斗循环；**不含**格子地图与具体棋子；**不迁移** AVZ。

**需求类型**: 新增功能（工程基建）  
**与现有功能关系**: 参考 Chess/Skill/Buff 精简重写；与现网并行

## 分析结果汇总

### Context 复用
✅ Chess / Skill / Buff Context + 打包规划 + 已完成 Core

### 选型
| 项 | 值 |
|----|-----|
| 范围 | 框架 + 属性/技能/Buff 骨架 |
| AVZ | 选项 A（不迁） |
| 地图 | 第一期不做 |
| 包名（建议） | `com.asoulchess.game.entity` |

### 需求卡片
✅ `docs/requirements/entity-framework-upm.md`

### Skills白名单检查

已覆盖:
✅ 状态机 → `@unity-state-machine`  
✅ 对象池 → `@unity-object-pool`（经 Core）  
✅ 协程 → `@unity-coroutine-system`（实体上开协程，对齐技能规范）  
✅ 设计模式 → `@unity-design-patterns`（组件/观察者）  
✅ ScriptableObject（若做配置基类）→ `@unity-scriptableobject-config`

未覆盖:
⚠️ UPM 多包依赖编排 — 查官方文档  
⚠️ 完整「战斗框架」无单一 Skill — 以本架构报告为准

检查结论: **通过（可继续）**

### 架构分析
✅ `docs/architecture/analysis/entity-framework-upm.md`  
风险: 中（范围控制是关键）

### 影响面分析
✅ 玩法脚本影响: **0**  
回归: 确认 AVZ 仍可 Play；新包可编译

## 风险总评

| 维度 | 评级 | 说明 |
|------|------|------|
| 技术可行性 | 中高 | 模式清晰，实现量大 |
| 改动范围（本期） | 小（对 AVZ） | 仅新包 |
| 性能风险 | 低 | 未接局内 |
| 时间评估 | 5～10 天 | A 级竖切 |

## 建议 Skills（开发时）

- `@unity-state-machine`
- `@unity-object-pool` / Core README
- `@unity-design-patterns`
- 项目规则：技能勿 AddComponent 运行时行为脚本

## 开发优先级

- **P0**: GameEntity + Property(HP/Death) + 最小 State + Sample 受击死亡  
- **P1**: Skill 骨架 + Buff/TimedBuff（Core Timer）  
- **P2**: Attack 示意、IBoardOccupant 占位、更多文档对照表  

## 决策审批

✅ **通过** - 可以开始开发（本仓 Packages，不迁 AVZ）  
⬜ 修改后通过 - 需要调整方案  
⬜ 驳回 - 暂不开发

审批意见: 用户确认继续 Monorepo；范围2 + A + 无地图  
日期: 2026-07-27
