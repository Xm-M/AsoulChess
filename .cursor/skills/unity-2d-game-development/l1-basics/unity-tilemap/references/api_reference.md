# Unity Tilemap API 完整参考文档

## 概述

Tilemap系统是Unity用于创建2D网格地图的核心系统，主要用于像素游戏、策略游戏、塔防游戏等。本文档提供完整的API参考，包括性能优化建议和白名单分类。

---

## 核心组件

### 1. Tilemap 组件

Tilemap是核心组件，负责存储和渲染瓦片数据。

**主要属性：**
- `origin` - Tilemap的起始坐标（左下角）
- `size` - Tilemap的尺寸（宽度x高度）
- `cellBounds` - Tilemap的边界范围
- `localBounds` - 本地空间边界

**关键方法：**

#### ✅ 推荐使用的API（高效）

##### SetTiles - 批量设置瓦片
```csharp
// ✅ 推荐：批量设置，性能最优
public void SetTiles(Vector3Int[] positions, TileBase[] tiles)
```
- **性能等级：** ⭐⭐⭐⭐⭐（最优）
- **适用场景：** 批量生成地图、大型地图初始化
- **性能对比：** 比SetTile逐个设置快10-50倍
- **注意事项：** 一次性设置大量瓦片，减少网格重建次数

##### GetTilesBlock - 批量获取瓦片
```csharp
// ✅ 推荐：批量获取，高效
public TileBase[] GetTilesBlock(BoundsInt bounds)
```
- **性能等级：** ⭐⭐⭐⭐⭐
- **适用场景：** 批量读取瓦片数据、地图序列化

##### BoxFill - 填充矩形区域
```csharp
// ✅ 推荐：快速填充区域
public void BoxFill(Vector3Int position, TileBase tile, 
                    int startX, int startY, int endX, int endY)
```
- **性能等级：** ⭐⭐⭐⭐
- **适用场景：** 快速填充背景、创建平台

#### ⚠️ 性能警告的API（需谨慎使用）

##### SetTile - 单个瓦片设置
```csharp
// ⚠️ 警告：逐个设置性能较差，避免在循环中使用
public void SetTile(Vector3Int position, TileBase tile)
```
- **性能等级：** ⭐⭐
- **问题：** 每次调用都会触发网格重建
- **正确用法：** 仅用于少量、单次修改
- **错误用法：** 在for循环中调用（导致严重卡顿）

**性能对比示例：**
```csharp
// ❌ 错误：在循环中使用SetTile（非常慢）
for (int x = 0; x < 100; x++) {
    for (int y = 0; y < 100; y++) {
        tilemap.SetTile(new Vector3Int(x, y, 0), tile); // 每次100ms+
    }
}

// ✅ 正确：使用SetTiles批量设置（快50倍）
Vector3Int[] positions = new Vector3Int[10000];
TileBase[] tiles = new TileBase[10000];
for (int i = 0; i < 10000; i++) {
    positions[i] = new Vector3Int(i % 100, i / 100, 0);
    tiles[i] = tile;
}
tilemap.SetTiles(positions, tiles); // 总共仅100ms
```

##### GetTile - 单个瓦片获取
```csharp
// ⚠️ 警告：频繁调用有性能开销
public TileBase GetTile(Vector3Int position)
```
- **性能等级：** ⭐⭐⭐
- **正确用法：** 少量查询、碰撞检测
- **错误用法：** 每帧大量调用

#### ❌ 禁止使用的API

##### ResizeBounds - 运行时调整边界
```csharp
// ❌ 禁止：运行时调用会导致严重性能问题
public void ResizeBounds()
```
- **原因：** 会重新计算整个网格，极其耗时
- **替代方案：** 预先设置足够大的地图范围

##### RefreshAllTiles - 刷新所有瓦片
```csharp
// ❌ 禁止：会导致整个地图重新渲染
public void RefreshAllTiles()
```
- **原因：** 性能灾难，可能导致游戏卡顿数秒
- **替代方案：** 使用RefreshTile刷新特定瓦片

---

### 2. TilemapRenderer 组件

控制瓦片的渲染方式和排序。

**主要属性：**
- `mode` - 渲染模式（Chunk/Individual）
- `chunkMode` - 批渲染模式（性能优化）
- `sortOrder` - 排序顺序
- `maskInteraction` - 遮罩交互

**渲染模式对比：**

| 模式 | 性能 | 适用场景 | 注意事项 |
|------|------|----------|----------|
| Chunk | ⭐⭐⭐⭐⭐ | 静态地图 | 最高性能，适合背景 |
| Individual | ⭐⭐⭐ | 动态地图 | 支持单个瓦片动画 |

**推荐设置：**
```csharp
// ✅ 静态背景使用Chunk模式
tilemapRenderer.mode = TilemapRenderer.Mode.Chunk;

// ✅ 动态瓦片使用Individual模式（但会降低性能）
tilemapRenderer.mode = TilemapRenderer.Mode.Individual;
```

---

### 3. TileBase 和 Tile 类

#### TileBase（抽象基类）
所有瓦片类型的基类，必须继承实现。

**关键方法：**
```csharp
public abstract void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData);
public virtual void RefreshTile(Vector3Int position, ITilemap tilemap);
public virtual bool StartUp(Vector3Int position, ITilemap tilemap, GameObject go);
```

#### Tile（常用实现）
Unity提供的标准瓦片实现。

**主要属性：**
- `sprite` - 瓦片精灵
- `color` - 瓦片颜色
- `transform` - 变换矩阵（旋转、缩放）
- `gameObject` - 关联的游戏对象
- `flags` - 瓦片标志
- `colliderType` - 碰撞器类型

**创建自定义Tile：**
```csharp
[CreateAssetMenu(fileName = "NewTile", menuName = "Tiles/CustomTile")]
public class CustomTile : Tile {
    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData) {
        base.GetTileData(position, tilemap, ref tileData);
        // 自定义瓦片数据
        tileData.color = Color.white;
        tileData.transform = Matrix4x4.identity;
    }
}
```

---

## 碰撞系统

### TilemapCollider2D

为Tilemap添加2D碰撞器。

**关键属性：**
- `usedByComposite` - 是否使用复合碰撞器
- `usedByEffector` - 是否被效果器使用
- `offset` - 碰撞器偏移
- `maximumTileChangeCount` - 最大瓦片变更计数（性能相关）

#### ✅ 推荐配置（高性能）

```csharp
// ✅ 使用CompositeCollider2D优化碰撞性能
TilemapCollider2D tilemapCollider = tilemap.GetComponent<TilemapCollider2D>();
tilemapCollider.usedByComposite = true;

// 添加CompositeCollider2D
CompositeCollider2D compositeCollider = tilemap.gameObject.AddComponent<CompositeCollider2D>();
compositeCollider.geometryType = CompositeCollider2D.GeometryType.Polygons;
compositeCollider.generationType = CompositeCollider2D.GenerationType.Manual;

// 需要Rigidbody2D
Rigidbody2D rb = tilemap.gameObject.GetComponent<Rigidbody2D>();
rb.bodyType = RigidbodyType2D.Static;
```

**性能优势：**
- 合并相邻瓦片的碰撞器
- 减少碰撞器数量80-90%
- 显著提升物理性能

---

## 坐标系统

### 世界坐标 ↔ 网格坐标转换

```csharp
// ✅ 推荐：使用WorldToCell和CellToWorld
Vector3Int cellPosition = tilemap.WorldToCell(worldPosition);
Vector3 worldPosition = tilemap.CellToWorld(cellPosition);

// 获取瓦片中心位置（推荐用于角色移动）
Vector3 cellCenter = tilemap.GetCellCenterWorld(cellPosition);
```

**注意事项：**
- `CellToWorld` 返回的是瓦片左下角位置
- `GetCellCenterWorld` 返回瓦片中心位置（更常用）

---

## 实用工具方法

### 1. 遍历所有瓦片

```csharp
// ✅ 推荐：使用cellBounds遍历
BoundsInt bounds = tilemap.cellBounds;
for (int x = bounds.xMin; x < bounds.xMax; x++) {
    for (int y = bounds.yMin; y < bounds.yMax; y++) {
        Vector3Int pos = new Vector3Int(x, y, 0);
        TileBase tile = tilemap.GetTile(pos);
        if (tile != null) {
            // 处理瓦片
        }
    }
}
```

### 2. 清空地图

```csharp
// ✅ 推荐：批量清空
BoundsInt bounds = tilemap.cellBounds;
TileBase[] emptyTiles = new TileBase[bounds.size.x * bounds.size.y];
tilemap.SetTilesBlock(bounds, emptyTiles);

// ⚠️ 警告：仅用于小地图
tilemap.ClearAllTiles(); // 性能较差
```

### 3. 检测瓦片是否存在

```csharp
// ✅ 推荐：使用HasTile
bool hasTile = tilemap.HasTile(cellPosition);

// ❌ 避免：使用GetTile != null（稍慢）
TileBase tile = tilemap.GetTile(cellPosition);
bool hasTile = tile != null;
```

---

## 性能优化清单

### ✅ 推荐做法

1. **使用批量API**
   - `SetTiles` 替代循环中的 `SetTile`
   - `GetTilesBlock` 替代循环中的 `GetTile`

2. **使用复合碰撞器**
   - 启用 `usedByComposite` 减少碰撞器数量
   - 使用 `CompositeCollider2D` 合并碰撞器

3. **选择合适的渲染模式**
   - 静态地图使用 `Chunk` 模式
   - 动态瓦片使用 `Individual` 模式

4. **使用对象池管理瓦片**
   - 预加载常用瓦片资源
   - 避免运行时动态加载

5. **合理设置地图大小**
   - 避免创建过大的Tilemap
   - 分块加载大型地图

### ❌ 禁止做法

1. **禁止在Update中调用SetTile**
   ```csharp
   // ❌ 严重错误
   void Update() {
       tilemap.SetTile(pos, tile); // 每帧触发网格重建
   }
   ```

2. **禁止在循环中使用SetTile**
   ```csharp
   // ❌ 错误
   for (int i = 0; i < 1000; i++) {
       tilemap.SetTile(positions[i], tiles[i]); // 性能灾难
   }
   
   // ✅ 正确
   tilemap.SetTiles(positions, tiles); // 批量设置
   ```

3. **禁止频繁调用RefreshAllTiles**
   - 仅在初始化时调用一次
   - 使用 `RefreshTile` 刷新特定瓦片

4. **禁止运行时ResizeBounds**
   - 预先规划地图大小
   - 使用固定的地图范围

---

## 常见问题

### Q1: 为什么SetTile在循环中很慢？
**A:** 每次SetTile都会触发网格重建，在循环中会导致重复重建。使用SetTiles批量设置只重建一次。

### Q2: 如何优化Tilemap碰撞性能？
**A:** 使用CompositeCollider2D合并碰撞器，可以将数百个碰撞器合并为一个，提升物理性能。

### Q3: Chunk模式和Individual模式有什么区别？
**A:** Chunk模式将瓦片合并为一个网格渲染，性能最高但无法单独修改瓦片。Individual模式允许单个瓦片修改，但性能较低。

### Q4: 如何实现动态瓦片动画？
**A:** 创建继承Tile的自定义Tile类，在GetTileData中根据时间返回不同的sprite。

---

## 相关API文档

- [Tilemap Component](https://docs.unity3d.com/Manual/class-Tilemap.html)
- [TilemapRenderer](https://docs.unity3d.com/Manual/class-TilemapRenderer.html)
- [TileBase Scripting API](https://docs.unity3d.com/ScriptReference/Tilemaps.TileBase.html)
- [Tile Scripting API](https://docs.unity3d.com/ScriptReference/Tilemaps.Tile.html)

---

## 版本说明

- 文档版本：1.0
- Unity版本：2021.3 LTS+
- 最后更新：2024年
