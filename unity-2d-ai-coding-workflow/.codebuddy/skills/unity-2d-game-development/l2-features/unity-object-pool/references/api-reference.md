# Object Pool API参考

## Unity官方ObjectPool<T>（Unity 2021+）

### 命名空间

```csharp
using UnityEngine.Pool;
```

### 构造函数

```csharp
public ObjectPool<T>(
    Func<T> createFunc,              // 创建新对象的方法
    Action<T> actionOnGet = null,    // 获取时执行
    Action<T> actionOnRelease = null,// 释放时执行
    Action<T> actionOnDestroy = null,// 销毁时执行
    bool collectionCheck = true,     // 检查重复回收
    int defaultCapacity = 10,        // 初始容量
    int maxSize = 10000              // 最大容量
)
```

### 完整示例

```csharp
using UnityEngine.Pool;

public class BulletPool : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    private ObjectPool<GameObject> pool;
    
    void Awake()
    {
        pool = new ObjectPool<GameObject>(
            createFunc: () => Instantiate(prefab),
            actionOnGet: obj => obj.SetActive(true),
            actionOnRelease: obj => obj.SetActive(false),
            actionOnDestroy: obj => Destroy(obj),
            collectionCheck: true,
            defaultCapacity: 20,
            maxSize: 100
        );
    }
    
    public GameObject Get() => pool.Get();
    public void Return(GameObject obj) => pool.Release(obj);
}
```

---

## 属性

| 属性 | 类型 | 说明 |
|------|------|------|
| `CountAll` | int | 总创建对象数量 |
| `CountActive` | int | 当前活跃对象数量 |
| `CountInactive` | int | 池中闲置对象数量 |

---

## 其他Unity官方池类型

| 类型 | 说明 |
|------|------|
| `LinkedPool<T>` | 基于链表的对象池 |
| `GenericPool<T>` | ObjectPool的静态实现 |
| `ListPool<T>` | List对象池 |
| `DictionaryPool<K,V>` | Dictionary对象池 |

---

## IPoolable接口

```csharp
public interface IPoolable
{
    void OnObjectSpawn();   // 从池中取出时调用
    void OnObjectReturn();  // 放回池中时调用
}

public class Bullet : MonoBehaviour, IPoolable
{
    private Rigidbody rb;
    
    public void OnObjectSpawn()
    {
        // 重置状态
        rb.velocity = Vector3.zero;
    }
    
    public void OnObjectReturn()
    {
        // 清理状态
    }
}
```
