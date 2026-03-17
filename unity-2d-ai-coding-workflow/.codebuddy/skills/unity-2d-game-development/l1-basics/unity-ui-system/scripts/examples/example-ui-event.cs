using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 示例：UI事件处理
/// 演示如何使用接口实现UI事件处理（推荐方式）
/// </summary>
public class ExampleUIEvent : MonoBehaviour, 
    IPointerClickHandler, 
    IPointerDownHandler, 
    IPointerUpHandler,
    IPointerEnterHandler, 
    IPointerExitHandler,
    IDragHandler, 
    IBeginDragHandler, 
    IEndDragHandler,
    IScrollHandler
{
    [Header("UI引用")]
    [SerializeField] private Image background;
    [SerializeField] private TextMeshProUGUI infoText;
    
    [Header("拖拽设置")]
    [SerializeField] private bool enableDrag = true;
    [SerializeField] private Canvas canvas;
    
    [Header("视觉效果")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = new Color(0.9f, 0.9f, 0.9f);
    [SerializeField] private Color pressedColor = new Color(0.8f, 0.8f, 0.8f);
    
    private RectTransform rectTransform;
    private Vector2 originalPosition;
    private bool isDragging = false;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        
        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();
    }

    // ========== 点击事件 ==========

    /// <summary>
    /// 点击事件（完整点击：按下+抬起）
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"[点击] 位置: {eventData.position}, 按钮: {eventData.button}");
        
        // 区分左键、右键、中键
        switch (eventData.button)
        {
            case PointerEventData.InputButton.Left:
                Debug.Log("左键点击");
                break;
            case PointerEventData.InputButton.Right:
                Debug.Log("右键点击");
                break;
            case PointerEventData.InputButton.Middle:
                Debug.Log("中键点击");
                break;
        }
        
        // 更新信息文本
        if (infoText != null)
        {
            infoText.text = $"点击: {eventData.position}";
        }
    }

    /// <summary>
    /// 按下事件
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log($"[按下] 位置: {eventData.position}");
        
        // 视觉反馈
        if (background != null)
        {
            background.color = pressedColor;
        }
        
        // 记录原始位置（用于拖拽）
        originalPosition = rectTransform.anchoredPosition;
    }

    /// <summary>
    /// 抬起事件
    /// </summary>
    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log($"[抬起] 位置: {eventData.position}");
        
        // 恢复颜色
        if (background != null)
        {
            background.color = normalColor;
        }
    }

    // ========== 悬停事件 ==========

    /// <summary>
    /// 鼠标进入
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("[进入] 鼠标进入UI元素");
        
        // 视觉反馈
        if (background != null && !isDragging)
        {
            background.color = hoverColor;
        }
        
        // 显示提示信息
        if (infoText != null)
        {
            infoText.text = "鼠标悬停中...";
        }
    }

    /// <summary>
    /// 鼠标离开
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("[离开] 鼠标离开UI元素");
        
        // 恢复颜色
        if (background != null && !isDragging)
        {
            background.color = normalColor;
        }
        
        // 清除提示信息
        if (infoText != null)
        {
            infoText.text = "";
        }
    }

    // ========== 拖拽事件 ==========

    /// <summary>
    /// 开始拖拽
    /// </summary>
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!enableDrag) return;
        
        isDragging = true;
        Debug.Log($"[开始拖拽] 起始位置: {rectTransform.anchoredPosition}");
        
        // 视觉反馈
        if (background != null)
        {
            background.color = pressedColor;
        }
    }

    /// <summary>
    /// 拖拽中
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        if (!enableDrag || !isDragging) return;
        
        // 计算拖拽增量（考虑Canvas缩放）
        Vector2 delta = eventData.delta;
        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            // Screen Space Overlay模式：直接使用delta
            rectTransform.anchoredPosition += delta;
        }
        else
        {
            // Screen Space Camera或World Space模式：需要转换
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform.parent as RectTransform,
                eventData.position,
                canvas.worldCamera,
                out Vector2 localPoint
            );
            rectTransform.anchoredPosition = localPoint;
        }
        
        // 更新信息文本
        if (infoText != null)
        {
            infoText.text = $"拖拽中: {rectTransform.anchoredPosition}";
        }
    }

    /// <summary>
    /// 结束拖拽
    /// </summary>
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!enableDrag) return;
        
        isDragging = false;
        Debug.Log($"[结束拖拽] 最终位置: {rectTransform.anchoredPosition}");
        
        // 恢复颜色
        if (background != null)
        {
            background.color = normalColor;
        }
    }

    // ========== 滚轮事件 ==========

    /// <summary>
    /// 滚轮滚动
    /// </summary>
    public void OnScroll(PointerEventData eventData)
    {
        Vector2 scrollDelta = eventData.scrollDelta;
        Debug.Log($"[滚轮] 滚动量: {scrollDelta}");
        
        // 向上滚动为正值，向下滚动为负值
        if (scrollDelta.y > 0)
        {
            Debug.Log("向上滚动");
            // 放大UI
            rectTransform.localScale *= 1.1f;
        }
        else if (scrollDelta.y < 0)
        {
            Debug.Log("向下滚动");
            // 缩小UI
            rectTransform.localScale *= 0.9f;
        }
    }

    // ========== 辅助方法 ==========

    /// <summary>
    /// 重置位置
    /// </summary>
    [ContextMenu("Reset Position")]
    public void ResetPosition()
    {
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = originalPosition;
        }
    }

    /// <summary>
    /// 重置缩放
    /// </summary>
    [ContextMenu("Reset Scale")]
    public void ResetScale()
    {
        if (rectTransform != null)
        {
            rectTransform.localScale = Vector3.one;
        }
    }

    /// <summary>
    /// 检查鼠标是否在UI元素内
    /// </summary>
    public bool IsPointerOverUI(RectTransform rect, Vector2 screenPosition)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(
            rect,
            screenPosition,
            canvas.worldCamera
        );
    }

    /// <summary>
    /// 获取鼠标在RectTransform内的本地坐标
    /// </summary>
    public bool GetLocalPointFromScreen(Vector2 screenPosition, out Vector2 localPoint)
    {
        return RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform,
            screenPosition,
            canvas.worldCamera,
            out localPoint
        );
    }
}

/// <summary>
/// 示例：使用EventTrigger组件处理事件（Inspector配置方式）
/// </summary>
public class ExampleEventTrigger : MonoBehaviour
{
    [SerializeField] private EventTrigger eventTrigger;

    void Start()
    {
        if (eventTrigger == null)
            eventTrigger = GetComponent<EventTrigger>();
        
        // 添加点击事件
        AddEventTrigger(EventTriggerType.PointerClick, OnPointerClick);
        
        // 添加悬停事件
        AddEventTrigger(EventTriggerType.PointerEnter, OnPointerEnter);
        AddEventTrigger(EventTriggerType.PointerExit, OnPointerExit);
    }

    void AddEventTrigger(EventTriggerType eventType, UnityEngine.Events.UnityAction<BaseEventData> callback)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = eventType;
        entry.callback.AddListener(callback);
        eventTrigger.triggers.Add(entry);
    }

    void OnPointerClick(BaseEventData data)
    {
        PointerEventData pointerData = data as PointerEventData;
        Debug.Log($"点击: {pointerData.position}");
    }

    void OnPointerEnter(BaseEventData data)
    {
        Debug.Log("鼠标进入");
    }

    void OnPointerExit(BaseEventData data)
    {
        Debug.Log("鼠标离开");
    }
}
