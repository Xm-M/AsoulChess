# 架构分析: survival-loadout-13-core-flex

**日期**: 2026-08-14

## 1. 定位
- UI：`PlantsShop` 携带上限与槽位背景  
- 流程：`PreParePlugun_ShowPlantShop` 生存锁定  
- 规则：`FetterController` 计数与效果分离  

## 2. 推荐方案

```
Survival: maxCount = Core(10) + Flex(3) = 13
ApplyLoadoutSlotLimitForCurrentLevel:
  Roguelike → GetLoadoutSlotCount (6~15)
  Survival  → 13
  else      → baseline 10

CheckFetter roster = shop.currentShopIcons[0..CoreCount)
FetterEffect / WhenPlantChess → 不按「是否机动」屏蔽（机动享受效果）

Round≥2 Prefill:
  按存档顺序勾选
  icons[0..9] 设为 locked（不可 RemoveSelection）
  icons[10..] 可自由换
```

ShopIcon / SelectIcon 增加 `isCoreLocked` 或按 index 判断。

## 3. 与肉鸽对齐
复用 `RefreshLoadoutSlotBackgrounds(totalSlotCount)`：  
`middleVisible = total - head - tail`，中间格数组已支持扩到 15。

## 4. 风险
- 计数 API 与效果遍历必须分离，避免误把机动排除出 Buff  
- 锁定 UX 要拦取消选中，不只是 Prefill  

## 5. 结论
可行，改动面中等，优先统一「Fetter 计数入口」。
