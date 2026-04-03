# AsoulChess 项目 Context

**生成日期**: 2026-04-02
**版本**: 1.0.0
**分析工具**: project-context-analyzer

## 项目概述

AsoulChess 是一个 Unity 2D 自走棋类游戏项目，采用组件化架构设计，支持棋子（Chess）的战斗、羁绊、Buff、关卡等核心系统。

## 快速导航

### 整体概览
- [项目概览](./overview.md) - 项目整体架构和统计信息
- [依赖图谱](./architecture/dependency-graph.md) - 模块依赖关系
- [架构决策](./architecture/decisions.md) - 关键架构决策记录

### 模块详情
| 模块 | 复杂度 | 依赖数 | 文档 |
|------|--------|--------|------|
| GameManage | 高 | 8个 | [链接](./modules/GameManage.md) |
| Chess | 高 | 6个 | [链接](./modules/Chess.md) |
| LevelSystem | 高 | 4个 | [链接](./modules/LevelSystem.md) |
| Buff | 中 | 3个 | [链接](./modules/Buff.md) |
| UI | 中 | 3个 | [链接](./modules/UI.md) |
| Event | 低 | 1个 | [链接](./modules/Event.md) |

## 关键信息

### 核心模块（按重要性排序）
1. **GameManage** - 全局游戏管理器，单例模式，管理所有子系统
2. **Chess** - 棋子核心类，组件化设计，包含属性/技能/状态/Buff等控制器
3. **LevelSystem** - 关卡系统，控制游戏流程和关卡切换
4. **EventController** - 全局事件系统，解耦模块间通信

### 高风险依赖
- **循环依赖**: Chess ↔ BuffController ↔ Buff（通过事件解耦）
- **强耦合**: 所有模块都依赖 GameManage 单例
- **性能风险**: 大量使用 UnityEvent，注意内存泄漏

### 技术栈
- Unity 2022+ (推测)
- Sirenix Odin Inspector（编辑器扩展）
- Pixel UI（第三方UI框架）
- 对象池模式（性能优化）

### 后续开发建议
- 新功能优先通过 EventController 通信，避免直接引用
- 棋子新能力通过继承 Controller 实现
- 数据配置使用 ScriptableObject

---

*本 Context 由 project-context-analyzer 自动生成，建议定期更新*
