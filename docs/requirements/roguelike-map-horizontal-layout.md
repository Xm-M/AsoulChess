# 功能需求卡片: RoguelikeMapPanel 横向布局

## 基本信息
- **功能名称**: RoguelikeMapPanel 地图生成方向改为横向
- **所属模块**: Roguelike / UI
- **需求类型**: 功能优化
- **优先级**: P1
- **预估复杂度**: L2
- **提出日期**: 2026-05-22

## 功能描述
将 `RoguelikeMapPanel` 从 STS 式**纵向**（起点在底、Boss 在顶）改为**横向**（起点在左、Boss 在右），以配合一张横向三段式背景图（校园 → 城市 → 舞台）。

### 用户故事
作为玩家，我希望肉鸽选路地图从左向右推进，以便背景与路线进度一致。

## 现有业务上下文

### 相关现有功能
| 功能 | 模块 | 关系 |
|------|------|------|
| RoguelikeMapPanel | UI | 直接修改 |
| MapGenerator | Roguelike/Map | 数据不变（layer/slot 语义不变） |
| RoguelikeMapLineWidget | UI | 复用，连线算法不变 |

### 布局语义变更
| 维度 | 改前（纵向） | 改后（横向） |
|------|-------------|-------------|
| 层（layer） | 一行 | 一列 |
| slot | 行内 X | 列内 Y |
| Content 扩展 | 高度 | 宽度 |
| ScrollRect | Vertical | Horizontal |
| 起点 | 底部 layer 0 | 左侧 layer 0 |
| Boss | 顶部 | 右侧 |

### Inspector 字段复用（避免 prefab 丢数值）
- `layerRowWidth` → **列高**（slot 方向可用高度，默认 1250）
- `layerRowHeight` → **列宽**（层间距，默认 200）
- `mapPaddingBottom` → **左侧**（起点）padding
- `mapPaddingTop` → **右侧**（Boss 端）padding

## 验收标准
- [ ] 起点节点在 Content 最左侧，Boss 在最右侧
- [ ] 横向 ScrollRect 可滚动查看全图
- [ ] Refresh 后滚到当前节点附近
- [ ] 节点连线正确连接相邻层
- [ ] `mapBackgroundImage` 随 Content 同宽

## 关联 Context
- `context/modules/UI.md`
