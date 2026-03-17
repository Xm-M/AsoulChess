# Unity 2D Sprite系统基础教程

## 一、Sprite基本概念

### 什么是Sprite?
Sprite(精灵)是Unity 2D开发中最基础的视觉元素,本质上是一张2D图像。它可以是游戏角色、道具、背景、特效等各种2D游戏对象。在Unity中,Sprite由纹理(Texture2D)和精灵渲染器(SpriteRenderer)共同组成。

**Sprite的核心特性:**
- 基于图片资源,支持PNG、JPG、PSD等格式
- 可以包含多个子精灵(Multiple模式)
- 支持图集打包优化性能
- 内置物理碰撞器支持

### Sprite导入流程

#### 步骤1: 导入图片资源
1. 在Project窗口右键 → Import New Asset
2. 选择图片文件(PNG推荐)
3. 或直接将图片拖拽到Project窗口

#### 步骤2: 配置导入设置
选中导入的图片,在Inspector窗口设置:

**基础配置:**
- Texture Type: Sprite(2D and UI)
- Sprite Mode: Single(单精灵)或Multiple(多精灵)
- Pixels Per Unit: 100(每单位像素数,决定精灵在世界中的大小)
- Filter Mode: Point(像素风)或Bilinear(平滑)
- Compression: None(高质量)或Normal(压缩)

**Multiple模式(多精灵切片):**
当一张图包含多个精灵时(如角色动画帧):
1. Sprite Mode选择Multiple
2. 点击Sprite Editor按钮
3. 选择切片方式:
   - Automatic: 自动检测
   - Grid By Cell Size: 按网格大小(如16x16像素)
   - Grid By Cell Count: 按网格数量
4. 点击Apply应用

**最佳实践:**
- 像素风游戏: Filter Mode用Point, Compression用None
- 高清游戏: Filter Mode用Bilinear
- 移动平台: 启用Compression减小包体

## 二、SpriteRenderer组件基础

### 组件介绍
SpriteRenderer是Unity专门用于渲染2D精灵的组件,负责将Sprite显示在场景中。

### 添加SpriteRenderer
```csharp
// 方法1: Inspector面板
// 点击Add Component → 搜索SpriteRenderer

// 方法2: 代码添加
SpriteRenderer renderer = gameObject.AddComponent<SpriteRenderer>();
```

### 核心属性详解

**1. Sprite属性**
```csharp
// 设置要显示的精灵
renderer.sprite = mySprite;

// 获取当前精灵
Sprite current = renderer.sprite;
```

**2. Color属性**
```csharp
// 设置颜色(会与原图颜色相乘)
renderer.color = Color.red;      // 变红
renderer.color = new Color(1f, 0.5f, 0.3f, 1f); // RGBA

// 设置透明度
Color c = renderer.color;
c.a = 0.5f; // 半透明
renderer.color = c;
```

**3. Flip属性**
```csharp
// 水平翻转(常用:角色面向控制)
renderer.flipX = true;  // 翻转
renderer.flipX = false; // 正常

// 垂直翻转
renderer.flipY = true;
```

**4. Size属性**
```csharp
// 获取精灵实际尺寸(世界单位)
Vector2 size = renderer.size;

// 设置尺寸(需要关闭Draw Mode的Tiled模式)
renderer.size = new Vector2(2f, 3f);
```

### Draw Mode(绘制模式)
- **Simple**: 标准模式,精灵随Transform缩放
- **Sliced**: 九宫格切片模式,适合UI边框
- **Tiled**: 平铺模式,重复纹理填充区域

## 三、SortingLayer和sortingOrder配置

### 渲染排序原理
Unity 2D使用SortingLayer和sortingOrder决定精灵的绘制顺序,确保正确的遮挡关系。

### 排序规则
**优先级从高到低:**
1. Sorting Layer(排序图层)
2. Sorting Order(图层内顺序)
3. Z轴位置(相同排序时)

### 创建和配置Sorting Layer

**步骤1: 创建Sorting Layer**
1. 打开Edit → Project Settings → Tags and Layers
2. 展开Sorting Layers
3. 点击+添加新图层
4. 拖拽调整顺序(顶部优先渲染)

**建议图层结构:**
```
Background (背景层)
Terrain (地形层)
Characters (角色层)
Effects (特效层)
UI (界面层)
```

### 代码中设置排序

```csharp
// 设置Sorting Layer
renderer.sortingLayerName = "Characters";

// 设置Sorting Order(数值越大越靠前)
renderer.sortingOrder = 10;

// 获取当前设置
string layer = renderer.sortingLayerName;
int order = renderer.sortingOrder;
```

### 实战示例

**角色在不同层显示:**
```csharp
// 背景精灵
backgroundRenderer.sortingLayerName = "Background";
backgroundRenderer.sortingOrder = 0;

// 地面精灵
groundRenderer.sortingLayerName = "Terrain";
groundRenderer.sortingOrder = 1;

// 玩家精灵
playerRenderer.sortingLayerName = "Characters";
playerRenderer.sortingOrder = 5;

// 特效精灵(最上层)
effectRenderer.sortingLayerName = "Effects";
effectRenderer.sortingOrder = 10;
```

**动态调整层级:**
```csharp
// Y轴排序:让角色根据Y坐标动态调整层级
void Update()
{
    // Y值越小(越靠上),层级越高
    spriteRenderer.sortingOrder = Mathf.RoundToInt(-transform.position.y * 100);
}
```

## 四、精灵翻转和颜色控制

### 翻转控制详解

**flipX实现角色面向:**
```csharp
public class PlayerController : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private float moveDirection;
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    void Update()
    {
        // 获取移动方向
        moveDirection = Input.GetAxis("Horizontal");
        
        // 根据移动方向翻转精灵
        if (moveDirection > 0)
            spriteRenderer.flipX = false; // 向右
        else if (moveDirection < 0)
            spriteRenderer.flipX = true;  // 向左
    }
}
```

**翻转 vs 旋转 vs 缩放:**
```csharp
// 方式1: Flip属性(推荐)
spriteRenderer.flipX = true; // 不改变Transform

// 方式2: 缩放(不推荐,可能影响物理)
transform.localScale = new Vector3(-1, 1, 1);

// 方式3: 旋转(不推荐,可能影响其他逻辑)
transform.rotation = Quaternion.Euler(0, 180, 0);
```

### 颜色控制技巧

**颜色闪烁效果:**
```csharp
public class ColorFlash : MonoBehaviour
{
    public Color flashColor = Color.red;
    public float flashDuration = 0.1f;
    
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
    }
    
    public void Flash()
    {
        StartCoroutine(FlashCoroutine());
    }
    
    IEnumerator FlashCoroutine()
    {
        spriteRenderer.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
    }
}
```

**渐变透明效果:**
```csharp
public class FadeEffect : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        StartCoroutine(FadeOut(2f)); // 2秒淡出
    }
    
    IEnumerator FadeOut(float duration)
    {
        float timer = 0f;
        Color color = spriteRenderer.color;
        
        while (timer < duration)
        {
            timer += Time.deltaTime;
            color.a = 1f - (timer / duration);
            spriteRenderer.color = color;
            yield return null;
        }
        
        color.a = 0f;
        spriteRenderer.color = color;
    }
}
```

**颜色循环动画:**
```csharp
public class ColorCycle : MonoBehaviour
{
    public float cycleSpeed = 1f;
    
    private SpriteRenderer spriteRenderer;
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    void Update()
    {
        float t = Time.time * cycleSpeed;
        Color color = new Color(
            Mathf.Sin(t) * 0.5f + 0.5f,
            Mathf.Sin(t + 2.094f) * 0.5f + 0.5f,
            Mathf.Sin(t + 4.189f) * 0.5f + 0.5f
        );
        spriteRenderer.color = color;
    }
}
```

## 五、图集(SpriteAtlas)基础

### 什么是SpriteAtlas?
SpriteAtlas是Unity的图集打包系统,将多个精灵合并到一张大纹理中,减少Draw Call,提升性能。

### 创建SpriteAtlas

**步骤1: 创建图集资源**
1. Project窗口右键 → Create → 2D → Sprite Atlas
2. 命名图集(如"GameAtlas")

**步骤2: 配置图集**
选中SpriteAtlas资源,Inspector设置:

**Objects for Packing:**
- 拖入文件夹:自动打包文件夹内所有精灵
- 拖入单个精灵:只打包指定精灵

**Pack Settings:**
- Include in Build: 构建时是否包含
- Allow Rotation: 允许旋转优化(可能导致精灵方向错误)
- Tight Packing: 紧密打包(节省空间)
- Padding: 精灵间距(避免边缘渗色)

**Texture Settings:**
- Read/Write Enabled: 是否允许CPU读取
- Generate Mipmaps: 是否生成Mipmap
- sRGB: 是否使用sRGB颜色空间
- Filter Mode: 过滤模式

**步骤3: 打包图集**
点击Pack Preview按钮预览打包结果

### 代码中使用SpriteAtlas

```csharp
using UnityEngine;
using UnityEngine.U2D;

public class AtlasLoader : MonoBehaviour
{
    public SpriteAtlas spriteAtlas;
    
    void Start()
    {
        // 方法1: 按名称获取精灵
        Sprite playerSprite = spriteAtlas.GetSprite("player_idle");
        
        // 方法2: 获取所有精灵
        Sprite[] allSprites = new Sprite[spriteAtlas.spriteCount];
        spriteAtlas.GetSprites(allSprites);
        
        // 使用精灵
        GetComponent<SpriteRenderer>().sprite = playerSprite;
    }
}
```

**动态加载图集:**
```csharp
public class DynamicAtlasLoader : MonoBehaviour
{
    void LoadSpriteFromAtlas()
    {
        // 从Resources加载图集
        SpriteAtlas atlas = Resources.Load<SpriteAtlas>("Atlases/GameAtlas");
        
        // 获取精灵
        Sprite sprite = atlas.GetSprite("enemy_01");
        
        // 使用
        GetComponent<SpriteRenderer>().sprite = sprite;
    }
}
```

### 图集优化建议

**1. 合理分组:**
- UI精灵单独一个图集
- 游戏角色一个图集
- 特效一个图集
- 避免单个图集过大

**2. 尺寸控制:**
```csharp
// 检查图集大小
Debug.Log($"Atlas size: {spriteAtlas.tag}");
```

**3. 运行时更新:**
```csharp
// 运行时重新打包(仅编辑器)
#if UNITY_EDITOR
spriteAtlas.Pack(spriteAtlas.GetPackingSettings());
#endif
```

## 六、常见问题与解决方案

### 问题1: 精灵显示模糊

**原因:** Filter Mode设置错误

**解决方案:**
```
导入设置中:
Filter Mode = Point (no filter)  // 像素风游戏
Compression = None               // 高清游戏
```

### 问题2: 精灵边缘有白边或黑边

**原因:** 图片导入设置或间距不足

**解决方案:**
```csharp
// 方法1: 调整Padding
// SpriteAtlas中设置Padding至少为2像素

// 方法2: 调整图片边缘
// 在图片编辑软件中添加1像素透明边框

// 方法3: 使用Sprite Mesh Type
// Inspector中设置Sprite Mesh Type = Full Rect
```

### 问题3: 精灵闪烁或穿插

**原因:** Sorting Order设置不当

**解决方案:**
```csharp
// 动态Y轴排序
void LateUpdate()
{
    spriteRenderer.sortingOrder = Mathf.RoundToInt(-transform.position.y * 100);
}

// 或使用专门的排序脚本
public class YSort : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    void LateUpdate()
    {
        spriteRenderer.sortingOrder = (int)(-transform.position.y * 100);
    }
}
```

### 问题4: 精灵翻转后碰撞器位置错误

**原因:** flipX/Y不影响物理碰撞器

**解决方案:**
```csharp
// 方式1: 使用缩放(需调整碰撞器offset)
void FlipWithCollision()
{
    Vector3 scale = transform.localScale;
    scale.x *= -1;
    transform.localScale = scale;
    
    // 调整碰撞器offset
    BoxCollider2D collider = GetComponent<BoxCollider2D>();
    collider.offset = new Vector2(-collider.offset.x, collider.offset.y);
}

// 方式2: 使用多动画(推荐)
// 制作左右两套动画,不使用flip
```

### 问题5: SpriteAtlas加载慢

**原因:** 图集过大或未异步加载

**解决方案:**
```csharp
using System.Collections;
using UnityEngine;
using UnityEngine.U2D;

public class AsyncAtlasLoader : MonoBehaviour
{
    public string atlasPath = "Atlases/GameAtlas";
    
    IEnumerator Start()
    {
        // 异步加载图集
        ResourceRequest request = Resources.LoadAsync<SpriteAtlas>(atlasPath);
        
        while (!request.isDone)
        {
            Debug.Log($"Loading progress: {request.progress * 100}%");
            yield return null;
        }
        
        SpriteAtlas atlas = request.asset as SpriteAtlas;
        Sprite sprite = atlas.GetSprite("player");
        GetComponent<SpriteRenderer>().sprite = sprite;
    }
}
```

### 问题6: 精灵颜色不受光照影响

**原因:** SpriteRenderer默认使用Unlit材质

**解决方案:**
```csharp
// 创建受光照影响的材质
// 1. 创建材质,Shader选择Sprites/Default
// 2. 材质中设置Normal Map等

public SpriteRenderer spriteRenderer;
public Material litMaterial;

void Start()
{
    spriteRenderer.material = litMaterial;
}
```

### 问题7: 多分辨率适配精灵显示异常

**原因:** 未设置Pixels Per Unit或Canvas缩放

**解决方案:**
```csharp
// 方法1: 统一Pixels Per Unit
// 所有精灵导入时设置相同的PPU(如16、32、100)

// 方法2: 运行时计算缩放
public class PixelPerfectScaler : MonoBehaviour
{
    public int pixelsPerUnit = 16;
    public int targetResolution = 480; // 目标垂直分辨率
    
    void Start()
    {
        float scaleFactor = (float)Screen.height / targetResolution;
        Camera.main.orthographicSize = Screen.height / (2f * pixelsPerUnit * scaleFactor);
    }
}
```

## 总结

Unity 2D Sprite系统是开发2D游戏的基础,掌握以下核心知识点:

1. **Sprite导入**: 正确配置Texture Type和Sprite Mode
2. **SpriteRenderer**: 理解核心属性和使用方式
3. **排序系统**: SortingLayer和sortingOrder的正确使用
4. **翻转控制**: flipX实现角色面向的最佳实践
5. **颜色控制**: 动态修改颜色实现各种视觉效果
6. **图集优化**: SpriteAtlas提升性能

**进阶学习路径:**
- Sprite动画(Animator + Animation)
- Sprite Shape(精灵形状工具)
- 2D IK(骨骼动画)
- 2D Lights(2D光照系统)