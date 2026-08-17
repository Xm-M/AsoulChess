# 需求开发审批报告 — AVZ L0 基础设施

## 基本信息

- **需求名称**: AVZ 全量逻辑框架导出 — Phase 0（L0）
- **所属模块**: `Packages/com.asoulchess.game.core` + AVZ Manage 适配
- **分析日期**: 2026-08-10

## 需求摘要

将 Event、Physics2D 查询缓冲池、ObjectPool、Timer、Audio（精简）、UI 壳、SceneLoad 七项迁入 `com.asoulchess.game.core`，AVZ 通过 `GameManage` 统一访问并保留内容模块（Weather/Fetter/Prop/Save 等）在游戏侧。

**需求类型**: 功能重构 + 工程基建  
**与现有功能的关系**: 合并已有 Core 0.1 包与 AVZ Manage 实现，渐进替换

## 分析结果汇总

### Skills 白名单检查

| 技术点 | Skill | 结论 |
|--------|-------|------|
| SceneManager 异步加载 | unity-scene-management | ✅ |
| GameObject 对象池 | unity-object-pool | ✅ |
| Physics2D NonAlloc | unity-2d-physics | ✅ |
| AudioSource 播放 | unity-audio-system | ✅ |
| Canvas / View UI | unity-ui-system | ✅ |
| 协程 / 延时 | unity-coroutine-system | ✅ |

**结论**: 全部覆盖，可进入开发。

### 架构决策

- **包**: 扩展 `com.asoulchess.game.core` → `0.2.0`
- **入口**: `GameServices` + `GameManage.instance` 字段
- **AVZ Shim**: `AvzAudioPlayer`（Unique 系列）、SceneManage 过场 Animator/ClockDemo
- **内容模块**: 不进包

### 影响面（L0 迁移）

| 模块 | 引用量级 | 风险 |
|------|----------|------|
| ObjectPool | ~60+ | 中 — 保持 `Create/Recycle` API |
| TimerManage | ~40+ | 中 — 保持 `Timer` 类 API |
| EventController | 全局 | 低 — 保持 `Instance` API |
| CheckObjectPool | ~20 | 低 |
| SceneManage | ~10 | 中 — 保留 MonoBehaviour 过场 |
| AudioManage | 中 | 中 — 去 Event 监听，保留 Limit dedup |

## 决策审批

✅ **通过** — 开始 L0 实现（用户确认 2026-08-10）

审批意见: L0 七项；统一 GameManage 访问；内容模块排除  
日期: 2026-08-10
