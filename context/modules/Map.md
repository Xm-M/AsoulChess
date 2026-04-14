# Map 模块

## 定位

**格子地图**：`Tile` 网格、`MapManage` 单例、坐标与占地、门/特殊格（冰、水等），为 `MoveController`、索敌、技能目标提供空间基础。

## 核心类型（节选）

| 类型 | 职责 |
|------|------|
| `MapManage` / `MapManage_PVZ` | 地图尺寸、`tiles[x,y]`、初始化 |
| `Tile` | 占地棋子 `stander`、地形类型、相邻关系 |
| `DoorTile` 等 | 特殊格行为 |

## 依赖

被 **Chess**、**LevelSystem**、**Weapon（索敌）**、大量 **SkillEffect** 引用。

## 扩展

- 新地形：扩展 `Tile` 或组件；同步 `PropertyCreator` 的 `chessTileType` 规则。  
- 坐标与 **mapPos** 约定变更时，全局搜 `tiles[` 与边界检查。
