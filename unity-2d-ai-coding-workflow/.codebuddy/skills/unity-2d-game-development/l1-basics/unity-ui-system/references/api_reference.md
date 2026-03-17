# Unity UI System API 完整参考文档

## 概述

Unity UI系统是创建游戏界面的核心系统，基于Canvas渲染系统，包含Text、Image、Button、Slider等组件。本文档提供完整的API参考，包括性能优化建议和白名单分类。

---

## 核心组件

### 1. Canvas（画布）

Canvas是所有UI元素的根容器，负责UI的渲染方式和排序。

#### ✅ 推荐使用的API

**渲染模式选择：**

```csharp
// ✅ 推荐：Screen Space - Overlay（最常用，性能最好）
// UI直接渲染在屏幕上，不受相机影响
canvas.renderMode = RenderMode.ScreenSpaceOverlay;

// ✅ 推荐：Screen Space - Camera（需要3D UI效果时使用）
// UI渲染在相机前方，可以有3D效果
canvas.renderMode = RenderMode.ScreenSpaceCamera;
canvas.worldCamera = uiCamera;

// ✅ 推荐：World Space（游戏内UI）
// UI作为3D对象存在于场景中
canvas.renderMode = RenderMode.WorldSpace;
```

**Canvas优化设置：**

```csharp
// ✅ 推荐：像素完美渲染（像素艺术游戏）
canvas.pixelPerfect = true;

// ✅ 推荐：排序顺序设置
canvas.sortingOrder = 100; // 越大越靠前

// ✅ 推荐：额外相机缓冲区设置
canvas.planeDistance = 100; // Screen Space Camera模式下的相机距离
```

#### ⚠️ 性能警告

**Canvas分离策略（重要）：**

```csharp
// ⚠️ 性能警告：静态UI和动态UI必须在不同的Canvas上

// ❌ 错误：静态和动态UI在同一Canvas
Canvas mainCanvas; // 同时包含静态背景和动态血条

// ✅ 正确：分离Canvas
Canvas staticCanvas;  // 静态UI：背景、标题、固定按钮
Canvas dynamicCanvas; // 动态UI：血条、动画元素、频繁变化的文本
```

**性能原理：**
- Canvas每次有元素变化都会重建整个Canvas的网格
- 静态UI和动态UI分离后，动态UI变化不会触发静态UI重建
- 性能提升可达5-10倍

---

### 2. RectTransform（矩形变换）

RectTransform是UI元素的核心组件，控制位置、大小、锚点。

#### ✅ 推荐使用的API

**锚点设置：**

```csharp
// ✅ 推荐：使用AnchorPresets快速设置
// 左上角
rectTransform.anchorMin = new Vector2(0, 1);
rectTransform.anchorMax = new Vector2(0, 1);
rectTransform.pivot = new Vector2(0, 1);

// 居中拉伸
rectTransform.anchorMin = Vector2.zero;
rectTransform.anchorMax = Vector2.one;
rectTransform.pivot = new Vector2(0.5f, 0.5f);

// ✅ 推荐：使用SetSizeWithCurrentAnchors
rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 100);
rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 50);
```

**位置设置：**

```csharp
// ✅ 推荐：使用anchoredPosition
rectTransform.anchoredPosition = new Vector2(100, 200);

// ✅ 推荐：使用offsetMin和offsetMax（拉伸锚点）
rectTransform.offsetMin = new Vector2(10, 10);   // 左下偏移
rectTransform.offsetMax = new Vector2(-10, -10); // 右上偏移

// ✅ 推荐：使用sizeDelta（非拉伸锚点）
rectTransform.sizeDelta = new Vector2(200, 100);
```

#### ❌ 禁止使用的API

```csharp
// ❌ 禁止：在Update中频繁修改RectTransform
void Update() {
    rectTransform.anchoredPosition = new Vector2(
        Mathf.Sin(Time.time) * 100,
        0
    ); // 每帧修改，触发Layout重建
}

// ✅ 正确：使用动画系统或协程
IEnumerator AnimatePosition() {
    float duration = 1f;
    float elapsed = 0f;
    Vector2 startPos = rectTransform.anchoredPosition;
    Vector2 endPos = new Vector2(100, 0);
    
    while (elapsed < duration) {
        elapsed += Time.deltaTime;
        rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, elapsed / duration);
        yield return null;
    }
}
```

---

### 3. Image（图像）

Image用于显示精灵图片，是最常用的UI组件。

#### ✅ 推荐使用的API

```csharp
// ✅ 推荐：设置Sprite
image.sprite = spriteAsset;

// ✅ 推荐：设置颜色（支持透明度）
image.color = Color.white;
image.color = new Color(1f, 1f, 1f, 0.5f); // 半透明

// ✅ 推荐：Raycast Target控制
image.raycastTarget = false; // 不参与射线检测（优化性能）

// ✅ 推荐：Image Type设置
image.type = Image.Type.Simple;      // 简单模式
image.type = Image.Type.Sliced;      // 九宫格切片
image.type = Image.Type.Tiled;       // 平铺
image.type = Image.Type.Filled;      // 填充（用于血条、进度条）

// ✅ 推荐：填充设置（Filled类型）
image.fillMethod = Image.FillMethod.Horizontal;
image.fillAmount = 0.5f; // 填充50%
image.fillOrigin = 0;    // 从左开始填充
```

#### ⚠️ 性能警告

```csharp
// ⚠️ 警告：避免使用大尺寸图片
// 图片尺寸应为2的幂次方（256, 512, 1024, 2048）

// ⚠️ 警告：九宫格切片性能
image.type = Image.Type.Sliced;
// 九宫格会增加顶点数，大量使用会影响性能

// ✅ 推荐：使用Sprite Atlas减少Draw Call
// 将多个小图打包成一个Atlas
```

---

### 4. Text / TextMeshPro（文本）

#### ✅ 推荐使用TextMeshPro

```csharp
// ✅ 强烈推荐：使用TextMeshPro替代Legacy Text
using TMPro;

[SerializeField] private TextMeshProUGUI textMeshPro;

// 设置文本
textMeshPro.text = "Hello World";

// 设置字体大小
textMeshPro.fontSize = 24;

// 设置颜色
textMeshPro.color = Color.white;

// 设置对齐方式
textMeshPro.alignment = TextAlignmentOptions.Center;

// 设置字体资源
textMeshPro.font = tmpFontAsset;
```

**TextMeshPro优势：**
- 更清晰的文本渲染（SDF技术）
- 更好的性能（减少Draw Call）
- 支持富文本、发光、描边等效果
- 更小的内存占用

#### ❌ 禁止使用Legacy Text

```csharp
// ❌ 不推荐：Legacy Text（性能差，渲染模糊）
using UnityEngine.UI;

[SerializeField] private Text legacyText; // 性能较差
```

---

### 5. Button（按钮）

#### ✅ 推荐使用的API

```csharp
// ✅ 推荐：使用UnityEvent订阅
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

// ✅ 推荐：代码触发点击
button.onClick.Invoke();

// ✅ 推荐：禁用按钮
button.interactable = false;
```

#### ⚠️ 性能警告

```csharp
// ⚠️ 警告：避免在Update中频繁添加/移除监听器
void Update() {
    button.onClick.AddListener(OnButtonClick); // ❌ 错误
}

// ✅ 正确：在OnEnable/OnDisable中管理
void OnEnable() {
    button.onClick.AddListener(OnButtonClick);
}

void OnDisable() {
    button.onClick.RemoveListener(OnButtonClick);
}
```

---

### 6. Slider（滑动条）

#### ✅ 推荐使用的API

```csharp
[SerializeField] private Slider slider;

// ✅ 推荐：设置值
slider.value = 0.5f;
slider.minValue = 0f;
slider.maxValue = 100f;

// ✅ 推荐：监听值变化
slider.onValueChanged.AddListener(OnSliderChanged);

void OnSliderChanged(float value) {
    Debug.Log($"Slider value: {value}");
}

// ✅ 推荐：控制方向
slider.direction = Slider.Direction.LeftToRight;
slider.direction = Slider.Direction.TopToBottom;

// ✅ 推荐：整数值模式
slider.wholeNumbers = true; // 只能取整数
```

---

### 7. Toggle（开关）

#### ✅ 推荐使用的API

```csharp
[SerializeField] private Toggle toggle;

// ✅ 推荐：设置状态
toggle.isOn = true;

// ✅ 推荐：监听状态变化
toggle.onValueChanged.AddListener(OnToggleChanged);

void OnToggleChanged(bool isOn) {
    Debug.Log($"Toggle is: {isOn}");
}

// ✅ 推荐：Toggle Group（单选组）
[SerializeField] private ToggleGroup toggleGroup;

toggle.group = toggleGroup; // 添加到组，实现单选
```

---

### 8. Layout Group（布局组件）

#### ⚠️ 性能警告（重要）

**Layout Group是性能杀手：**

```csharp
// ⚠️ 警告：Layout Group会频繁触发重建
using UnityEngine.UI;

// 常见的Layout Group：
// - HorizontalLayoutGroup（水平布局）
// - VerticalLayoutGroup（垂直布局）
// - GridLayoutGroup（网格布局）

// ❌ 错误：大量嵌套Layout Group
VerticalLayoutGroup
  └─ HorizontalLayoutGroup
      └─ VerticalLayoutGroup  // 性能灾难

// ✅ 正确：最小化Layout Group使用
// 使用Anchor固定位置，而非Layout Group
```

**Layout优化建议：**

```csharp
// ✅ 推荐：禁用Layout Group的计算
// LayoutGroup.enabled = false; // 禁用自动布局

// ✅ 推荐：手动控制布局（性能更好）
void UpdateLayout() {
    float spacing = 10f;
    float startX = 0f;
    
    for (int i = 0; i < items.Count; i++) {
        RectTransform itemRect = items[i].GetComponent<RectTransform>();
        itemRect.anchoredPosition = new Vector2(startX + i * spacing, 0);
    }
}
```

---

### 9. Content Size Fitter（内容大小适配）

#### ⚠️ 性能警告

```csharp
// ⚠️ 警告：Content Size Fitter会触发Layout重建
ContentSizeFitter fitter = GetComponent<ContentSizeFitter>();

fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

// ❌ 错误：频繁修改文本内容（配合Content Size Fitter）
void Update() {
    textMeshPro.text = Time.time.ToString(); // 每帧触发重建
}

// ✅ 正确：仅在必要时更新文本
float lastUpdateTime;
void Update() {
    if (Time.time - lastUpdateTime > 0.1f) {
        textMeshPro.text = Time.time.ToString("F1");
        lastUpdateTime = Time.time;
    }
}
```

---

## UI事件系统

### 1. EventTrigger

#### ✅ 推荐使用的API

```csharp
using UnityEngine.EventSystems;

// ✅ 推荐：使用EventTrigger组件
[SerializeField] private EventTrigger eventTrigger;

void Start() {
    // 添加点击事件
    EventTrigger.Entry clickEntry = new EventTrigger.Entry();
    clickEntry.eventID = EventTriggerType.PointerClick;
    clickEntry.callback.AddListener((data) => { OnPointerClick((PointerEventData)data); });
    eventTrigger.triggers.Add(clickEntry);
    
    // 添加悬停事件
    EventTrigger.Entry hoverEntry = new EventTrigger.Entry();
    hoverEntry.eventID = EventTriggerType.PointerEnter;
    hoverEntry.callback.AddListener((data) => { OnPointerEnter((PointerEventData)data); });
    eventTrigger.triggers.Add(hoverEntry);
}

void OnPointerClick(PointerEventData data) {
    Debug.Log("Clicked!");
}

void OnPointerEnter(PointerEventData data) {
    Debug.Log("Hover!");
}
```

### 2. 接口实现（推荐）

#### ✅ 推荐使用的API

```csharp
using UnityEngine;
using UnityEngine.EventSystems;

// ✅ 推荐：实现接口（更灵活，性能更好）
public class UIEventHandler : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public void OnPointerClick(PointerEventData eventData) {
        Debug.Log("Clicked!");
    }
    
    public void OnPointerEnter(PointerEventData eventData) {
        Debug.Log("Pointer Enter");
    }
    
    public void OnPointerExit(PointerEventData eventData) {
        Debug.Log("Pointer Exit");
    }
}

// 其他常用接口：
// IPointerDownHandler - 按下
// IPointerUpHandler - 抬起
// IDragHandler - 拖拽
// IBeginDragHandler - 开始拖拽
// IEndDragHandler - 结束拖拽
// IScrollHandler - 滚轮
```

---

## 性能优化清单

### ✅ 推荐做法

1. **Canvas分离**
   ```csharp
   // 静态UI和动态UI分离
   Canvas staticCanvas;  // 背景等不变化的UI
   Canvas dynamicCanvas; // 血条等频繁变化的UI
   ```

2. **禁用Raycast Target**
   ```csharp
   // 纯显示的UI元素禁用Raycast Target
   image.raycastTarget = false;
   textMeshPro.raycastTarget = false;
   ```

3. **使用TextMeshPro**
   ```csharp
   // 替代Legacy Text
   using TMPro;
   TextMeshProUGUI textMeshPro;
   ```

4. **减少Layout Group使用**
   ```csharp
   // 使用Anchor替代Layout Group
   // 避免嵌套Layout Group
   ```

5. **使用Sprite Atlas**
   ```csharp
   // 打包多个Sprite到Atlas，减少Draw Call
   ```

6. **对象池管理UI**
   ```csharp
   // 复用UI元素，避免频繁Instantiate/Destroy
   ```

### ❌ 禁止做法

1. **禁止在Update中频繁修改UI**
   ```csharp
   // ❌ 禁止
   void Update() {
       textMeshPro.text = Time.time.ToString();
       rectTransform.anchoredPosition = new Vector2(Time.time * 100, 0);
   }
   ```

2. **禁止大量嵌套Layout Group**
   ```csharp
   // ❌ 禁止
   VerticalLayoutGroup → HorizontalLayoutGroup → VerticalLayoutGroup
   ```

3. **禁止频繁调用ForceRebuildLayoutImmediate**
   ```csharp
   // ❌ 禁止
   void Update() {
       LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
   }
   ```

4. **禁止同时播放大量UI动画**
   ```csharp
   // ❌ 建议：同时播放的UI动画 < 10个
   ```

---

## 常见问题

### Q1: UI不显示？
**A:** 检查以下几点：
1. Canvas是否存在
2. RectTransform是否在Canvas范围内
3. Image/Text的Alpha是否为0
4. Layer是否设置为UI

### Q2: UI响应慢？
**A:** 性能优化：
1. 分离静态和动态Canvas
2. 禁用不必要的Raycast Target
3. 减少Layout Group使用
4. 使用Sprite Atlas

### Q3: UI模糊？
**A:** 使用TextMeshPro替代Legacy Text

### Q4: UI适配不同分辨率？
**A:** 使用Canvas Scaler
```csharp
canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
canvasScaler.referenceResolution = new Vector2(1920, 1080);
canvasScaler.matchWidthOrHeight = 0.5f;
```

---

## 相关资源

- [Unity UI官方文档](https://docs.unity3d.com/Manual/UISystem.html)
- [TextMeshPro文档](https://docs.unity3d.com/Packages/com.unity.textmeshpro@latest)
- [UI优化指南](https://learn.unity.com/tutorial/optimizing-ui-in-unity)

---

## 版本说明

- 文档版本：1.0
- Unity版本：2021.3 LTS+
- 最后更新：2024年
