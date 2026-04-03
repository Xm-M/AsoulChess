using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Unity Tilemap 基础使用示例
/// 演示 Tilemap 的基本操作：设置瓦片、获取瓦片、清除瓦片
/// </summary>
public class ExampleTilemapBasic : MonoBehaviour
{
    [Header("Tilemap 引用")]
    [Tooltip("Tilemap 组件的引用")]
    public Tilemap tilemap;

    [Header("瓦片资源")]
    [Tooltip("草地瓦片")]
    public TileBase grassTile;
    
    [Tooltip("水域瓦片")]
    public TileBase waterTile;
    
    [Tooltip("石头瓦片")]
    public TileBase stoneTile;

    [Header("地图设置")]
    [Tooltip("地图宽度")]
    public int mapWidth = 20;
    
    [Tooltip("地图高度")]
    public int mapHeight = 15;

    private void Start()
    {
        // 如果没有手动分配 Tilemap，尝试自动获取
        if (tilemap == null)
        {
            tilemap = GetComponent<Tilemap>();
            if (tilemap == null)
            {
                Debug.LogError("未找到 Tilemap 组件！请在 Inspector 中分配或确保脚本附加在 Tilemap 对象上。");
                return;
            }
        }

        // 创建基础地图
        CreateBasicMap();
    }

    /// <summary>
    /// 创建基础地图
    /// 上半部分是草地，下半部分是水域
    /// </summary>
    private void CreateBasicMap()
    {
        Debug.Log("开始创建基础地图...");

        // 创建位置和瓦片数组
        Vector3Int[] positions = new Vector3Int[mapWidth * mapHeight];
        TileBase[] tiles = new TileBase[mapWidth * mapHeight];

        // 填充数组
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                int index = y * mapWidth + x;
                positions[index] = new Vector3Int(x, y, 0);

                // 简单的地形分割
                if (y >= mapHeight / 2)
                {
                    tiles[index] = grassTile; // 上半部分：草地
                }
                else
                {
                    tiles[index] = waterTile; // 下半部分：水域
                }
            }
        }

        // 批量设置瓦片
        tilemap.SetTiles(positions, tiles);

        Debug.Log($"地图创建完成！尺寸: {mapWidth}x{mapHeight}");
    }

    /// <summary>
    /// 在指定位置设置单个瓦片
    /// </summary>
    /// <param name="x">X 坐标</param>
    /// <param name="y">Y 坐标</param>
    /// <param name="tile">要设置的瓦片</param>
    public void SetTileAt(int x, int y, TileBase tile)
    {
        Vector3Int position = new Vector3Int(x, y, 0);
        tilemap.SetTile(position, tile);
        Debug.Log($"在位置 ({x}, {y}) 设置瓦片");
    }

    /// <summary>
    /// 获取指定位置的瓦片
    /// </summary>
    /// <param name="x">X 坐标</param>
    /// <param name="y">Y 坐标</param>
    /// <returns>该位置的瓦片，如果为空则返回 null</returns>
    public TileBase GetTileAt(int x, int y)
    {
        Vector3Int position = new Vector3Int(x, y, 0);
        TileBase tile = tilemap.GetTile(position);
        
        if (tile != null)
        {
            Debug.Log($"位置 ({x}, {y}) 的瓦片: {tile.name}");
        }
        else
        {
            Debug.Log($"位置 ({x}, {y}) 没有瓦片");
        }
        
        return tile;
    }

    /// <summary>
    /// 清除指定位置的瓦片
    /// </summary>
    /// <param name="x">X 坐标</param>
    /// <param name="y">Y 坐标</param>
    public void ClearTileAt(int x, int y)
    {
        Vector3Int position = new Vector3Int(x, y, 0);
        tilemap.SetTile(position, null); // 传入 null 清除瓦片
        Debug.Log($"清除位置 ({x}, {y}) 的瓦片");
    }

    /// <summary>
    /// 随机放置一些石头瓦片
    /// </summary>
    /// <param name="count">要放置的石头数量</param>
    public void PlaceRandomStones(int count)
    {
        if (stoneTile == null)
        {
            Debug.LogWarning("未分配石头瓦片！");
            return;
        }

        for (int i = 0; i < count; i++)
        {
            // 随机生成位置（在草地范围内）
            int x = Random.Range(0, mapWidth);
            int y = Random.Range(mapHeight / 2, mapHeight);
            
            SetTileAt(x, y, stoneTile);
        }

        Debug.Log($"随机放置了 {count} 个石头");
    }

    /// <summary>
    /// 检查指定位置是否为水域
    /// </summary>
    /// <param name="x">X 坐标</param>
    /// <param name="y">Y 坐标</param>
    /// <returns>是否为水域</returns>
    public bool IsWaterAt(int x, int y)
    {
        TileBase tile = GetTileAt(x, y);
        return tile == waterTile;
    }

    /// <summary>
    /// 检查指定位置是否为草地
    /// </summary>
    /// <param name="x">X 坐标</param>
    /// <param name="y">Y 坐标</param>
    /// <returns>是否为草地</returns>
    public bool IsGrassAt(int x, int y)
    {
        TileBase tile = GetTileAt(x, y);
        return tile == grassTile;
    }

    /// <summary>
    /// 获取地图边界的 BoundsInt
    /// </summary>
    /// <returns>地图边界</returns>
    public BoundsInt GetMapBounds()
    {
        return new BoundsInt(0, 0, 0, mapWidth, mapHeight, 1);
    }

    /// <summary>
    /// 将所有水域替换为草地
    /// </summary>
    public void FillWaterToGrass()
    {
        tilemap.SwapTile(waterTile, grassTile);
        Debug.Log("将所有水域替换为草地");
    }

    /// <summary>
    /// 清除整个地图的所有瓦片
    /// </summary>
    public void ClearMap()
    {
        tilemap.ClearAllTiles();
        Debug.Log("地图已清空");
    }

    /// <summary>
    /// 重新生成地图
    /// </summary>
    public void RegenerateMap()
    {
        ClearMap();
        CreateBasicMap();
        PlaceRandomStones(10); // 放置一些石头
        Debug.Log("地图已重新生成");
    }

    // ============================================
    // 在 Unity 编辑器中使用的辅助方法（仅编辑器）
    // ============================================
    #if UNITY_EDITOR
    [Header("编辑器工具")]
    [Tooltip("在编辑器中快速测试功能")]
    [SerializeField] private bool editorTestMode = false;

    private void OnValidate()
    {
        if (editorTestMode && Application.isPlaying)
        {
            editorTestMode = false;
            RegenerateMap();
        }
    }
    #endif
}
