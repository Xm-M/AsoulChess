# 需求开发审批报告 · RG-009

## 基本信息
- **需求名称**: 肉鸽战斗「提前进下一波」可选按钮
- **所属模块**: LevelSystem / UI
- **分析日期**: 2026-05-20

## 需求摘要

肉鸽普关战斗在 ProgressBar 旁增加「下一波」按钮：**mintime 后才显示**，清场后可点进波；**保留自动进波**，按钮仅为可选项。仅肉鸽 + 基类 `LevelController`；w9/w19 与最后一波、Boss/Snake 等无按钮。

**需求类型**: 功能扩展

## 分析结果汇总

### Context 复用
✅ 已读取 `context/index.md` — LevelSystem、UI 模块

### 设计决策（已确认）
| 项 | 决策 |
|----|------|
| 自动进波 | **保留**，不改为全手动 |
| 按钮定位 | mintime 后的 **可选项** |
| 显隐 | `t ≤ mintime` 隐藏；`t > mintime` 显示；进波后隐藏 |
| 范围 | **仅肉鸽** + **仅 LevelController 基类** |
| 大波前一波 | w9/w19… **无按钮 / 不可点** |
| mintime | **不可跳过** |
| 文案 | 「下一波」 |

### Skills 白名单
✅ UI Button / Canvas → `@unity-ui-system`  
✅ 无未覆盖技术点 — **通过**

### 架构分析（L1 摘要）
- 逻辑放 `LevelController`，UI 放 `ProgressBar`
- 不新建 Controller 子类
- `Update` 末尾刷新按钮状态
- **风险**: 低

### 影响面分析
| 文件 | 改动 |
|------|------|
| `LevelController.cs` | +3 方法，Update 内可选跳过已覆盖（自动不变） |
| `ProgressBar.cs` | +Button 字段与刷新 |
| `ProgressBar.prefab` | +Button 节点（Editor） |
| `docs/roguelike-progress.md` | 勾选 RG-009 |

**回归**: 主线关卡、Boss、Snake、无尽波次推进无变化

## 风险总评

| 维度 | 评级 |
|------|------|
| 技术可行性 | 高 |
| 改动范围 | 小 |
| 性能风险 | 低 |
| 时间评估 | 2~3h |

## 建议 Skills
- `@unity-ui-system` — Button 显隐与 interactable

## 决策审批
✅ **通过** — 可按需求卡片开发（用户 2026-05-20 确认：保留自动、mintime 后显隐、仅肉鸽）
