using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 示例：基础UI创建和控制
/// 演示如何使用代码创建和配置常见UI组件
/// </summary>
public class ExampleUIBasic : MonoBehaviour
{
    [Header("Canvas引用")]
    [SerializeField] private Canvas canvas;
    
    [Header("UI组件引用")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Button startButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Toggle soundToggle;
    
    [Header("设置")]
    [SerializeField] private float defaultVolume = 0.7f;

    void Start()
    {
        // 初始化UI
        InitializeUI();
        
        // 订阅事件
        SubscribeEvents();
    }

    void InitializeUI()
    {
        // 设置背景图片
        if (backgroundImage != null)
        {
            backgroundImage.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);
            backgroundImage.raycastTarget = false; // 优化性能
        }
        
        // 设置标题文本
        if (titleText != null)
        {
            titleText.text = "游戏标题";
            titleText.fontSize = 48;
            titleText.alignment = TextAlignmentOptions.Center;
        }
        
        // 设置按钮文本
        if (startButton != null)
        {
            var buttonText = startButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = "开始游戏";
            }
        }
        
        // 设置滑动条初始值
        if (volumeSlider != null)
        {
            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 1f;
            volumeSlider.value = defaultVolume;
        }
        
        // 设置Toggle初始状态
        if (soundToggle != null)
        {
            soundToggle.isOn = true;
        }
    }

    void SubscribeEvents()
    {
        // ✅ 推荐：在OnEnable中订阅
        if (startButton != null)
            startButton.onClick.AddListener(OnStartButtonClicked);
        
        if (settingsButton != null)
            settingsButton.onClick.AddListener(OnSettingsButtonClicked);
        
        if (volumeSlider != null)
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        
        if (soundToggle != null)
            soundToggle.onValueChanged.AddListener(OnSoundToggleChanged);
    }

    void UnsubscribeEvents()
    {
        // ✅ 推荐：在OnDisable中取消订阅
        if (startButton != null)
            startButton.onClick.RemoveListener(OnStartButtonClicked);
        
        if (settingsButton != null)
            settingsButton.onClick.RemoveListener(OnSettingsButtonClicked);
        
        if (volumeSlider != null)
            volumeSlider.onValueChanged.RemoveListener(OnVolumeChanged);
        
        if (soundToggle != null)
            soundToggle.onValueChanged.RemoveListener(OnSoundToggleChanged);
    }

    void OnEnable()
    {
        SubscribeEvents();
    }

    void OnDisable()
    {
        UnsubscribeEvents();
    }

    // ========== 按钮事件处理 ==========

    void OnStartButtonClicked()
    {
        Debug.Log("开始游戏按钮被点击");
        // 加载游戏场景
        // UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
    }

    void OnSettingsButtonClicked()
    {
        Debug.Log("设置按钮被点击");
        // 打开设置面板
    }

    // ========== 滑动条事件处理 ==========

    void OnVolumeChanged(float value)
    {
        Debug.Log($"音量改变: {value:F2}");
        
        // 应用音量到AudioManager
        // AudioManager.Instance.SetVolume(value);
        
        // 音量转换为分贝（可选）
        float dB = value > 0 ? 20f * Mathf.Log10(value) : -80f;
        Debug.Log($"分贝值: {dB:F2} dB");
    }

    // ========== Toggle事件处理 ==========

    void OnSoundToggleChanged(bool isOn)
    {
        Debug.Log($"声音开关: {(isOn ? "开启" : "关闭")}");
        
        // 应用声音设置
        // AudioManager.Instance.SetSoundEnabled(isOn);
    }

    // ========== 动态创建UI示例 ==========

    /// <summary>
    /// 动态创建按钮
    /// </summary>
    public Button CreateButton(string buttonText, Vector2 position)
    {
        // 创建按钮GameObject
        GameObject buttonObj = new GameObject("DynamicButton");
        buttonObj.transform.SetParent(canvas.transform, false);
        
        // 添加RectTransform
        RectTransform rectTransform = buttonObj.AddComponent<RectTransform>();
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = new Vector2(160, 40);
        
        // 添加Image组件（按钮背景）
        Image image = buttonObj.AddComponent<Image>();
        image.color = Color.white;
        
        // 添加Button组件
        Button button = buttonObj.AddComponent<Button>();
        
        // 创建文本子对象
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = buttonText;
        text.fontSize = 18;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.black;
        
        return button;
    }

    /// <summary>
    /// 动态创建文本
    /// </summary>
    public TextMeshProUGUI CreateText(string content, Vector2 position, int fontSize = 24)
    {
        GameObject textObj = new GameObject("DynamicText");
        textObj.transform.SetParent(canvas.transform, false);
        
        RectTransform rectTransform = textObj.AddComponent<RectTransform>();
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = new Vector2(200, 50);
        
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = content;
        text.fontSize = fontSize;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.raycastTarget = false; // 优化性能
        
        return text;
    }

    /// <summary>
    /// 动态创建Image
    /// </summary>
    public Image CreateImage(Sprite sprite, Vector2 position, Vector2 size)
    {
        GameObject imageObj = new GameObject("DynamicImage");
        imageObj.transform.SetParent(canvas.transform, false);
        
        RectTransform rectTransform = imageObj.AddComponent<RectTransform>();
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = size;
        
        Image image = imageObj.AddComponent<Image>();
        image.sprite = sprite;
        image.raycastTarget = false; // 优化性能
        
        return image;
    }

    // ========== UI工具方法 ==========

    /// <summary>
    /// 设置UI元素颜色
    /// </summary>
    public void SetUIColor(Graphic uiElement, Color color)
    {
        if (uiElement != null)
        {
            uiElement.color = color;
        }
    }

    /// <summary>
    /// 设置UI元素透明度
    /// </summary>
    public void SetUIAlpha(Graphic uiElement, float alpha)
    {
        if (uiElement != null)
        {
            Color color = uiElement.color;
            color.a = Mathf.Clamp01(alpha);
            uiElement.color = color;
        }
    }

    /// <summary>
    /// 启用/禁用按钮
    /// </summary>
    public void SetButtonEnabled(Button button, bool enabled)
    {
        if (button != null)
        {
            button.interactable = enabled;
        }
    }

    /// <summary>
    /// 设置滑动条值（带动画）
    /// </summary>
    public System.Collections.IEnumerator AnimateSlider(Slider slider, float targetValue, float duration)
    {
        if (slider == null) yield break;
        
        float startValue = slider.value;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            slider.value = Mathf.Lerp(startValue, targetValue, elapsed / duration);
            yield return null;
        }
        
        slider.value = targetValue;
    }

    /// <summary>
    /// 打印UI层级信息（调试用）
    /// </summary>
    [ContextMenu("Print UI Hierarchy")]
    public void PrintUIHierarchy()
    {
        PrintChildren(canvas.transform, 0);
    }

    void PrintChildren(Transform parent, int level)
    {
        string indent = new string(' ', level * 2);
        
        foreach (Transform child in parent)
        {
            Debug.Log($"{indent}{child.name}");
            PrintChildren(child, level + 1);
        }
    }
}
