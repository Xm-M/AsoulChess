---
name: unity-scene-management
description: Unity场景管理系统，负责场景加载、卸载、切换以及场景状态管理。提供异步加载、加载进度、场景数据传递等核心功能。
---

# Unity Scene Management 场景管理

## 概述

Unity场景管理系统，负责场景加载、卸载、切换以及场景状态管理。提供异步加载、加载进度、场景数据传递等核心功能，是游戏关卡流转和场景切换的基础。

## API白名单 ⚠️ 强制遵守

### ✅ 推荐使用的API

| API | 用途 | 使用场景 | 性能等级 |
|-----|------|---------|---------|
| `SceneManager.LoadSceneAsync()` | 异步加载场景 | 所有非首次场景加载 | ⭐⭐⭐ 高性能 |
| `SceneManager.UnloadSceneAsync()` | 异步卸载场景 | 卸载不再需要的场景 | ⭐⭐⭐ 高性能 |
| `SceneManager.GetActiveScene()` | 获取当前激活场景 | 获取当前场景信息 | ⭐⭐⭐ 高性能 |
| `SceneManager.GetSceneByName()` | 按名称查找场景 | 场景查询操作 | ⭐⭐⭐ 高性能 |
| `SceneManager.SetActiveScene()` | 设置活动场景 | Additive模式下切换活动场景 | ⭐⭐⭐ 高性能 |
| `SceneManager.sceneLoaded` | 场景加载完成事件 | 监听场景加载 | ⭐⭐⭐ 高性能 |
| `SceneManager.sceneUnloaded` | 场景卸载完成事件 | 监听场景卸载 | ⭐⭐⭐ 高性能 |
| `AsyncOperation.progress` | 加载进度 | 进度条显示 | ⭐⭐⭐ 高性能 |
| `AsyncOperation.allowSceneActivation` | 允许场景激活 | 控制场景激活时机 | ⭐⭐⭐ 高性能 |
| `Scene.IsValid()` | 检查场景有效性 | 安全检查 | ⭐⭐⭐ 高性能 |
| `Scene.isLoaded` | 场景是否已加载 | 状态判断 | ⭐⭐⭐ 高性能 |

### ⚠️ 性能警告API

| API | 性能问题 | 替代方案 |
|-----|----------|----------|
| `LoadScene()` 同步 | 阻塞主线程，导致卡顿 | 使用 `LoadSceneAsync()` |
| `GetRootGameObjects()` | 大场景可能产生GC开销 | 缓存结果，避免频繁调用 |
| `MergeScenes()` | 可能触发大量对象重组 | 提前规划场景结构 |

### ❌ 禁止使用的API

| API | 禁用原因 | 替代方案 |
|-----|---------|----------|
| `Application.LoadLevel()` | 已废弃（Unity 5.3+） | `SceneManager.LoadScene()` |
| `Application.LoadLevelAsync()` | 已废弃 | `SceneManager.LoadSceneAsync()` |
| `Application.LoadLevelAdditive()` | 已废弃 | `LoadSceneMode.Additive` |
| `SceneManager.UnloadScene()` | 已废弃（同步卸载） | `UnloadSceneAsync()` |

---

## 功能边界 ⚠️ 强制说明

### 本Skill涵盖范围

- ✅ 场景加载/卸载核心API（LoadScene、LoadSceneAsync、UnloadSceneAsync）
- ✅ 场景状态查询（GetActiveScene、GetSceneByName、sceneCount）
- ✅ 异步加载进度管理
- ✅ 多场景叠加加载（Additive模式）
- ✅ 场景切换过渡效果
- ✅ 场景数据传递

### 不在本Skill范围内

- ❌ Addressables资源系统 → L3级 unity-addressables Skill
- ❌ AssetBundle场景加载 → 不涉及，使用Addressables
- ❌ 网络场景同步 → 本项目不涉及
- ❌ 场景流式加载（Streaming）→ 不涉及
- ❌ 编辑器场景操作 → Editor脚本

### 跨Skill功能依赖

**完整场景管理系统需要**：
- unity-scene-management（场景管理）← 当前Skill
- unity-singleton-manager（SceneManager单例）
- unity-ui-panel（加载界面UI）
- unity-audio-system（场景切换音效）

**场景持久化需要**：
- unity-scene-management（场景切换）← 当前Skill
- unity-save-system（存档系统）
- unity-scriptableobject-config（场景配置）

### 性能限制

| 指标 | 建议值 | 说明 |
|------|--------|------|
| 同时加载场景数 | ≤ 3 | 避免内存压力 |
| DontDestroyOnLoad对象 | ≤ 10 | 统一管理 |
| 异步加载进度更新频率 | 每帧 | 协程或Task |
| 场景切换最小间隔 | ≥ 0.5s | 避免误操作 |

---

## 渐进式学习路径

### 阶段1：基础使用

**同步加载场景**

```csharp
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
    
    public void LoadLevel(int levelIndex)
    {
        SceneManager.LoadScene(levelIndex);
    }
}
```

**获取当前场景信息**

```csharp
void PrintSceneInfo()
{
    Scene currentScene = SceneManager.GetActiveScene();
    Debug.Log($"当前场景: {currentScene.name}");
    Debug.Log($"场景索引: {currentScene.buildIndex}");
    Debug.Log($"已加载场景数: {SceneManager.sceneCount}");
}
```

### 阶段2：常用功能

**异步加载场景**

```csharp
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private Slider loadingBar;
    [SerializeField] private Text loadingText;
    
    public void LoadSceneAsync(string sceneName)
    {
        StartCoroutine(LoadSceneCoroutine(sceneName));
    }
    
    private IEnumerator LoadSceneCoroutine(string sceneName)
    {
        AsyncOperation asyncOp = SceneManager.LoadSceneAsync(sceneName);
        asyncOp.allowSceneActivation = false;
        
        while (asyncOp.progress < 0.9f)
        {
            loadingBar.value = asyncOp.progress;
            loadingText.text = $"加载中... {(int)(asyncOp.progress * 100)}%";
            yield return null;
        }
        
        loadingBar.value = 1f;
        loadingText.text = "加载完成！";
        
        asyncOp.allowSceneActivation = true;
    }
}
```

**叠加加载场景（Additive）**

```csharp
// 叠加加载UI场景到游戏场景之上
public void LoadUISceneAdditive()
{
    SceneManager.LoadSceneAsync("UI_Scene", LoadSceneMode.Additive);
}

// 卸载叠加场景
public void UnloadUIScene()
{
    SceneManager.UnloadSceneAsync("UI_Scene");
}
```

### 阶段3：进阶功能

**场景管理器单例**

```csharp
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneMgr : MonoBehaviour
{
    public static SceneMgr Instance { get; private set; }
    
    [Header("加载设置")]
    [SerializeField] private float minimumLoadTime = 1f;
    [SerializeField] private string loadingSceneName = "LoadingScene";
    
    private Action onSceneLoaded;
    
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
    
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }
    
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }
    
    public void LoadScene(string sceneName, Action onComplete = null)
    {
        onSceneLoaded = onComplete;
        StartCoroutine(LoadSceneRoutine(sceneName));
    }
    
    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        AsyncOperation loadTargetScene = SceneManager.LoadSceneAsync(sceneName);
        loadTargetScene.allowSceneActivation = false;
        
        float startTime = Time.time;
        
        while (!loadTargetScene.isDone)
        {
            if (loadTargetScene.progress >= 0.9f)
            {
                if (Time.time - startTime >= minimumLoadTime)
                {
                    loadTargetScene.allowSceneActivation = true;
                }
            }
            yield return null;
        }
        
        onSceneLoaded?.Invoke();
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"场景已加载: {scene.name}");
    }
    
    private void OnSceneUnloaded(Scene scene)
    {
        Debug.Log($"场景已卸载: {scene.name}");
    }
}
```

**场景数据传递**

```csharp
// 场景数据容器
public static class SceneData
{
    public static int CurrentLevel { get; set; }
    public static Vector3 PlayerPosition { get; set; }
    public static int PlayerHealth { get; set; }
}

// 使用
public class GameLevelLoader : MonoBehaviour
{
    public void LoadLevel(int levelIndex)
    {
        SceneData.CurrentLevel = levelIndex;
        SceneData.PlayerHealth = 100;
        SceneManager.LoadScene("GameLevel");
    }
}

// 目标场景中读取
public class LevelManager : MonoBehaviour
{
    private void Start()
    {
        Debug.Log($"进入关卡: {SceneData.CurrentLevel}");
    }
}
```

### 阶段4：最佳实践

**场景加载最佳实践**

```csharp
// ✅ 推荐：使用异步加载 + 进度显示
public async Task LoadSceneWithProgress(string sceneName, IProgress<float> progress)
{
    var asyncOp = SceneManager.LoadSceneAsync(sceneName);
    
    while (!asyncOp.isDone)
    {
        progress?.Report(asyncOp.progress);
        await Task.Yield();
    }
}

// ❌ 避免：同步加载大场景（会卡顿）
SceneManager.LoadScene("HugeLevel"); // 不推荐

// ✅ 推荐：使用BuildSettings索引而非名称
SceneManager.LoadScene(1); // 使用buildIndex

// ✅ 推荐：场景切换前释放资源
async void SwitchScene(string nextScene)
{
    await Resources.UnloadUnusedAssets().AsTask();
    GC.Collect();
    await LoadSceneWithProgress(nextScene, null);
}
```

---

## 相关Skills

- **unity-singleton-manager** - 创建SceneManager单例
- **unity-ui-panel** - 加载界面UI面板
- **unity-coroutine-system** - 协程加载
- **unity-save-system** - 场景进度存档
- **unity-audio-system** - 场景切换音效

---

## References

- [API参考文档](references/api-reference.md)
- [基础教程](references/basics.md)
