---
name: unity-prefab-system
description: Unity预制体系统，负责预制体的创建、实例化、编辑和覆盖管理。支持嵌套预制体、预制体变体和对象池化。
---

# Unity Prefab System 预制体系统

## 概述

Unity预制体系统，负责预制体的创建、实例化、编辑和覆盖管理。预制体是Unity中复用GameObject的核心机制，支持嵌套预制体、预制体变体和预制体覆盖，是游戏资源管理的基石。

## API白名单 ⚠️ 强制遵守

### ✅ 推荐使用的API

| API | 用途 | 性能等级 |
|-----|------|---------|
| `Object.Instantiate()` | 运行时实例化预制体 | ⭐⭐⭐ 高性能 |
| `Object.Destroy()` | 销毁实例 | ⭐⭐ 中等性能 |
| `PrefabUtility.InstantiatePrefab()` | 编辑器实例化（保持连接） | ⭐⭐⭐ 高性能 |
| `PrefabUtility.SaveAsPrefabAsset()` | 保存为预制体资源 | ⭐⭐⭐ 高性能 |
| `PrefabUtility.GetCorrespondingObjectFromSource()` | 获取预制体资源 | ⭐⭐⭐ 高性能 |
| `PrefabUtility.IsPartOfPrefabInstance()` | 检查是否为预制体实例 | ⭐⭐⭐ 高性能 |
| `PrefabUtility.ApplyPrefabInstance()` | 应用覆盖到预制体 | ⭐⭐ 中等性能 |
| `PrefabUtility.RevertPrefabInstance()` | 还原预制体实例 | ⭐⭐⭐ 高性能 |

### ⚠️ 性能警告API

| API | 性能问题 | 替代方案 |
|-----|----------|----------|
| 频繁 `Instantiate()`/`Destroy()` | 增加GC压力、CPU开销 | 使用对象池 |
| `Resources.Load()` 大量预制体 | 内存占用高 | 使用Addressables |
| 深层嵌套预制体实例化 | 可能触发栈溢出 | 限制嵌套深度 |

### ❌ 禁止使用的API

| API | 禁用原因 | 替代方案 |
|-----|---------|----------|
| `PrefabUtility.CreatePrefab()` | Unity 2018.3后已过时 | `SaveAsPrefabAsset()` |

---

## 功能边界 ⚠️ 强制说明

### 本Skill涵盖范围

- ✅ Instantiate实例化方法重载
- ✅ PrefabUtility编辑器API
- ✅ 预制体变体（Prefab Variant）
- ✅ 嵌套预制体（Nested Prefab）
- ✅ 预制体覆盖（Override）管理
- ✅ 对象池化基础

### 不在本Skill范围内

- ❌ Addressables资源系统 → L3级 unity-addressables Skill
- ❌ AssetBundle预制体加载 → 不涉及
- ❌ 运行时预制体创建 → 不涉及

### 性能限制

| 指标 | 建议值 | 说明 |
|------|--------|------|
| 单帧实例化数量 | ≤ 50 | 分帧实例化 |
| 预制体嵌套深度 | ≤ 5 | 避免复杂嵌套 |
| 同时活跃实例数 | ≤ 500 | 配合对象池 |

---

## 渐进式学习路径

### 阶段1：基础使用

```csharp
// 基础实例化
public class Spawner : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    
    public void Spawn(Vector3 position)
    {
        GameObject instance = Instantiate(prefab, position, Quaternion.identity);
    }
}
```

### 阶段2：常用功能

```csharp
// 带父级实例化
GameObject instance = Instantiate(prefab, parentTransform);

// 指定位置旋转
GameObject instance = Instantiate(prefab, position, rotation, parent);

// 泛型实例化（推荐）
Bullet bullet = Instantiate(bulletPrefab, position, rotation);
```

### 阶段3：进阶功能

```csharp
// 对象池化
public class BulletPool : MonoBehaviour
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

---

## References

- [API参考文档](references/api-reference.md)
