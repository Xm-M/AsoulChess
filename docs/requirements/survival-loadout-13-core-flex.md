# 功能需求卡片: survival-loadout-13-core-flex

## 基本信息
- **功能名称**: 生存模式 13 携带格（10 核心锁定 + 3 机动）
- **所属模块**: UI / PlantsShop / Fetter / Survival Prepare
- **需求类型**: 功能扩展
- **优先级**: P0
- **预估复杂度**: L2
- **预估耗时**: 6–10 小时（含选卡锁定 UX 与回归）
- **提出日期**: 2026-08-14

## 功能描述
### 详细描述
生存模式下 `PlantsShop` 可选携带格为 **13**（复用现有 loadout 槽位背景扩展，与肉鸽 6～15 同源 UI）：

| 区间 | 格数 | 轮间 | 羁绊人数统计 | 羁绊效果 |
|------|------|------|--------------|----------|
| 核心 | 索引 0～9 | **人与顺序锁死**，不可更换 | **计入** | 享受 |
| 机动 | 索引 10～12 | **每轮可换** | **不计入** | **仍享受**（点亮后的 Buff / 种植触发等） |

开战条件：核心必须满 **10**；机动 0～3 均可。

示例：核心凑出主唱羁绊后，机动位同 tag 植物**不增加**主唱计数，种下后**仍吃**主唱 Buff。

非生存、非肉鸽仍默认 10；肉鸽仍 `GetLoadoutSlotCount`。

### 用户故事
作为生存玩家，我希望前 10 张固定成羁绊底盘，后 3 张每轮灵活换人且不破坏羁绊人数，同时机动位仍能吃到已点亮羁绊的增益。

## 现有业务上下文

### 相关现有功能
| 功能 | 模块 | 关系 | 说明 |
|------|------|------|------|
| `PlantsShop.maxCount` + `RefreshLoadoutSlotBackgrounds` | UI | 扩展 | 肉鸽已支持动态格数 |
| `PreParePlugun_ShowPlantShop` lockedHand | Level | 修改 | 现整手可换 → 拆核心锁 / 机动可换 |
| `FetterController.CheckFetter` | Fetter | 修改 | roster 仅核心 10 |
| 各 `FetterEffect` / `WhenPlantChess` | Fetter | 保持/微调 | 效果仍作用于机动位实例 |

### 需求类型判定理由
在现有选卡与羁绊上增加生存专用格数与锁定规则，非新模块。

### 集成点
- **调用**: `ApplyLoadoutSlotLimitForCurrentLevel`、`RefreshLoadoutSlotBackgrounds`、`CheckFetter`、`EnsureLockedHandFromShop` / Prefill
- **事件**: `WhenPlantChess`（机动位仍触发）；`GameStart` 时 CheckFetter
- **新接口建议**: `PlantsShop.GetFetterRosterCreators()`（仅核心）、`IsCoreSlot(index)`、`SurvivalCoreSlotCount=10`、`SurvivalFlexSlotCount=3`

### 对现有功能的影响
- **行为**: 生存选卡上限 13；轮间前 10 不可改；羁绊计数忽略机动
- **数据**: `plantsShopData.selectedCreatorIds` **有序**；前 10=核心，其后=机动（或显式拆字段，推荐有序以少改存档）
- **冒险/肉鸽**: 不改默认逻辑

## 关联 Context
- LevelSystem, UI, Fetter（`context/modules/`）

## 技术要求
- 复用 `loadoutSlotBackgrounds` 中间格显隐（与肉鸽相同）
- 生存：`maxCount = 13`（常量可配）
- 兼容：旧生存档不足 10 张 → 须补满核心才能开战；超过 10 的视为机动

## 已确认决策
1. **Q1** 机动不计入人数，但享受已点亮羁绊效果 / 种植触发 — ✅  
2. **Q2** 核心必须满 10，机动可选 — ✅  
3. **Q3** 核心人与顺序均锁死 — ✅  
4. **Q4** Prefab 已扩展，跟肉鸽格子显隐 — ✅  

## 功能清单
### 核心（必须）
- [ ] 生存 `maxCount=13` + 槽位背景刷新
- [ ] 开战校验：核心满 10
- [ ] 轮间：预填后锁定前 10 不可取消/替换；后 3 可改
- [ ] `CheckFetter` 仅用核心 10 计数；效果仍可加到机动卡/场上单位
- [ ] 存档有序手牌读写与读档预填

### 可选
- [ ] 机动格视觉区分（色条/标签「机动」）
- [ ] 试图改核心格时的 UI 提示

## 验收标准
- [ ] 生存选卡最多 13；核心不满 10 不能开战
- [ ] 第 2 轮起前 10 无法换成其他植物、顺序不变
- [ ] 仅核心影响羁绊点亮人数；机动种下仍获已点亮羁绊 Buff
- [ ] 肉鸽 / 冒险选卡行为回归正常
- [ ] 槽位背景在 13 时显示正确

## 风险评估
| 风险 | 概率 | 影响 | 应对 |
|------|------|------|------|
| FetterEffect 内手写遍历未统一走 roster API | 中 | 中 | 提供 `GetFetterRosterCreators`；效果侧继续全量 icons 若需给机动减 CD |
| 预填顺序错乱导致锁错人 | 中 | 高 | 存档严格按 shop 栏顺序 |
| 核心不满时的开战按钮拦不住 | 低 | 中 | GameStart 前二次校验 |
