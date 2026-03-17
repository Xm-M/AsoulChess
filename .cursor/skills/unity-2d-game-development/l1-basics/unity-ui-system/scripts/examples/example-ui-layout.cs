using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// 示例：动态布局管理
/// 演示如何使用代码控制UI布局，包括手动布局和自动布局
/// 
/// 性能要点：
/// - ⚠️ Layout Group会频繁触发重建，影响性能
/// - ✅ 推荐使用手动布局替代Layout Group
/// - ✅ 避免嵌套Layout Group
/// </summary>
public class ExampleUILayout : MonoBehaviour
{
    [Header("布局容器")]
    [SerializeField] private RectTransform container;
    
    [Header("布局设置")]
    [SerializeField] private float spacing = 10f;
    [SerializeField] private RectOffset padding = new RectOffset(10, 10, 10, 10);
    [SerializeField] private bool useLayoutGroup = false;
    
    [Header("子元素模板")]
    [SerializeField] private GameObject itemTemplate;
    
    private List<GameObject> items = new List<GameObject>();

    // ========== 手动布局（推荐，性能更好） ==========

    /// <summary>
    /// 水平布局
    /// </summary>
    public void HorizontalLayout()
    {
        float currentX = padding.left;
        
        for (int i = 0; i < items.Count; i++)
        {
            RectTransform itemRect = items[i].GetComponent<RectTransform>();
            
            // 设置位置
            Vector2 anchoredPosition = new Vector2(currentX, -padding.top);
            itemRect.anchoredPosition = anchoredPosition;
            
            // 累加X坐标
            currentX += itemRect.rect.width + spacing;
        }
        
        // 更新容器大小
        UpdateContainerSize(new Vector2(currentX + padding.right, 0));
    }

    /// <summary>
    /// 垂直布局
    /// </summary>
    public void VerticalLayout()
    {
        float currentY = -padding.top;
        
        for (int i = 0; i < items.Count; i++)
        {
            RectTransform itemRect = items[i].GetComponent<RectTransform>();
            
            // 设置位置（Y轴向下为负）
            Vector2 anchoredPosition = new Vector2(padding.left, currentY);
            itemRect.anchoredPosition = anchoredPosition;
            
            // 累加Y坐标（向下为负）
            currentY -= itemRect.rect.height + spacing;
        }
        
        // 更新容器大小
        UpdateContainerSize(new Vector2(0, Mathf.Abs(currentY) + padding.bottom));
    }

    /// <summary>
    /// 网格布局
    /// </summary>
    public void GridLayout(int columns)
    {
        if (columns <= 0) return;
        
        float startX = padding.left;
        float startY = -padding.top;
        float currentX = startX;
        float currentY = startY;
        float rowHeight = 0f;
        
        for (int i = 0; i < items.Count; i++)
        {
            RectTransform itemRect = items[i].GetComponent<RectTransform>();
            
            // 设置位置
            itemRect.anchoredPosition = new Vector2(currentX, currentY);
            
            // 记录行高
            rowHeight = Mathf.Max(rowHeight, itemRect.rect.height);
            
            // 移动到下一列
            currentX += itemRect.rect.width + spacing;
            
            // 换行
            if ((i + 1) % columns == 0)
            {
                currentX = startX;
                currentY -= rowHeight + spacing;
                rowHeight = 0f;
            }
        }
    }

    /// <summary>
    /// 环形布局
    /// </summary>
    public void CircularLayout(float radius)
    {
        float angleStep = 360f / items.Count;
        
        for (int i = 0; i < items.Count; i++)
        {
            RectTransform itemRect = items[i].GetComponent<RectTransform>();
            
            // 计算角度（弧度）
            float angle = (angleStep * i) * Mathf.Deg2Rad;
            
            // 计算位置
            float x = Mathf.Cos(angle) * radius;
            float y = Mathf.Sin(angle) * radius;
            
            itemRect.anchoredPosition = new Vector2(x, y);
        }
    }

    /// <summary>
    /// 更新容器大小
    /// </summary>
    void UpdateContainerSize(Vector2 size)
    {
        if (container != null)
        {
            container.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
            container.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
        }
    }

    // ========== Layout Group自动布局（不推荐，性能差） ==========

    /// <summary>
    /// 使用VerticalLayoutGroup自动布局
    /// ⚠️ 警告：性能较差，频繁触发重建
    /// </summary>
    public void AutoVerticalLayout()
    {
        // 添加VerticalLayoutGroup组件
        VerticalLayoutGroup layoutGroup = container.GetComponent<VerticalLayoutGroup>();
        if (layoutGroup == null)
        {
            layoutGroup = container.gameObject.AddComponent<VerticalLayoutGroup>();
        }
        
        // 配置布局
        layoutGroup.spacing = spacing;
        layoutGroup.padding = padding;
        layoutGroup.childControlWidth = true;
        layoutGroup.childControlHeight = false;
        layoutGroup.childForceExpandWidth = true;
        layoutGroup.childForceExpandHeight = false;
        
        // ⚠️ 强制重建布局（性能开销大）
        LayoutRebuilder.ForceRebuildLayoutImmediate(container);
    }

    /// <summary>
    /// 使用GridLayoutGroup自动布局
    /// ⚠️ 警告：性能较差
    /// </summary>
    public void AutoGridLayout()
    {
        GridLayoutGroup gridLayout = container.GetComponent<GridLayoutGroup>();
        if (gridLayout == null)
        {
            gridLayout = container.gameObject.AddComponent<GridLayoutGroup>();
        }
        
        // 配置网格
        gridLayout.cellSize = new Vector2(100, 100);
        gridLayout.spacing = new Vector2(spacing, spacing);
        gridLayout.padding = padding;
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = 3;
    }

    // ========== 子元素管理 ==========

    /// <summary>
    /// 添加子元素
    /// </summary>
    public void AddItem()
    {
        if (itemTemplate == null || container == null) return;
        
        // 实例化新元素
        GameObject newItem = Instantiate(itemTemplate, container);
        newItem.SetActive(true);
        newItem.name = $"Item_{items.Count}";
        
        items.Add(newItem);
        
        // 更新布局
        if (useLayoutGroup)
        {
            // 使用LayoutRebuilder标记需要重建
            LayoutRebuilder.MarkLayoutForRebuild(container);
        }
        else
        {
            // 手动更新布局
            VerticalLayout();
        }
    }

    /// <summary>
    /// 移除子元素
    /// </summary>
    public void RemoveItem(int index)
    {
        if (index < 0 || index >= items.Count) return;
        
        GameObject itemToRemove = items[index];
        items.RemoveAt(index);
        
        // 销毁元素
        if (Application.isPlaying)
        {
            Destroy(itemToRemove);
        }
        else
        {
            DestroyImmediate(itemToRemove);
        }
        
        // 更新布局
        if (useLayoutGroup)
        {
            LayoutRebuilder.MarkLayoutForRebuild(container);
        }
        else
        {
            VerticalLayout();
        }
    }

    /// <summary>
    /// 清空所有子元素
    /// </summary>
    public void ClearAllItems()
    {
        foreach (var item in items)
        {
            if (Application.isPlaying)
            {
                Destroy(item);
            }
            else
            {
                DestroyImmediate(item);
            }
        }
        
        items.Clear();
    }

    /// <summary>
    /// 批量添加子元素
    /// </summary>
    public void AddItems(int count)
    {
        for (int i = 0; i < count; i++)
        {
            AddItem();
        }
    }

    // ========== 对象池优化 ==========

    /// <summary>
    /// 使用对象池创建列表（性能优化）
    /// </summary>
    public void CreateListWithPool(int count)
    {
        // 清空现有元素
        ClearAllItems();
        
        // 预创建对象池
        Queue<GameObject> pool = new Queue<GameObject>();
        
        for (int i = 0; i < count; i++)
        {
            GameObject item;
            
            if (pool.Count > 0)
            {
                // 从池中取出
                item = pool.Dequeue();
                item.SetActive(true);
            }
            else
            {
                // 创建新对象
                item = Instantiate(itemTemplate, container);
            }
            
            items.Add(item);
        }
        
        // 更新布局
        VerticalLayout();
    }

    // ========== 编辑器工具 ==========

    [ContextMenu("添加10个元素")]
    public void AddTenItems()
    {
        AddItems(10);
    }

    [ContextMenu("清空所有元素")]
    public void ClearAll()
    {
        ClearAllItems();
    }

    [ContextMenu("垂直布局")]
    public void ApplyVerticalLayout()
    {
        VerticalLayout();
    }

    [ContextMenu("网格布局(3列)")]
    public void ApplyGridLayout()
    {
        GridLayout(3);
    }

    [ContextMenu("环形布局")]
    public void ApplyCircularLayout()
    {
        CircularLayout(200f);
    }

    [ContextMenu("打印子元素数量")]
    public void PrintItemCount()
    {
        Debug.Log($"子元素数量: {items.Count}");
    }
}

/// <summary>
/// 示例：虚拟列表（大数据列表优化）
/// 适用于显示大量数据（如排行榜、背包等）
/// </summary>
public class ExampleVirtualList : MonoBehaviour
{
    [Header("配置")]
    [SerializeField] private RectTransform viewport;
    [SerializeField] private RectTransform content;
    [SerializeField] private GameObject itemTemplate;
    [SerializeField] private int totalItemCount = 1000;
    [SerializeField] private float itemHeight = 50f;
    
    private List<GameObject> visibleItems = new List<GameObject>();
    private ScrollRect scrollRect;
    private int firstVisibleIndex = 0;
    
    void Start()
    {
        scrollRect = viewport.GetComponentInParent<ScrollRect>();
        if (scrollRect != null)
        {
            scrollRect.onValueChanged.AddListener(OnScroll);
        }
        
        // 设置Content大小
        content.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Vertical,
            totalItemCount * itemHeight
        );
        
        // 创建可见项
        CreateVisibleItems();
    }

    void CreateVisibleItems()
    {
        // 计算可见区域大小
        float viewportHeight = viewport.rect.height;
        int visibleCount = Mathf.CeilToInt(viewportHeight / itemHeight) + 2;
        
        // 创建可见的Item
        for (int i = 0; i < visibleCount; i++)
        {
            GameObject item = Instantiate(itemTemplate, content);
            item.SetActive(true);
            visibleItems.Add(item);
        }
        
        // 更新Item位置和内容
        UpdateVisibleItems();
    }

    void OnScroll(Vector2 position)
    {
        // 计算第一个可见Item的索引
        float scrollY = content.anchoredPosition.y;
        firstVisibleIndex = Mathf.FloorToInt(scrollY / itemHeight);
        firstVisibleIndex = Mathf.Max(0, firstVisibleIndex);
        
        // 更新可见Item
        UpdateVisibleItems();
    }

    void UpdateVisibleItems()
    {
        for (int i = 0; i < visibleItems.Count; i++)
        {
            int dataIndex = firstVisibleIndex + i;
            
            if (dataIndex >= 0 && dataIndex < totalItemCount)
            {
                GameObject item = visibleItems[i];
                
                // 设置位置
                RectTransform rect = item.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(0, -dataIndex * itemHeight);
                
                // 更新内容（这里可以根据dataIndex显示不同内容）
                // item.GetComponent<ItemUI>().SetData(dataIndex);
                
                item.SetActive(true);
            }
            else
            {
                visibleItems[i].SetActive(false);
            }
        }
    }

    void OnDestroy()
    {
        if (scrollRect != null)
        {
            scrollRect.onValueChanged.RemoveListener(OnScroll);
        }
    }
}
