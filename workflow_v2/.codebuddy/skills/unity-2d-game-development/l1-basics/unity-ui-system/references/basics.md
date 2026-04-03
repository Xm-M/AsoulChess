# Unity UI System 基础教程

## 快速开始

### 1. 创建Canvas

```
Hierarchy右键 → UI → Canvas
```

Canvas是所有UI元素的根容器，会自动创建EventSystem。

### 2. Canvas渲染模式

**Screen Space - Overlay（推荐）：**
- UI直接渲染在屏幕上
- 不受相机影响
- 性能最好

**Screen Space - Camera：**
- UI渲染在相机前方
- 可以有3D效果（如模糊、光照）
- 需要指定相机

**World Space：**
- UI作为3D对象存在于场景
- 适用于游戏内UI（如NPC对话框）

### 3. 创建UI元素

```
Hierarchy右键 → UI → Button
Hierarchy右键 → UI → Text - TextMeshPro
Hierarchy右键 → UI → Image
Hierarchy右键 → UI → Slider
Hierarchy右键 → UI → Toggle
```

---

## 核心概念

### RectTransform（矩形变换）

RectTransform是UI元素的变换组件，控制位置、大小、锚点。

**关键属性：**
- `anchoredPosition` - 相对于锚点的位置
- `sizeDelta` - UI元素的尺寸
- `anchorMin / anchorMax` - 锚点位置
- `pivot` - 中心点位置
- `offsetMin / offsetMax` - 锚点偏移

### Anchor（锚点）

锚点决定UI元素相对于父容器的位置和大小。

**常用锚点预设：**

| 预设 | 效果 | 使用场景 |
|------|------|----------|
| 左上角 | 固定在左上角 | 标题、Logo |
| 居中 | 固定在中心 | 弹窗、按钮 |
| 拉伸 | 填充整个父容器 | 背景、面板 |
| 底部拉伸 | 底部拉伸 | 底部导航栏 |

### Pivot（中心点）

中心点决定UI元素的旋转和缩放中心。

- (0, 0) - 左下角
- (0.5, 0.5) - 中心（默认）
- (1, 1) - 右上角

---

## Canvas Scaler（画布缩放）

Canvas Scaller用于UI适配不同分辨率。

**推荐设置：**

```csharp
// ✅ 推荐：按参考分辨率缩放
canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
canvasScaler.referenceResolution = new Vector2(1920, 1080);
canvasScaler.matchWidthOrHeight = 0.5f; // 0=宽度优先, 1=高度优先
```

**matchWidthOrHeight参数：**
- `0` - 宽度优先（适配横屏）
- `1` - 高度优先（适配竖屏）
- `0.5` - 平衡（推荐）

---

## 常用UI组件

### 1. Image（图像）

```csharp
using UnityEngine.UI;

[SerializeField] private Image image;

// 设置Sprite
image.sprite = sprite;

// 设置颜色
image.color = Color.white;

// 九宫格切片
image.type = Image.Type.Sliced;

// 填充（血条、进度条）
image.type = Image.Type.Filled;
image.fillAmount = 0.5f; // 50%填充
```

### 2. TextMeshPro（文本）

```csharp
using TMPro;

[SerializeField] private TextMeshProUGUI text;

// 设置文本
text.text = "Hello World";

// 设置字体大小
text.fontSize = 24;

// 设置颜色
text.color = Color.white;

// 富文本
text.text = "<color=red>红色文字</color> <b>粗体</b>";

// 对齐方式
text.alignment = TextAlignmentOptions.Center;
```

### 3. Button（按钮）

```csharp
using UnityEngine.UI;

[SerializeField] private Button button;

void OnEnable() {
    button.onClick.AddListener(OnButtonClick);
}

void OnDisable() {
    button.onClick.RemoveListener(OnButtonClick);
}

void OnButtonClick() {
    Debug.Log("Button clicked");
}
```

### 4. Slider（滑动条）

```csharp
using UnityEngine.UI;

[SerializeField] private Slider slider;

void Start() {
    slider.minValue = 0f;
    slider.maxValue = 100f;
    slider.value = 50f;
    
    slider.onValueChanged.AddListener(OnSliderChanged);
}

void OnSliderChanged(float value) {
    Debug.Log($"Slider value: {value}");
}
```

### 5. Toggle（开关）

```csharp
using UnityEngine.UI;

[SerializeField] private Toggle toggle;

void Start() {
    toggle.isOn = true;
    toggle.onValueChanged.AddListener(OnToggleChanged);
}

void OnToggleChanged(bool isOn) {
    Debug.Log($"Toggle is: {isOn}");
}
```

---

## UI事件处理

### 方式1：Inspector配置（简单）

在Button组件的Inspector中：
1. 点击"+"添加事件
2. 拖拽脚本对象
3. 选择要调用的方法

### 方式2：代码订阅（推荐）

```csharp
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Slider slider;
    
    void OnEnable() {
        button.onClick.AddListener(OnButtonClick);
        slider.onValueChanged.AddListener(OnSliderChanged);
    }
    
    void OnDisable() {
        button.onClick.RemoveListener(OnButtonClick);
        slider.onValueChanged.RemoveListener(OnSliderChanged);
    }
    
    void OnButtonClick() {
        Debug.Log("Button clicked");
    }
    
    void OnSliderChanged(float value) {
        Debug.Log($"Slider value: {value}");
    }
}
```

### 方式3：接口实现（高级）

```csharp
using UnityEngine;
using UnityEngine.EventSystems;

public class UIEventHandler : MonoBehaviour, IPointerClickHandler, IDragHandler
{
    public void OnPointerClick(PointerEventData eventData) {
        Debug.Log($"Clicked at: {eventData.position}");
    }
    
    public void OnDrag(PointerEventData eventData) {
        transform.position = eventData.position;
    }
}
```

**常用接口：**
- `IPointerClickHandler` - 点击
- `IPointerDownHandler` - 按下
- `IPointerUpHandler` - 抬起
- `IPointerEnterHandler` - 鼠标进入
- `IPointerExitHandler` - 鼠标离开
- `IDragHandler` - 拖拽
- `IBeginDragHandler` - 开始拖拽
- `IEndDragHandler` - 结束拖拽
- `IScrollHandler` - 滚轮

---

## 布局系统

### Layout Group（自动布局）

**Horizontal Layout Group（水平布局）：**
```
UI → Layout → Horizontal Layout Group
```

**Vertical Layout Group（垂直布局）：**
```
UI → Layout → Vertical Layout Group
```

**Grid Layout Group（网格布局）：**
```
UI → Layout → Grid Layout Group
```

**示例：**

```csharp
using UnityEngine.UI;

[SerializeField] private VerticalLayoutGroup layoutGroup;

void Start() {
    layoutGroup.spacing = 10f;         // 间距
    layoutGroup.padding = new RectOffset(10, 10, 10, 10); // 内边距
    layoutGroup.childAlignment = TextAnchor.UpperCenter;  // 对齐方式
    layoutGroup.childControlWidth = true;  // 控制子对象宽度
    layoutGroup.childControlHeight = false;
    layoutGroup.childForceExpandWidth = true; // 强制扩展宽度
}
```

### Content Size Fitter（内容大小适配）

```csharp
using UnityEngine.UI;

[SerializeField] private ContentSizeFitter sizeFitter;

void Start() {
    sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
    sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
}
```

**⚠️ 注意：** Layout Group会频繁触发重建，影响性能。建议减少使用。

---

## UI动画

### 方式1：Animation（传统动画）

```
1. 创建Animation Clip
2. 录制RectTransform、Color等属性变化
3. Animator播放
```

### 方式2：DOTween（推荐）

```csharp
using DG.Tweening;

// 淡入淡出
image.DOFade(0f, 1f); // 1秒淡出

// 移动
rectTransform.DOAnchorPos(new Vector2(100, 0), 0.5f);

// 缩放
rectTransform.DOScale(1.2f, 0.3f);

// 组合动画
rectTransform.DOScale(1.2f, 0.3f)
    .SetEase(Ease.OutBack)
    .OnComplete(() => Debug.Log("Animation complete"));
```

### 方式3：协程

```csharp
IEnumerator FadeIn(CanvasGroup canvasGroup, float duration) {
    canvasGroup.alpha = 0f;
    float elapsed = 0f;
    
    while (elapsed < duration) {
        elapsed += Time.deltaTime;
        canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
        yield return null;
    }
    
    canvasGroup.alpha = 1f;
}

IEnumerator MoveUI(RectTransform rect, Vector2 targetPos, float duration) {
    Vector2 startPos = rect.anchoredPosition;
    float elapsed = 0f;
    
    while (elapsed < duration) {
        elapsed += Time.deltaTime;
        rect.anchoredPosition = Vector2.Lerp(startPos, targetPos, elapsed / duration);
        yield return null;
    }
    
    rect.anchoredPosition = targetPos;
}
```

---

## UI适配

### 不同分辨率适配

```csharp
// Canvas Scaler设置
CanvasScaler canvasScaler = GetComponent<CanvasScaler>();
canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
canvasScaler.referenceResolution = new Vector2(1920, 1080);
canvasScaler.matchWidthOrHeight = 0.5f;
```

### 安全区域适配

```csharp
using UnityEngine;

public class SafeAreaAdapter : MonoBehaviour
{
    [SerializeField] private RectTransform panel;
    
    void Start() {
        ApplySafeArea();
    }
    
    void ApplySafeArea() {
        Rect safeArea = Screen.safeArea;
        
        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;
        
        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;
        
        panel.anchorMin = anchorMin;
        panel.anchorMax = anchorMax;
    }
}
```

---

## 常见问题

### Q1: UI不显示？
**A:** 检查以下几点：
1. Canvas是否存在
2. RectTransform是否在Canvas范围内
3. Image/Text的Alpha是否为0
4. Layer是否设置为UI

### Q2: UI点击无响应？
**A:** 检查以下几点：
1. EventSystem是否存在
2. GraphicRaycaster是否在Canvas上
3. Image的Raycast Target是否启用
4. 是否有其他UI遮挡

### Q3: UI模糊？
**A:** 使用TextMeshPro替代Legacy Text

### Q4: UI性能差？
**A:** 性能优化：
1. 分离静态和动态Canvas
2. 禁用不必要的Raycast Target
3. 减少Layout Group使用
4. 使用Sprite Atlas

---

## 最佳实践

1. **Canvas分离** - 静态UI和动态UI分离
2. **使用TextMeshPro** - 替代Legacy Text
3. **减少Layout Group** - 使用Anchor替代
4. **对象池管理** - 复用UI元素
5. **禁用Raycast Target** - 纯显示UI禁用

---

## 版本说明

- 教程版本：1.0
- Unity版本：2021.3 LTS+
- 最后更新：2024年
