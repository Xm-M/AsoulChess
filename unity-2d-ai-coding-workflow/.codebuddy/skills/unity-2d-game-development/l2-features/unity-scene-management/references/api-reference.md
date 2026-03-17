# Scene Management API参考

## SceneManager 类

### 静态方法

#### LoadScene
```csharp
public static void LoadScene(string sceneName);
public static void LoadScene(int sceneBuildIndex);
public static void LoadScene(string sceneName, LoadSceneMode mode);
public static void LoadScene(int sceneBuildIndex, LoadSceneMode mode);
```

**参数说明**：
- `sceneName`：场景名称或路径
- `sceneBuildIndex`：Build Settings中的场景索引
- `mode`：加载模式（Single/Additive）

**使用示例**：
```csharp
// 同步加载场景（Single模式，关闭其他场景）
SceneManager.LoadScene("Level1");

// 叠加加载场景
SceneManager.LoadScene("UI_Scene", LoadSceneMode.Additive);
```

**注意事项**：
- ⚠️ 同步加载会阻塞主线程
- 建议使用 `LoadSceneAsync` 替代

---

#### LoadSceneAsync
```csharp
public static AsyncOperation LoadSceneAsync(string sceneName);
public static AsyncOperation LoadSceneAsync(int sceneBuildIndex);
public static AsyncOperation LoadSceneAsync(string sceneName, LoadSceneMode mode);
public static AsyncOperation LoadSceneAsync(int sceneBuildIndex, LoadSceneMode mode);
```

**返回值**：`AsyncOperation` 异步操作对象

**使用示例**：
```csharp
IEnumerator LoadLevelAsync(string levelName)
{
    AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(levelName);
    asyncLoad.allowSceneActivation = false;
    
    while (asyncLoad.progress < 0.9f)
    {
        Debug.Log($"Loading progress: {asyncLoad.progress * 100}%");
        yield return null;
    }
    
    asyncLoad.allowSceneActivation = true;
}
```

---

#### UnloadSceneAsync
```csharp
public static AsyncOperation UnloadSceneAsync(string sceneName);
public static AsyncOperation UnloadSceneAsync(int sceneBuildIndex);
public static AsyncOperation UnloadSceneAsync(Scene scene);
```

**使用示例**：
```csharp
IEnumerator UnloadUnusedScene()
{
    AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync("TempScene");
    yield return asyncUnload;
    
    // 清理资源
    Resources.UnloadUnusedAssets();
}
```

---

#### GetActiveScene
```csharp
public static Scene GetActiveScene();
```

**返回值**：当前激活的场景对象

**使用示例**：
```csharp
Scene currentScene = SceneManager.GetActiveScene();
Debug.Log($"当前场景: {currentScene.name}");
Debug.Log($"场景索引: {currentScene.buildIndex}");
```

---

#### SetActiveScene
```csharp
public static bool SetActiveScene(Scene scene);
```

**返回值**：设置是否成功

**使用示例**：
```csharp
Scene newScene = SceneManager.GetSceneByName("Level1");
if (newScene.IsValid())
{
    SceneManager.SetActiveScene(newScene);
}
```

---

#### GetSceneByName
```csharp
public static Scene GetSceneByName(string sceneName);
```

**返回值**：匹配的场景对象（如果未加载则返回无效场景）

---

#### GetSceneAt
```csharp
public static Scene GetSceneAt(int index);
```

**参数**：场景在加载列表中的索引

---

### 静态属性

| 属性 | 类型 | 说明 |
|------|------|------|
| `sceneCount` | int | 当前已加载的场景总数 |
| `sceneCountInBuildSettings` | int | Build Settings中的场景数量 |

---

### 静态事件

#### sceneLoaded
```csharp
public static event Action<Scene, LoadSceneMode> sceneLoaded;
```

**使用示例**：
```csharp
void OnEnable()
{
    SceneManager.sceneLoaded += OnSceneLoaded;
}

void OnDisable()
{
    SceneManager.sceneLoaded -= OnSceneLoaded;
}

void OnSceneLoaded(Scene scene, LoadSceneMode mode)
{
    Debug.Log($"场景加载完成: {scene.name}, 模式: {mode}");
}
```

---

#### sceneUnloaded
```csharp
public static event Action<Scene> sceneUnloaded;
```

---

#### activeSceneChanged
```csharp
public static event Action<Scene, Scene> activeSceneChanged;
```

---

## Scene 结构体

### 属性

| 属性 | 类型 | 说明 |
|------|------|------|
| `name` | string | 场景名称 |
| `path` | string | 场景路径 |
| `buildIndex` | int | Build Settings索引 |
| `isLoaded` | bool | 是否已加载 |
| `isDirty` | bool | 是否被修改 |
| `rootCount` | int | 根级GameObject数量 |

### 方法

#### IsValid
```csharp
public bool IsValid();
```

#### GetRootGameObjects
```csharp
public GameObject[] GetRootGameObjects();
```

---

## AsyncOperation 类

### 属性

| 属性 | 类型 | 说明 |
|------|------|------|
| `progress` | float | 加载进度（0~0.9，激活后为1.0） |
| `isDone` | bool | 操作是否完成 |
| `allowSceneActivation` | bool | 是否允许场景激活 |

---

## LoadSceneMode 枚举

| 值 | 说明 |
|------|------|
| `Single` | 关闭所有当前场景，加载新场景 |
| `Additive` | 保留当前场景，叠加新场景 |

---

## 性能优化建议

### 同步 vs 异步加载

| 特性 | 同步加载 | 异步加载 |
|------|---------|---------|
| 主线程阻塞 | ✅ 完全阻塞 | ❌ 不阻塞 |
| 帧率影响 | 严重掉帧 | 影响较小 |
| 用户体验 | 差（画面冻结） | 好（可显示进度） |

### 推荐做法

1. **始终使用异步加载**
2. **使用allowSceneActivation控制激活时机**
3. **Additive模式下及时卸载不需要的场景**
4. **场景切换前释放未使用资源**
