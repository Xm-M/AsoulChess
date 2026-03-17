# Prefab System API参考

## Object.Instantiate

### 重载方法

```csharp
// 基础实例化
public static Object Instantiate(Object original);

// 指定父级
public static Object Instantiate(Object original, Transform parent);

// 指定父级和坐标空间
public static Object Instantiate(Object original, Transform parent, bool worldPositionStays);

// 指定位置和旋转
public static Object Instantiate(Object original, Vector3 position, Quaternion rotation);

// 完整参数
public static Object Instantiate(Object original, Vector3 position, Quaternion rotation, Transform parent);

// 泛型版本
public static T Instantiate<T>(T original) where T : Object;
```

### 使用示例

```csharp
// 基础实例化
GameObject enemy = Instantiate(enemyPrefab);

// 指定位置
GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

// 指定父级
GameObject uiElement = Instantiate(uiPrefab, canvasTransform);

// 泛型实例化（推荐）
Bullet bullet = Instantiate(bulletPrefab, position, rotation);
```

---

## PrefabUtility（编辑器）

### 核心方法

| 方法 | 说明 |
|------|------|
| `SaveAsPrefabAsset()` | 保存为预制体资源 |
| `InstantiatePrefab()` | 实例化预制体（保持连接） |
| `ApplyPrefabInstance()` | 应用覆盖到预制体 |
| `RevertPrefabInstance()` | 还原预制体实例 |
| `GetCorrespondingObjectFromSource()` | 获取预制体资源 |
| `IsPartOfPrefabInstance()` | 检查是否为预制体实例 |
| `UnpackPrefabInstance()` | 解包预制体实例 |

---

## 预制体变体

### 创建方式

1. **Project窗口**：右键预制体 → Create → Prefab Variant
2. **Hierarchy**：拖拽预制体实例 → 选择 Prefab Variant

### 变体特性

- 继承父预制体属性
- 可覆盖特定属性
- 父预制体修改自动传播
