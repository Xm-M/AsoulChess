# ScriptableObject API参考

## 核心API

### ScriptableObject类

#### 创建ScriptableObject

```csharp
// 运行时创建实例
public static ScriptableObject CreateInstance(string className);
public static T CreateInstance<T>() where T : ScriptableObject;

// 示例
WeaponData weapon = ScriptableObject.CreateInstance<WeaponData>();
```

#### 生命周期方法

```csharp
// 创建时调用（仅在编辑器）
void Awake();

// 启用时调用
void OnEnable();

// 禁用时调用
void OnDisable();

// 销毁时调用
void OnDestroy();

// 编辑器验证
void OnValidate();

// 编辑器重置
void Reset();
```

---

## 编辑器特性

### CreateAssetMenu

```csharp
// 基础用法
[CreateAssetMenu]
public class WeaponData : ScriptableObject { }

// 完整配置
[CreateAssetMenu(
    fileName = "NewWeapon",           // 默认文件名
    menuName = "Game/Weapon Data",    // 菜单路径
    order = 1                         // 菜单顺序
)]
public class WeaponData : ScriptableObject { }
```

### Header

```csharp
// 分组显示
[Header("基础属性")]
public string weaponName;
public int weaponId;

[Header("战斗属性")]
public int attackDamage;
public float attackSpeed;

[Header("视觉效果")]
public Sprite icon;
public GameObject model;
```

### Tooltip

```csharp
// 添加提示信息
[Tooltip("武器名称，显示在UI中")]
public string weaponName;

[Tooltip("攻击伤害值，范围1-100")]
[Range(1, 100)]
public int attackDamage;
```

### Range

```csharp
// 数值范围限制
[Range(0f, 1f)]
public float criticalChance;

[Range(1, 100)]
public int attackDamage;
```

### Space

```csharp
// 添加空行
public int id;

[Space(10)] // 10像素空行
public string name;
```

---

## 序列化相关

### SerializeField

```csharp
// 私有字段序列化
[SerializeField] private int weaponId;
[SerializeField] private string weaponName;
[SerializeField] private Sprite icon;
```

### SerializeReference

```csharp
// 引用序列化（支持多态）
[SerializeReference]
private IEffect effect;

[SerializeReference, SubclassSelector]
private List<IEffect> effects;
```

### NonSerialized

```csharp
// 不序列化
[NonSerialized]
public int runtimeValue; // 运行时临时数据
```

---

## 资源管理

### Resources加载

```csharp
// 从Resources加载
WeaponData weapon = Resources.Load<WeaponData>("Data/Weapons/Sword");

// 异步加载
ResourceRequest request = Resources.LoadAsync<WeaponData>("Data/Weapons/Sword");
yield return request;
WeaponData weapon = request.asset as WeaponData;
```

### AssetDatabase（仅编辑器）

```csharp
#if UNITY_EDITOR
using UnityEditor;

// 查找所有ScriptableObject资产
string[] guids = AssetDatabase.FindAssets("t:WeaponData");
foreach (string guid in guids)
{
    string path = AssetDatabase.GUIDToAssetPath(guid);
    WeaponData weapon = AssetDatabase.LoadAssetAtPath<WeaponData>(path);
}

// 创建资产
WeaponData weapon = ScriptableObject.CreateInstance<WeaponData>();
AssetDatabase.CreateAsset(weapon, "Assets/Data/NewWeapon.asset");

// 保存修改
EditorUtility.SetDirty(weapon);
AssetDatabase.SaveAssets();
#endif
```

### Addressables（推荐）

```csharp
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

// 异步加载
AsyncOperationHandle<WeaponData> handle = Addressables.LoadAssetAsync<WeaponData>("Weapon_Sword");
handle.Completed += (op) =>
{
    WeaponData weapon = op.Result;
    // 使用weapon
};

// 释放
Addressables.Release(handle);
```

---

## API白名单

### ✅ 推荐使用

| API | 用途 | 性能等级 |
|-----|------|---------|
| CreateAssetMenu | 编辑器创建资产 | ⭐⭐⭐⭐⭐ |
| OnValidate | 数据验证 | ⭐⭐⭐⭐⭐ |
| Resources.Load | 简单加载 | ⭐⭐⭐⭐ |
| Addressables | 异步加载（推荐） | ⭐⭐⭐⭐⭐ |
| SerializeField | 私有字段序列化 | ⭐⭐⭐⭐⭐ |

### ⚠️ 警告使用

| API | 问题 | 替代方案 |
|-----|------|---------|
| CreateInstance | 运行时创建开销大 | 预加载资产 |
| Resources.LoadAll | 性能开销大 | 按需加载 |
| AssetDatabase | 仅编辑器可用 | 使用Addressables |
| FindObjectOfType | 性能差 | 使用引用 |

### ❌ 禁止使用

| API | 原因 |
|-----|------|
| 运行时修改SO数据 | 修改会持久化 |
| 大量Resources.Load | 内存占用高 |
| 每帧FindObjectsOfType | 性能极差 |

---

## 常用配置模板

### 角色配置

```csharp
[CreateAssetMenu(fileName = "NewCharacter", menuName = "Game/Character Data")]
public class CharacterData : ScriptableObject
{
    [Header("基础信息")]
    [SerializeField] private int characterId;
    [SerializeField] private string characterName;
    [SerializeField] private Sprite portrait;
    
    [Header("属性")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private int attackPower = 10;
    
    [Header("技能")]
    [SerializeField] private List<SkillData> skills;
    
    // 属性访问器
    public int CharacterId => characterId;
    public string CharacterName => characterName;
    public Sprite Portrait => portrait;
    public int MaxHealth => maxHealth;
    public float MoveSpeed => moveSpeed;
    public int AttackPower => attackPower;
    public IReadOnlyList<SkillData> Skills => skills;
    
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(characterName))
            Debug.LogWarning("角色名称不能为空");
        if (maxHealth <= 0)
            maxHealth = 1;
    }
}
```

### 武器配置

```csharp
[CreateAssetMenu(fileName = "NewWeapon", menuName = "Game/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("基础属性")]
    [SerializeField] private int weaponId;
    [SerializeField] private string weaponName;
    [Tooltip("武器类型")]
    [SerializeField] private WeaponType weaponType;
    [SerializeField] private Sprite icon;
    
    [Header("战斗属性")]
    [Range(1, 100)] [SerializeField] private int attackDamage = 10;
    [Range(0.5f, 3f)] [SerializeField] private float attackSpeed = 1f;
    [Range(0f, 10f)] [SerializeField] private float attackRange = 1f;
    
    [Header("特殊效果")]
    [SerializeField] private List<EffectData> effects;
    
    public int WeaponId => weaponId;
    public string WeaponName => weaponName;
    public WeaponType WeaponType => weaponType;
    public Sprite Icon => icon;
    public int AttackDamage => attackDamage;
    public float AttackSpeed => attackSpeed;
    public float AttackRange => attackRange;
}
```

### 关卡配置

```csharp
[CreateAssetMenu(fileName = "NewLevel", menuName = "Game/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("关卡信息")]
    [SerializeField] private int levelId;
    [SerializeField] private string levelName;
    [SerializeField] private SceneReference scene;
    
    [Header("难度设置")]
    [Range(1, 10)] [SerializeField] private int difficulty = 1;
    [SerializeField] private float timeLimit = 180f;
    
    [Header("敌人配置")]
    [SerializeField] private List<EnemySpawnData> enemies;
    
    [Header("奖励")]
    [SerializeField] private int goldReward = 100;
    [SerializeField] private int experienceReward = 50;
}
```

---

## 验证模式

### OnValidate验证

```csharp
private void OnValidate()
{
    // 验证必填字段
    if (weaponId <= 0)
        Debug.LogError($"武器ID无效: {weaponId}", this);
    
    if (string.IsNullOrEmpty(weaponName))
        Debug.LogWarning("武器名称未设置", this);
    
    // 验证数值范围
    attackDamage = Mathf.Max(1, attackDamage);
    attackSpeed = Mathf.Clamp(attackSpeed, 0.5f, 3f);
    
    // 验证引用
    if (icon == null)
        Debug.LogWarning("武器图标未设置", this);
    
    // 验证列表
    if (effects != null)
    {
        for (int i = effects.Count - 1; i >= 0; i--)
        {
            if (effects[i] == null)
                effects.RemoveAt(i);
        }
    }
}
```

### 重置默认值

```csharp
private void Reset()
{
    // 设置默认值
    weaponId = System.Guid.NewGuid().GetHashCode();
    attackDamage = 10;
    attackSpeed = 1f;
    attackRange = 1f;
    effects = new List<EffectData>();
}
```

---

## 运行时访问模式

### 静态访问

```csharp
public class GameData : ScriptableObject
{
    private static GameData instance;
    
    public static GameData Instance
    {
        get
        {
            if (instance == null)
            {
                instance = Resources.Load<GameData>("GameData");
            }
            return instance;
        }
    }
}

// 使用
int damage = GameData.Instance.GetWeaponDamage(weaponId);
```

### 数据库模式

```csharp
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
    
    public WeaponData GetWeapon(int id)
    {
        return weaponDict.TryGetValue(id, out var weapon) ? weapon : null;
    }
}
```

---

## 扩展资源

- [Unity ScriptableObject官方文档](https://docs.unity3d.com/Manual/class-ScriptableObject.html)
- [CreateAssetMenu文档](https://docs.unity3d.com/ScriptReference/CreateAssetMenuAttribute.html)
- [数据驱动架构最佳实践](https://unity.com/how-to/architect-game-code-scriptable-objects)
