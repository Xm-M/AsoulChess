# 需求开发审批报告: 肉鸽携带格顶栏背景显隐（RG-008 UI）

## 基本信息
- **需求名称**: PlantsShop 携带格背景槽位显隐
- **所属模块**: UI / PlantsShop
- **分析日期**: 2026-05-20

## 需求摘要

Prefab 预置最多 15 个**独立**背景空槽；背景层、种植栏装饰、`shopIconParent` 植物卡 **三层分离**。代码仅在 `ApplyLoadoutSlotLimitForCurrentLevel` 后根据 `maxCount` 对背景节点 `SetActive`，不改动选牌实例化逻辑。

**需求类型**: 功能扩展  
**与现有功能的关系**: 基于 RG-008 数值层（`maxCount`）补 UI 展示

## 分析结果汇总

### Context 复用
- 已读取 `context/index.md`
- 涉及模块: UI、Roguelike Run

### 现有业务分析
- **相关现有功能**: `runLoadoutSlotCount`、`PlantsShop.maxCount`、`ApplyLoadoutSlotLimitForCurrentLevel`
- **需求类型判定**: 功能扩展 — UI 展示补全
- **集成点**: `ApplyLoadoutSlotLimitForCurrentLevel` 末尾刷新背景显隐
- **对现有功能的影响**: 仅顶栏视觉效果；选牌/存档/开战流程不变

### 需求卡片
- 已生成 → `docs/requirements/roguelike-loadout-slot-ui.md`

### Skills 白名单检查
- 已覆盖: `@unity-ui-system`（RectTransform、SetActive）、`@unity-prefab-system`（Editor 绑引用）
- 未覆盖: 无
- **结论**: 通过

### 架构分析（简）
- **设计模式**: 展示与逻辑分离；背景层纯 View 状态
- **关键决策**: 放弃背景与 `shopIconParent` 对齐方案（用户确认三层独立）
- **代码归属**: 仅 `PlantsShop` 增加数组 + 刷新方法
- **风险等级**: 低

### 影响面分析（简）
| 文件 | 改动 |
|------|------|
| `PlantsShop.cs` | +`loadoutSlotBackgrounds`、+`RefreshLoadoutSlotBackgrounds`、1 行调用 |
| `PlantsShop.prefab` | Editor 绑数组（非 AI 必做） |

- **影响文件数**: 1（代码）
- **高风险点**: 0
- **回归测试**: 冒险选牌 10 张、肉鸽 7/10/15 格、休息扩容后下一关 Show

## 风险总评

| 维度 | 评级 | 说明 |
|------|------|------|
| 技术可行性 | 高 | ~15 行代码 |
| 改动范围 | 小 | 单类扩展 |
| 性能风险 | 低 | 无分配 |
| 时间评估 | 1~2h | 含 Play 验收 |

## 建议的 Skills 使用清单
- `@unity-ui-system` — Prefab 层级与 SetActive
- `@unity-prefab-system` — 数组引用绑定

## 开发优先级建议
- **P0**: `RefreshLoadoutSlotBackgrounds` + `ApplyLoadoutSlotLimit` 挂钩
- **P1**: Prefab 绑 15 背景格（Editor）
- **P2**: 扩容动效（本期不做）

## 职责划分

| 角色 | 职责 |
|------|------|
| 策划/美术 | 背景格、种植栏头尾底图分离排版；15 格绑到 `loadoutSlotBackgrounds` |
| 程序 | 仅 `SetActive` 显隐逻辑 |

## 决策审批
- [x] **通过** — 可以开始开发（范围：代码 SetActive；Prefab 绑定由用户在 Editor 完成）
- [ ] 修改后通过
- [ ] 驳回

**审批意见**: 用户确认背景与种植栏、植物卡 parent 完全独立，无需对齐逻辑；开发侧只负责 `SetActive`。  
**日期**: 2026-05-20
