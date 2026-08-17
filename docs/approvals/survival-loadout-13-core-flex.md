# 需求开发审批报告

## 基本信息
- **需求名称**: survival-loadout-13-core-flex
- **所属模块**: PlantsShop / Fetter / Survival
- **分析日期**: 2026-08-14

## 需求摘要
生存携带 13 格：前 10 核心锁死并计入羁绊人数；后 3 机动可换、不计入人数但享受羁绊效果。UI 复用肉鸽 loadout 槽位扩展。

**需求类型**: 功能扩展

## 分析结果汇总

### Context
✅ 已读；涉及 UI / Fetter / Level Prepare

### 需求卡片
✅ `docs/requirements/survival-loadout-13-core-flex.md`

### Skills 白名单
✅ UI 系统、存档（手牌）  
⚠️ Prefab 槽位已有肉鸽扩展，开发时对照现有 `RefreshLoadoutSlotBackgrounds`  
结论: 通过

### 架构 / 影响面
✅ `docs/architecture/survival-loadout-13-core-flex.md`  
✅ `docs/architecture/survival-loadout-13-core-flex-impact.md`  
- 风险: 中（锁定 UX + 计数/效果分离）  
- 预估文件: 4～6 + 可能 Prefab 点检

## 风险总评

| 维度 | 评级 |
|------|------|
| 技术可行性 | 高 |
| 改动范围 | 中 |
| 性能 | 低 |
| 耗时 | 6–10h |

## 开发优先级
- P0: maxCount=13、核心满 10 开战、轮间锁核心、Fetter 计数仅核心  
- P0: 机动仍享效果（全量 icon / WhenPlantChess 不关）  
- P1: 机动格视觉区分、改核心时提示  

## 决策审批
✅ 通过 - 可以开始开发  
⬜ 修改后通过  
⬜ 驳回  

审批意见: 用户确认通过  
日期: 2026-08-14
