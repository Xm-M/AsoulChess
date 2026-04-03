using UnityEngine;
using System.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Unity.SaveSystem.Examples
{
    #region 示例1：PlayerPrefs基础使用

    /// <summary>
    /// 示例1：PlayerPrefs基础使用
    /// 适用于简单设置项存储
    /// </summary>
    public class PlayerPrefsExample : MonoBehaviour
    {
        private const string KeyHighScore = "HighScore";
        private const string KeyMusicVolume = "MusicVolume";
        private const string KeyPlayerName = "PlayerName";

        public void SaveSettings()
        {
            PlayerPrefs.SetInt(KeyHighScore, 1000);
            PlayerPrefs.SetFloat(KeyMusicVolume, 0.8f);
            PlayerPrefs.SetString(KeyPlayerName, "Hero");
            PlayerPrefs.Save(); // 立即写入磁盘
        }

        public void LoadSettings()
        {
            int highScore = PlayerPrefs.GetInt(KeyHighScore, 0);
            float volume = PlayerPrefs.GetFloat(KeyMusicVolume, 1.0f);
            string playerName = PlayerPrefs.GetString(KeyPlayerName, "Player");
        }

        public void DeleteSettings()
        {
            PlayerPrefs.DeleteKey(KeyHighScore);
            PlayerPrefs.DeleteAll(); // 删除所有
        }

        public bool HasSettings()
        {
            return PlayerPrefs.HasKey(KeyHighScore);
        }
    }

    #endregion

    #region 示例2：JsonUtility序列化

    /// <summary>
    /// 示例2：JsonUtility数据序列化
    /// 适用于中等复杂度的游戏数据
    /// </summary>
    [Serializable]
    public class PlayerData
    {
        public string playerName;
        public int level;
        public int experience;
        public int gold;
        public float health;
        public float mana;
        
        // Vector3需要使用可序列化包装
        public Vector3Data position;
        
        // List支持
        public List<int> inventory = new List<int>();
        public List<string> skills = new List<string>();
    }

    [Serializable]
    public struct Vector3Data
    {
        public float x, y, z;
        
        public Vector3Data(Vector3 v)
        {
            x = v.x;
            y = v.y;
            z = v.z;
        }
        
        public Vector3 ToVector3() => new Vector3(x, y, z);
        public static Vector3Data FromVector3(Vector3 v) => new Vector3Data(v);
    }

    public class JsonSaveExample : MonoBehaviour
    {
        private string savePath;

        private void Awake()
        {
            savePath = Path.Combine(Application.persistentDataPath, "Save", "player.json");
        }

        public void SavePlayerData(PlayerData data)
        {
            string json = JsonUtility.ToJson(data, true);
            
            // 确保目录存在
            Directory.CreateDirectory(Path.GetDirectoryName(savePath));
            
            File.WriteAllText(savePath, json);
            Debug.Log($"存档已保存: {savePath}");
        }

        public PlayerData LoadPlayerData()
        {
            if (File.Exists(savePath))
            {
                string json = File.ReadAllText(savePath);
                return JsonUtility.FromJson<PlayerData>(json);
            }
            return new PlayerData();
        }
    }

    #endregion

    #region 示例3：完整存档系统

    /// <summary>
    /// 示例3：完整的存档系统
    /// 包含版本管理、加密、多槽位
    /// </summary>
    [Serializable]
    public class SaveData
    {
        public const int CurrentVersion = 2;
        
        public int version = CurrentVersion;
        public string saveTime;
        public string playerName;
        public int slotIndex;
        
        public PlayerData player;
        public GameProgressData progress;
        public SettingsData settings;
        
        public void Upgrade()
        {
            if (version < 1)
            {
                // v0 -> v1 升级逻辑
                version = 1;
            }
            if (version < 2)
            {
                // v1 -> v2 升级逻辑
                version = 2;
            }
        }
    }

    [Serializable]
    public class GameProgressData
    {
        public int currentLevel;
        public List<int> unlockedLevels = new List<int>();
        public float totalPlayTime;
    }

    [Serializable]
    public class SettingsData
    {
        public float musicVolume = 1f;
        public float sfxVolume = 1f;
        public int qualityLevel = 2;
        public bool fullscreen = true;
    }

    public class SaveSystem : MonoBehaviour
    {
        public static SaveSystem Instance { get; private set; }
        
        private const int MaxSlots = 3;
        private string saveDirectory;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            saveDirectory = Path.Combine(Application.persistentDataPath, "Saves");
            Directory.CreateDirectory(saveDirectory);
        }

        public string GetSlotPath(int slotIndex)
        {
            return Path.Combine(saveDirectory, $"slot_{slotIndex}.sav");
        }

        public void Save(int slotIndex, SaveData data)
        {
            data.slotIndex = slotIndex;
            data.saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            
            string json = JsonUtility.ToJson(data, true);
            string encrypted = Encrypt(json);
            
            string path = GetSlotPath(slotIndex);
            
            // 写入临时文件，完成后替换（防止写入中断导致损坏）
            string tempPath = path + ".tmp";
            File.WriteAllText(tempPath, encrypted);
            
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            File.Move(tempPath, path);
            
            Debug.Log($"存档已保存: 槽位{slotIndex}");
        }

        public SaveData Load(int slotIndex)
        {
            string path = GetSlotPath(slotIndex);
            
            if (!File.Exists(path))
            {
                Debug.LogWarning($"存档不存在: 槽位{slotIndex}");
                return null;
            }

            try
            {
                string encrypted = File.ReadAllText(path);
                string json = Decrypt(encrypted);
                SaveData data = JsonUtility.FromJson<SaveData>(json);
                
                // 版本升级
                if (data.version < SaveData.CurrentVersion)
                {
                    data.Upgrade();
                    Save(slotIndex, data); // 保存升级后的数据
                }
                
                return data;
            }
            catch (Exception e)
            {
                Debug.LogError($"加载存档失败: {e.Message}");
                return null;
            }
        }

        public bool HasSlotData(int slotIndex)
        {
            return File.Exists(GetSlotPath(slotIndex));
        }

        public void DeleteSlot(int slotIndex)
        {
            string path = GetSlotPath(slotIndex);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        public List<SaveSlotInfo> GetAllSlotInfos()
        {
            var infos = new List<SaveSlotInfo>();
            
            for (int i = 0; i < MaxSlots; i++)
            {
                if (HasSlotData(i))
                {
                    SaveData data = Load(i);
                    if (data != null)
                    {
                        infos.Add(new SaveSlotInfo
                        {
                            slotIndex = i,
                            playerName = data.playerName,
                            level = data.player?.level ?? 1,
                            saveTime = data.saveTime
                        });
                    }
                }
            }
            
            return infos;
        }

        // 简单加密（生产环境应使用AES）
        private string Encrypt(string data)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            byte[] key = Encoding.UTF8.GetBytes("SaveKey123");
            
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] ^= key[i % key.Length];
            }
            
            return Convert.ToBase64String(bytes);
        }

        private string Decrypt(string encryptedData)
        {
            byte[] bytes = Convert.FromBase64String(encryptedData);
            byte[] key = Encoding.UTF8.GetBytes("SaveKey123");
            
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] ^= key[i % key.Length];
            }
            
            return Encoding.UTF8.GetString(bytes);
        }
    }

    [Serializable]
    public class SaveSlotInfo
    {
        public int slotIndex;
        public string playerName;
        public int level;
        public string saveTime;
    }

    #endregion

    #region 示例4：自动存档

    /// <summary>
    /// 示例4：自动存档系统
    /// 定时存档 + 关键事件触发存档
    /// </summary>
    public class AutoSaveSystem : MonoBehaviour
    {
        [SerializeField] private float autoSaveInterval = 300f; // 5分钟
        
        private float timer;
        private bool isDirty;

        public event Action OnAutoSave;

        private void Update()
        {
            timer += Time.deltaTime;
            
            if (timer >= autoSaveInterval)
            {
                AutoSave();
                timer = 0f;
            }
        }

        public void MarkDirty()
        {
            isDirty = true;
        }

        private void AutoSave()
        {
            if (!isDirty) return;
            
            // 收集并存档数据
            var saveData = CollectSaveData();
            SaveSystem.Instance?.Save(0, saveData);
            
            isDirty = false;
            OnAutoSave?.Invoke();
            
            Debug.Log("自动存档完成");
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && isDirty)
            {
                AutoSave();
            }
        }

        private void OnApplicationQuit()
        {
            if (isDirty)
            {
                AutoSave();
            }
        }

        private SaveData CollectSaveData()
        {
            // 收集游戏数据
            return new SaveData();
        }
    }

    #endregion

    #region 示例5：运行时存档管理器

    /// <summary>
    /// 示例5：运行时存档管理器
    /// 管理当前游戏状态和存档操作
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }
        
        private SaveData currentSave;
        private int currentSlot = -1;
        
        public SaveData CurrentSave => currentSave;
        public int CurrentSlot => currentSlot;
        public bool HasLoadedSave => currentSave != null;

        public event Action<SaveData> OnSaveLoaded;
        public event Action<SaveData> OnSaveCreated;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void NewGame(string playerName)
        {
            currentSave = new SaveData
            {
                playerName = playerName,
                player = new PlayerData
                {
                    playerName = playerName,
                    level = 1,
                    health = 100f,
                    mana = 50f,
                    position = new Vector3Data(Vector3.zero)
                },
                progress = new GameProgressData
                {
                    currentLevel = 1,
                    unlockedLevels = new List<int> { 1 }
                },
                settings = new SettingsData()
            };
            
            OnSaveCreated?.Invoke(currentSave);
        }

        public void LoadGame(int slotIndex)
        {
            currentSave = SaveSystem.Instance?.Load(slotIndex);
            
            if (currentSave != null)
            {
                currentSlot = slotIndex;
                OnSaveLoaded?.Invoke(currentSave);
            }
        }

        public void SaveGame()
        {
            if (currentSave == null || currentSlot < 0)
            {
                Debug.LogError("无法存档：没有活跃的游戏");
                return;
            }
            
            SaveSystem.Instance?.Save(currentSlot, currentSave);
        }

        public void SaveGame(int slotIndex)
        {
            if (currentSave == null)
            {
                Debug.LogError("无法存档：没有活跃的游戏");
                return;
            }
            
            currentSlot = slotIndex;
            SaveSystem.Instance?.Save(slotIndex, currentSave);
        }

        public void UpdatePlayerData(PlayerData data)
        {
            if (currentSave != null)
            {
                currentSave.player = data;
            }
        }

        public void UpdateProgress(GameProgressData progress)
        {
            if (currentSave != null)
            {
                currentSave.progress = progress;
            }
        }
    }

    #endregion

    #region 示例6：存档数据收集器

    /// <summary>
    /// 示例6：存档数据收集器
    /// 从场景中收集需要保存的数据
    /// </summary>
    public class SaveDataCollector : MonoBehaviour
    {
        [SerializeField] private Transform playerTransform;
        
        public PlayerData CollectPlayerData()
        {
            var data = new PlayerData
            {
                position = Vector3Data.FromVector3(playerTransform.position),
                level = PlayerPrefs.GetInt("PlayerLevel", 1),
                experience = PlayerPrefs.GetInt("PlayerExp", 0),
                gold = PlayerPrefs.GetInt("PlayerGold", 0),
                health = PlayerPrefs.GetFloat("PlayerHealth", 100f),
                mana = PlayerPrefs.GetFloat("PlayerMana", 50f)
            };
            
            // 收集物品栏
            // data.inventory = inventoryManager.GetItems();
            
            return data;
        }

        public void ApplyPlayerData(PlayerData data)
        {
            playerTransform.position = data.position.ToVector3();
            PlayerPrefs.SetInt("PlayerLevel", data.level);
            PlayerPrefs.SetInt("PlayerExp", data.experience);
            PlayerPrefs.SetInt("PlayerGold", data.gold);
            PlayerPrefs.SetFloat("PlayerHealth", data.health);
            PlayerPrefs.SetFloat("PlayerMana", data.mana);
        }
    }

    #endregion
}
