# Unity Tilemap 基础教程

## 概述

Tilemap是Unity用于创建2D网格地图的系统，特别适合像素游戏、平台游戏、策略游戏和塔防游戏。本教程将详细介绍Tilemap的核心概念和使用方法。

---

## 核心概念

### 1. Grid（网格）
Grid是Tilemap的父组件，定义了网格的布局方式（矩形、六边形、等距）。

### 2. Tilemap（瓦片地图）
Tilemap是具体的地图层，每个Tilemap包含：
- 瓦片数据（位置和类型）
- 渲染信息（Sprite、颜色）
- 碰撞信息

### 3. Tile（瓦片）
Tile是单个格子单元，包含：
- Sprite（精灵图）
- Collider（碰撞器）
- Color（颜色）
- Transform（变换）

---

## 快速开始

### 第一步：创建Tilemap

**方法1：通过菜单创建（推荐）**
```
Hierarchy右键 → 2D Object → Tilemap → Rectangular
```
这会自动创建：
- Grid（网格容器）
- Tilemap（瓦片地图）
- TilemapRenderer（渲染器）

**方法2：手动创建**
1. 创建空对象命名为"Grid"
2. 添加Grid组件
3. 创建子对象命名为"Tilemap"
4. 添加Tilemap组件和TilemapRenderer组件

### 第二步：创建Tile资源

**方法1：使用Tile Palette（推荐）**
1. 打开Tile Palette窗口：`Window → 2D → Tile Palette`
2. 创建新的Palette：`Create New Palette`
3. 将Sprite拖入Palette，自动生成Tile资源

**方法2：手动创建Tile**
1. Project窗口右键：`Create → Tiles → Tile`
2. 配置Tile属性（Sprite、Color、Collider Type）
3. 保存为.asset文件

### 第三步：绘制地图

使用Tile Palette工具：
1. 选择Active Tilemap（目标地图层）
2. 选择绘制工具（Brush）
3. 在Scene视图中点击绘制

---

## 工作流程

### 标准流程

```
1. 创建Grid和Tilemap
   ↓
2. 准备Tile资源（创建或导入）
   ↓
3. 使用Tile Palette绘制地图
   ↓
4. 添加TilemapCollider2D（如需碰撞）
   ↓
5. 使用CompositeCollider2D优化（可选）
```

### 多层地图

常见的分层方案：

```
Grid/
├── Background Tilemap (背景层)
│   └── TilemapRenderer: Order = -10
├── Ground Tilemap (地面层)
│   └── TilemapRenderer: Order = 0
│   └── TilemapCollider2D (碰撞)
├── Decoration Tilemap (装饰层)
│   └── TilemapRenderer: Order = 5
└── Foreground Tilemap (前景层)
    └── TilemapRenderer: Order = 10
```

**设置渲染顺序：**
```csharp
// 通过代码设置
backgroundTilemap.GetComponent<TilemapRenderer>().sortOrder = -10;
groundTilemap.GetComponent<TilemapRenderer>().sortOrder = 0;
foregroundTilemap.GetComponent<TilemapRenderer>().sortOrder = 10;

// 或在Inspector中设置：
// TilemapRenderer → Order in Layer
```

---

## Tile Palette 工具

### 主要工具

| 工具 | 快捷键 | 功能 |
|------|--------|------|
| Select | S | 选择瓦片 |
| Move | M | 移动瓦片 |
| Brush | B | 绘制瓦片 |
| Box Fill | U | 矩形填充 |
| Eraser | D | 擦除瓦片 |
| Flood Fill | G | 泛洪填充 |

### 创建自定义Brush

```csharp
using UnityEditor.Tilemaps;
using UnityEngine;

[CreateAssetMenu(fileName = "CustomBrush", menuName = "Brushes/Custom Brush")]
public class CustomBrush : GridBrush {
    public override void Paint(GridLayout grid, GameObject brushTarget, Vector3Int position) {
        // 自定义绘制逻辑
        base.Paint(grid, brushTarget, position);
    }
}
```

---

## Tile Assets 配置

### 基础属性

**Sprite：** 瓦片显示的精灵图  
**Color：** 瓦片颜色（默认白色）  
**Collider Type：** 碰撞器类型
- None - 无碰撞
- Sprite - 根据Sprite形状生成
- Grid - 根据网格形状生成

### 高级属性

**Flags：**
- None - 无特殊标志
- LockColor - 锁定颜色，无法通过脚本修改
- LockTransform - 锁定变换

**Transform：** 瓦片的变换矩阵（旋转、缩放）

---

## 碰撞系统

### 添加基础碰撞

```csharp
// 1. 添加TilemapCollider2D
TilemapCollider2D collider = tilemap.GetComponent<TilemapCollider2D>();
if (collider == null) {
    collider = tilemap.gameObject.AddComponent<TilemapCollider2D>();
}

// 2. 设置Collider Type（在Tile资源上）
tile.colliderType = Tile.ColliderType.Sprite;
```

### 使用复合碰撞器（推荐）

**优点：**
- 合并相邻瓦片的碰撞器
- 减少碰撞器数量80-90%
- 显著提升物理性能

**设置步骤：**
```csharp
// 1. 添加TilemapCollider2D
TilemapCollider2D tilemapCollider = tilemap.gameObject.AddComponent<TilemapCollider2D>();
tilemapCollider.usedByComposite = true;

// 2. 添加CompositeCollider2D
CompositeCollider2D compositeCollider = tilemap.gameObject.AddComponent<CompositeCollider2D>();

// 3. 需要Rigidbody2D（自动添加）
Rigidbody2D rb = tilemap.gameObject.GetComponent<Rigidbody2D>();
rb.bodyType = RigidbodyType2D.Static; // 静态碰撞

// 4. 优化设置
compositeCollider.geometryType = CompositeCollider2D.GeometryType.Polygons;
compositeCollider.generationType = CompositeCollider2D.GenerationType.Manual;
```

---

## 坐标系统详解

### 三种坐标系

**1. 世界坐标（World Position）**
- 场景中的实际位置
- Vector3类型
- 例如：(1.5f, 2.3f, 0f)

**2. 网格坐标（Cell Position）**
- 瓦片网格中的整数坐标
- Vector3Int类型
- 例如：(1, 2, 0)

**3. 本地坐标（Local Position）**
- 相对于Grid的本地坐标
- Vector3类型

### 坐标转换

```csharp
// 世界坐标 → 网格坐标
Vector3 worldPos = new Vector3(1.5f, 2.3f, 0f);
Vector3Int cellPos = tilemap.WorldToCell(worldPos);

// 网格坐标 → 世界坐标（瓦片左下角）
Vector3 worldPosBottomLeft = tilemap.CellToWorld(cellPos);

// 网格坐标 → 瓦片中心世界坐标
Vector3 worldCenter = tilemap.GetCellCenterWorld(cellPos);

// 网格坐标 → 本地坐标
Vector3 localPos = tilemap.CellToLocal(cellPos);
```

### 实用示例

**鼠标点击获取瓦片：**
```csharp
void Update() {
    if (Input.GetMouseButtonDown(0)) {
        // 1. 获取鼠标世界坐标
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0; // 2D游戏z=0
        
        // 2. 转换为网格坐标
        Vector3Int cellPos = tilemap.WorldToCell(mouseWorldPos);
        
        // 3. 获取瓦片
        TileBase tile = tilemap.GetTile(cellPos);
        if (tile != null) {
            Debug.Log($"点击了瓦片: {cellPos}");
        }
    }
}
```

---

## 程序化生成地图

### 批量设置瓦片

```csharp
using UnityEngine.Tilemaps;
using UnityEngine;

public class MapGenerator : MonoBehaviour {
    public Tilemap tilemap;
    public TileBase grassTile;
    public int width = 50;
    public int height = 50;
    
    void Start() {
        GenerateMap();
    }
    
    void GenerateMap() {
        // ✅ 正确：使用SetTiles批量设置
        Vector3Int[] positions = new Vector3Int[width * height];
        TileBase[] tiles = new TileBase[width * height];
        
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                int index = x + y * width;
                positions[index] = new Vector3Int(x, y, 0);
                tiles[index] = grassTile;
            }
        }
        
        tilemap.SetTiles(positions, tiles);
    }
}
```

### 使用Perlin噪声生成地形

```csharp
public class TerrainGenerator : MonoBehaviour {
    public Tilemap tilemap;
    public TileBase grassTile;
    public TileBase waterTile;
    public TileBase sandTile;
    
    public float noiseScale = 0.1f;
    public int width = 100;
    public int height = 100;
    
    void Start() {
        GenerateTerrain();
    }
    
    void GenerateTerrain() {
        Vector3Int[] positions = new Vector3Int[width * height];
        TileBase[] tiles = new TileBase[width * height];
        
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                int index = x + y * width;
                positions[index] = new Vector3Int(x, y, 0);
                
                // 使用Perlin噪声
                float noise = Mathf.PerlinNoise(x * noiseScale, y * noiseScale);
                
                // 根据噪声值选择瓦片
                if (noise < 0.3f) {
                    tiles[index] = waterTile;
                } else if (noise < 0.4f) {
                    tiles[index] = sandTile;
                } else {
                    tiles[index] = grassTile;
                }
            }
        }
        
        tilemap.SetTiles(positions, tiles);
    }
}
```

---

## 运行时修改地图

### 安全的修改方法

```csharp
public class MapModifier : MonoBehaviour {
    public Tilemap tilemap;
    public TileBase newTile;
    
    // ✅ 正确：单次修改（少量瓦片）
    public void ChangeTile(Vector3Int position, TileBase tile) {
        tilemap.SetTile(position, tile);
    }
    
    // ✅ 正确：批量修改
    public void ChangeTiles(Vector3Int[] positions, TileBase[] tiles) {
        tilemap.SetTiles(positions, tiles);
    }
    
    // ✅ 正确：清除特定区域
    public void ClearArea(BoundsInt area) {
        TileBase[] emptyTiles = new TileBase[area.size.x * area.size.y * area.size.z];
        tilemap.SetTilesBlock(area, emptyTiles);
    }
    
    // ⚠️ 警告：仅在必要时使用
    public void ClearAll() {
        tilemap.ClearAllTiles(); // 性能开销大
    }
}
```

---

## 常见用例

### 1. 2D平台游戏

```csharp
// 创建多层地图
public class PlatformMapSetup : MonoBehaviour {
    void Start() {
        // 背景层
        Tilemap background = CreateTilemap("Background", -10);
        
        // 地面层（有碰撞）
        Tilemap ground = CreateTilemap("Ground", 0);
        AddCollider(ground, true); // 使用复合碰撞器
        
        // 装饰层
        Tilemap decoration = CreateTilemap("Decoration", 5);
    }
    
    Tilemap CreateTilemap(string name, int sortOrder) {
        GameObject go = new GameObject(name);
        go.transform.parent = transform;
        
        Tilemap tilemap = go.AddComponent<Tilemap>();
        TilemapRenderer renderer = go.AddComponent<TilemapRenderer>();
        renderer.sortOrder = sortOrder;
        
        return tilemap;
    }
    
    void AddCollider(Tilemap tilemap, bool useComposite) {
        TilemapCollider2D collider = tilemap.gameObject.AddComponent<TilemapCollider2D>();
        collider.usedByComposite = useComposite;
        
        if (useComposite) {
            CompositeCollider2D composite = tilemap.gameObject.AddComponent<CompositeCollider2D>();
            Rigidbody2D rb = tilemap.gameObject.GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Static;
        }
    }
}
```

### 2. 塔防游戏

```csharp
public class TowerDefenseMap : MonoBehaviour {
    public Tilemap pathTilemap;
    public Tilemap buildableTilemap;
    public TileBase pathTile;
    public TileBase buildableTile;
    
    // 标记可建造区域
    public void MarkBuildableArea(Vector3Int position) {
        if (pathTilemap.GetTile(position) == null) {
            buildableTilemap.SetTile(position, buildableTile);
        }
    }
    
    // 检查是否可建造
    public bool CanBuild(Vector3Int position) {
        return buildableTilemap.HasTile(position);
    }
    
    // 建造塔
    public void BuildTower(Vector3Int position, GameObject towerPrefab) {
        if (CanBuild(position)) {
            Vector3 worldPos = buildableTilemap.GetCellCenterWorld(position);
            Instantiate(towerPrefab, worldPos, Quaternion.identity);
            buildableTilemap.SetTile(position, null); // 移除可建造标记
        }
    }
}
```

### 3. 农场游戏

```csharp
public class FarmingSystem : MonoBehaviour {
    public Tilemap farmTilemap;
    public TileBase soilTile;
    public TileBase wateredTile;
    public TileBase plantedTile;
    
    // 耕地
    public void TillSoil(Vector3Int position) {
        farmTilemap.SetTile(position, soilTile);
    }
    
    // 浇水
    public void WaterSoil(Vector3Int position) {
        if (farmTilemap.GetTile(position) == soilTile) {
            farmTilemap.SetTile(position, wateredTile);
        }
    }
    
    // 种植
    public void Plant(Vector3Int position) {
        if (farmTilemap.GetTile(position) == wateredTile) {
            farmTilemap.SetTile(position, plantedTile);
        }
    }
}
```

---

## 调试技巧

### 可视化网格

```csharp
void OnDrawGizmosSelected() {
    if (tilemap == null) return;
    
    BoundsInt bounds = tilemap.cellBounds;
    Gizmos.color = Color.green;
    
    for (int x = bounds.xMin; x < bounds.xMax; x++) {
        for (int y = bounds.yMin; y < bounds.yMax; y++) {
            Vector3Int cellPos = new Vector3Int(x, y, 0);
            Vector3 center = tilemap.GetCellCenterWorld(cellPos);
            
            if (tilemap.HasTile(cellPos)) {
                Gizmos.DrawWireCube(center, Vector3.one * 0.9f);
            }
        }
    }
}
```

### 打印地图信息

```csharp
void PrintMapInfo() {
    BoundsInt bounds = tilemap.cellBounds;
    int tileCount = 0;
    
    for (int x = bounds.xMin; x < bounds.xMax; x++) {
        for (int y = bounds.yMin; y < bounds.yMax; y++) {
            if (tilemap.HasTile(new Vector3Int(x, y, 0))) {
                tileCount++;
            }
        }
    }
    
    Debug.Log($"地图尺寸: {bounds.size}");
    Debug.Log($"瓦片数量: {tileCount}");
    Debug.Log($"原点: {bounds.position}");
}
```

---

## 常见问题

### Q1: 瓦片显示不出来？
**解决方案：**
1. 检查Sprite是否正确设置
2. 检查TilemapRenderer的Order in Layer
3. 检查Camera是否在正确的Z位置（通常-10）

### Q2: 碰撞器不生效？
**解决方案：**
1. 检查Tile的Collider Type是否为Sprite或Grid
2. 检查TilemapCollider2D是否已添加
3. 如果使用CompositeCollider2D，检查Rigidbody2D是否为Static

### Q3: 性能太差？
**解决方案：**
1. 使用SetTiles替代循环中的SetTile
2. 使用CompositeCollider2D合并碰撞器
3. 静态地图使用Chunk渲染模式

### Q4: 如何保存地图数据？
**解决方案：**
```csharp
[System.Serializable]
public class TileData {
    public Vector3Int position;
    public string tileName;
}

public class MapSaver : MonoBehaviour {
    public List<TileData> SaveMap(Tilemap tilemap) {
        List<TileData> data = new List<TileData>();
        BoundsInt bounds = tilemap.cellBounds;
        
        for (int x = bounds.xMin; x < bounds.xMax; x++) {
            for (int y = bounds.yMin; y < bounds.yMax; y++) {
                Vector3Int pos = new Vector3Int(x, y, 0);
                TileBase tile = tilemap.GetTile(pos);
                if (tile != null) {
                    data.Add(new TileData {
                        position = pos,
                        tileName = tile.name
                    });
                }
            }
        }
        
        return data;
    }
}
```

---

## 进阶主题

### 自定义Tile

```csharp
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "NewAnimatedTile", menuName = "Tiles/Animated Tile")]
public class AnimatedTile : Tile {
    public Sprite[] animatedSprites;
    public float animationSpeed = 1f;
    public float animationStartTime;
    
    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData) {
        // 计算当前帧
        int frame = (int)((Time.time - animationStartTime) * animationSpeed);
        frame = frame % animatedSprites.Length;
        
        tileData.sprite = animatedSprites[frame];
        tileData.color = Color.white;
        tileData.flags = TileFlags.LockTransform;
        tileData.colliderType = colliderType;
    }
}
```

---

## 参考资料

- [Unity Tilemap官方文档](https://docs.unity3d.com/Manual/Tilemap.html)
- [Tile Palette用户指南](https://docs.unity3d.com/Manual/Tilemap-Palette.html)
- [Tilemap脚本API](https://docs.unity3d.com/ScriptReference/Tilemaps.Tilemap.html)

---

## 版本说明

- 教程版本：1.0
- Unity版本：2021.3 LTS+
- 最后更新：2024年
