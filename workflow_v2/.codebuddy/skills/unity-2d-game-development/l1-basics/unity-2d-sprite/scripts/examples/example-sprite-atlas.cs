using UnityEngine;
using UnityEngine.U2D;
using System.Collections.Generic;

/// <summary>
/// 图集使用示例
/// 演示如何使用SpriteAtlas动态加载和管理精灵
/// </summary>
public class ExampleSpriteAtlas : MonoBehaviour
{
    [Header("图集资源")]
    [Tooltip("SpriteAtlas资源引用")]
    public SpriteAtlas spriteAtlas;
    
    [Header("加载设置")]
    [Tooltip("要加载的精灵名称")]
    public string spriteNameToLoad = "player_idle";
    
    [Tooltip("是否在启动时加载所有精灵")]
    public bool loadAllOnStart = false;
    
    [Header("显示设置")]
    [Tooltip("显示精灵的SpriteRenderer")]
    public SpriteRenderer targetRenderer;
    
    [Tooltip("自动切换精灵的间隔时间")]
    public float autoSwitchInterval = 1f;
    
    // 已加载的精灵字典(名称 -> 精灵)
    private Dictionary<string, Sprite> loadedSprites = new Dictionary<string, Sprite>();
    
    // 所有精灵列表
    private List<Sprite> allSprites = new List<Sprite>();
    
    // 当前显示的精灵索引
    private int currentSpriteIndex = 0;
    
    // 自动切换计时器
    private float autoSwitchTimer = 0f;
    
    void Start()
    {
        // 检查图集是否指定
        if (spriteAtlas == null)
        {
            Debug.LogError("未指定SpriteAtlas! 请在Inspector中设置。");
            enabled = false;
            return;
        }
        
        // 获取SpriteRenderer组件
        if (targetRenderer == null)
        {
            targetRenderer = GetComponent<SpriteRenderer>();
            if (targetRenderer == null)
            {
                targetRenderer = gameObject.AddComponent<SpriteRenderer>();
                Debug.Log("已自动添加SpriteRenderer组件");
            }
        }
        
        // 输出图集信息
        PrintAtlasInfo();
        
        // 加载所有精灵到缓存
        if (loadAllOnStart)
        {
            LoadAllSpritesToCache();
        }
        
        // 加载初始精灵
        LoadSprite(spriteNameToLoad);
    }
    
    void Update()
    {
        // 自动切换精灵演示
        if (loadAllOnStart && allSprites.Count > 1)
        {
            autoSwitchTimer += Time.deltaTime;
            if (autoSwitchTimer >= autoSwitchInterval)
            {
                autoSwitchTimer = 0f;
                ShowNextSprite();
            }
        }
        
        // 键盘控制
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ShowNextSprite();
        }
        
        if (Input.GetKeyDown(KeyCode.R))
        {
            LoadRandomSprite();
        }
    }
    
    /// <summary>
    /// 打印图集信息
    /// </summary>
    void PrintAtlasInfo()
    {
        int spriteCount = spriteAtlas.spriteCount;
        Debug.Log($"=== SpriteAtlas 信息 ===");
        Debug.Log($"图集名称: {spriteAtlas.name}");
        Debug.Log($"精灵总数: {spriteCount}");
        Debug.Log($"======================");
    }
    
    /// <summary>
    /// 加载所有精灵到缓存
    /// 用于需要频繁切换精灵的场景
    /// </summary>
    void LoadAllSpritesToCache()
    {
        // 获取精灵总数
        int count = spriteAtlas.spriteCount;
        
        // 创建临时数组存储所有精灵
        Sprite[] sprites = new Sprite[count];
        spriteAtlas.GetSprites(sprites);
        
        // 清空并重新填充字典和列表
        loadedSprites.Clear();
        allSprites.Clear();
        
        foreach (Sprite sprite in sprites)
        {
            if (sprite != null)
            {
                // 去掉(Clone)后缀
                string spriteName = sprite.name.Replace("(Clone)", "");
                
                // 添加到字典
                if (!loadedSprites.ContainsKey(spriteName))
                {
                    loadedSprites[spriteName] = sprite;
                }
                
                // 添加到列表
                allSprites.Add(sprite);
            }
        }
        
        Debug.Log($"已缓存 {loadedSprites.Count} 个精灵");
    }
    
    /// <summary>
    /// 按名称加载精灵
    /// </summary>
    /// <param name="spriteName">精灵名称</param>
    /// <returns>是否加载成功</returns>
    public bool LoadSprite(string spriteName)
    {
        Sprite sprite = null;
        
        // 首先尝试从缓存获取
        if (loadedSprites.TryGetValue(spriteName, out sprite))
        {
            Debug.Log($"从缓存加载精灵: {spriteName}");
        }
        else
        {
            // 从图集获取
            sprite = spriteAtlas.GetSprite(spriteName);
            
            if (sprite != null)
            {
                // 添加到缓存
                loadedSprites[spriteName] = sprite;
                Debug.Log($"从图集加载精灵: {spriteName}");
            }
            else
            {
                Debug.LogError($"找不到精灵: {spriteName}");
                return false;
            }
        }
        
        // 显示精灵
        targetRenderer.sprite = sprite;
        return true;
    }
    
    /// <summary>
    /// 显示下一个精灵
    /// </summary>
    public void ShowNextSprite()
    {
        if (allSprites.Count == 0)
        {
            Debug.LogWarning("精灵列表为空!");
            return;
        }
        
        // 切换到下一个索引
        currentSpriteIndex = (currentSpriteIndex + 1) % allSprites.Count;
        
        // 显示精灵
        Sprite sprite = allSprites[currentSpriteIndex];
        targetRenderer.sprite = sprite;
        
        Debug.Log($"显示精灵 [{currentSpriteIndex + 1}/{allSprites.Count}]: {sprite.name}");
    }
    
    /// <summary>
    /// 显示上一个精灵
    /// </summary>
    public void ShowPreviousSprite()
    {
        if (allSprites.Count == 0)
        {
            Debug.LogWarning("精灵列表为空!");
            return;
        }
        
        // 切换到上一个索引
        currentSpriteIndex--;
        if (currentSpriteIndex < 0)
            currentSpriteIndex = allSprites.Count - 1;
        
        // 显示精灵
        Sprite sprite = allSprites[currentSpriteIndex];
        targetRenderer.sprite = sprite;
        
        Debug.Log($"显示精灵 [{currentSpriteIndex + 1}/{allSprites.Count}]: {sprite.name}");
    }
    
    /// <summary>
    /// 随机加载精灵
    /// </summary>
    public void LoadRandomSprite()
    {
        if (allSprites.Count == 0)
        {
            Debug.LogWarning("精灵列表为空!");
            return;
        }
        
        // 随机选择一个精灵
        int randomIndex = Random.Range(0, allSprites.Count);
        Sprite sprite = allSprites[randomIndex];
        
        targetRenderer.sprite = sprite;
        currentSpriteIndex = randomIndex;
        
        Debug.Log($"随机显示精灵: {sprite.name}");
    }
    
    /// <summary>
    /// 获取所有精灵名称
    /// </summary>
    /// <returns>精灵名称列表</returns>
    public List<string> GetAllSpriteNames()
    {
        return new List<string>(loadedSprites.Keys);
    }
    
    /// <summary>
    /// 获取指定精灵
    /// </summary>
    /// <param name="spriteName">精灵名称</param>
    /// <returns>精灵对象,如果不存在返回null</returns>
    public Sprite GetSprite(string spriteName)
    {
        Sprite sprite = null;
        
        // 尝试从缓存获取
        if (loadedSprites.TryGetValue(spriteName, out sprite))
        {
            return sprite;
        }
        
        // 从图集获取
        sprite = spriteAtlas.GetSprite(spriteName);
        if (sprite != null)
        {
            loadedSprites[spriteName] = sprite;
        }
        
        return sprite;
    }
    
    /// <summary>
    /// 清空精灵缓存
    /// </summary>
    public void ClearCache()
    {
        loadedSprites.Clear();
        Debug.Log("精灵缓存已清空");
    }
    
    /// <summary>
    /// 获取缓存中的精灵数量
    /// </summary>
    /// <returns>缓存的精灵数量</returns>
    public int GetCacheCount()
    {
        return loadedSprites.Count;
    }
    
    /// <summary>
    /// 异步加载图集中的所有精灵
    /// </summary>
    public System.Collections.IEnumerator LoadAllSpritesAsync()
    {
        int count = spriteAtlas.spriteCount;
        Sprite[] sprites = new Sprite[count];
        spriteAtlas.GetSprites(sprites);
        
        loadedSprites.Clear();
        allSprites.Clear();
        
        int loadedCount = 0;
        
        foreach (Sprite sprite in sprites)
        {
            if (sprite != null)
            {
                string spriteName = sprite.name.Replace("(Clone)", "");
                
                if (!loadedSprites.ContainsKey(spriteName))
                {
                    loadedSprites[spriteName] = sprite;
                }
                
                allSprites.Add(sprite);
                loadedCount++;
                
                // 每加载10个精灵暂停一帧
                if (loadedCount % 10 == 0)
                {
                    Debug.Log($"加载进度: {loadedCount}/{count}");
                    yield return null;
                }
            }
        }
        
        Debug.Log($"异步加载完成,共 {loadedSprites.Count} 个精灵");
    }
    
    /// <summary>
    /// 从Resources动态加载图集
    /// </summary>
    /// <param name="atlasPath">图集路径(相对于Resources文件夹)</param>
    /// <returns>是否加载成功</returns>
    public bool LoadAtlasFromResources(string atlasPath)
    {
        SpriteAtlas loadedAtlas = Resources.Load<SpriteAtlas>(atlasPath);
        
        if (loadedAtlas != null)
        {
            spriteAtlas = loadedAtlas;
            Debug.Log($"成功加载图集: {atlasPath}");
            
            // 重新缓存精灵
            LoadAllSpritesToCache();
            return true;
        }
        else
        {
            Debug.LogError($"找不到图集资源: {atlasPath}");
            return false;
        }
    }
    
    // 在Inspector中绘制辅助信息
    void OnDrawGizmosSelected()
    {
        if (targetRenderer != null && targetRenderer.sprite != null)
        {
            // 绘制精灵边界
            Gizmos.color = Color.cyan;
            Bounds bounds = targetRenderer.sprite.bounds;
            Gizmos.DrawWireCube(transform.position + bounds.center, bounds.size);
            
            // 显示精灵名称
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(
                transform.position + Vector3.up * bounds.size.y,
                $"Sprite: {targetRenderer.sprite.name}"
            );
            #endif
        }
    }
}