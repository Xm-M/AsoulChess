# Object Pool 基础概念

## 什么是对象池？

对象池（Object Pool）是一种设计模式，通过预先创建并复用对象来避免频繁的实例化和销毁操作，从而提升性能和减少内存碎片。在Unity中，对象池主要用于优化Instantiate/Destroy的性能开销。

---

## 为什么需要对象池？

### Instantiate/Destroy的性能问题

```
Instantiate开销：
1. 内存分配
2. 组件数据复制
3. 脚本初始化（Awake）
4. 物理组件初始化
5. 渲染组件初始化

Destroy开销：
1. 内存标记
2. 组件清理
3. 脚本销毁（OnDestroy）
4. GC回收

问题：
- 频繁调用导致性能峰值
- GC频繁触发造成卡顿
- 内存碎片化
```

### 对象池的优势

```
优势：
1. 避免运行时实例化开销
2. 减少GC触发频率
3. 对象状态可控
4. 内存使用更稳定

代价：
1. 初始内存占用增加
2. 需要管理池生命周期
3. 对象状态需要重置
```

---

## 对象池核心概念

### 1. 池化对象生命周期

```
对象池生命周期：
1. 初始化（Preload）- 创建初始对象
2. 获取（Get）     - 从池中取出对象
3. 使用（Use）     - 对象处于激活状态
4. 释放（Release） - 对象返回池中
5. 销毁（Destroy） - 清理池中对象（可选）
```

### 2. 池容量管理

```
池容量概念：
- defaultCapacity：初始容量
- maxSize：最大容量
- 当前大小：池中可用对象数

容量策略：
- 固定容量：不扩容，超出时销毁
- 动态扩容：需要时创建新对象
- 混合策略：限制最大容量
```

### 3. 对象状态重置

```
对象重置时机：
- 从池中取出前
- 返回池中后

需要重置的内容：
- Transform位置、旋转、缩放
- Rigidbody速度
- 组件状态（血量、颜色等）
- 粒子系统重置
- 动画状态重置
```

---

## Unity官方ObjectPool<T>

### 命名空间

```csharp
using UnityEngine.Pool;
```

### Unity 2021+内置对象池

Unity 2021及以上版本提供了官方的对象池实现：

```csharp
public class ObjectPool<T> where T : class
{
    // 构造函数
    public ObjectPool(
        Func<T> createFunc,              // 创建新对象
        Action<T> actionOnGet,           // 获取时执行
        Action<T> actionOnRelease,       // 释放时执行
        Action<T> actionOnDestroy,       // 销毁时执行
        bool collectionCheck,            // 检查重复释放
        int defaultCapacity,             // 初始容量
        int maxSize                      // 最大容量
    );
    
    // 核心方法
    public T Get();                      // 获取对象
    public PooledObject<T> Get(out T v); // 获取对象（using语法）
    public void Release(T element);      // 释放对象
    public void Clear();                 // 清空池
    public void WarmUp(int count);       // 预热对象
    
    // 属性
    public int CountAll { get; }         // 总创建数
    public int CountActive { get; }      // 活跃数
    public int CountInactive { get; }    // 池中可用数
}
```

---

## 对象池使用场景

### 适合池化的对象

```
✅ 适合池化：
- 子弹、导弹等投射物
- 粒子特效（爆炸、烟雾）
- 敌人、NPC
- 音效播放器
- UI元素（列表项）
- 路径点、标记

特点：
- 频繁创建销毁
- 生命周期短
- 数量较多
```

### 不适合池化的对象

```
❌ 不适合池化：
- 场景中的静态对象
- 生命周期很长的对象
- 创建销毁频率很低的对象
- 状态复杂难以重置的对象

原因：
- 内存占用不划算
- 管理复杂度高
- 性能提升有限
```

---

## 性能对比数据

### Instantiate vs 对象池

```
测试场景：实例化1000个GameObject

方式          耗时        GC Alloc
----------------------------------------
Instantiate   15-20ms     约2MB
对象池Get     0.5-1ms     约0KB

性能提升：约15-20倍
```

### Destroy vs 对象池Release

```
测试场景：销毁1000个GameObject

方式           耗时        GC Alloc
-----------------------------------------
Destroy        10-15ms     约1.5MB
对象池Release  0.3-0.5ms   约0KB

性能提升：约20-30倍
```

---

## 对象池实现方式

### 1. Unity官方ObjectPool（推荐）

```csharp
// Unity 2021+推荐使用
using UnityEngine.Pool;

public class BulletPool : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    private ObjectPool<GameObject> pool;
    
    private void Awake()
    {
        pool = new ObjectPool<GameObject>(
            createFunc: () => Instantiate(bulletPrefab),
            actionOnGet: (obj) => obj.SetActive(true),
            actionOnRelease: (obj) => obj.SetActive(false),
            actionOnDestroy: (obj) => Destroy(obj),
            collectionCheck: true,
            defaultCapacity: 10,
            maxSize: 100
        );
    }
}
```

### 2. 自定义简单对象池

```csharp
public class SimplePool<T> where T : class, new()
{
    private Stack<T> pool = new Stack<T>();
    
    public T Get()
    {
        return pool.Count > 0 ? pool.Pop() : new T();
    }
    
    public void Release(T item)
    {
        pool.Push(item);
    }
}
```

### 3. MonoBehaviour对象池

```csharp
public class GameObjectPool
{
    private Queue<GameObject> pool = new Queue<GameObject>();
    private GameObject prefab;
    private Transform parent;
    
    public GameObjectPool(GameObject prefab, int initialSize, Transform parent = null)
    {
        this.prefab = prefab;
        this.parent = parent;
        
        for (int i = 0; i < initialSize; i++)
        {
            GameObject obj = Object.Instantiate(prefab, parent);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }
    
    public GameObject Get()
    {
        GameObject obj = pool.Count > 0 ? pool.Dequeue() : Object.Instantiate(prefab, parent);
        obj.SetActive(true);
        return obj;
    }
    
    public void Release(GameObject obj)
    {
        obj.SetActive(false);
        obj.transform.SetParent(parent);
        pool.Enqueue(obj);
    }
}
```

---

## 对象池管理器模式

### 多类型对象池管理

```csharp
public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }
    
    private Dictionary<string, ObjectPool<GameObject>> pools = new Dictionary<string, ObjectPool<GameObject>>();
    
    public void CreatePool(string key, GameObject prefab, int initialSize)
    {
        if (!pools.ContainsKey(key))
        {
            var pool = new ObjectPool<GameObject>(
                () => Instantiate(prefab),
                (obj) => obj.SetActive(true),
                (obj) => obj.SetActive(false),
                (obj) => Destroy(obj),
                true, initialSize, initialSize * 2
            );
            pools.Add(key, pool);
        }
    }
    
    public GameObject Get(string key)
    {
        return pools.TryGetValue(key, out var pool) ? pool.Get() : null;
    }
    
    public void Release(string key, GameObject obj)
    {
        if (pools.TryGetValue(key, out var pool))
        {
            pool.Release(obj);
        }
    }
}
```

---

## 对象状态重置策略

### 重置接口设计

```csharp
public interface IPoolable
{
    void OnGetFromPool();    // 从池中取出时调用
    void OnReturnToPool();   // 返回池中时调用
}

public class Bullet : MonoBehaviour, IPoolable
{
    private Rigidbody rb;
    
    public void OnGetFromPool()
    {
        gameObject.SetActive(true);
        rb.velocity = Vector3.zero;
    }
    
    public void OnReturnToPool()
    {
        gameObject.SetActive(false);
        rb.velocity = Vector3.zero;
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
    }
}
```

### 自动重置组件

```csharp
public class PoolableObject : MonoBehaviour
{
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Vector3 initialScale;
    
    private void Awake()
    {
        initialPosition = transform.localPosition;
        initialRotation = transform.localRotation;
        initialScale = transform.localScale;
    }
    
    public void ResetToInitial()
    {
        transform.localPosition = initialPosition;
        transform.localRotation = initialRotation;
        transform.localScale = initialScale;
        
        // 重置Rigidbody
        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}
```

---

## 常见问题

### Q: 对象池占用内存过多？

```
原因：
- 初始容量设置过大
- 最大容量无限制
- 未使用的对象未释放

解决：
- 根据实际使用调整容量
- 设置合理的maxSize
- 定期清理池中闲置对象
```

### Q: 对象状态未正确重置？

```
原因：
- 缺少重置逻辑
- 重置不完整
- 重置时机错误

解决：
- 实现IPoolable接口
- 在actionOnGet/actionOnRelease中重置
- 测试所有状态变化
```

### Q: 多场景对象池管理？

```
方案1：场景级对象池
- 每个场景独立的PoolManager
- 场景切换时自动销毁

方案2：全局对象池
- DontDestroyOnLoad
- 场景切换后清理池

方案3：混合模式
- 全局池管理常用对象
- 场景池管理场景专属对象
```

---

## 扩展学习

### 官方文档
- [Unity ObjectPool API](https://docs.unity3d.com/ScriptReference/Pool.ObjectPool_1.html)
- [Unity Object Pooling Tutorial](https://learn.unity.com/tutorial/introduction-to-object-pooling)

### 进阶主题
- 对象池与设计模式
- 大规模对象池优化
- 对象池与ECS架构
- 网络对象同步与对象池
