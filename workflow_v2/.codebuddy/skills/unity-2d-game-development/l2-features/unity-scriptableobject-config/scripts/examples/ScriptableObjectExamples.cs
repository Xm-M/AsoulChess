using UnityEngine;
using System.Collections.Generic;

namespace Unity.ScriptableObject.Examples
{
    #region 示例1：角色配置数据

    /// <summary>
    /// 示例1：角色配置数据
    /// 演示基础的数据配置类
    /// </summary>
    [CreateAssetMenu(
        fileName = "NewCharacter",
        menuName = "Game/Data/Character",
        order = 1
    )]
    public class CharacterData : ScriptableObject
    {
        #region 字段

        [Header("基础信息")]
        [SerializeField] private int characterId;
        [SerializeField] private string characterName;
        [SerializeField] private string description;
        [SerializeField] private Sprite portrait;

        [Header("属性")]
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private int attackPower = 10;
        [SerializeField] private int defense = 5;

        [Header("技能")]
        [SerializeField] private List<SkillData> skills = new List<SkillData>();

        #endregion

        #region 属性访问器

        public int CharacterId => characterId;
        public string CharacterName => characterName;
        public string Description => description;
        public Sprite Portrait => portrait;
        public int MaxHealth => maxHealth;
        public float MoveSpeed => moveSpeed;
        public int AttackPower => attackPower;
        public int Defense => defense;
        public IReadOnlyList<SkillData> Skills => skills;

        #endregion

        #region 验证

        private void OnValidate()
        {
            // 验证必填字段
            if (string.IsNullOrEmpty(characterName))
            {
                Debug.LogWarning($"[CharacterData] 角色名称未设置: {name}", this);
            }

            // 验证数值范围
            maxHealth = Mathf.Max(1, maxHealth);
            moveSpeed = Mathf.Max(0.1f, moveSpeed);
            attackPower = Mathf.Max(0, attackPower);
            defense = Mathf.Max(0, defense);

            // 验证ID唯一性
            if (characterId <= 0)
            {
                Debug.LogError($"[CharacterData] 无效的角色ID: {characterId}", this);
            }
        }

        private void Reset()
        {
            characterId = System.Guid.NewGuid().GetHashCode();
            characterName = "New Character";
            maxHealth = 100;
            moveSpeed = 5f;
            attackPower = 10;
            defense = 5;
            skills = new List<SkillData>();
        }

        #endregion
    }

    #endregion

    #region 示例2：武器配置数据

    /// <summary>
    /// 示例2：武器配置数据
    /// 演示枚举、范围限制、嵌套数据
    /// </summary>
    [CreateAssetMenu(
        fileName = "NewWeapon",
        menuName = "Game/Data/Weapon",
        order = 2
    )]
    public class WeaponData : ScriptableObject
    {
        public enum WeaponType
        {
            Melee,      // 近战
            Ranged,     // 远程
            Magic,      // 魔法
            Thrown      // 投掷
        }

        [Header("基础属性")]
        [SerializeField] private int weaponId;
        [SerializeField] private string weaponName;
        [SerializeField] private WeaponType weaponType;
        [SerializeField] private Sprite icon;
        [SerializeField] private GameObject model;

        [Header("战斗属性")]
        [Range(1, 100)] [SerializeField] private int attackDamage = 10;
        [Range(0.5f, 3f)] [SerializeField] private float attackSpeed = 1f;
        [Range(0f, 10f)] [SerializeField] private float attackRange = 1f;
        [Range(0f, 1f)] [SerializeField] private float criticalChance = 0.1f;

        [Header("特殊效果")]
        [SerializeField] private List<EffectData> effects = new List<EffectData>();

        [Header("要求")]
        [SerializeField] private int levelRequired = 1;
        [SerializeField] private int strengthRequired = 0;

        // 属性访问器
        public int WeaponId => weaponId;
        public string WeaponName => weaponName;
        public WeaponType Type => weaponType;
        public Sprite Icon => icon;
        public GameObject Model => model;
        public int AttackDamage => attackDamage;
        public float AttackSpeed => attackSpeed;
        public float AttackRange => attackRange;
        public float CriticalChance => criticalChance;
        public IReadOnlyList<EffectData> Effects => effects;
        public int LevelRequired => levelRequired;
        public int StrengthRequired => strengthRequired;
    }

    #endregion

    #region 示例3：武器数据库

    /// <summary>
    /// 示例3：武器数据库
    /// 演示数据集合和查询功能
    /// </summary>
    [CreateAssetMenu(
        fileName = "WeaponDatabase",
        menuName = "Game/Data/Weapon Database",
        order = 3
    )]
    public class WeaponDatabase : ScriptableObject
    {
        [SerializeField] private List<WeaponData> weapons = new List<WeaponData>();

        private Dictionary<int, WeaponData> weaponDict;
        private Dictionary<WeaponData.WeaponType, List<WeaponData>> weaponsByType;

        /// <summary>
        /// 初始化数据库（游戏启动时调用）
        /// </summary>
        public void Initialize()
        {
            weaponDict = new Dictionary<int, WeaponData>();
            weaponsByType = new Dictionary<WeaponData.WeaponType, List<WeaponData>>();

            foreach (var weapon in weapons)
            {
                // ID索引
                weaponDict[weapon.WeaponId] = weapon;

                // 类型索引
                var type = weapon.Type;
                if (!weaponsByType.ContainsKey(type))
                {
                    weaponsByType[type] = new List<WeaponData>();
                }
                weaponsByType[type].Add(weapon);
            }
        }

        /// <summary>
        /// 根据ID获取武器
        /// </summary>
        public WeaponData GetWeapon(int id)
        {
            return weaponDict.TryGetValue(id, out var weapon) ? weapon : null;
        }

        /// <summary>
        /// 获取指定类型的所有武器
        /// </summary>
        public IEnumerable<WeaponData> GetWeaponsByType(WeaponData.WeaponType type)
        {
            return weaponsByType.TryGetValue(type, out var list) ? list : null;
        }

        /// <summary>
        /// 获取所有武器
        /// </summary>
        public IEnumerable<WeaponData> GetAllWeapons() => weapons;

        private void OnValidate()
        {
            // 检查重复ID
            var idSet = new HashSet<int>();
            foreach (var weapon in weapons)
            {
                if (weapon != null && idSet.Contains(weapon.WeaponId))
                {
                    Debug.LogError($"[WeaponDatabase] 重复的武器ID: {weapon.WeaponId}", this);
                }
                idSet.Add(weapon.WeaponId);
            }
        }
    }

    #endregion

    #region 示例4：事件系统

    /// <summary>
    /// 示例4：游戏事件
    /// 演示ScriptableObject事件系统
    /// </summary>
    [CreateAssetMenu(
        fileName = "GameEvent",
        menuName = "Game/Events/Game Event",
        order = 10
    )]
    public class GameEvent : ScriptableObject
    {
        private List<GameEventListener> listeners = new List<GameEventListener>();

        /// <summary>
        /// 触发事件
        /// </summary>
        public void Raise()
        {
            for (int i = listeners.Count - 1; i >= 0; i--)
            {
                listeners[i].OnEventRaised();
            }
        }

        /// <summary>
        /// 注册监听器
        /// </summary>
        public void RegisterListener(GameEventListener listener)
        {
            if (!listeners.Contains(listener))
            {
                listeners.Add(listener);
            }
        }

        /// <summary>
        /// 取消注册
        /// </summary>
        public void UnregisterListener(GameEventListener listener)
        {
            listeners.Remove(listener);
        }
    }

    /// <summary>
    /// 事件监听器
    /// </summary>
    public class GameEventListener : MonoBehaviour
    {
        [SerializeField] private GameEvent gameEvent;
        [SerializeField] private UnityEngine.Events.UnityEvent response;

        private void OnEnable()
        {
            gameEvent?.RegisterListener(this);
        }

        private void OnDisable()
        {
            gameEvent?.UnregisterListener(this);
        }

        public void OnEventRaised()
        {
            response?.Invoke();
        }
    }

    #endregion

    #region 示例5：变量引用

    /// <summary>
    /// 示例5：可引用的整型变量
    /// 演示运行时数据管理
    /// </summary>
    [CreateAssetMenu(
        fileName = "IntVariable",
        menuName = "Game/Variables/Int Variable",
        order = 20
    )]
    public class IntVariable : ScriptableObject
    {
        [SerializeField] private int initialValue;

        [System.NonSerialized]
        private int runtimeValue;

        public int Value
        {
            get => runtimeValue;
            set => runtimeValue = value;
        }

        public int InitialValue => initialValue;

        private void OnEnable()
        {
            runtimeValue = initialValue;
        }

        public void ResetValue()
        {
            runtimeValue = initialValue;
        }
    }

    /// <summary>
    /// 变量引用器
    /// </summary>
    public class VariableReference : MonoBehaviour
    {
        [SerializeField] private IntVariable healthVariable;

        public void TakeDamage(int damage)
        {
            healthVariable.Value -= damage;
        }

        public void Heal(int amount)
        {
            healthVariable.Value += amount;
        }
    }

    #endregion

    #region 示例6：游戏设置

    /// <summary>
    /// 示例6：游戏全局设置
    /// 演示单例访问模式
    /// </summary>
    [CreateAssetMenu(
        fileName = "GameSettings",
        menuName = "Game/Settings/Game Settings",
        order = 100
    )]
    public class GameSettings : ScriptableObject
    {
        private static GameSettings _instance;
        public static GameSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<GameSettings>("GameSettings");
                }
                return _instance;
            }
        }

        [Header("音频")]
        [Range(0f, 1f)] [SerializeField] private float masterVolume = 1f;
        [Range(0f, 1f)] [SerializeField] private float musicVolume = 0.7f;
        [Range(0f, 1f)] [SerializeField] private float sfxVolume = 1f;

        [Header("画面")]
        [SerializeField] private int targetFrameRate = 60;
        [SerializeField] private bool vSync = true;
        [SerializeField] private int qualityLevel = 2;

        [Header("游戏")]
        [SerializeField] private float defaultGravity = -9.81f;
        [SerializeField] private string defaultLanguage = "zh-CN";

        public float MasterVolume => masterVolume;
        public float MusicVolume => musicVolume;
        public float SFXVolume => sfxVolume;
        public int TargetFrameRate => targetFrameRate;
        public bool VSync => vSync;
        public int QualityLevel => qualityLevel;
        public float DefaultGravity => defaultGravity;
        public string DefaultLanguage => defaultLanguage;

        public void ApplySettings()
        {
            AudioListener.volume = masterVolume;
            QualitySettings.vSyncCount = vSync ? 1 : 0;
            Application.targetFrameRate = targetFrameRate;
            QualitySettings.SetQualityLevel(qualityLevel);
        }
    }

    #endregion

    #region 示例7：关卡配置

    /// <summary>
    /// 示例7：关卡配置数据
    /// 演示复杂嵌套数据结构
    /// </summary>
    [CreateAssetMenu(
        fileName = "NewLevel",
        menuName = "Game/Data/Level",
        order = 4
    )]
    public class LevelData : ScriptableObject
    {
        [System.Serializable]
        public class EnemySpawnData
        {
            public CharacterData enemyPrefab;
            public Vector2 spawnPosition;
            public int count = 1;
            public float spawnDelay = 0f;
        }

        [Header("关卡信息")]
        [SerializeField] private int levelId;
        [SerializeField] private string levelName;
        [SerializeField] private string sceneName;

        [Header("难度设置")]
        [Range(1, 10)] [SerializeField] private int difficulty = 1;
        [SerializeField] private float timeLimit = 180f;

        [Header("敌人配置")]
        [SerializeField] private List<EnemySpawnData> enemies = new List<EnemySpawnData>();

        [Header("奖励")]
        [SerializeField] private int goldReward = 100;
        [SerializeField] private int experienceReward = 50;
        [SerializeField] private List<WeaponData> weaponRewards = new List<WeaponData>();

        public int LevelId => levelId;
        public string LevelName => levelName;
        public string SceneName => sceneName;
        public int Difficulty => difficulty;
        public float TimeLimit => timeLimit;
        public IReadOnlyList<EnemySpawnData> Enemies => enemies;
        public int GoldReward => goldReward;
        public int ExperienceReward => experienceReward;
        public IReadOnlyList<WeaponData> WeaponRewards => weaponRewards;
    }

    #endregion

    #region 辅助类

    /// <summary>
    /// 技能数据
    /// </summary>
    [System.Serializable]
    public class SkillData
    {
        public string skillName;
        public float cooldown = 5f;
        public int manaCost = 10;
        public float duration = 3f;
        public Sprite icon;
    }

    /// <summary>
    /// 效果数据
    /// </summary>
    [System.Serializable]
    public class EffectData
    {
        public string effectName;
        public float duration = 5f;
        public int value = 10;
    }

    #endregion
}
