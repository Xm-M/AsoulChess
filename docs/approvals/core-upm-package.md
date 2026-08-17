# 需求开发审批报告：Core UPM 包

## 基本信息
- **需求名称**: 实现 Core UPM 包（事件/计时/对象池）
- **所属模块**: `com.asoulchess.game.core`
- **分析日期**: 2026-07-27
- **需求 ID**: `core-upm-package`

## 需求摘要

在 AVZ 仓库 `Packages/` 下新增本地 UPM 包，精简实现事件总线、计时服务、对象池与服务入口，附 Sample 与 README；**不修改 AVZ 玩法代码**。

**需求类型**: 新增功能（工程基建）  
**与现有功能的关系**: 参考 EventController / TimerManage / ObjectPool，并行存在、本期不替换

## 分析结果汇总

### Context 复用
✅ Context + 已审批规划 `reusable-systems-packaging`

### 选型确认
| 项 | 值 |
|----|-----|
| 位置 | Monorepo `Packages/com.asoulchess.game.core/` |
| AVZ | 选项 A：不改玩法 |
| 包名 | `com.asoulchess.game.core` |

### 需求卡片
✅ `docs/requirements/core-upm-package.md`

### Skills白名单检查

已覆盖:
✅ 对象池 → `@unity-object-pool`  
✅ 预制体 → `@unity-prefab-system`（Sample 用）  
✅ 协程/帧更新相关可参考 → `@unity-coroutine-system`（Timer 也可用 Update）  
✅ 设计模式 → `@unity-design-patterns`（观察者、对象池）

未覆盖:
⚠️ UPM `package.json` / 本地包布局 — 无专项 Skill → 实现时按 Unity Package Manager 官方文档  
⚠️ asmdef — 无专项 Skill → 按官方 Assembly Definition 指南；Runtime 不引 Odin

检查结论: **通过（少量未覆盖，可继续）**

### 架构分析
✅ `docs/architecture/analysis/core-upm-package.md`  
- 模式: 精简重写 + 命名空间 + 接口  
- 风险: 低（选项 A）

### 影响面分析
✅ 新增包目录；**玩法脚本影响文件数: 0**  
- 高风险点: 0（本期）  
- 回归: 确认 AVZ 仍能进 Play；包能编译

## 风险总评

| 维度 | 评级 | 说明 |
|------|------|------|
| 技术可行性 | 高 | 规划已铺路；范围清晰 |
| 改动范围 | 小 | 仅 Packages + 文档 |
| 性能风险 | 低 | 不进现网热路径 |
| 时间评估 | 1～3 天 | 含 Sample/README |

## 建议的Skills使用清单

- `@unity-object-pool`
- `@unity-prefab-system`
- `@unity-design-patterns`
- Unity 官方 UPM / asmdef 文档

## 开发优先级建议

- **P0**: package.json + asmdef + EventBus + TimerService + GameObjectPool + 服务入口
- **P1**: Samples~ 最小演示 + README
- **P2**: 泛型池 / Editor 工具（可砍）

## 决策审批

✅ **通过** - 可以开始开发  
⬜ 修改后通过 - 需要调整方案  
⬜ 驳回 - 暂不开发

审批意见: 用户确认 Monorepo + 选项 A + com.asoulchess.game.core  
日期: 2026-07-27
