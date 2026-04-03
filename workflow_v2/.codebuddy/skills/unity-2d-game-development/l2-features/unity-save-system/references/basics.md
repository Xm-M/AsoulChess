# Save System 基础概念

## 什么是存档系统？

存档系统（Save System）是游戏开发中用于持久化存储游戏数据的核心系统，负责将游戏状态、玩家进度、配置信息等数据保存到存储介质中，并在需要时恢复这些数据。

---

## 存储方案对比

### 主要存储方案

| 方案 | 优点 | 缺点 | 适用场景 |
|------|------|------|---------|
| PlayerPrefs | 简单易用、跨平台 | 仅支持简单类型、无加密 | 设置项、简单标记 |
| JsonUtility | Unity内置、易序列化 | 仅支持public字段 | 中等复杂度数据 |
| BinaryFormatter | 二进制紧凑 | 已废弃、安全风险 | ❌ 不推荐使用 |
| SQLite | 结构化查询、大数据 | 需要额外库、复杂 | 大量结构化数据 |
| 文件系统 | 灵活、可控 | 需手动管理 | 自定义格式 |

### 推荐方案组合

```
简单设置     → PlayerPrefs
游戏进度     → JsonUtility + 文件系统
敏感数据     → JsonUtility + 加密
大数据/复杂查询 → SQLite
```

---

## PlayerPrefs基础

### 特点

```
优点：
- 使用简单，无需文件操作
- 跨平台自动处理路径
- 自动持久化

缺点：
- 仅支持int、float、string
- 数据量受限
- 无加密，易被篡改
- 不适合复杂数据结构
```

### 存储位置

```
Windows：注册表
  HKEY_CURRENT_USER\Software\[Company]\[GameName]

macOS：plist文件
  ~/Library/Preferences/[BundleIdentifier].plist

Linux：配置文件
  ~/.config/unity3d/[Company]/[GameName]

Android：SharedPreferences
  SharedPreferences

iOS：NSUserDefaults
  NSUserDefaults
```

### 使用示例

```csharp
// 存储数据
PlayerPrefs.SetInt("HighScore", 100);
PlayerPrefs.SetFloat("MusicVolume", 0.8f);
PlayerPrefs.SetString("PlayerName", "Hero");
PlayerPrefs.Save(); // 立即写入磁盘

// 读取数据
int highScore = PlayerPrefs.GetInt("HighScore", 0);
float volume = PlayerPrefs.GetFloat("MusicVolume", 1.0f);
string name = PlayerPrefs.GetString("PlayerName", "Player");

// 检查和删除
bool hasKey = PlayerPrefs.HasKey("HighScore");
PlayerPrefs.DeleteKey("HighScore");
PlayerPrefs.DeleteAll();
```

---

## JsonUtility基础

### 特点

```
优点：
- Unity内置，无需额外依赖
- 序列化结果可读
- 支持自定义类

缺点：
- 仅支持public字段或[SerializeField]
- 不支持Dictionary
- 不支持多态
- 不支持null值
```

### 序列化规则

```csharp
[Serializable]
public class PlayerData
{
    public string playerName;        // ✅ public字段
    [SerializeField] private int level; // ✅ SerializeField
    public int[] inventory;          // ✅ 数组
    public List<string> skills;      // ✅ List
    
    // ❌ 不支持的类型
    // public Dictionary<string, int> stats;
    // private int health; // 无SerializeField
}
```

### 使用示例

```csharp
using UnityEngine;

[Serializable]
public class GameData
{
    public int level;
    public float playTime;
    public Vector3 playerPosition;
    public List<int> inventory;
}

// 序列化
GameData data = new GameData { level = 5, playTime = 3600f };
string json = JsonUtility.ToJson(data, true); // true = 格式化

// 反序列化
GameData loadedData = JsonUtility.FromJson<GameData>(json);

// 从文件加载
GameData data = JsonUtility.FromJson<GameData>(File.ReadAllText(path));

// 写入文件
File.WriteAllText(path, JsonUtility.ToJson(data, true));
```

---

## 文件系统操作

### 持久化路径

```csharp
// 推荐路径
string savePath = Path.Combine(Application.persistentDataPath, "Save");

// 路径说明
Application.persistentDataPath：
- Windows: C:\Users\[User]\AppData\LocalLow\[Company]\[Game]
- macOS: ~/Library/Application Support/[Company]/[Game]
- Android: /storage/emulated/0/Android/data/[Package]/files
- iOS: Application/xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx/Documents
```

### 文件操作示例

```csharp
using System.IO;

public class FileSaveSystem
{
    private string saveDirectory;
    
    public FileSaveSystem()
    {
        saveDirectory = Path.Combine(Application.persistentDataPath, "Save");
        if (!Directory.Exists(saveDirectory))
        {
            Directory.CreateDirectory(saveDirectory);
        }
    }
    
    public void Save(string fileName, string content)
    {
        string filePath = Path.Combine(saveDirectory, fileName);
        File.WriteAllText(filePath, content);
    }
    
    public string Load(string fileName)
    {
        string filePath = Path.Combine(saveDirectory, fileName);
        if (File.Exists(filePath))
        {
            return File.ReadAllText(filePath);
        }
        return null;
    }
    
    public bool FileExists(string fileName)
    {
        return File.Exists(Path.Combine(saveDirectory, fileName));
    }
    
    public void DeleteFile(string fileName)
    {
        string filePath = Path.Combine(saveDirectory, fileName);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }
}
```

---

## 存档数据设计

### 数据分层

```
存档数据结构：
├── PlayerData（玩家数据）
│   ├── 基础信息（等级、经验、金币）
│   ├── 属性数据（血量、攻击力）
│   └── 装备数据（武器、防具）
├── ProgressData（进度数据）
│   ├── 关卡进度
│   ├── 任务进度
│   └── 成就数据
├── SettingsData（设置数据）
│   ├── 音量设置
│   ├── 画面设置
│   └── 控制设置
└── WorldData（世界数据）
    ├── 场景状态
    ├── NPC状态
    └── 物品状态
```

### 存档类示例

```csharp
[Serializable]
public class SaveData
{
    public int version;                    // 存档版本
    public string saveTime;                // 保存时间
    public PlayerData player;              // 玩家数据
    public List<LevelProgress> progress;   // 进度数据
    public SettingsData settings;          // 设置数据
}

[Serializable]
public class PlayerData
{
    public string playerName;
    public int level;
    public int experience;
    public int gold;
    public Vector3Serializable position;  // 使用可序列化类型
    public List<int> inventory;
}

[Serializable]
public struct Vector3Serializable  // JsonUtility不支持Vector3
{
    public float x, y, z;
    
    public Vector3Serializable(Vector3 v)
    {
        x = v.x; y = v.y; z = v.z;
    }
    
    public Vector3 ToVector3() => new Vector3(x, y, z);
}
```

---

## 多存档槽位

### 槽位管理

```csharp
public class SaveSlotManager
{
    private const int MaxSlots = 3;
    private string saveDirectory;
    
    public SaveSlotManager()
    {
        saveDirectory = Path.Combine(Application.persistentDataPath, "Saves");
        Directory.CreateDirectory(saveDirectory);
    }
    
    public string GetSlotPath(int slotIndex)
    {
        return Path.Combine(saveDirectory, $"slot_{slotIndex}.json");
    }
    
    public bool HasSlotData(int slotIndex)
    {
        return File.Exists(GetSlotPath(slotIndex));
    }
    
    public List<SaveSlotInfo> GetAllSlotInfos()
    {
        var infos = new List<SaveSlotInfo>();
        for (int i = 0; i < MaxSlots; i++)
        {
            if (HasSlotData(i))
            {
                infos.Add(LoadSlotInfo(i));
            }
        }
        return infos;
    }
    
    public SaveSlotInfo LoadSlotInfo(int slotIndex)
    {
        string json = File.ReadAllText(GetSlotPath(slotIndex));
        var data = JsonUtility.FromJson<SaveData>(json);
        return new SaveSlotInfo
        {
            slotIndex = slotIndex,
            playerName = data.player.playerName,
            level = data.player.level,
            saveTime = data.saveTime
        };
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
```

---

## 存档版本管理

### 版本兼容性设计

```csharp
[Serializable]
public class SaveData
{
    public const int CurrentVersion = 2;
    
    public int version;
    public PlayerData player;
    
    // 升级存档数据
    public void Upgrade()
    {
        if (version < 1)
        {
            // v0 -> v1: 添加新字段
            player.newField = defaultValue;
            version = 1;
        }
        
        if (version < 2)
        {
            // v1 -> v2: 重构数据结构
            player.renamedField = player.oldField;
            version = 2;
        }
    }
}

// 加载时自动升级
public SaveData LoadSaveData(string path)
{
    string json = File.ReadAllText(path);
    var data = JsonUtility.FromJson<SaveData>(json);
    
    // 自动升级旧版本存档
    if (data.version < SaveData.CurrentVersion)
    {
        data.Upgrade();
        SaveSaveData(path, data); // 保存升级后的存档
    }
    
    return data;
}
```

---

## 数据安全

### 加密方案

```csharp
using System.Security.Cryptography;
using System.Text;

public class SaveEncryption
{
    private static readonly string Key = "YourSecretKey123"; // 应使用更安全的密钥管理
    
    public static string Encrypt(string data)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(data);
        byte[] key = Encoding.UTF8.GetBytes(Key);
        
        // 简单XOR加密（示例，生产环境应使用AES）
        for (int i = 0; i < bytes.Length; i++)
        {
            bytes[i] ^= key[i % key.Length];
        }
        
        return Convert.ToBase64String(bytes);
    }
    
    public static string Decrypt(string encryptedData)
    {
        byte[] bytes = Convert.FromBase64String(encryptedData);
        byte[] key = Encoding.UTF8.GetBytes(Key);
        
        for (int i = 0; i < bytes.Length; i++)
        {
            bytes[i] ^= key[i % key.Length];
        }
        
        return Encoding.UTF8.GetString(bytes);
    }
}

// 使用示例
public void SaveEncrypted(SaveData data)
{
    string json = JsonUtility.ToJson(data);
    string encrypted = SaveEncryption.Encrypt(json);
    File.WriteAllText(savePath, encrypted);
}

public SaveData LoadEncrypted()
{
    string encrypted = File.ReadAllText(savePath);
    string json = SaveEncryption.Decrypt(encrypted);
    return JsonUtility.FromJson<SaveData>(json);
}
```

---

## 自动存档机制

### 自动存档实现

```csharp
public class AutoSaveSystem : MonoBehaviour
{
    [SerializeField] private float autoSaveInterval = 300f; // 5分钟
    
    private float timer;
    
    private void Update()
    {
        timer += Time.deltaTime;
        
        if (timer >= autoSaveInterval)
        {
            AutoSave();
            timer = 0f;
        }
    }
    
    private void AutoSave()
    {
        var saveData = CollectSaveData();
        SaveSystem.Save(saveData);
        Debug.Log("自动存档完成");
    }
    
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            AutoSave(); // 应用暂停时自动存档
        }
    }
    
    private void OnApplicationQuit()
    {
        AutoSave(); // 退出时自动存档
    }
}
```

---

## 常见问题

### Q: 存档损坏怎么办？

```
预防措施：
1. 写入前备份旧存档
2. 使用临时文件写入完成后替换
3. 添加存档校验和

恢复方案：
1. 加载备份存档
2. 重置为默认值
3. 提示用户存档损坏
```

### Q: 版本更新后存档不兼容？

```
解决：
1. 使用存档版本号
2. 实现版本升级逻辑
3. 保留旧版本兼容代码
4. 重大版本更新时迁移工具
```

### Q: 跨平台路径问题？

```
原则：
1. 始终使用Application.persistentDataPath
2. 使用Path.Combine拼接路径
3. 避免硬编码路径分隔符
4. 测试各平台存储权限
```

---

## 扩展学习

### 官方文档
- [Unity PlayerPrefs](https://docs.unity3d.com/ScriptReference/PlayerPrefs.html)
- [Unity JsonUtility](https://docs.unity3d.com/ScriptReference/JsonUtility.html)
- [Persistent Data Path](https://docs.unity3d.com/ScriptReference/Application-persistentDataPath.html)

### 进阶主题
- 存档加密与安全
- 云存档同步
- 存档压缩
- SQLite集成
