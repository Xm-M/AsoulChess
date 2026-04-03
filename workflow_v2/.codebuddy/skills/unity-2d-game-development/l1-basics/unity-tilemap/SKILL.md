---
name: unity-tilemap
description: Unity 2D瓦片地图系统，用于创建网格地图。支持批量API优化（SetTiles比SetTile快50倍）、碰撞优化（CompositeCollider2D）、多种布局方式。适用于横版过关、RPG、策略游戏等2D游戏类型。
---

# Unity Tilemap 系统

## 技能描述

Unity Tilemap 系统是 Unity 引擎内置的 2D 瓦片地图编辑工具，自 Unity 2017.2 版本起集成到引擎中。该系统允许开发者通过组合小尺寸的瓦片（Tile）资源来快速构建复杂的 2D 游戏场景和关卡，大幅提升关卡制作效率和地图多样性。

Tilemap 系统支持多种布局方式，包括矩形（Rectangle）、六边形（Hexagonal）和等距视角（Isometric）等多种网格布局，适用于横版过关游戏、RPG 地图、策略游戏等多种 2D 游戏类型。

## API 白名单分类

### ✅ 推荐使用（高效且稳定）

以下 API 方法在日常开发中优先推荐使用：

1. **SetTile(Vector3Int position, TileBase tile)** - 设置单个位置的瓦片
   - 适用于少量、零散的瓦片设置操作
   - 使用 `null` 作为 tile 参数可以清除指定位置的瓦片

2. **GetTile(Vector3Int position)** - 获取指定位置的瓦片
   - 用于查询特定单元格的瓦片信息
   - 返回 TileBase 类型，可判断瓦片类型或为 null

3. **SetTiles(Vector3Int[] positionArray, TileBase[] tileArray)** - 批量设置多个瓦片
   - **性能优化核心 API**，用于一次性设置大量瓦片
   - 显著减少渲染和物理系统的更新开销
   - 两个数组长度必须一致，一一对应

4. **SwapTile(TileBase oldTile, TileBase newTile)** - 批量替换瓦片
   - 将地图上所有某种类型的瓦片统一替换为另一种类型
   - 适用于地形变更、季节切换等场景

5. **ClearAllTiles()** - 清除所有瓦片
   - 快速清空整个 Tilemap 的所有瓦片
   - 用于重置地图或初始化场景

6. **WorldToCell(Vector3 worldPosition)** - 世界坐标转网格坐标
   - 将世界空间坐标转换为 Tilemap 的网格坐标
   - 常用于鼠标点击放置瓦片、角色位置查询等场景

7. **CellToWorld(Vector3Int cellPosition)** - 网格坐标转世界坐标
   - 将网格坐标转换为世界空间坐标
   - 用于获取瓦片的世界位置信息

8. **GetBounds(Vector3Int position)** - 获取指定位置的边界
   - 返回瓦片在世界空间中的包围盒
   - 用于碰撞检测、相机跟随等场景

### ⚠️ 性能警告（谨慎使用）

以下 API 在特定场景下可能导致性能问题，需要谨慎评估后使用：

1. **频繁调用 SetTile() 设置大量瓦片**
   - 每次调用都会触发渲染和物理系统的更新
   - 在循环中调用会导致严重的性能下降
   - **替代方案**：使用 `SetTiles()` 批量操作

2. **SetTilesBlock() 大面积矩形操作**
   - 虽然是批量操作，但对于超大面积（如 1000x1000）可能仍有性能压力
   - 建议分批次处理，每帧处理一部分

3. **RefreshAllTiles()** - 刷新所有瓦片
   - 强制刷新整个 Tilemap 的所有瓦片数据
   - 仅在必要情况下使用，避免频繁调用

4. **动态添加/删除 Tilemap Collider 2D**
   - 碰撞体重建是昂贵操作，会导致帧率波动
   - 建议在场景加载时完成碰撞体配置

5. **频繁查询 GetTile()** - 大范围遍历查询
   - 遍历大范围区域查询瓦片信息会增加 CPU 开销
   - 考虑缓存查询结果或使用数据结构优化

### ❌ 禁止使用（严重性能问题或已废弃）

以下 API 和使用方式应避免：

1. **在 Update/FixedUpdate 中频繁修改 Tilemap**
   - 每帧修改瓦片会导致严重的性能问题
   - 应将瓦片修改操作移至初始化阶段或低频事件中

2. **使用 SetTile() 进行程序化地图生成**
   - 程序化生成应始终使用 `SetTiles()` 批量操作
   - 单个瓦片设置会显著增加生成时间

3. **不必要地启用 Composite Collider 2D 的自动更新**
   - 当地图频繁变化时，自动更新碰撞体会导致卡顿
   - 应设置为手动更新，在修改完成后统一更新碰撞体

4. **使用已废弃的 API 或版本不兼容的方法**
   - 避免使用 Unity 早期版本的实验性 API
   - 始终使用官方文档推荐的稳定 API

## 最佳实践和性能优化要点

### 1. 批量操作优先

**核心原则**：尽可能使用 `SetTiles()` 批量操作，而非循环调用 `SetTile()`

```csharp
// ❌ 性能差的实现
for (int x = 0; x < width; x++) {
    for (int y = 0; y < height; y++) {
        tilemap.SetTile(new Vector3Int(x, y, 0), tile);
    }
}

// ✅ 性能优秀的实现
Vector3Int[] positions = new Vector3Int[width * height];
TileBase[] tiles = new TileBase[width * height];
for (int i = 0; i < width * height; i++) {
    positions[i] = new Vector3Int(i % width, i / width, 0);
    tiles[i] = tile;
}
tilemap.SetTiles(positions, tiles);
```

**性能对比**：
- 在 100x100 的 Tilemap 上设置瓦片
- 使用 `SetTile()` 循环：约 500-800ms
- 使用 `SetTiles()` 批量：约 5-10ms
- **性能提升：50-100 倍**

### 2. 合理使用 Tilemap 层

将不同类型的瓦片分配到不同的 Tilemap 层：
- 背景层（Background）：静态背景，不参与碰撞
- 地形层（Terrain）：可交互的地形，需要碰撞体
- 装饰层（Decoration）：装饰物，可能参与碰撞
- 交互层（Interactive）：可交互对象，动态更新

这样可以：
- 减少不必要的碰撞体计算
- 提高渲染批处理效率
- 便于逻辑管理和分层控制

### 3. 优化碰撞体配置

- 对于静态地形，使用 Tilemap Collider 2D + Composite Collider 2D
- 将碰撞体类型设置为 Static，减少物理计算
- 避免在运行时频繁修改碰撞体
- 使用 `Used by Composite` 合并相邻碰撞体，减少物理计算量

### 4. 渲染模式选择

Tilemap Renderer 组件提供两种渲染模式：

**Chunk 模式（推荐）**：
- 将相邻瓦片批量渲染
- 性能最优，适用于大多数场景
- 不支持与其他 Sprite 的深度排序

**Individual 模式**：
- 每个瓦片单独渲染
- 支持与其他 Sprite 的深度排序
- 性能开销较大，仅在有特定排序需求时使用

### 5. 瓦片资源管理

- 复用 Tile 资源，避免创建大量相同的 Tile 实例
- 使用 RuleTile 和 AnimatedTile 实现动态效果，而非运行时创建
- 将常用的 Tile 预加载到内存中，避免运行时加载

### 6. 坐标转换优化

- 缓存频繁使用的坐标转换结果
- 避免在循环中重复计算相同的坐标转换
- 使用 `Vector3Int` 进行网格坐标计算，避免浮点精度问题

### 7. 内存管理

- 对于超大型地图（超过 2000x2000），考虑分块加载
- 使用对象池管理动态创建的 Tile
- 及时清除不再使用的 Tilemap 资源

## 功能边界 ⚠️ 强制说明

### 本Skill涵盖范围

- ✅ Tilemap核心API（SetTile、GetTile、SetTiles、SwapTile）
- ✅ 网格坐标系统（WorldToCell、CellToWorld）
- ✅ Tilemap Collider 2D碰撞配置
- ✅ Composite Collider 2D碰撞优化
- ✅ Tilemap Renderer渲染配置
- ✅ RuleTile规则瓦片
- ✅ AnimatedTile动画瓦片
- ✅ 批量操作性能优化

### 不在本Skill范围内

- ❌ 3D地形系统（Terrain）→ 本项目不涉及
- ❌ 程序化网格生成 → 不涉及
- ❌ Tilemap动画系统（Animator）→ 见 unity-2d-animation Skill
- ❌ Tilemap物理效果器 → 见 unity-2d-physics Skill（L3级扩展）
- ❌ 自定义Tile渲染Shader → 不涉及

### 跨Skill功能依赖

**地图碰撞系统需要**：
- unity-tilemap（瓦片地图）← 当前Skill
- unity-2d-physics（碰撞系统）
- unity-tilemap（Tilemap Collider 2D）

**动态地形系统需要**：
- unity-tilemap（瓦片操作）← 当前Skill
- unity-object-pool（对象池）→ L2级Skill
- unity-scriptableobject-config（数据配置）

### 性能限制

| 指标 | 建议值 | 说明 |
|------|--------|------|
| Tilemap尺寸 | ≤ 500x500 | 超大地图应分块加载 |
| 活跃Tilemap数量 | ≤ 5 | 减少渲染开销 |
| 每帧SetTile调用 | ≤ 100 | 使用SetTiles批量操作 |
| Tile资源数量 | ≤ 500 | 复用Tile资源 |

---

## 适用场景

### 最佳适用场景

1. **横版过关游戏**
   - 平台跳跃、动作冒险
   - 地形固定，需要精确的碰撞检测

2. **RPG 地图**
   - 网格移动的角色
   - 大范围探索地图
   - 地形变化（如季节切换）

3. **策略游戏**
   - 网格战斗系统
   - 地形影响移动和攻击范围
   - 建筑放置和资源管理

4. **解谜游戏**
   - 网格解谜元素
   - 可交互的地图物体
   - 动态地形变化

5. **俯视角游戏**
   - 等距视角地图
   - 伪 3D 效果
   - 层级遮挡关系

### 不适用场景

1. **完全动态地形**
   - 地形每帧都在变化
   - 建议使用程序化网格生成

2. **非网格游戏**
   - 自由移动、无网格约束
   - 建议使用 Sprite 系统或自定义渲染

3. **超大开放世界**
   - 单个 Tilemap 尺寸过大（如 5000x5000）
   - 建议使用分块加载或 World Streaming

4. **复杂 3D 地形**
   - 高度变化频繁的 3D 地形
   - 建议使用 Terrain 系统

## 学习建议

1. **掌握基础**：先熟悉 Tilemap 工作流程和基本 API
2. **实践优化**：在项目中应用批量操作和分层管理
3. **深入高级**：学习 RuleTile、AnimatedTile 和自定义 Tile
4. **性能分析**：使用 Profiler 分析和优化 Tilemap 性能
5. **参考案例**：学习 Unity 官方示例和开源项目中的 Tilemap 使用

## 相关资源

- Unity 官方文档：[2D Tilemap](https://docs.unity3d.com/Manual/class-Tilemap.html)
- Unity 2D Extras 包：[RuleTile 和 AnimatedTile](https://github.com/Unity-Technologies/2d-extras)
- Unity 示例项目：[2D Platformer Microgame](https://learn.unity.com/project/2d-platformer-microgame)
