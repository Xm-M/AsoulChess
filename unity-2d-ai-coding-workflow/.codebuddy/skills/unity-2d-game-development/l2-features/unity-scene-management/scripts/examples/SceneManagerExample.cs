using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 完整的场景管理器示例
/// 支持异步加载、进度显示、场景数据传递
/// </summary>
public class SceneManagerExample : MonoBehaviour
{
    public static SceneManagerExample Instance { get; private set; }
    
    [Header("UI引用")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private Slider progressBar;
    [SerializeField] private Text progressText;
    
    [Header("设置")]
    [SerializeField] private float minimumLoadTime = 1f;
    
    // 场景数据传递
    public static class SceneData
    {
        public static int TargetLevel { get; set; }
        public static Vector3 SpawnPosition { get; set; }
    }
    
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
    
    /// <summary>
    /// 加载场景（带进度显示）
    /// </summary>
    public void LoadScene(string sceneName, Action onComplete = null)
    {
        StartCoroutine(LoadSceneCoroutine(sceneName, onComplete));
    }
    
    private IEnumerator LoadSceneCoroutine(string sceneName, Action onComplete)
    {
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(true);
        }
        
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;
        
        float startTime = Time.time;
        float displayProgress = 0f;
        
        while (!operation.isDone)
        {
            float targetProgress = operation.progress;
            
            if (operation.progress >= 0.9f)
            {
                targetProgress = 1f;
                
                if (Time.time - startTime >= minimumLoadTime)
                {
                    operation.allowSceneActivation = true;
                }
            }
            
            // 平滑进度显示
            displayProgress = Mathf.Lerp(displayProgress, targetProgress, Time.deltaTime * 5f);
            
            if (progressBar != null)
            {
                progressBar.value = displayProgress;
            }
            
            if (progressText != null)
            {
                progressText.text = $"加载中... {(int)(displayProgress * 100)}%";
            }
            
            yield return null;
        }
        
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(false);
        }
        
        onComplete?.Invoke();
    }
    
    /// <summary>
    /// 叠加加载场景
    /// </summary>
    public void LoadSceneAdditive(string sceneName)
    {
        StartCoroutine(LoadAdditiveCoroutine(sceneName));
    }
    
    private IEnumerator LoadAdditiveCoroutine(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        yield return operation;
        
        Scene loadedScene = SceneManager.GetSceneByName(sceneName);
        if (loadedScene.IsValid())
        {
            SceneManager.SetActiveScene(loadedScene);
            Debug.Log($"叠加场景已加载: {sceneName}");
        }
    }
    
    /// <summary>
    /// 卸载场景
    /// </summary>
    public void UnloadScene(string sceneName)
    {
        StartCoroutine(UnloadSceneCoroutine(sceneName));
    }
    
    private IEnumerator UnloadSceneCoroutine(string sceneName)
    {
        AsyncOperation operation = SceneManager.UnloadSceneAsync(sceneName);
        yield return operation;
        
        // 清理资源
        yield return Resources.UnloadUnusedAssets();
        
        Debug.Log($"场景已卸载: {sceneName}");
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[SceneManager] 场景加载完成: {scene.name}, 模式: {mode}");
    }
    
    private void OnSceneUnloaded(Scene scene)
    {
        Debug.Log($"[SceneManager] 场景卸载完成: {scene.name}");
    }
}

/// <summary>
/// 使用示例
/// </summary>
public class SceneManagerUsage : MonoBehaviour
{
    private void Start()
    {
        // 示例1：加载场景
        SceneManagerExample.Instance.LoadScene("GameLevel");
        
        // 示例2：设置场景数据后加载
        SceneManagerExample.SceneData.TargetLevel = 5;
        SceneManagerExample.SceneData.SpawnPosition = new Vector3(10, 0, 10);
        SceneManagerExample.Instance.LoadScene("GameLevel");
        
        // 示例3：叠加加载UI场景
        SceneManagerExample.Instance.LoadSceneAdditive("UI_Scene");
        
        // 示例4：卸载场景
        SceneManagerExample.Instance.UnloadScene("UI_Scene");
    }
}
