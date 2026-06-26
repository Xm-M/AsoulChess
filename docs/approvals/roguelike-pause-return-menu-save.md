# 需求开发审批报告：肉鸽暂停返回主菜单（存档+关地图）

## 基本信息

- **需求名称**：肉鸽暂停面板 — 返回主菜单存档并关闭地图
- **所属模块**：Roguelike / UI / LevelSystem
- **分析日期**：2026-05-30

## 需求摘要

肉鸽 Run 进行中，暂停面板「返回主菜单」应**保存 Run 进度**、**关闭地图/HUD**、回到 `StartUI`，支持「继续冒险」；**不**放弃 Run。

**需求类型**：功能扩展  
**与现有功能的关系**：基于 `ParsePanel.ReturnMenu` + `RoguelikeRunService.SaveRun` 扩展，区别于 `AbandonRun` 与 `ReturnToMapUI`。

## 分析结果汇总

### Context 复用

✅ 已读取项目 Context / 既有肉鸽存档需求文档  
- 涉及模块：Roguelike、UI、LevelSystem、SaveSystem  
- 参考：`docs/requirements/roguelike-run-save.md`

### 现有业务分析

- **相关现有功能**：`ParsePanel.ReturnMenu`、`RoguelikeRunService.SaveRun/AbandonRun/ReturnToMapUI`、`StartUI.ContinueRoguelikeRun`
- **需求类型判定**：功能扩展 — 在已有 Run 存档体系上补「保存退出」路径
- **集成点**：`HasActiveRun` 分支 → `SaveRun` → 关面板 → `RestoreMainlinePlantsFromPlayerSave` → 回主菜单
- **对现有功能的影响**：仅肉鸽分支；战斗中途不存关卡快照（与现有设计一致）

### 需求卡片

✅ 已生成 → `docs/requirements/roguelike-pause-return-menu-save.md`

### Skills 白名单检查

| 技术点 | Skill | 状态 |
|--------|-------|------|
| UI 面板 Show/Hide | unity-ui-system | ✅ |
| 场景加载 | unity-scene-management | ✅ |
| JSON 存档 | unity-save-system | ✅ |

**结论**：全部覆盖，可开发。

### 架构分析（摘要）

**建议方案**：在 `RoguelikeRunService` 新增 `SaveAndExitToMainMenu()`，集中处理：

```
SaveRun()
→ RestoreMainlinePlantsFromPlayerSave()（若在战斗）
→ Hide 肉鸽相关面板
→ LevelManage.ReturnMenu() 或等价流程
→ 不调用 AbandonRun / 不 Delete active.json
```

`ParsePanel.ReturnMenu()` 仅做路由：

```csharp
if (RoguelikeRunService.HasActiveRun)
    RoguelikeRunService.SaveAndExitToMainMenu();
else
    LevelManage.ReturnMenu();
```

**设计模式**：服务层编排 + UI 薄路由  
**风险等级**：中（场景与面板叠层需一次测全路径）

### 影响面分析（摘要）

| 文件 | 改动 |
|------|------|
| `RoguelikeRunService.cs` | 新增 `SaveAndExitToMainMenu()` |
| `ParsePanel.cs` | `ReturnMenu` 肉鸽分支 |
| （可选）`RoguelikeUiFlow` 或各 Panel | 统一 Hide 入口 |

- **影响文件数**：2–4  
- **高风险点**：战斗内返回后植物池、地图面板残留  
- **回归测试**：主线返回主菜单；肉鸽地图/战斗两路径；继续冒险读档

## 风险总评

| 维度 | 评级 | 说明 |
|------|------|------|
| 技术可行性 | 高 | 存档与 UI 能力已具备 |
| 改动范围 | 小 | 约 30–80 行 |
| 性能风险 | 低 | 单次存档写入 |
| 时间评估 | 1–2h | 含双路径测试 |

## 建议 Skills

- @unity-ui-system — 面板 Hide/Show  
- @unity-scene-management — 回「开始」场景  
- @unity-save-system — Run JSON 存档  

## 开发优先级

- **P0**：SaveRun + 关地图 + 回 StartUI + 继续冒险可用  
- **P1**：返回前确认弹窗（可选）

## 决策审批

✅ **通过** — 2026-05-30

审批意见:
1. 战斗内返回视为放弃当前战斗；主菜单「继续冒险」读档后直接进入该关卡（`TryResumePendingCombatNode`）。
2. 无需额外二次确认（暂停面板 `confirmPanel` 已有）。

日期: 2026-05-30
