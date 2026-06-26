# 功能需求卡片: 肉鸽地图 Header 仅显示地图名

## 基本信息
- **功能名称**: RoguelikeMapPanel Header 文案简化
- **所属模块**: Roguelike / UI
- **需求类型**: 功能优化（UI 文案）
- **优先级**: P2
- **预估复杂度**: L1（< 10 行）
- **提出日期**: 2026-05-20

## 功能描述

### 详细描述

`RoguelikeMapPanel` 顶部 `headerText` 在正常进图时**只显示当前 Act 地图名称**（`ActMapConfig.displayName`），不再拼接种子等调试信息。

### 用户故事

作为玩家，我希望地图顶部只看到当前区域名称（如「前院」），界面更干净；种子、进度等放在 HUD 其它位置。

## 现有业务上下文

### 相关现有功能

| 功能 | 模块 | 关系 | 说明 |
|------|------|------|------|
| `RoguelikeMapPanel.BuildHeaderText` | Roguelike UI | 修改点 | 原：`{displayName} \| 种子 {runSeed}` |
| `RoguelikeRunInfoPanel` | Roguelike UI | 并行展示 | `actTitleText` 含 Act 序号；`runSeedText` 可选显示种子 |
| `ActMapConfig.displayName` | Roguelike Map | 数据源 | 各 Act 策划配置名 |

### 需求类型判定理由

UI 文案优化：不改变选路、存档、地图生成逻辑。

### 集成点

- **数据源**: `RoguelikeRunService.ActiveRunConfig.GetAct(state.currentActIndex).displayName`
- **刷新时机**: 现有 `Refresh()` → `RefreshHeader(BuildHeaderText(state))` 不变

### 对现有功能的影响

- **接口变更**: 无
- **行为变更**: Header 文案缩短；异常态（「本局失败」「通关！」等）仍走 `RefreshHeader` 直写，不受影响
- **数据变更**: 无

## 技术要求

- **依赖**: `TMP_Text headerText`、`ActMapConfig.displayName`
- **Skills**: `@unity-ui-system`（TextMeshPro 赋值）
- **兼容性**: 向后兼容；未配置 `displayName` 时回退 `Act {index+1}`

## 功能清单

### 核心功能（必须）
- [x] `BuildHeaderText` 仅返回 `displayName`（或回退文案）
- [x] 移除 Header 中的种子显示

### 扩展功能（可选）
- [ ] 与 `RoguelikeRunInfoFormatter` 抽共用方法（本次未做，避免过度抽象）

## 验收标准

- [x] 新开局 / 继续冒险进地图：Header 只显示 Act 名（如「前院」）
- [x] 不含 `种子`、`|` 等后缀
- [x] `displayName` 为空时显示 `Act 1` 等回退
- [x] 失败 / 通关等异常文案仍正常显示

## 风险评估

| 风险点 | 概率 | 影响 | 应对措施 |
|-------|------|------|---------|
| 玩家找不到种子 | 低 | 低 | `RoguelikeRunInfoPanel.runSeedText` 仍可绑 |

## 关联 Context

- 涉及模块: UI、Roguelike
- 参考: `context/modules/UI.md`

## 关联文档

- [肉鸽地图Panel配置说明.md](../game-design/肉鸽地图Panel配置说明.md)
