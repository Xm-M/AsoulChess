# Save System API参考

## PlayerPrefs 类

### 存储方法

```csharp
// 存储整数
PlayerPrefs.SetInt(string key, int value);

// 存储浮点数
PlayerPrefs.SetFloat(string key, float value);

// 存储字符串
PlayerPrefs.SetString(string key, string value);

// 立即写入磁盘
PlayerPrefs.Save();
```

### 读取方法

```csharp
// 读取整数（带默认值）
int value = PlayerPrefs.GetInt(string key, int defaultValue);

// 读取浮点数
float value = PlayerPrefs.GetFloat(string key, float defaultValue);

// 读取字符串
string value = PlayerPrefs.GetString(string key, string defaultValue);
```

### 管理方法

```csharp
// 检查键是否存在
bool exists = PlayerPrefs.HasKey(string key);

// 删除指定键
PlayerPrefs.DeleteKey(string key);

// 删除所有数据
PlayerPrefs.DeleteAll();
```

---

## JsonUtility 类

### 序列化方法

```csharp
// 序列化为JSON
public static string ToJson(object obj);
public static string ToJson(object obj, bool prettyPrint);  // 格式化输出

// 示例
[Serializable]
public class PlayerData
{
    public string name;
    public int level;
}

PlayerData data = new PlayerData { name = "Hero", level = 10 };
string json = JsonUtility.ToJson(data, true);
```

### 反序列化方法

```csharp
// 反序列化为对象
public static T FromJson<T>(string json);

// 覆盖现有对象
public static void FromJsonOverwrite(string json, object obj);

// 示例
PlayerData data = JsonUtility.FromJson<PlayerData>(json);
```

---

## File 类

### 文件操作

```csharp
using System.IO;

// 写入文件
File.WriteAllText(string path, string contents);
File.WriteAllBytes(string path, byte[] bytes);

// 读取文件
string text = File.ReadAllText(string path);
byte[] bytes = File.ReadAllBytes(string path);

// 检查文件存在
bool exists = File.Exists(string path);

// 删除文件
File.Delete(string path);

// 创建目录
Directory.CreateDirectory(string path);
```

---

## Path 类

### 路径操作

```csharp
using System.IO;

// 合并路径
string fullPath = Path.Combine(folder, filename);

// 获取文件名
string fileName = Path.GetFileName(path);

// 获取目录名
string dirName = Path.GetDirectoryName(path);

// 获取扩展名
string ext = Path.GetExtension(path);
```

---

## Application 路径

```csharp
// 持久化数据路径（推荐用于存档）
string path = Application.persistentDataPath;

// 流资源路径
string path = Application.streamingAssetsPath;

// 数据路径
string path = Application.dataPath;
```
