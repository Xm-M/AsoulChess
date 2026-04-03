---
name: unity-save-system
description: Unity存档系统，提供本地数据持久化方案，包括PlayerPrefs、JsonUtility序列化、加密存储、多存档槽位等功能。
---

# Unity Save System 存档系统

## 概述

Unity存档系统，提供本地数据持久化方案，包括PlayerPrefs、JsonUtility序列化、加密存储、多存档槽位等功能。是游戏进度保存和设置持久化的核心模块。

## 存储方案对比

| 方案 | 适用场景 | 安全性 | 性能 | 推荐度 |
|------|---------|-------|------|--------|
| **PlayerPrefs** | 简单配置、少量数据 | 低（明文） | 高 | ⭐⭐⭐ |
| **JsonUtility** | 复杂对象、单机存档 | 中（可加密） | 高 | ⭐⭐⭐⭐⭐ |
| **SQLite** | 大量数据、复杂查询 | 高 | 中 | ⭐⭐⭐⭐ |
| **BinaryFormatter** | ❌ 已废弃 | ❌ 安全漏洞 | - | ❌ |

## API白名单 ⚠️ 强制遵守

### ✅ 推荐使用的API

| API | 用途 | 性能等级 |
|-----|------|---------|
| `PlayerPrefs.SetInt/Get` | 存取整数配置 | ⭐⭐⭐ 高性能 |
| `PlayerPrefs.SetString/Get` | 存取字符串配置 | ⭐⭐⭐ 高性能 |
| `JsonUtility.ToJson()` | 序列化对象 | ⭐⭐⭐ 高性能 |
| `JsonUtility.FromJson<T>()` | 反序列化JSON | ⭐⭐⭐ 高性能 |
| `File.WriteAllText()` | 写入文件 | ⭐⭐ 中等性能 |
| `File.ReadAllText()` | 读取文件 | ⭐⭐ 中等性能 |
| `Application.persistentDataPath` | 存档路径 | ⭐⭐⭐ 高性能 |
| `Aes.Create()` | AES加密 | ⭐⭐ 中等性能 |

### ❌ 禁止使用的API

| API | 禁用原因 | 替代方案 |
|-----|---------|----------|
| `BinaryFormatter` | 安全漏洞，微软已废弃 | JsonUtility + 加密 |

---

## 功能边界 ⚠️ 强制说明

### 本Skill涵盖范围

- ✅ PlayerPrefs简单存储
- ✅ JsonUtility序列化
- ✅ 文件读写操作
- ✅ AES加密存储
- ✅ 多存档槽位
- ✅ 自动存档机制
- ✅ 版本迁移

### 不在本Skill范围内

- ❌ 云存档同步 → 需要后端服务
- ❌ SQLite数据库 → 不涉及，使用JSON
- ❌ 服务器验证 → 不涉及

### 性能限制

| 指标 | 建议值 | 说明 |
|------|--------|------|
| 单次存档大小 | ≤ 10MB | 避免卡顿 |
| 存档槽位数 | ≤ 10 | 控制文件数量 |
| 自动存档间隔 | ≥ 60s | 避免频繁IO |

---

## 渐进式学习路径

### 阶段1：基础使用

```csharp
// PlayerPrefs
PlayerPrefs.SetString("PlayerName", "Hero");
PlayerPrefs.SetInt("Level", 10);
PlayerPrefs.Save();

// JsonUtility
[Serializable]
public class PlayerData
{
    public string name;
    public int level;
    public List<string> items;
}

PlayerData data = new PlayerData { name = "Hero", level = 10 };
string json = JsonUtility.ToJson(data);
File.WriteAllText(savePath, json);
```

### 阶段2：完整存档系统

```csharp
public class SaveSystem : MonoBehaviour
{
    private string savePath;
    
    void Awake()
    {
        savePath = Path.Combine(Application.persistentDataPath, "Saves");
        Directory.CreateDirectory(savePath);
    }
    
    public void Save(int slot, GameData data)
    {
        string path = Path.Combine(savePath, $"slot_{slot}.json");
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);
    }
    
    public GameData Load(int slot)
    {
        string path = Path.Combine(savePath, $"slot_{slot}.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<GameData>(json);
        }
        return null;
    }
}
```

---

## References

- [API参考文档](references/api-reference.md)
