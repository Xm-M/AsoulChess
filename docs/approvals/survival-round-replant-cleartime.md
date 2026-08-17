# 需求开发审批报告

## 基本信息
- **需求名称**: survival-round-replant-cleartime
- **所属模块**: LevelSystem / Endless / SaveSystem
- **分析日期**: 2026-08-14

## 需求摘要
生存轮界与读档统一为「模拟重载」：轮末 ClearTime + 无 Buff 快照；选卡前展示种植；GameStart 插件后全灭再重种以重挂 Buff/Timer。生存存档不保存 Buff，修复读档 `Buff_Mujica_Doloris` NRE。

**需求类型**: 功能重构 + Bug修复  
**与现有功能的关系**: 重构 Endless 轮界生命周期；收窄生存存档植物字段

## 分析结果汇总

### Context 复用
✅ 已读取项目 Context（2026-03-30）
- 涉及模块: LevelSystem, Buff, Manage, Chess
- 参考: `context/modules/LevelSystem.md`, `Buff.md`, `Manage.md`

### 现有业务分析
- **相关功能**: 轮末 Timeline/选卡、RestorePlayerPlants、ClearTime、GameStart 插件
- **判定**: 重构 + Bug 修复
- **集成点**: ClearTime → 展示种 → 插件 → 静默灭 → 进战重种
- **影响**: 生存每轮重建植物实例；不恢复存档 Buff

### 需求卡片
✅ `docs/requirements/survival-round-replant-cleartime.md`

### Skills白名单检查
已覆盖:
✅ 协程 → unity-coroutine-system（轮末流程）
✅ 对象池/棋子复用 → unity-object-pool / 现有 ChessTeamManage
✅ 存档 → unity-save-system
✅ Timeline 现有信号流（不新增 Timeline API）

未覆盖: 无阻塞项  
**结论**: 通过

### 架构分析
✅ `docs/architecture/survival-round-replant-cleartime.md`
- 模式: 快照 + 双次种植 + ClearTime
- 风险: 中（插件时机 / 静默移除）

### 影响面分析
✅ `docs/architecture/survival-round-replant-cleartime-impact.md`
- 影响文件: ~4–6
- 高风险点: 静默灭植物、羁绊与重种顺序
- 回归项: 见影响面文档

## 风险总评

| 维度 | 评级 | 说明 |
|------|------|------|
| 技术可行性 | 高 | 复用现有种植/ClearTime |
| 改动范围 | 中 | Endless + Save 生存分支 |
| 性能风险 | 低 | 每轮一次全场重种 |
| 时间评估 | 4–6h | 含回归 |

## 建议的Skills使用清单
- @unity-save-system — 生存快照字段
- @unity-coroutine-system — 轮末/GameStart 顺序
- @unity-object-pool — 棋子回收创建

## 开发优先级建议
- P0: ClearTime + 生存无 buff 快照 + 展示种 + 插件后灭再种
- P0: 读档同路径；忽略 buffs
- P1: `Buff_Mujica_Doloris` null 防护；静默移除工具
- P2: 展示种减特效噪音

## 决策审批
✅ 通过 - 可以开始开发  
⬜ 修改后通过 - 需要调整方案  
⬜ 驳回 - 暂不开发

审批意见: 用户确认通过；HP 保留快照值；冒险 Buff 存读不变  
日期: 2026-08-14
