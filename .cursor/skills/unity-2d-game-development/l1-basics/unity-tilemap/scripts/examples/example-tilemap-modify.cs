using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// 示例：运行时动态修改Tilemap
/// 演示如何安全地修改地图，包括挖矿、种植、建造等常见功能
/// 
/// 性能要点：
/// - ✅ 少量修改使用SetTile
/// - ✅ 批量修改使用SetTiles
/// - ✅ 使用HasTile检查瓦片存在
/// - ⚠️ 避免在Update中频繁修改
/// </summary>
public class ExampleTilemapModify : MonoBehaviour
{
    [Header("Tilemap引用")]
    public Tilemap tilemap;
    
    [Header("瓦片资源")]
    public TileBase dirtTile;
    public TileBase grassTile;
    public TileBase stoneTile;
    public TileBase waterTile;
    
    [Header("交互设置")]
    [Tooltip("交互范围")]
    public float interactionRange = 2f;
    
    [Tooltip("玩家Transform")]
    public Transform player;

    /// <summary>
    /// 示例1：挖矿系统
    /// 移除瓦片并掉落物品
    /// </summary>
    public void MineTile(Vector3Int position)
    {
        // 检查瓦片是否存在
        if (!tilemap.HasTile(position))
        {
            Debug.Log("此处没有瓦片可挖掘");
            return;
        }
        
        // 检查距离
        if (!IsInRange(position))
        {
            Debug.Log("距离太远，无法挖掘");
            return;
        }
        
        // 获取瓦片类型（用于掉落物品）
        TileBase minedTile = tilemap.GetTile(position);
        
        // 移除瓦片
        tilemap.SetTile(position, null);
        
        // 生成掉落物（示例）
        SpawnDropItem(minedTile, position);
        
        Debug.Log($"挖掘了瓦片: {minedTile.name} at {position}");
    }

    /// <summary>
    /// 示例2：放置系统
    /// 在空位置放置瓦片
    /// </summary>
    public void PlaceTile(Vector3Int position, TileBase tileToPlace)
    {
        // 检查位置是否已有瓦片
        if (tilemap.HasTile(position))
        {
            Debug.Log("此处已有瓦片，无法放置");
            return;
        }
        
        // 检查距离
        if (!IsInRange(position))
        {
            Debug.Log("距离太远，无法放置");
            return;
        }
        
        // 放置瓦片
        tilemap.SetTile(position, tileToPlace);
        
        Debug.Log($"放置了瓦片: {tileToPlace.name} at {position}");
    }

    /// <summary>
    /// 示例3：种植系统
    /// 将草地变为耕地
    /// </summary>
    public void TillSoil(Vector3Int position)
    {
        TileBase currentTile = tilemap.GetTile(position);
        
        // 只能在草地上耕地
        if (currentTile != grassTile)
        {
            Debug.Log("只能在草地上耕地");
            return;
        }
        
        if (!IsInRange(position))
        {
            Debug.Log("距离太远");
            return;
        }
        
        // 替换为耕地
        tilemap.SetTile(position, dirtTile);
        Debug.Log($"耕地成功: {position}");
    }

    /// <summary>
    /// 示例4：批量填充区域
    /// 使用SetTiles批量修改（性能优化）
    /// </summary>
    public void FillArea(BoundsInt area, TileBase tile)
    {
        // ✅ 批量设置性能最优
        Vector3Int[] positions = new Vector3Int[area.size.x * area.size.y];
        TileBase[] tiles = new TileBase[area.size.x * area.size.y];
        
        int index = 0;
        for (int x = 0; x < area.size.x; x++)
        {
            for (int y = 0; y < area.size.y; y++)
            {
                positions[index] = new Vector3Int(
                    area.x + x,
                    area.y + y,
                    area.z
                );
                tiles[index] = tile;
                index++;
            }
        }
        
        tilemap.SetTiles(positions, tiles);
        Debug.Log($"填充区域: {area}, 瓦片数: {positions.Length}");
    }

    /// <summary>
    /// 示例5：爆炸效果
    /// 破坏范围内的所有瓦片
    /// </summary>
    public void Explosion(Vector3Int center, int radius)
    {
        System.Collections.Generic.List<Vector3Int> positions = 
            new System.Collections.Generic.List<Vector3Int>();
        
        // 圆形爆炸范围
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                Vector3Int pos = center + new Vector3Int(x, y, 0);
                
                // 检查是否在圆形范围内
                if (Vector3Int.Distance(center, pos) <= radius)
                {
                    if (tilemap.HasTile(pos))
                    {
                        positions.Add(pos);
                    }
                }
            }
        }
        
        // ✅ 批量清除
        TileBase[] emptyTiles = new TileBase[positions.Count];
        tilemap.SetTiles(positions.ToArray(), emptyTiles);
        
        Debug.Log($"爆炸破坏了 {positions.Count} 个瓦片");
    }

    /// <summary>
    /// 示例6：流动的水
    /// 模拟水流传播（简化版）
    /// </summary>
    public void FlowWater(Vector3Int source, int maxDistance)
    {
        System.Collections.Generic.Queue<Vector3Int> queue = 
            new System.Collections.Generic.Queue<Vector3Int>();
        System.Collections.Generic.HashSet<Vector3Int> visited = 
            new System.Collections.Generic.HashSet<Vector3Int>();
        
        System.Collections.Generic.List<Vector3Int> waterPositions = 
            new System.Collections.Generic.List<Vector3Int>();
        
        queue.Enqueue(source);
        visited.Add(source);
        waterPositions.Add(source);
        
        int distance = 0;
        
        while (queue.Count > 0 && distance < maxDistance)
        {
            int levelSize = queue.Count;
            
            for (int i = 0; i < levelSize; i++)
            {
                Vector3Int current = queue.Dequeue();
                
                // 尝试向四个方向流动
                Vector3Int[] directions = new Vector3Int[]
                {
                    Vector3Int.left,
                    Vector3Int.right,
                    Vector3Int.down,
                    Vector3Int.up
                };
                
                foreach (Vector3Int dir in directions)
                {
                    Vector3Int next = current + dir;
                    
                    // 边界检查
                    if (visited.Contains(next))
                        continue;
                    
                    // 检查是否可以流动（空位置）
                    if (!tilemap.HasTile(next))
                    {
                        visited.Add(next);
                        queue.Enqueue(next);
                        waterPositions.Add(next);
                    }
                }
            }
            
            distance++;
        }
        
        // ✅ 批量设置水
        TileBase[] waterTiles = new TileBase[waterPositions.Count];
        for (int i = 0; i < waterTiles.Length; i++)
        {
            waterTiles[i] = waterTile;
        }
        
        tilemap.SetTiles(waterPositions.ToArray(), waterTiles);
        
        Debug.Log($"水流传播了 {waterPositions.Count} 个瓦片");
    }

    /// <summary>
    /// 示例7：区域检测
    /// 检查区域内是否有特定类型的瓦片
    /// </summary>
    public bool AreaContainsTile(BoundsInt area, TileBase tileToCheck)
    {
        for (int x = area.xMin; x < area.xMax; x++)
        {
            for (int y = area.yMin; y < area.yMax; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, area.z);
                if (tilemap.GetTile(pos) == tileToCheck)
                {
                    return true;
                }
            }
        }
        
        return false;
    }

    /// <summary>
    /// 示例8：复制地图区域
    /// 将一个区域的瓦片复制到另一个位置
    /// </summary>
    public void CopyArea(BoundsInt sourceArea, Vector3Int destination)
    {
        // 读取源区域数据
        TileBase[] sourceTiles = tilemap.GetTilesBlock(sourceArea);
        
        // 创建目标位置数组
        Vector3Int[] destPositions = new Vector3Int[sourceTiles.Length];
        int index = 0;
        
        for (int x = 0; x < sourceArea.size.x; x++)
        {
            for (int y = 0; y < sourceArea.size.y; y++)
            {
                destPositions[index] = new Vector3Int(
                    destination.x + x,
                    destination.y + y,
                    destination.z
                );
                index++;
            }
        }
        
        // ✅ 批量设置到目标位置
        tilemap.SetTiles(destPositions, sourceTiles);
        
        Debug.Log($"复制了 {sourceTiles.Length} 个瓦片到 {destination}");
    }

    /// <summary>
    /// 示例9：瓦片交换
    /// 替换地图中所有特定类型的瓦片
    /// </summary>
    public void SwapTiles(TileBase oldTile, TileBase newTile)
    {
        System.Collections.Generic.List<Vector3Int> positions = 
            new System.Collections.Generic.List<Vector3Int>();
        
        BoundsInt bounds = tilemap.cellBounds;
        
        // 查找所有旧瓦片位置
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                if (tilemap.GetTile(pos) == oldTile)
                {
                    positions.Add(pos);
                }
            }
        }
        
        // ✅ 批量替换
        TileBase[] newTiles = new TileBase[positions.Count];
        for (int i = 0; i < newTiles.Length; i++)
        {
            newTiles[i] = newTile;
        }
        
        tilemap.SetTiles(positions.ToArray(), newTiles);
        
        Debug.Log($"替换了 {positions.Count} 个瓦片");
    }

    /// <summary>
    /// 示例10：保存和加载地图数据
    /// </summary>
    [System.Serializable]
    public class MapData
    {
        public System.Collections.Generic.List<Vector3Int> positions;
        public System.Collections.Generic.List<string> tileNames;
    }
    
    public MapData SaveMap()
    {
        MapData data = new MapData();
        data.positions = new System.Collections.Generic.List<Vector3Int>();
        data.tileNames = new System.Collections.Generic.List<string>();
        
        BoundsInt bounds = tilemap.cellBounds;
        
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                TileBase tile = tilemap.GetTile(pos);
                
                if (tile != null)
                {
                    data.positions.Add(pos);
                    data.tileNames.Add(tile.name);
                }
            }
        }
        
        Debug.Log($"保存了 {data.positions.Count} 个瓦片");
        return data;
    }
    
    public void LoadMap(MapData data, System.Collections.Generic.Dictionary<string, TileBase> tileDictionary)
    {
        // 清空现有地图
        tilemap.ClearAllTiles();
        
        // 准备批量数据
        Vector3Int[] positions = data.positions.ToArray();
        TileBase[] tiles = new TileBase[positions.Length];
        
        for (int i = 0; i < positions.Length; i++)
        {
            string tileName = data.tileNames[i];
            tiles[i] = tileDictionary.ContainsKey(tileName) ? tileDictionary[tileName] : null;
        }
        
        // ✅ 批量加载
        tilemap.SetTiles(positions, tiles);
        
        Debug.Log($"加载了 {positions.Length} 个瓦片");
    }

    // ========== 辅助方法 ==========

    /// <summary>
    /// 检查位置是否在交互范围内
    /// </summary>
    bool IsInRange(Vector3Int cellPosition)
    {
        if (player == null) return true;
        
        Vector3 cellWorldPos = tilemap.GetCellCenterWorld(cellPosition);
        float distance = Vector3.Distance(player.position, cellWorldPos);
        
        return distance <= interactionRange;
    }

    /// <summary>
    /// 生成掉落物品（示例）
    /// </summary>
    void SpawnDropItem(TileBase minedTile, Vector3Int position)
    {
        Vector3 worldPos = tilemap.GetCellCenterWorld(position);
        // 这里可以实例化掉落物品预制体
        Debug.Log($"生成掉落物: {minedTile.name} at {worldPos}");
    }

    /// <summary>
    /// 鼠标交互示例（需要在Update中调用）
    /// </summary>
    void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0)) // 左键挖掘
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;
            Vector3Int cellPos = tilemap.WorldToCell(mouseWorldPos);
            
            MineTile(cellPos);
        }
        else if (Input.GetMouseButtonDown(1)) // 右键放置
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;
            Vector3Int cellPos = tilemap.WorldToCell(mouseWorldPos);
            
            PlaceTile(cellPos, dirtTile);
        }
    }

    // 取消注释以启用鼠标交互
    // void Update()
    // {
    //     HandleMouseInput();
    // }
}
