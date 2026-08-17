# 需求开发审批报告

## 基本信息
- **需求名称**: 生存模式存档修复 + 波次 HUD
- **需求 ID**: `survival-save-wave-hud`
- **所属模块**: LevelSystem / SaveSystem / UI
- **分析日期**: 2026-08-14

## 需求摘要
修复生存「重开残留选卡」「读档不恢复草坪植物」；ProgressBar 显示 `关卡名 · 第X/Y波`。

**需求类型**: Bug修复 + 功能扩展  
**与现有功能的关系**: 修补 `SaveSystem` + `LevelController_Endless` + `PreParePlugun_ShowPlantShop`；扩展 `ProgressBar` 生存标题

## 分析结果汇总

### Context 复用
✅ 已读取项目 Context（2026-03-30 / v2.0.0）  
- 涉及模块: LevelSystem, UI  
- 参考: `docs/requirements/survival-endless-mode.md`

### 现有业务分析
- 共用关卡 JSON 存档；生存额外 `selectionIndex` / `totalWavesCleared` / 出怪池  
- `lockedHand` 在 LevelData SerializeReference 上跨场景残留 → 重开带卡  
- 轮间 `IfGameStart=false` 不再写盘；读档靠上一份档的 `playerPlants` + `EnterMap.Restore`

### 需求卡片
✅ `docs/requirements/survival-save-wave-hud.md`

### Skills白名单检查
| 技术点 | Skill | 结论 |
|--------|-------|------|
| UI 文案/ProgressBar | @unity-ui-system | ✅ |
| 存档读写 | @unity-save-system | ✅ |
| 场景重载 | @unity-scene-management | ✅ |
| Timeline 读档流 | @unity-timeline | ✅ 仅沿用现有，不新增 Timeline API |

**结论**: 全部覆盖

### 架构分析
✅ `docs/architecture/survival-save-wave-hud.md`  
- 模式: 最小修补（方案 A）  
- 风险等级: 中（存档门闸需 Survival-only）

### 影响面分析
✅ `docs/architecture/survival-save-wave-hud-impact.md`  
- 影响文件: ~5  
- 高风险点: 1（Save 门闸）  
- 回归测试项: 6

## 风险总评

| 维度 | 评级 | 说明 |
|------|------|------|
| 技术可行性 | 高 | 根因清晰 |
| 改动范围 | 小 | 5 文件级 |
| 性能风险 | 低 | 无热路径 |
| 时间评估 | 2–3h | 含联调 |

## 建议的Skills使用清单
- @unity-save-system — 存档门闸与采集  
- @unity-ui-system — ProgressBar 标题  
- @unity-scene-management — Restart 场景流  

## 开发优先级建议
- P0: 清 `lockedHand` + 读档恢复植物（含轮间）  
- P0: ProgressBar `关卡名 · 第X/Y波`  
- P1: 读档保留本轮 `currentWave`（不冲成 -1）

## 决策审批
✅ 通过 - 可以开始开发  

审批意见: 用户确认通过（2026-08-14）  
日期: 2026-08-14

### 实现备注
- 生存模式在 `EnableLevelSaveLoad=false` 时仍可存读（`CanSaveLoadLevel`）
- `SaveSurvivalSnapshotAllowPaused`：轮间/离场补存植物
- `ClearLockedHand` + Restart/无档进关清理
- ProgressBar 生存标题：`关卡名 · 第X/Y波`
- 轮末存档前将 `currentWave` 置 `-1`；读档战斗中保留波次
