# 功能需求卡片: 肉鸽携带格顶栏背景显隐（RG-008 UI）

## 基本信息
- **功能名称**: PlantsShop 携带格背景槽位显隐
- **所属模块**: UI / PlantsShop / Roguelike（RG-008）
- **需求类型**: 功能扩展（数值层已完成，补展示层）
- **优先级**: P1
- **预估复杂度**: L1
- **预估耗时**: 1~2 小时（仅代码；Prefab/美术由策划在 Editor 独立完成）
- **提出日期**: 2026-05-20
- **关联**: `docs/requirements/roguelike-rest-node.md`、`docs/roguelike-progress.md` RG-008

## 功能描述

### 详细描述

肉鸽 Run 内 `runLoadoutSlotCount` 可在 **7~15** 间变化（默认 10，休息房扩容 +1）。数值层已通过 `PlantsShop.maxCount` 限制选牌数量。

本需求仅补 **中间段背景空槽展示**：Prefab 中预置 **8** 个中间可变格（头 6 + 尾 1 在 Editor 常亮，**不**放入数组）。总携带格 **7~15** = 头 + 中间显隐 + 尾。运行时根据 `maxCount` 对中间 8 格执行 `SetActive`，**不**改动植物卡实例化逻辑。

### 用户故事

作为玩家，我在肉鸽休息房扩容后进入下一关选牌，希望顶栏能看到与当前可携带数量一致的空槽背景，且选牌数量仍由原有逻辑限制。

### 设计约定（已确认）

| 层级 | 职责 | 代码是否改动 |
|------|------|--------------|
| 种植栏头/尾/底图 | 纯美术装饰，与格数无关或 Editor 内自行拼接 | 否 |
| 背景空槽（中间 **8** 格） | 仅展示中间可变段 | **是：`SetActive`** |
| `shopIconParent` | 动态 `Instantiate` 植物卡，`maxCount` 限选 | **否** |

**分离原则**：背景格与植物卡 parent **不同节点、不同布局**，互不对齐绑定，因此 **不需要** 同步宽度/spacing，也 **不需要** 改 `AddSelection` / `Instantiate` 挂点。

### 显隐规则

```text
总格数 maxCount ∈ [7, 15]（头 6 + 中间 0~8 + 尾 1）
中间亮起数量 = Clamp(maxCount - headSlotCount - tailSlotCount, 0, 8)
对 loadoutSlotBackgrounds[i]（仅中间 8 格，i 从 0 起）:
  SetActive(i < 中间亮起数量)
```

示例（head=6, tail=1）：

| maxCount | 中间亮起 |
|----------|----------|
| 7 | 0 |
| 10 | 3 |
| 15 | 8 |

- 肉鸽：`maxCount` = `RoguelikeRunState.GetLoadoutSlotCount()`
- 非肉鸽：`maxCount` = `_baselineMaxCount`（默认 10 → 中间亮 3）
- 数组绑 **8** 个中间格；头尾常亮节点 **不放入数组**

### 调用时机

在 `ApplyLoadoutSlotLimitForCurrentLevel()` 末尾调用 `RefreshLoadoutSlotBackgrounds(maxCount)`，覆盖：

- `Show()` 正常开局
- `Show()` 冒险读档分支
- `ShowForLoad` / `ShowLockedHand` 前若已调用 `ApplyLoadoutSlotLimitForCurrentLevel` 则一并生效

## 现有业务上下文

### 相关现有功能

| 功能 | 模块 | 关系 | 说明 |
|------|------|------|------|
| `runLoadoutSlotCount` | Roguelike Run | 上游 | 7~15，休息房 +1 |
| `PlantsShop.maxCount` | UI | 已实现 | 选牌上限 |
| `ApplyLoadoutSlotLimitForCurrentLevel` | PlantsShop | 扩展点 | 追加背景显隐 |
| `AddSelection` | PlantsShop | 不变 | `Count < maxCount` |
| HUD `携带格 10/15` | RoguelikeRunInfoPanel | 平行 | 数值展示，本需求不碰 |

### 需求类型判定理由

RG-008 数值与休息选项已落地；本卡为 **同一需求的 UI 展示扩展**，不新增玩法规则。

### 集成点

- **调用**: `RoguelikeRunService.State.GetLoadoutSlotCount(economy)`（经现有 `ApplyLoadoutSlotLimitForCurrentLevel`）
- **触发**: `PlantsShop.Show()` / 读档展示流程
- **新增字段**: `GameObject[] loadoutSlotBackgrounds`（或等效 `Transform` + 子节点枚举）

### 对现有功能的影响

- **接口变更**: 无公共 API 变更
- **行为变更**: 顶栏可见空槽数随 `maxCount` 变化；选牌逻辑不变
- **数据变更**: 无

## 技术要求

- **Unity**: 项目当前版本
- **依赖模块**: `PlantsShop`、`RoguelikeRunService`（只读 Run 态）
- **性能**: 每次 Show 最多 15 次 `SetActive`，可忽略
- **兼容性**: 数组未绑定时静默跳过（便于冒险模式 Prefab 渐进迁移）
- **平台**: 全平台

## 功能清单

### 核心功能（必须）
- [x] `PlantsShop` 增加 `loadoutSlotBackgrounds` 序列化引用
- [x] `RefreshLoadoutSlotBackgrounds(int visibleCount)`：`slot[i].SetActive(i < visibleCount)`
- [x] `ApplyLoadoutSlotLimitForCurrentLevel()` 末尾调用刷新
- [x] Editor：中间背景格绑定（头尾常亮，不绑数组）

### 扩展功能（可选）
- [ ] 扩容时槽位出现简单动效（P2，本期不做）

## 验收标准

- [ ] 肉鸽 `runLoadoutSlotCount=10`：中间背景亮 **3** 格（6+3+1）
- [ ] 休息扩容至 11：中间亮 **4** 格
- [ ] 上限 15：中间 **8** 格全亮；下限 7：中间 **0** 格（仅头尾）
- [ ] 非肉鸽关卡：`maxCount=10`，背景亮 10 格（与冒险一致）
- [ ] 选牌仍受 `maxCount` 限制，与改前一致
- [ ] 未配置 `loadoutSlotBackgrounds` 时不报错

## 风险评估

| 风险点 | 概率 | 影响 | 应对措施 |
|--------|------|------|----------|
| Prefab 未绑满 15 格 | 中 | 低 | 代码按数组长度迭代；文档说明 Editor 职责 |
| 头尾格误放入数组 | 低 | 低 | 文档：常亮装饰不放入数组 |
| 动画整页滑动回归 | 低 | 低 | 背景为子节点，随 `种植栏` 整体动画 |

## 关联 Context

- 涉及模块: UI、Roguelike
- 参考: `context/modules/UI.md`、`Assets/Script/UI/Shop/PlantsShop.cs`

## 代码草案（审批后实现）

```csharp
[SerializeField] int loadoutBarHeadSlotCount = 6;
[SerializeField] int loadoutBarTailSlotCount = 1;
[SerializeField] GameObject[] loadoutSlotBackgrounds; // 长度 8，仅中间格

void RefreshLoadoutSlotBackgrounds(int totalSlotCount)
{
    int middleVisible = Mathf.Clamp(
        totalSlotCount - loadoutBarHeadSlotCount - loadoutBarTailSlotCount,
        0, loadoutSlotBackgrounds.Length);
    for (int i = 0; i < loadoutSlotBackgrounds.Length; i++)
        loadoutSlotBackgrounds[i]?.SetActive(i < middleVisible);
}
```
