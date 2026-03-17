# Unity 2D Sprite API参考文档

## SpriteRenderer 组件

**命名空间**: UnityEngine  
**继承自**: Renderer  
**描述**:SpriteRenderer是用于在2D游戏中渲染Sprite的核心组件。它会根据附加的Transform组件的位置、旋转和缩放信息,在屏幕上绘制指定的Sprite资源。

---

### sprite

**类型**: Sprite  
**描述**:设置或获取要渲染的Sprite对象。这是SpriteRenderer最核心的属性,决定了游戏对象显示的图像内容。

**参数**:无(属性访问)  
**返回值**:Sprite对象

**代码示例**:
```csharp
using UnityEngine;

public class SpriteSwitcher : MonoBehaviour
{
    public Sprite defaultSprite;
    public Sprite alternateSprite;
    private SpriteRenderer spriteRenderer;
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = defaultSprite;
    }
    
    public void ToggleSprite()
    {
        // 切换Sprite
        if (spriteRenderer.sprite == defaultSprite)
            spriteRenderer.sprite = alternateSprite;
        else
            spriteRenderer.sprite = defaultSprite;
    }
}
```

**性能注意事项**:
- 切换Sprite时会产生少量GC,频繁切换应考虑使用SpriteAtlas
- 建议在初始化时缓存Sprite引用,避免运行时Resources.Load

---

### color

**类型**: Color  
**描述**:设置精灵的渲染颜色。该颜色会与Sprite的原始颜色进行混合,支持透明度控制(Color.a)。可用于实现闪烁、渐隐、染色等效果。

**参数**:无(属性访问)  
**返回值**:Color结构体,包含r、g、b、a四个分量(范围0-1)

**代码示例**:
```csharp
using UnityEngine;
using System.Collections;

public class SpriteColorEffect : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    // 闪烁效果
    public IEnumerator Flash(Color flashColor, float duration)
    {
        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = flashColor;
        yield return new WaitForSeconds(duration);
        spriteRenderer.color = originalColor;
    }
    
    // 渐隐效果
    public IEnumerator FadeOut(float duration)
    {
        float elapsed = 0f;
        Color startColor = spriteRenderer.color;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }
    }
    
    // 受伤变红
    public void OnDamage()
    {
        StartCoroutine(Flash(Color.red, 0.1f));
    }
}
```

**性能注意事项**:
- 修改color属性不会打断批处理,性能开销很小
- 可以安全地在Update中使用,但建议仅在值改变时设置
- 使用color实现透明效果时,确保Shader支持透明渲染

---

### flipX / flipY

**类型**: bool  
**描述**:控制精灵在X轴或Y轴上的翻转。flipX常用于控制角色面向(左/右),flipY用于特殊效果。翻转只影响视觉渲染,不影响Transform、碰撞体或子对象。

**参数**:无(属性访问)  
**返回值**:布尔值,true表示翻转,false表示不翻转

**代码示例**:
```csharp
using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    public float moveSpeed = 5f;
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        
        // 移动
        transform.Translate(Vector2.right * horizontalInput * moveSpeed * Time.deltaTime);
        
        // 根据移动方向翻转精灵
        if (horizontalInput > 0.01f)
            spriteRenderer.flipX = false;  // 向右
        else if (horizontalInput < -0.01f)
            spriteRenderer.flipX = true;   // 向左
    }
}
```

**性能注意事项**:
- flip属性修改性能开销极小,可以频繁使用
- 使用flip翻转比修改Transform.scale更优,因为不影响碰撞体和子对象
- 注意在动画切换时保持flip状态一致

---

### sortingOrder

**类型**: int  
**描述**:控制渲染器在SortingLayer内的渲染顺序。值越大,渲染越靠后(显示在越前面)。值域范围:-32768 到 32767。同一SortingLayer内,sortingOrder决定渲染先后。

**参数**:无(属性访问)  
**返回值**:整数,表示排序顺序

**代码示例**:
```csharp
using UnityEngine;

public class DynamicSorting : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Transform playerTransform;
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerTransform = transform;
    }
    
    // 根据Y坐标动态调整排序(Y值越大越靠后)
    void LateUpdate()
    {
        // 使用负Y值作为sortingOrder,实现"越靠下显示越前"的效果
        // 注意:仅在特定场景使用,不要每帧都修改
        int baseOrder = 1000;
        int yOrder = Mathf.RoundToInt(-playerTransform.position.y * 10);
        spriteRenderer.sortingOrder = baseOrder + yOrder;
    }
    
    // 设置为特定层级
    public void SetToFront()
    {
        spriteRenderer.sortingOrder = 9999;
    }
    
    public void ResetOrder()
    {
        spriteRenderer.sortingOrder = 0;
    }
}
```

**性能注意事项**:
- 不建议在Update中频繁修改sortingOrder
- 如需动态排序,建议在LateUpdate中限制修改频率
- 为同类型对象预留足够的sortingOrder区间

---

### sortingLayerName

**类型**: string  
**描述**:设置或获取渲染器所属的排序图层名称。SortingLayer决定了不同图层间的整体渲染顺序,在Project Settings中配置。

**参数**:无(属性访问)  
**返回值**:字符串,排序图层名称

**代码示例**:
```csharp
using UnityEngine;

public class LayerManager : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    // 切换到前景层
    public void MoveToForeground()
    {
        spriteRenderer.sortingLayerName = "Foreground";
        spriteRenderer.sortingOrder = 0;
    }
    
    // 切换到背景层
    public void MoveToBackground()
    {
        spriteRenderer.sortingLayerName = "Background";
        spriteRenderer.sortingOrder = 0;
    }
    
    // 检查当前图层
    public bool IsInLayer(string layerName)
    {
        return spriteRenderer.sortingLayerName == layerName;
    }
}
```

**性能注意事项**:
- sortingLayerName使用字符串比较,频繁切换时应考虑使用sortingLayerID
- 建议在初始化时设置好图层,避免运行时修改

---

### drawMode

**类型**: SpriteDrawMode(枚举)  
**描述**:设置精灵的绘制模式。支持三种模式:Simple(简单模式,默认)、Sliced(九宫格切片)、Tiled(平铺模式)。

**枚举值**:
- SpriteDrawMode.Simple:简单模式,标准精灵渲染
- SpriteDrawMode.Sliced:九宫格切片,用于可拉伸的UI元素
- SpriteDrawMode.Tiled:平铺模式,用于重复纹理

**代码示例**:
```csharp
using UnityEngine;

public class StretchableSprite : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // 设置为切片模式(需要Sprite有border设置)
        spriteRenderer.drawMode = SpriteDrawMode.Sliced;
        
        // 设置渲染尺寸
        spriteRenderer.size = new Vector2(5f, 3f);
    }
    
    public void Resize(Vector2 newSize)
    {
        if (spriteRenderer.drawMode != SpriteDrawMode.Simple)
        {
            spriteRenderer.size = newSize;
        }
    }
}
```

**性能注意事项**:
- Sliced和Tiled模式需要Sprite设置了border属性
- 动态修改size不会产生额外GC开销

---

### size

**类型**: Vector2  
**描述**:当drawMode设置为Sliced或Tiled时,用于设置或获取精灵的渲染尺寸。单位为世界空间单位。

**参数**:无(属性访问)  
**返回值**:Vector2,表示宽度和高度

**代码示例**:
```csharp
using UnityEngine;

public class PlatformScaler : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.drawMode = SpriteDrawMode.Sliced;
    }
    
    public void SetWidth(float width)
    {
        Vector2 currentSize = spriteRenderer.size;
        spriteRenderer.size = new Vector2(width, currentSize.y);
    }
}
```

---

### maskInteraction

**类型**: SpriteMaskInteraction(枚举)  
**描述**:指定精灵与SpriteMask遮罩的交互方式。

**枚举值**:
- SpriteMaskInteraction.None:不与遮罩交互
- SpriteMaskInteraction.VisibleInsideMask:只在遮罩区域内可见
- SpriteMaskInteraction.VisibleOutsideMask:只在遮罩区域外可见

**代码示例**:
```csharp
using UnityEngine;

public class MaskedSprite : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    // 显示在遮罩内部
    public void ShowInsideMask()
    {
        spriteRenderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
    }
    
    // 显示在遮罩外部
    public void ShowOutsideMask()
    {
        spriteRenderer.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;
    }
    
    // 不受遮罩影响
    public void IgnoreMask()
    {
        spriteRenderer.maskInteraction = SpriteMaskInteraction.None;
    }
}
```

---

### spriteSortPoint

**类型**: SpriteSortPoint(枚举)  
**描述**:决定SpriteRenderer进行排序时使用的参考点位置。

**枚举值**:
- SpriteSortPoint.Center:使用精灵中心点进行排序
- SpriteSortPoint.Pivot:使用精灵轴心点进行排序

**代码示例**:
```csharp
using UnityEngine;

public class SortPointExample : MonoBehaviour
{
    void Start()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        
        // 使用轴心点排序(适合角色脚底排序)
        sr.spriteSortPoint = SpriteSortPoint.Pivot;
    }
}
```

---

## Sprite 资源类

**命名空间**: UnityEngine  
**继承自**: Object  
**描述**:Sprite表示用于2D游戏的精灵对象,是从Texture2D纹理中提取的2D图形资源。Sprite类标识了图像中应作为特定精灵使用的部分。

---

### Create(静态方法)

**描述**:创建新的Sprite对象。有多个重载版本,最常用的包含texture、rect、pivot三个参数。

**参数**:
- texture (Texture2D):源纹理图像
- rect (Rect):纹理中用于Sprite的矩形区域(像素单位)
- pivot (Vector2):轴心点位置,标准化坐标(0-1),(0.5, 0.5)为中心
- pixelsPerUnit (float,可选):每单位像素数,默认100
- extrude (uint,可选):网格扩展像素数,默认0
- meshType (SpriteMeshType,可选):网格类型,默认FullRect
- border (Vector4,可选):九宫格边界,默认Vector4.zero

**返回值**:Sprite对象

**代码示例**:
```csharp
using UnityEngine;

public class DynamicSpriteCreator : MonoBehaviour
{
    public Texture2D sourceTexture;
    private SpriteRenderer spriteRenderer;
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // 从纹理创建完整的Sprite
        Sprite fullSprite = Sprite.Create(
            sourceTexture,
            new Rect(0, 0, sourceTexture.width, sourceTexture.height),
            new Vector2(0.5f, 0.5f),  // 中心轴心
            100f  // pixelsPerUnit
        );
        
        spriteRenderer.sprite = fullSprite;
    }
    
    // 创建纹理的局部Sprite
    public Sprite CreatePartialSprite(int x, int y, int width, int height)
    {
        return Sprite.Create(
            sourceTexture,
            new Rect(x, y, width, height),
            new Vector2(0.5f, 0.5f),
            100f
        );
    }
    
    // 从Texture2D创建Sprite的扩展方法
    public static Sprite CreateFromTexture(Texture2D texture)
    {
        return Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            Vector2.one * 0.5f
        );
    }
}
```

**性能注意事项**:
- Sprite.Create会在运行时生成新对象,产生GC
- 避免频繁调用,建议预加载或使用SpriteAtlas
- 创建后应缓存Sprite引用,避免重复创建

---

### texture

**类型**: Texture2D(只读)  
**描述**:获取Sprite使用的纹理。如果Sprite被打包在图集中,则返回图集纹理。

**返回值**:Texture2D对象

**代码示例**:
```csharp
using UnityEngine;

public class SpriteTextureInfo : MonoBehaviour
{
    public Sprite targetSprite;
    
    void Start()
    {
        if (targetSprite != null)
        {
            Texture2D tex = targetSprite.texture;
            Debug.Log($"纹理尺寸: {tex.width} x {tex.height}");
        }
    }
}
```

---

### rect

**类型**: Rect(只读)  
**描述**:获取Sprite在原始纹理上的矩形区域,以像素为单位。

**返回值**:Rect结构体

**代码示例**:
```csharp
using UnityEngine;

public class SpriteRectInfo : MonoBehaviour
{
    public Sprite sprite;
    
    void Start()
    {
        Rect spriteRect = sprite.rect;
        Debug.Log($"Sprite区域: 位置({spriteRect.x}, {spriteRect.y}), 尺寸({spriteRect.width} x {spriteRect.height})");
    }
}
```

---

### pivot

**类型**: Vector2(只读)  
**描述**:获取Sprite轴心点在原始纹理上的位置,以像素为单位。

**返回值**:Vector2,像素坐标

**代码示例**:
```csharp
using UnityEngine;

public class PivotExample : MonoBehaviour
{
    public Sprite sprite;
    
    void Start()
    {
        Vector2 pivot = sprite.pivot;
        Debug.Log($"轴心点位置: ({pivot.x}, {pivot.y}) 像素");
        
        // 计算标准化轴心点(0-1范围)
        Vector2 normalizedPivot = new Vector2(
            pivot.x / sprite.rect.width,
            pivot.y / sprite.rect.height
        );
        Debug.Log($"标准化轴心点: ({normalizedPivot.x}, {normalizedPivot.y})");
    }
}
```

---

### bounds

**类型**: Bounds(只读)  
**描述**:获取Sprite在世界空间中的边界包围盒。

**返回值**:Bounds结构体,包含center和extents

**代码示例**:
```csharp
using UnityEngine;

public class SpriteBoundsCheck : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        Bounds bounds = spriteRenderer.bounds;
        
        Debug.Log($"包围盒中心: {bounds.center}");
        Debug.Log($"包围盒尺寸: {bounds.size}");
    }
    
    // 检查点是否在Sprite范围内
    public bool IsPointInside(Vector3 point)
    {
        return spriteRenderer.bounds.Contains(point);
    }
}
```

---

### textureRect

**类型**: Rect(只读)  
**描述**:获取Sprite在纹理上的矩形区域。如果Sprite使用紧密打包,可能抛出异常。

**返回值**:Rect结构体

**代码示例**:
```csharp
using UnityEngine;

public class TextureRectExample : MonoBehaviour
{
    public Sprite sprite;
    
    void Start()
    {
        if (!sprite.packed || sprite.packingMode == SpritePackingMode.Rectangle)
        {
            Rect texRect = sprite.textureRect;
            Debug.Log($"纹理矩形: {texRect}");
        }
    }
}
```

---

## SortingLayer 类

**命名空间**: UnityEngine  
**描述**:SortingLayer用于控制多个精灵的渲染顺序。初始时所有精灵都位于名为"Default"的默认排序层。

---

### layers(静态属性)

**类型**: SortingLayer[](只读)  
**描述**:获取项目中定义的所有排序层数组。

**返回值**:SortingLayer数组

**代码示例**:
```csharp
using UnityEngine;

public class SortingLayerList : MonoBehaviour
{
    void Start()
    {
        SortingLayer[] allLayers = SortingLayer.layers;
        
        Debug.Log($"项目共有 {allLayers.Length} 个排序层:");
        foreach (var layer in allLayers)
        {
            Debug.Log($"  - {layer.name} (ID: {layer.id}, Value: {layer.value})");
        }
    }
}
```

---

### GetLayerValueFromID(静态方法)

**描述**:根据层ID返回排序值,用于比较不同层的渲染顺序。

**参数**:
- id (int):排序层ID

**返回值**:int,排序值

**代码示例**:
```csharp
using UnityEngine;

public class LayerComparison : MonoBehaviour
{
    public int layerA_ID;
    public int layerB_ID;
    
    void Start()
    {
        int valueA = SortingLayer.GetLayerValueFromID(layerA_ID);
        int valueB = SortingLayer.GetLayerValueFromID(layerB_ID);
        
        if (valueA < valueB)
            Debug.Log("LayerA 渲染在前(显示在后)");
        else
            Debug.Log("LayerB 渲染在前(显示在后)");
    }
}
```

---

### id

**类型**: int(只读)  
**描述**:获取排序层的唯一标识符。注意这不是有序递增值。

**返回值**:整数ID

---

### name

**类型**: string(只读)  
**描述**:获取排序层的名称。

**返回值**:字符串名称

**代码示例**:
```csharp
using UnityEngine;

public class LayerNameCheck : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // 通过ID获取层名称
        int layerID = spriteRenderer.sortingLayerID;
        string layerName = SortingLayer.IDToName(layerID);
        Debug.Log($"当前层: {layerName}");
    }
}
```

---

## SpriteAtlas 类

**命名空间**: UnityEngine.U2D  
**继承自**: Object  
**描述**:SpriteAtlas是Unity内置的图集打包方案,用于管理Sprite的打包和运行时访问,可有效减少DrawCall。

---

### GetSprite

**描述**:根据名称从图集中获取Sprite的克隆对象。

**参数**:
- name (string):Sprite的名称

**返回值**:Sprite对象

**代码示例**:
```csharp
using UnityEngine;
using UnityEngine.U2D;

public class AtlasLoader : MonoBehaviour
{
    public SpriteAtlas characterAtlas;
    private SpriteRenderer spriteRenderer;
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // 从图集获取Sprite
        Sprite walkSprite = characterAtlas.GetSprite("character_walk_01");
        
        if (walkSprite != null)
        {
            spriteRenderer.sprite = walkSprite;
        }
        else
        {
            Debug.LogWarning("未找到指定的Sprite");
        }
    }
    
    // 切换Sprite示例
    public void ChangeSprite(string spriteName)
    {
        Sprite newSprite = characterAtlas.GetSprite(spriteName);
        if (newSprite != null)
        {
            spriteRenderer.sprite = newSprite;
        }
    }
}
```

**性能注意事项**:
- GetSprite返回的是克隆对象,每次调用会产生少量GC
- 建议在初始化时预加载常用Sprite并缓存
- Sprite名称需与图集中的原始名称完全匹配

---

### GetSprites

**描述**:获取图集中所有Sprite的克隆对象,填充到提供的数组中。

**参数**:
- sprites (Sprite[]):用于存储Sprite的数组

**返回值**:int,实际填充的Sprite数量

**代码示例**:
```csharp
using UnityEngine;
using UnityEngine.U2D;

public class AtlasBatchLoader : MonoBehaviour
{
    public SpriteAtlas spriteAtlas;
    private Sprite[] loadedSprites;
    
    void Start()
    {
        // 获取Sprite数量
        int spriteCount = spriteAtlas.spriteCount;
        loadedSprites = new Sprite[spriteCount];
        
        // 批量获取所有Sprite
        int actualCount = spriteAtlas.GetSprites(loadedSprites);
        
        Debug.Log($"加载了 {actualCount} 个Sprite");
        
        // 遍历Sprite
        foreach (Sprite sprite in loadedSprites)
        {
            if (sprite != null)
            {
                Debug.Log($"Sprite: {sprite.name}");
            }
        }
    }
}
```

---

### spriteCount

**类型**: int(只读)  
**描述**:获取图集中打包的Sprite总数。

**返回值**:整数,Sprite数量

**代码示例**:
```csharp
using UnityEngine;
using UnityEngine.U2D;

public class AtlasInfo : MonoBehaviour
{
    public SpriteAtlas atlas;
    
    void Start()
    {
        Debug.Log($"图集包含 {atlas.spriteCount} 个Sprite");
    }
}
```

---

### isVariant

**类型**: bool(只读)  
**描述**:判断此SpriteAtlas是否为变体图集。变体图集基于主图集按比例缩放生成。

**返回值**:布尔值

**代码示例**:
```csharp
using UnityEngine;
using UnityEngine.U2D;

public class VariantAtlasCheck : MonoBehaviour
{
    public SpriteAtlas atlas;
    
    void Start()
    {
        if (atlas.isVariant)
        {
            Debug.Log("这是变体图集");
        }
        else
        {
            Debug.Log("这是主图集");
        }
    }
}
```

---

## SpriteMask 组件

**命名空间**: UnityEngine  
**继承自**: Renderer  
**描述**:SpriteMask用于对Sprite和ParticleSystem进行遮罩,可实现揭示、遮盖等效果。

---

### sprite

**类型**: Sprite  
**描述**:设置用作遮罩形状的Sprite。遮罩的形状由Sprite的可见区域决定。

**参数**:无(属性访问)  
**返回值**:Sprite对象

**代码示例**:
```csharp
using UnityEngine;

public class MaskController : MonoBehaviour
{
    private SpriteMask spriteMask;
    
    void Start()
    {
        spriteMask = GetComponent<SpriteMask>();
    }
    
    public void SetMaskSprite(Sprite maskSprite)
    {
        spriteMask.sprite = maskSprite;
    }
}
```

---

### alphaCutoff

**类型**: float  
**描述**:遮罩根据Sprite像素的Alpha值确定影响区域的最小阈值(0~1)。像素Alpha值大于此值才被视为遮罩区域。

**参数**:无(属性访问)  
**返回值**:浮点数,范围0到1

**代码示例**:
```csharp
using UnityEngine;

public class MaskAlphaControl : MonoBehaviour
{
    private SpriteMask spriteMask;
    
    void Start()
    {
        spriteMask = GetComponent<SpriteRenderer>();
        spriteMask.alphaCutoff = 0.5f;
    }
    
    // 动态调整遮罩阈值
    public void SetCutoff(float cutoff)
    {
        spriteMask.alphaCutoff = Mathf.Clamp01(cutoff);
    }
}
```

---

### frontSortingOrder / backSortingOrder

**类型**: int  
**描述**:自定义遮罩范围的起始和结束sortingOrder。配合isCustomRangeActive使用,可以精确控制遮罩影响的层级范围。

**参数**:无(属性访问)  
**返回值**:整数,sortingOrder值

**代码示例**:
```csharp
using UnityEngine;

public class MaskRangeControl : MonoBehaviour
{
    private SpriteMask spriteMask;
    
    void Start()
    {
        spriteMask = GetComponent<SpriteMask>();
        
        // 启用自定义范围
        spriteMask.isCustomRangeActive = true;
        
        // 设置遮罩范围:只影响sortingOrder在1到10之间的Sprite
        spriteMask.backSortingOrder = 1;
        spriteMask.frontSortingOrder = 10;
    }
}
```

---

### isCustomRangeActive

**类型**: bool  
**描述**:是否启用自定义遮罩范围。启用后,遮罩只影响backSortingOrder到frontSortingOrder范围内的Sprite。

**参数**:无(属性访问)  
**返回值**:布尔值

**代码示例**:
```csharp
using UnityEngine;

public class MaskRangeToggle : MonoBehaviour
{
    private SpriteMask spriteMask;
    
    void Start()
    {
        spriteMask = GetComponent<SpriteMask>();
    }
    
    // 启用遮罩对所有层的影响
    public void SetGlobalMask()
    {
        spriteMask.isCustomRangeActive = false;
    }
    
    // 启用遮罩自定义范围
    public void SetCustomRangeMask(int backOrder, int frontOrder)
    {
        spriteMask.isCustomRangeActive = true;
        spriteMask.backSortingOrder = backOrder;
        spriteMask.frontSortingOrder = frontOrder;
    }
}
```
