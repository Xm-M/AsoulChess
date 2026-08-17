# 影响面分析: survival-loadout-13-core-flex

**日期**: 2026-08-14

## 改动文件（预估）

| 文件 | 改动 |
|------|------|
| `PlantsShop.cs` | 生存 maxCount=13；开战校验；锁定核心选中；可选 `GetFetterRosterCreators` |
| `PreParePlugun_ShowPlantShop.cs` | Prefill 后锁核心；EnsureLocked 保序 |
| `FetterController.cs` | CheckFetter 只用核心 roster |
| `ShopSelectIcon` / `ShopIcon`（若有） | 锁定态不可点取消 |
| Prefab `PlantsShop` | 确认中间背景格 ≥ 支撑 13（已有肉鸽扩展则可能 0 改） |

`Fetter.cs` 内 `currentShopIcons` 全量遍历（如结束乐队减 CD）：**保留全量**，使机动位也吃减 CD。

## 回归

- [ ] 生存第 1 轮：必须 10 核心才能开；可带 0～3 机动  
- [ ] 第 2 轮：前 10 锁死；后 3 可换；羁绊人数与仅核心一致  
- [ ] 机动位种下享受已点亮羁绊 Buff  
- [ ] 机动位同 tag **不**把羁绊人数加一档  
- [ ] 冒险 10 格、肉鸽 6～15 格正常  
- [ ] 生存读档手牌顺序与锁定正确  
