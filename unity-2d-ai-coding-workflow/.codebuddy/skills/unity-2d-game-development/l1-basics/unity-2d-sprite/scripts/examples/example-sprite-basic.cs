using UnityEngine;

/// <summary>
/// 基础Sprite显示示例
/// 演示SpriteRenderer的基本使用方法
/// </summary>
public class ExampleSpriteBasic : MonoBehaviour
{
    [Header("精灵资源")]
    [Tooltip("要显示的精灵图片")]
    public Sprite spriteToDisplay;
    
    [Header("颜色设置")]
    [Tooltip("精灵颜色")]
    public Color spriteColor = Color.white;
    
    [Header("排序设置")]
    [Tooltip("排序图层名称")]
    public string sortingLayerName = "Default";
    
    [Tooltip("排序顺序(数值越大越靠前)")]
    public int sortingOrder = 0;
    
    // SpriteRenderer组件引用
    private SpriteRenderer spriteRenderer;
    
    void Start()
    {
        // 获取或添加SpriteRenderer组件
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            Debug.Log("已自动添加SpriteRenderer组件");
        }
        
        // 应用初始设置
        ApplySpriteSettings();
        
        // 示例: 动态加载精灵
        // LoadSpriteFromResources();
    }
    
    /// <summary>
    /// 应用精灵设置
    /// </summary>
    void ApplySpriteSettings()
    {
        // 设置精灵图片
        if (spriteToDisplay != null)
        {
            spriteRenderer.sprite = spriteToDisplay;
        }
        else
        {
            Debug.LogWarning("未指定精灵图片,请在Inspector中设置spriteToDisplay");
        }
        
        // 设置颜色
        spriteRenderer.color = spriteColor;
        
        // 设置排序图层
        spriteRenderer.sortingLayerName = sortingLayerName;
        
        // 设置排序顺序
        spriteRenderer.sortingOrder = sortingOrder;
    }
    
    /// <summary>
    /// 从Resources文件夹动态加载精灵
    /// 使用方法: 将精灵放在Resources/Sprites文件夹中
    /// </summary>
    /// <param name="spriteName">精灵资源名称(不含扩展名)</param>
    public void LoadSpriteFromResources(string spriteName = "player_idle")
    {
        // 从Resources加载精灵
        Sprite loadedSprite = Resources.Load<Sprite>($"Sprites/{spriteName}");
        
        if (loadedSprite != null)
        {
            spriteRenderer.sprite = loadedSprite;
            Debug.Log($"成功加载精灵: {spriteName}");
        }
        else
        {
            Debug.LogError($"找不到精灵资源: Sprites/{spriteName}");
        }
    }
    
    /// <summary>
    /// 更改精灵颜色
    /// 可在运行时动态调用
    /// </summary>
    /// <param name="newColor">新的颜色</param>
    public void ChangeColor(Color newColor)
    {
        spriteColor = newColor;
        spriteRenderer.color = spriteColor;
        Debug.Log($"颜色已更改为: {newColor}");
    }
    
    /// <summary>
    /// 更改精灵图片
    /// 可在运行时动态调用
    /// </summary>
    /// <param name="newSprite">新的精灵</param>
    public void ChangeSprite(Sprite newSprite)
    {
        if (newSprite != null)
        {
            spriteToDisplay = newSprite;
            spriteRenderer.sprite = spriteToDisplay;
            Debug.Log($"精灵已更改为: {newSprite.name}");
        }
    }
    
    /// <summary>
    /// 更改排序设置
    /// 可在运行时动态调用
    /// </summary>
    /// <param name="layerName">排序图层名称</param>
    /// <param name="order">排序顺序</param>
    public void ChangeSortingSettings(string layerName, int order)
    {
        sortingLayerName = layerName;
        sortingOrder = order;
        
        spriteRenderer.sortingLayerName = sortingLayerName;
        spriteRenderer.sortingOrder = sortingOrder;
        
        Debug.Log($"排序设置已更改 - 图层: {layerName}, 顺序: {order}");
    }
    
    /// <summary>
    /// 设置精灵透明度
    /// </summary>
    /// <param name="alpha">透明度值(0-1)</param>
    public void SetAlpha(float alpha)
    {
        Color color = spriteRenderer.color;
        color.a = Mathf.Clamp01(alpha);
        spriteRenderer.color = color;
        Debug.Log($"透明度已设置为: {alpha}");
    }
    
    /// <summary>
    /// 获取精灵尺寸(世界单位)
    /// </summary>
    /// <returns>精灵的宽度和高度</returns>
    public Vector2 GetSpriteSize()
    {
        if (spriteRenderer.sprite != null)
        {
            Vector2 size = spriteRenderer.sprite.bounds.size;
            Debug.Log($"精灵尺寸: 宽={size.x}, 高={size.y}");
            return size;
        }
        return Vector2.zero;
    }
    
    // 在Inspector中修改值时自动应用
    void OnValidate()
    {
        // 确保在编辑器模式下也能实时看到变化
        if (spriteRenderer != null)
        {
            ApplySpriteSettings();
        }
    }
    
    // 绘制Gizmos显示精灵边界
    void OnDrawGizmosSelected()
    {
        if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            // 获取精灵边界
            Bounds bounds = spriteRenderer.sprite.bounds;
            
            // 绘制边界框
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position + bounds.center, bounds.size);
        }
    }
}