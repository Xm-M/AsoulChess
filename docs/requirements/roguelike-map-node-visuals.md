# 功能需求卡片: 肉鸽地图节点视觉分层（RG-P07）

## 基本信息
- **功能名称**: RoguelikeMapPanel 节点状态视觉（已通关 ✔ / 不可达变暗 / 房间图标亮色）
- **所属模块**: UI / Roguelike（RG-P07）
- **需求类型**: 功能优化（体验 Polish）
- **优先级**: P1
- **预估复杂度**: L1~L2
- **预估耗时**: 2~4 小时（代码 + 节点 Prefab；✔ Sprite 由美术提供）
- **提出日期**: 2026-05-20
- **关联**: `docs/roguelike-known-issues.md` RG-P07、`docs/roguelike-progress.md`

## 功能描述

### 详细描述

当前 `RoguelikeMapNodeWidget` 在 **Cleared / Visited** 状态下对整颗节点底图乘 `clearedTint` / `visitedTint`，导致房间类型图标发灰、远处难以辨认。

本需求改为 **分层表达状态**：

| 层级 | 职责 | 状态影响 |
|------|------|----------|
| 底图 / 图标 | 表达房间类型（Normal/Elite/Boss/Rest/Shop/Event） | **始终保持亮色**（白/原色） |
| 变暗 Overlay | 半透明遮罩 | Locked / Visited / Cleared 时叠加 |
| ✔ 标记 | Image + Sprite | 仅 **Cleared** 显示 |
| 按钮 | 点击 | Locked 不可点；Selectable / Current 可点（不变） |

### 用户故事

作为玩家，我在肉鸽地图上能一眼看出每个节点的房间类型（图标亮色），同时区分：已通关（✔）、当前/可选、不可达（变暗）。

### 设计约定（已确认）

- ✔ 使用 **Image + 美术 Sprite**，可在 `RoguelikeMapNodeStateStyle` / VisualSettings 配置
- 交付范围：**代码分层 + 新建 `RoguelikeMapNode` 预制体 + 绑到 `RoguelikeMapPanel.nodePrefab`**
- 不改动 `MapGenerator`、选路逻辑、`ResolveVisualState` 状态机

## 现有业务上下文

### 相关现有功能

| 功能 | 模块 | 关系 | 说明 |
|------|------|------|------|
| `RoguelikeMapNodeWidget` | UI | 直接修改 | 当前整节点 tint |
| `RoguelikeMapVisualSettings` | UI | 扩展 | 状态色、✔ Sprite |
| `RoguelikeMapPanel` | UI | 绑 Prefab | `nodePrefab` 当前为空 |
| `ResolveVisualState` | UI | 不变 | Locked/Selectable/Current/Cleared/Visited |
| `nodeTypeStyles` | MapPanel Prefab | 复用 | 各房间类型 iconSprite 已配 |

### 需求类型判定理由

在现有地图 UI 上优化视觉表达，不改变玩法与数据流。

### 集成点

- **调用**: `RoguelikeMapPanel` 刷新节点时 `widget.Bind(...)`（不变）
- **触发**: 地图 `Refresh` / 战斗返回 / Act 切换
- **新增**: 节点 Prefab 子节点 `StateOverlay`、`ClearedCheckmark`

### 对现有功能的影响

- **接口变更**: `RoguelikeMapNodeWidget` 增加序列化引用；`RoguelikeMapNodeStateStyle` 增加 overlay / checkmark 字段
- **行为变更**: Cleared 不再整节点发灰，改为 overlay + ✔
- **数据变更**: 无

## 技术要求

- **Unity**: 项目当前版本
- **依赖模块**: `RoguelikeMapNodeWidget`、`RoguelikeMapPanel`、`RoguelikeMapVisualSettings`
- **性能**: 每节点多 1~2 个 Image，Refresh 时 SetActive，可忽略
- **兼容性**: `nodePrefab` 未绑时仍可用 `CreateRuntime`（简化版，无 ✔ 图 unless 运行时创建）
- **平台**: 全平台

## 功能清单

### 核心功能（必须）

- [ ] `RoguelikeMapNodeWidget` 拆分：`background`、`iconImage`、`stateOverlay`、`clearedCheckmark`
- [ ] 图标/底图：**Locked 以外**保持 `baseColor` 全亮；Locked 仅 overlay 变暗，不乘暗色到 icon
- [ ] `Cleared`：轻 overlay + 显示 ✔ Sprite
- [ ] `Visited`：轻 overlay，无 ✔
- [ ] `Locked`：`lockedColor` overlay + `button.interactable = false`（保持现状）
- [ ] `RoguelikeMapNodeStateStyle` 增加 `clearedCheckmarkSprite`、overlay 相关字段
- [ ] 新建 `Assets/Prefab/UI/Roguelike/RoguelikeMapNodeWidget.prefab`
- [ ] `RoguelikeMapPanel.prefab` 绑定 `nodePrefab`

### 扩展功能（可选，本期不做）

- [ ] Current 节点边框高亮 Sprite（P2）
- [ ] Cleared ✔ 出现动效（P2）

## 验收标准

- [x] **Cleared** 节点：房间图标仍为亮色；右上角显示 ✔（需 MapPanel 配 Sprite）
- [x] **Locked** 节点：明显变暗，按钮不可点
- [x] **Selectable / Current**：图标亮色，无 ✔
- [x] **Visited**：轻 overlay，图标仍可辨类型
- [x] 各 `MapRoomType` 图标在任意状态下类型可区分
- [x] MapPanel `clearedCheckmarkSprite` 已绑
- [x] Play：各状态与 ✔ 显示正常

## 风险评估

| 风险点 | 概率 | 影响 | 应对措施 |
|--------|------|------|----------|
| ✔ Sprite 未就绪 | 中 | 低 | Prefab 留空位 + 占位图；VisualSettings 可后填 |
| 运行时节点与 Prefab 行为不一致 | 低 | 中 | `CreateRuntime` 同步创建 overlay/checkmark 层 |
| overlay 挡住点击 | 低 | 中 | overlay `raycastTarget = false` |

## 代码草案（审批后实现）

```csharp
// RoguelikeMapNodeStateStyle 新增
public Sprite clearedCheckmarkSprite;
public Color lockedOverlayColor = new Color(0, 0, 0, 0.55f);
public Color visitedOverlayColor = new Color(0, 0, 0, 0.25f);
public Color clearedOverlayColor = new Color(0, 0, 0, 0.2f);

// Bind 内：icon/background 始终 baseColor（Current 可 Lerp 高亮）
// stateOverlay.gameObject.SetActive(state != Selectable && state != Current);
// stateOverlay.color = state switch { Locked => lockedOverlay, ... };
// clearedCheckmark.enabled = state == Cleared && sprite != null;
```

## Prefab 层级（建议）

```text
RoguelikeMapNodeWidget (Button)
├── Background (Image)      — 房间底图，raycast 由 Button 承担
├── Icon (Image)            — 房间类型图标，raycastTarget=false
├── StateOverlay (Image)    — 全铺半透明，raycastTarget=false
└── ClearedCheck (Image)    — ✔，raycastTarget=false
```

## 关联 Context

- 涉及模块: UI、Roguelike
- 参考: `context/modules/UI.md`、`Assets/Script/Roguelike/UI/RoguelikeMapNodeWidget.cs`
