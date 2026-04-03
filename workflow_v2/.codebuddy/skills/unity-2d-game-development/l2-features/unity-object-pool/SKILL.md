---
name: unity-object-pool
description: Unity对象池系统，通过预先创建和复用对象，减少运行时Instantiate/Destroy开销，提升性能和稳定性。
---

# Unity Object Pool 对象池

## 概述

Unity对象池系统，通过预先创建和复用对象，减少运行时Instantiate/Destroy开销，提升性能和稳定性。适用于子弹、敌人、特效等高频创建/销毁的对象。

## 核心概念

### 对象池原理

- **预先创建**：初始化时创建对象存入池中
- **重复使用**：需要时取出激活，用完后放回失活
- **避免频繁分配**：减少Instantiate/Destroy操作

### 适用场景

| 场景 | 适用性 | 示例 |
|------|--------|------|
| 高频创建/销毁 | ✅ 非常适合 | 子弹、敌人、特效 |
| 短生命周期对象 | ✅ 适合 | 伤害数字、UI提示 |
| 长期存在对象 | ❌ 不适合 | 场景建筑、静态物体 |

## API白名单 ⚠️ 强制遵守

### ✅ 推荐使用的API（Unity 2021+）

| API | 用途 | 说明 |
|-----|------|------|
| `ObjectPool<T>` | 官方对象池类 | UnityEngine.Pool命名空间 |
| `ObjectPool<T>.Get()` | 从池获取对象 | 池空则创建新对象 |
| `ObjectPool<T>.Release(T)` | 释放对象回池 | 池满则销毁 |
| `ObjectPool<T>.Clear()` | 清空对象池 | 销毁所有闲置对象 |

### ✅ 自定义实现API

| API | 用途 | 说明 |
|-----|------|------|
| `Queue<GameObject>` | 队列存储 | 简单对象池 |
| `Stack<GameObject>` | 栈存储 | LIFO顺序 |
| `Dictionary<string, Queue>` | 多类型池 | 按标签管理 |

---

## 功能边界 ⚠️ 强制说明

### 本Skill涵盖范围

- ✅ ObjectPool<T>官方实现
- ✅ 自定义对象池实现
- ✅ 对象池管理器
- ✅ IPoolable接口
- ✅ 对象重置策略

### 不在本Skill范围内

- ❌ ECS对象池 → 不涉及
- ❌ DOTS优化 → 不涉及

### 性能限制

| 指标 | 建议值 | 说明 |
|------|--------|------|
| 初始池大小 | 平均活跃数×1.5 | 预创建数量 |
| 最大池大小 | 峰值活跃数×2 | 防止内存泄漏 |
| 单类型池数量 | ≤ 10 | 控制内存占用 |

---

## 渐进式学习路径

### 阶段1：基础实现

```csharp
public class SimplePool : MonoBehaviour
{
    private Queue<GameObject> pool = new Queue<GameObject>();
    [SerializeField] private GameObject prefab;
    
    public GameObject Get()
    {
        if (pool.Count > 0)
        {
            GameObject obj = pool.Dequeue();
            obj.SetActive(true);
            return obj;
        }
        return Instantiate(prefab);
    }
    
    public void Return(GameObject obj)
    {
        obj.SetActive(false);
        pool.Enqueue(obj);
    }
}
```

### 阶段2：Unity官方ObjectPool

```csharp
using UnityEngine.Pool;

public class BulletPool : MonoBehaviour
{
    private ObjectPool<GameObject> pool;
    [SerializeField] private GameObject prefab;
    
    void Awake()
    {
        pool = new ObjectPool<GameObject>(
            createFunc: () => Instantiate(prefab),
            actionOnGet: obj => obj.SetActive(true),
            actionOnRelease: obj => obj.SetActive(false),
            actionOnDestroy: obj => Destroy(obj),
            maxSize: 100
        );
    }
    
    public GameObject Get() => pool.Get();
    public void Return(GameObject obj) => pool.Release(obj);
}
```

---

## References

- [API参考文档](references/api-reference.md)
