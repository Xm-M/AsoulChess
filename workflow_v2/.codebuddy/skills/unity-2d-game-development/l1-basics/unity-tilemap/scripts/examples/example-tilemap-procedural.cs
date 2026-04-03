using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// 示例：使用Perlin噪声程序化生成地形
/// 演示如何使用SetTiles批量API高效生成大型地图
/// 
/// 性能要点：
/// - ✅ 使用SetTiles批量设置（比SetTile快50倍）
/// - ✅ 一次性生成所有瓦片数据
/// - ✅ 使用噪声函数创建自然地形
/// </summary>
public class ExampleTilemapProcedural : MonoBehaviour
{
    [Header("Tilemap引用")]
    public Tilemap tilemap;
    
    [Header("瓦片资源")]
    public TileBase waterTile;
    public TileBase sandTile;
    public TileBase grassTile;
    public TileBase forestTile;
    public TileBase mountainTile;
    
    [Header("生成参数")]
    [Tooltip("地图宽度")]
    public int width = 100;
    
    [Tooltip("地图高度")]
    public int height = 100;
    
    [Tooltip("噪声缩放 - 值越小地形越平缓")]
    [Range(0.01f, 0.5f)]
    public float noiseScale = 0.05f;
    
    [Tooltip("噪声种子 - 不同种子产生不同地形")]
    public int seed = 12345;
    
    [Tooltip("使用随机种子")]
    public bool useRandomSeed = true;
    
    [Header("地形阈值")]
    [Tooltip("低于此值为水域")]
    [Range(0f, 1f)]
    public float waterThreshold = 0.3f;
    
    [Tooltip("低于此值为沙滩")]
    [Range(0f, 1f)]
    public float sandThreshold = 0.35f;
    
    [Tooltip("低于此值为草地")]
    [Range(0f, 1f)]
    public float grassThreshold = 0.6f;
    
    [Tooltip("低于此值为森林")]
    [Range(0f, 1f)]
    public float forestThreshold = 0.75f;

    void Start()
    {
        GenerateMap();
    }

    /// <summary>
    /// 生成程序化地图
    /// 使用SetTiles批量API，性能最优
    /// </summary>
    public void GenerateMap()
    {
        if (tilemap == null)
        {
            Debug.LogError("Tilemap未设置！");
            return;
        }

        // 设置种子
        float seedOffset = useRandomSeed ? Random.Range(0f, 10000f) : seed;

        // 准备批量数据
        Vector3Int[] positions = new Vector3Int[width * height];
        TileBase[] tiles = new TileBase[width * height];

        // 生成地形数据
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int index = x + y * width;
                positions[index] = new Vector3Int(x, y, 0);
                
                // 计算噪声值
                float noise = Mathf.PerlinNoise(
                    (x + seedOffset) * noiseScale,
                    (y + seedOffset) * noiseScale
                );
                
                // 根据阈值选择瓦片
                tiles[index] = GetTileForNoise(noise);
            }
        }

        // ✅ 批量设置瓦片（性能最优）
        tilemap.SetTiles(positions, tiles);
        
        Debug.Log($"地图生成完成！尺寸：{width}x{height}，瓦片数：{width * height}");
    }

    /// <summary>
    /// 根据噪声值获取对应的瓦片
    /// </summary>
    TileBase GetTileForNoise(float noise)
    {
        if (noise < waterThreshold)
            return waterTile;
        else if (noise < sandThreshold)
            return sandTile;
        else if (noise < grassThreshold)
            return grassTile;
        else if (noise < forestThreshold)
            return forestTile;
        else
            return mountainTile;
    }

    /// <summary>
    /// 重新生成地图（带新种子）
    /// </summary>
    [ContextMenu("重新生成地图")]
    public void RegenerateMap()
    {
        if (useRandomSeed)
        {
            seed = Random.Range(0, 10000);
        }
        
        // 清空现有地图
        tilemap.ClearAllTiles();
        
        // 重新生成
        GenerateMap();
    }

    /// <summary>
    /// 在指定位置生成小岛
    /// 使用径向渐变创建岛屿形状
    /// </summary>
    public void GenerateIsland(Vector2Int center, int radius)
    {
        // 计算边界
        int startX = Mathf.Max(0, center.x - radius);
        int endX = Mathf.Min(width - 1, center.x + radius);
        int startY = Mathf.Max(0, center.y - radius);
        int endY = Mathf.Min(height - 1, center.y + radius);
        
        int sizeX = endX - startX + 1;
        int sizeY = endY - startY + 1;
        
        Vector3Int[] positions = new Vector3Int[sizeX * sizeY];
        TileBase[] tiles = new TileBase[sizeX * sizeY];
        
        int index = 0;
        for (int x = startX; x <= endX; x++)
        {
            for (int y = startY; y <= endY; y++)
            {
                positions[index] = new Vector3Int(x, y, 0);
                
                // 计算到中心的距离
                float distance = Vector2.Distance(new Vector2(x, y), center);
                float normalizedDistance = distance / radius;
                
                // 创建径向渐变
                if (normalizedDistance <= 1f)
                {
                    float noise = Mathf.PerlinNoise(x * noiseScale, y * noiseScale);
                    // 结合距离和噪声
                    float islandNoise = noise * (1f - normalizedDistance * 0.5f);
                    tiles[index] = GetTileForNoise(islandNoise);
                }
                else
                {
                    tiles[index] = waterTile;
                }
                
                index++;
            }
        }
        
        // ✅ 批量设置
        tilemap.SetTiles(positions, tiles);
    }

    /// <summary>
    /// 添加细节装饰（随机放置）
    /// 在现有地图上添加装饰物
    /// </summary>
    public void AddDecorations(TileBase decorationTile, int count, float probability)
    {
        System.Collections.Generic.List<Vector3Int> positions = 
            new System.Collections.Generic.List<Vector3Int>();
        System.Collections.Generic.List<TileBase> tiles = 
            new System.Collections.Generic.List<TileBase>();
        
        for (int i = 0; i < count; i++)
        {
            int x = Random.Range(0, width);
            int y = Random.Range(0, height);
            Vector3Int pos = new Vector3Int(x, y, 0);
            
            // 只在草地上放置装饰
            TileBase existingTile = tilemap.GetTile(pos);
            if (existingTile == grassTile && Random.value < probability)
            {
                positions.Add(pos);
                tiles.Add(decorationTile);
            }
        }
        
        // ✅ 批量设置装饰
        tilemap.SetTiles(positions.ToArray(), tiles.ToArray());
    }

    /// <summary>
    /// 生成河流（使用随机游走算法）
    /// </summary>
    public void GenerateRiver(Vector2Int startPos, int length, TileBase riverTile)
    {
        Vector2Int currentPos = startPos;
        System.Collections.Generic.List<Vector3Int> riverPositions = 
            new System.Collections.Generic.List<Vector3Int>();
        
        for (int i = 0; i < length; i++)
        {
            // 添加当前位置
            riverPositions.Add(new Vector3Int(currentPos.x, currentPos.y, 0));
            
            // 随机选择方向（偏向某个方向）
            Vector2Int[] directions = new Vector2Int[]
            {
                Vector2Int.up,
                Vector2Int.down,
                Vector2Int.left,
                Vector2Int.right,
                // 增加向下和向右的概率
                Vector2Int.down,
                Vector2Int.right
            };
            
            Vector2Int direction = directions[Random.Range(0, directions.Length)];
            currentPos += direction;
            
            // 边界检查
            currentPos.x = Mathf.Clamp(currentPos.x, 0, width - 1);
            currentPos.y = Mathf.Clamp(currentPos.y, 0, height - 1);
        }
        
        // ✅ 批量设置河流
        TileBase[] riverTiles = new TileBase[riverPositions.Count];
        for (int i = 0; i < riverTiles.Length; i++)
        {
            riverTiles[i] = riverTile;
        }
        
        tilemap.SetTiles(riverPositions.ToArray(), riverTiles);
    }

    /// <summary>
    /// 打印地图统计信息
    /// </summary>
    [ContextMenu("打印地图信息")]
    public void PrintMapInfo()
    {
        BoundsInt bounds = tilemap.cellBounds;
        
        int waterCount = 0;
        int sandCount = 0;
        int grassCount = 0;
        int forestCount = 0;
        int mountainCount = 0;
        
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                TileBase tile = tilemap.GetTile(new Vector3Int(x, y, 0));
                if (tile == waterTile) waterCount++;
                else if (tile == sandTile) sandCount++;
                else if (tile == grassTile) grassCount++;
                else if (tile == forestTile) forestCount++;
                else if (tile == mountainTile) mountainCount++;
            }
        }
        
        Debug.Log("=== 地图统计信息 ===");
        Debug.Log($"地图尺寸: {bounds.size}");
        Debug.Log($"水域: {waterCount} ({(float)waterCount / (width * height) * 100:F1}%)");
        Debug.Log($"沙滩: {sandCount} ({(float)sandCount / (width * height) * 100:F1}%)");
        Debug.Log($"草地: {grassCount} ({(float)grassCount / (width * height) * 100:F1}%)");
        Debug.Log($"森林: {forestCount} ({(float)forestCount / (width * height) * 100:F1}%)");
        Debug.Log($"山地: {mountainCount} ({(float)mountainCount / (width * height) * 100:F1}%)");
    }

    /// <summary>
    /// 编辑器按钮：在场景视图显示网格
    /// </summary>
    void OnDrawGizmosSelected()
    {
        if (tilemap == null) return;
        
        Gizmos.color = Color.yellow;
        Vector3 center = tilemap.GetCellCenterWorld(new Vector3Int(width / 2, height / 2, 0));
        Vector3 size = new Vector3(width, height, 1);
        Gizmos.DrawWireCube(center, size);
    }
}
