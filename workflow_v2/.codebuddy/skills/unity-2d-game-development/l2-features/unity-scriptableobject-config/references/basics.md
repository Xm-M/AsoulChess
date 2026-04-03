# ScriptableObject 基础概念

## 什么是ScriptableObject？

ScriptableObject是Unity中一种特殊的数据容器类型，用于存储大量数据的同时减少内存开销。与MonoBehaviour不同，ScriptableObject不绑定到GameObject上，而是作为独立的资产文件存在。

---

## 核心特点

### 与MonoBehaviour对比

| 特性 | ScriptableObject | MonoBehaviour |
|------|-----------------|---------------|
| 绑定对象 | 资产文件 | GameObject |
| 内存共享 | ✅ 多对象共享 | ❌ 每个实例独立 |
| 场景依赖 | ❌ 独立存在 | ✅ 依赖场景 |
| 序列化 | 资产文件 | 场景/Prefab |
| 生命周期 | 应用程序级 | GameObject级 |
| 适用场景 | 数据配置 | 行为逻辑 |

### 内存优势

```
MonoBehaviour方式（❌ 内存浪费）：
├── Enemy1: EnemyData (100KB)
├── Enemy2: EnemyData (100KB)
├── Enemy3: EnemyData (100KB)
└── 总计: 300KB

ScriptableObject方式（✅ 内存共享）：
├── EnemyData资产: 100KB
├── Enemy1: 引用
├── Enemy2: 引用
├── Enemy3: 引用
└── 总计: 100KB
```

---

## 主要用途

### 1. 游戏配置数据

```
适用场景：
├── 角色属性（生命值、攻击力、速度）
├── 武器数据（伤害、射速、范围）
├── 物品信息（名称、描述、价格）
├── 关卡配置（难度、奖励、敌人）
└── 技能配置（冷却、消耗、效果）

优点：
✅ 数据与代码分离
✅ 策划友好配置
✅ 版本控制清晰
```

### 2. 共享数据

```
适用场景：
├── 全局设置（音量、画质、语言）
├── 游戏常量（重力、摩擦系数）
├── 颜色配置（主题色、UI颜色）
└── 文本配置（对话、提示）

优点：
✅ 一处修改全局生效
✅ 避免硬编码
✅ 统一管理
```

### 3. 事件系统

```
适用场景：
├── 游戏事件（关卡完成、角色死亡）
├── UI事件（按钮点击、面板打开）
└── 系统事件（存档完成、设置更改）

优点：
✅ 解耦系统依赖
✅ 灵活的事件订阅
✅ 无需单例
```

### 4. 状态管理

```
适用场景：
├── 运行时状态（玩家数据、游戏进度）
├── 临时数据（当前关卡、选中角色）
└── 缓存数据（计算结果、预加载）

注意：
⚠️ 运行时修改会持久化
⚠️ 需要手动重置或克隆
```

---

## 数据驱动设计模式

### 架构原则

```
数据驱动架构：
├── 数据层（ScriptableObject）
│   ├── 静态配置数据
│   └── 共享运行时数据
│
├── 逻辑层（MonoBehaviour）
│   ├── 读取配置数据
│   └── 执行游戏逻辑
│
└── 表现层（View）
    ├── 接收数据更新
    └── 渲染UI/特效
```

### 设计模式

```
模式1：配置-行为分离
├── WeaponData (ScriptableObject) - 武器配置
└── WeaponController (MonoBehaviour) - 武器逻辑

模式2：数据引用
├── CharacterData (ScriptableObject) - 角色配置
└── Character (MonoBehaviour) - 引用配置

模式3：数据集合
├── WeaponDatabase (ScriptableObject) - 所有武器
└── InventorySystem - 查询武器数据
```

---

## 生命周期

### 编辑器模式

```
创建流程：
1. CreateAssetMenu定义菜单项
2. 用户通过菜单创建资产
3. Reset() 调用 - 设置默认值
4. OnValidate() 调用 - 验证数据
5. 资产保存到磁盘

修改流程：
1. Inspector修改字段
2. OnValidate() 调用 - 验证数据
3. 自动保存到磁盘
```

### 运行时模式

```
加载流程：
1. 加载资产（Resources/Addressables）
2. OnEnable() 调用
3. 数据可用于游戏逻辑

运行时：
⚠️ 修改数据会持久化
⚠️ 下次运行时数据仍然是修改后的值

卸载流程：
1. OnDisable() 调用
2. 资产被卸载
```

---

## 创建方式

### 方式1：CreateAssetMenu（推荐）

```csharp
[CreateAssetMenu(
    fileName = "NewWeapon",
    menuName = "Game/Weapon Data",
    order = 1
)]
public class WeaponData : ScriptableObject
{
    public string weaponName;
    public int attackDamage;
}
```

```
使用步骤：
1. 右键 Project窗口
2. Create → Game → Weapon Data
3. 配置数据
```

### 方式2：编辑器脚本

```csharp
#if UNITY_EDITOR
using UnityEditor;

public static class AssetCreator
{
    [MenuItem("Tools/Create Weapon Data")]
    public static void CreateWeaponData()
    {
        WeaponData asset = ScriptableObject.CreateInstance<WeaponData>();
        asset.weaponName = "New Weapon";
        
        AssetDatabase.CreateAsset(asset, "Assets/Data/NewWeapon.asset");
        AssetDatabase.SaveAssets();
        
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
    }
}
#endif
```

### 方式3：运行时创建

```csharp
// ⚠️ 警告：运行时创建不会保存到磁盘
WeaponData runtimeWeapon = ScriptableObject.CreateInstance<WeaponData>();
runtimeWeapon.weaponName = "Runtime Weapon";
```

---

## 数据访问方式

### 直接引用

```csharp
public class Player : MonoBehaviour
{
    [SerializeField] private CharacterData characterData;
    
    private void Start()
    {
        int health = characterData.MaxHealth;
        float speed = characterData.MoveSpeed;
    }
}
```

### Resources加载

```csharp
public class WeaponManager : MonoBehaviour
{
    private WeaponData weaponData;
    
    private void Start()
    {
        weaponData = Resources.Load<WeaponData>("Data/Weapons/Sword");
    }
}
```

### 静态实例

```csharp
[CreateAssetMenu(fileName = "GameData", menuName = "Game/Game Data")]
public class GameData : ScriptableObject
{
    private static GameData _instance;
    public static GameData Instance => _instance ??= Resources.Load<GameData>("GameData");
    
    public int maxLevel;
    public float defaultGravity;
}
```

### 数据库模式

```csharp
[CreateAssetMenu(fileName = "WeaponDatabase", menuName = "Game/Weapon Database")]
public class WeaponDatabase : ScriptableObject
{
    [SerializeField] private List<WeaponData> weapons;
    
    private Dictionary<int, WeaponData> weaponDict;
    
    public void Initialize()
    {
        weaponDict = new Dictionary<int, WeaponData>();
        foreach (var weapon in weapons)
        {
            weaponDict[weapon.WeaponId] = weapon;
        }
    }
    
    public WeaponData GetWeapon(int id) => weaponDict.GetValueOrDefault(id);
    public IEnumerable<WeaponData> GetAllWeapons() => weapons;
}
```

---

## 运行时修改问题

### 问题说明

```
ScriptableObject数据特点：
✅ 编辑器修改：保存到磁盘
⚠️ 运行时修改：也会保存到磁盘！

问题场景：
1. 游戏运行中修改了角色生命值
2. 停止游戏
3. 生命值仍然是修改后的值
4. 下次运行数据不正确
```

### 解决方案

```csharp
// 方案1：克隆数据
public class Player : MonoBehaviour
{
    private CharacterData runtimeData;
    
    public void Initialize(CharacterData data)
    {
        // 克隆一份数据用于运行时
        runtimeData = Instantiate(data);
    }
}

// 方案2：运行时状态类
[System.Serializable]
public class CharacterRuntimeData
{
    public int currentHealth;
    public int maxHealth;
    
    public CharacterRuntimeData(CharacterData data)
    {
        maxHealth = data.MaxHealth;
        currentHealth = maxHealth;
    }
}

// 方案3：OnDisable重置
private void OnDisable()
{
    // 重置运行时数据
    currentHealth = maxHealth;
}
```

---

## 常见设计模式

### 模式1：简单配置类

```csharp
[CreateAssetMenu(fileName = "PlayerSettings", menuName = "Settings/Player")]
public class PlayerSettings : ScriptableObject
{
    [Header("移动")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    
    [Header("属性")]
    public int maxHealth = 100;
    public float attackDamage = 10f;
}
```

### 模式2：事件系统

```csharp
[CreateAssetMenu(fileName = "GameEvent", menuName = "Events/Game Event")]
public class GameEvent : ScriptableObject
{
    private List<GameEventListener> listeners = new List<GameEventListener>();
    
    public void Raise()
    {
        for (int i = listeners.Count - 1; i >= 0; i--)
        {
            listeners[i].OnEventRaised();
        }
    }
    
    public void RegisterListener(GameEventListener listener) => listeners.Add(listener);
    public void UnregisterListener(GameEventListener listener) => listeners.Remove(listener);
}
```

### 模式3：变量引用

```csharp
[CreateAssetMenu(fileName = "IntVariable", menuName = "Variables/Int")]
public class IntVariable : ScriptableObject
{
    [SerializeField] private int initialValue;
    [NonSerialized] private int runtimeValue;
    
    public int Value
    {
        get => runtimeValue;
        set => runtimeValue = value;
    }
    
    private void OnEnable() => runtimeValue = initialValue;
}
```

---

## 性能优化

### 加载优化

```
推荐方式：
✅ 启动时预加载
✅ 使用Addressables
✅ 异步加载

避免：
❌ 每帧Resources.Load
❌ 频繁创建实例
❌ 同步加载大量数据
```

### 内存优化

```
策略：
├── 共享配置数据
├── 按需加载数据库
├── 及时卸载未使用数据
└── 使用引用代替复制
```

---

## 常见问题

### Q: 修改ScriptableObject数据不保存？

```
原因：
1. 没有标记为Dirty
2. 没有调用SaveAssets

解决：
EditorUtility.SetDirty(data);
AssetDatabase.SaveAssets();
```

### Q: 运行时数据丢失？

```
原因：
使用NonSerialized标记的字段不持久化

解决：
// 运行时数据
[NonSerialized]
private int runtimeValue;

// 在OnEnable中初始化
private void OnEnable()
{
    runtimeValue = initialValue;
}
```

### Q: 数据在多个场景不一致？

```
原因：
不同场景引用了不同的ScriptableObject实例

解决：
✅ 使用全局单例访问
✅ 使用Resources统一加载
✅ 使用Addressables统一管理
```

---

## 最佳实践

### 命名规范

```
文件命名：
✅ WeaponData.asset
✅ CharacterConfig.asset
✅ GameSettings.asset

类命名：
✅ xxxData - 数据配置
✅ xxxConfig - 配置设置
✅ xxxSettings - 系统设置
✅ xxxDatabase - 数据集合
```

### 目录结构

```
Assets/
├── Data/
│   ├── Characters/
│   │   ├── PlayerData.asset
│   │   └── EnemyData.asset
│   ├── Weapons/
│   │   ├── Sword.asset
│   │   └── Bow.asset
│   ├── Levels/
│   │   └── Level1.asset
│   └── Settings/
│       ├── GameSettings.asset
│       └── AudioSettings.asset
```

---

## 扩展学习

### 官方资源
- [Unity ScriptableObject教程](https://learn.unity.com/tutorial/introduction-to-scriptable-objects)
- [数据驱动架构指南](https://unity.com/how-to/architect-game-code-scriptable-objects)
- [ScriptableObject最佳实践](https://docs.unity3d.com/Manual/BestPracticeUnderstandingPerformanceInUnity7.html)

### 进阶主题
- ScriptableObject事件系统
- 运行时数据管理
- 多态与继承
- 编辑器扩展
