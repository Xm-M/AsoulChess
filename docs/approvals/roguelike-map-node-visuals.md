# 需求开发审批报告: 肉鸽地图节点视觉分层（RG-P07）

## 基本信息
- **需求名称**: RoguelikeMapPanel 节点状态视觉（RG-P07）
- **所属模块**: UI / Roguelike
- **分析日期**: 2026-05-20

## 需求摘要

地图节点改为 **类型层（亮色图标）+ 状态 Overlay + Cleared ✔ Sprite** 分层展示；新建节点 Prefab 并绑到 `RoguelikeMapPanel.nodePrefab`。不改动选路与 `RoguelikeNodeVisualState` 状态机。

**需求类型**: 功能优化  
**与现有功能的关系**: 基于 `RoguelikeMapNodeWidget` / `RoguelikeMapPanel` 的视觉层扩展

## 分析结果汇总

### Context 复用
- ✅ 已读取 `context/index.md`、`context/modules/UI.md`
- 涉及模块: UI、Roguelike Map

### 现有业务分析
- **相关现有功能**: `RoguelikeMapNodeWidget.Bind`、`RoguelikeMapPanel.ResolveVisualState`、`nodeTypeStyles`（Panel Prefab 已配各类型 icon）
- **现状问题**: `clearedTint`/`visitedTint` 乘在底图上；无 ✔；`nodePrefab` 为空（运行时简易节点）
- **集成点**: 仅 `Bind()` 内视觉逻辑 + Prefab 引用
- **对现有功能的影响**: 纯 UI；选路/存档/战斗流程不变

### 需求卡片
- ✅ 已生成 → `docs/requirements/roguelike-map-node-visuals.md`

### Skills 白名单检查

| 技术点 | Skill | 结论 |
|--------|-------|------|
| Image / Button / SetActive | @unity-ui-system | ✅ |
| Prefab 实例化 nodePrefab | @unity-prefab-system | ✅ |
| ScriptableObject 视觉配置 | @unity-scriptableobject-config | ✅（可选扩展 VisualSettings Asset） |

**结论**: 全部覆盖

### 架构分析（简）

- **设计模式**: 视图分层（类型层 vs 状态层）
- **关键决策**: 状态不再乘到 icon/baseColor；用独立 Overlay + Checkmark
- **代码归属**: `RoguelikeMapNodeWidget.cs`、`RoguelikeMapVisualSettings.cs`；新建 Prefab；改 `RoguelikeMapPanel.prefab` 引用
- **风险等级**: 低

### 影响面分析（简）

| 文件 | 改动 |
|------|------|
| `RoguelikeMapNodeWidget.cs` | 分层 Bind 逻辑、CreateRuntime 同步 |
| `RoguelikeMapVisualSettings.cs` | +checkmark/overlay 字段 |
| `RoguelikeMapNodeWidget.prefab` | 新建 |
| `RoguelikeMapPanel.prefab` | `nodePrefab` 引用 |
| `RoguelikeMapPanel.cs` | 预计无改或仅透传样式 |

- **影响文件数**: 3~4
- **高风险点**: 0
- **回归测试**: 地图 Refresh；Cleared/Locked/Selectable 各状态；各 RoomType 图标；战斗返回后节点更新

## 用户确认项

| 项 | 选择 |
|----|------|
| ✔ 形式 | Image + 美术 Sprite |
| 范围 | 代码 + 节点 Prefab + MapPanel 绑定 |

## 风险总评

| 维度 | 评级 | 说明 |
|------|------|------|
| 技术可行性 | 高 | 单 Widget 重构 |
| 改动范围 | 小 | 无 Run 数据变更 |
| 性能风险 | 低 | 每节点 +1 Image |
| 时间评估 | 2~4h | 含 Prefab；✔ 图可后补 |

## 建议的 Skills 使用清单
- `@unity-ui-system` — 分层 Image、raycast
- `@unity-prefab-system` — 节点 Prefab、MapPanel 引用

## 开发优先级建议
- **P0**: Widget 分层逻辑 + overlay/checkmark
- **P1**: 节点 Prefab + MapPanel.nodePrefab 绑定
- **P2**: Current 边框、✔ 动效

## 职责划分

| 角色 | 职责 |
|------|------|
| 程序 | Widget 分层、CreateRuntime 降级、MapPanel 绑 Prefab |
| 美术/策划 | 提供 ✔ Sprite；确认 overlay 深浅；各 RoomType 图标已在 Panel 配好 |

## 决策审批
- [x] **通过** — 可以开始开发（2026-05-20 用户确认）
- [ ] 修改后通过
- [ ] 驳回

**审批意见**: ________________  
**日期**: __________
