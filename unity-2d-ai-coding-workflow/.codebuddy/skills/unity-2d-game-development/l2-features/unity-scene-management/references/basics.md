# 场景管理基础教程

## 1. 场景基础概念

### 什么是场景？

场景（Scene）是Unity中组织游戏内容的基本单位，包含：
- GameObject（游戏对象）
- Component（组件）
- 资源引用

### 场景加载模式

#### Single模式
- 关闭所有当前场景
- 加载新场景作为唯一场景
- 适用于：关卡切换、返回主菜单

#### Additive模式
- 保留当前场景
- 叠加加载新场景
- 适用于：UI层、区域流式加载

---

## 2. 添加场景到构建

### 步骤

1. 打开 Build Settings（File → Build Settings）
2. 将场景拖入 "Scenes In Build" 列表
3. 记录场景的 Build Index

### 代码获取

```csharp
// 获取Build Settings中的场景数量
int totalScenes = SceneManager.sceneCountInBuildSettings;
```

---

## 3. 基础场景加载

### 同步加载（不推荐）

```csharp
using UnityEngine.SceneManagement;

public class SimpleLoader : MonoBehaviour
{
    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
    
    public void LoadByIndex(int index)
    {
        SceneManager.LoadScene(index);
    }
}
```

### 异步加载（推荐）

```csharp
using System.Collections;
using UnityEngine.SceneManagement;

public class AsyncLoader : MonoBehaviour
{
    public void LoadSceneAsync(string sceneName)
    {
        StartCoroutine(LoadSceneRoutine(sceneName));
    }
    
    IEnumerator LoadSceneRoutine(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        
        while (!operation.isDone)
        {
            Debug.Log($"进度: {operation.progress * 100}%");
            yield return null;
        }
    }
}
```

---

## 4. 加载进度UI

### 完整示例

```csharp
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    [Header("UI引用")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private Slider progressBar;
    [SerializeField] private Text progressText;
    
    [Header("设置")]
    [SerializeField] private float minimumLoadTime = 1f;
    
    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneCoroutine(sceneName));
    }
    
    IEnumerator LoadSceneCoroutine(string sceneName)
    {
        loadingPanel.SetActive(true);
        
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;
        
        float startTime = Time.time;
        float displayProgress = 0f;
        
        while (!operation.isDone)
        {
            // 计算实际进度
            float targetProgress = operation.progress;
            
            // 如果加载完成（progress达到0.9）
            if (operation.progress >= 0.9f)
            {
                targetProgress = 1f;
                
                // 确保最小加载时间
                if (Time.time - startTime >= minimumLoadTime)
                {
                    operation.allowSceneActivation = true;
                }
            }
            
            // 平滑显示进度
            displayProgress = Mathf.Lerp(displayProgress, targetProgress, Time.deltaTime * 5f);
            progressBar.value = displayProgress;
            progressText.text = $"加载中... {(int)(displayProgress * 100)}%";
            
            yield return null;
        }
        
        loadingPanel.SetActive(false);
    }
}
```

---

## 5. 场景事件监听

### 注册事件

```csharp
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneEventHandler : MonoBehaviour
{
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }
    
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
    }
    
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"场景加载完成: {scene.name}");
        
        // 场景加载后的初始化
        if (scene.name == "GameLevel")
        {
            InitializeGameLevel();
        }
    }
    
    void OnSceneUnloaded(Scene scene)
    {
        Debug.Log($"场景卸载完成: {scene.name}");
    }
    
    void OnActiveSceneChanged(Scene oldScene, Scene newScene)
    {
        Debug.Log($"活动场景变更: {oldScene.name} -> {newScene.name}");
    }
    
    void InitializeGameLevel()
    {
        // 初始化游戏关卡
    }
}
```

---

## 6. 叠加场景管理

### 加载叠加场景

```csharp
public class AdditiveSceneManager : MonoBehaviour
{
    public void LoadUIScene()
    {
        StartCoroutine(LoadAdditiveScene("UI_Scene"));
    }
    
    IEnumerator LoadAdditiveScene(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        yield return operation;
        
        // 设置为活动场景
        Scene loadedScene = SceneManager.GetSceneByName(sceneName);
        if (loadedScene.IsValid())
        {
            SceneManager.SetActiveScene(loadedScene);
        }
    }
    
    public void UnloadUIScene()
    {
        SceneManager.UnloadSceneAsync("UI_Scene");
    }
}
```

---

## 7. 场景数据传递

### 静态数据传递

```csharp
// 场景数据类
public static class SceneTransitionData
{
    public static int TargetLevel { get; set; }
    public static Vector3 SpawnPosition { get; set; }
    public static bool FromSave { get; set; }
}

// 发送端
public class LevelSelect : MonoBehaviour
{
    public void SelectLevel(int level)
    {
        SceneTransitionData.TargetLevel = level;
        SceneTransitionData.SpawnPosition = Vector3.zero;
        SceneManager.LoadScene("GameLevel");
    }
}

// 接收端
public class LevelInitializer : MonoBehaviour
{
    void Start()
    {
        int level = SceneTransitionData.TargetLevel;
        Vector3 spawnPos = SceneTransitionData.SpawnPosition;
        
        // 初始化关卡
        InitializeLevel(level, spawnPos);
    }
}
```

---

## 8. 常见问题解决

### 场景加载卡顿

**原因**：
- 使用同步加载
- 场景资源过大
- Awake/Start初始化开销

**解决方案**：
```csharp
// 分帧初始化
IEnumerator InitializeObjects()
{
    for (int i = 0; i < objects.Count; i++)
    {
        InitializeObject(objects[i]);
        
        if (i % 10 == 0)
        {
            yield return null; // 每10个对象等待一帧
        }
    }
}
```

### 进度条卡在90%

**原因**：`allowSceneActivation = false` 时，进度停在0.9

**解决方案**：
```csharp
while (!operation.isDone)
{
    // 映射进度到0-100%
    float realProgress = Mathf.Clamp01(operation.progress / 0.9f);
    progressBar.value = realProgress;
    
    if (operation.progress >= 0.9f)
    {
        // 加载完成，可以激活
        operation.allowSceneActivation = true;
    }
    
    yield return null;
}
```
