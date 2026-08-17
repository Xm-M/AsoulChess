# 需求开发审批报告 — Chess/Entity P1-2 Buff

## 基本信息

- **需求名称**: Buff 与 BuffController
- **分析日期**: 2026-08-10
- **用户确认**: Buff 系统很通用，可以按 P1-2 做

## 需求摘要

框架侧实现通用 Buff 挂载/刷新/定时/改属性；具体 Buff 与存档留 AVZ。

**需求类型**: 功能重构 | **风险**: 中低（仅 Packages）

## 决策

✅ **通过** — 框架 P1-2 已实现（2026-08-10）

## 下一步

- Unity 验证 Entity Demo
- **P1-3** StateController
- 可选 **P1-2b**: AVZ Buff Shim（`BuffEffect` → `OnApply` 适配）
