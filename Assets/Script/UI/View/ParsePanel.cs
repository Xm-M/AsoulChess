using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 运行中的暂停页面 / 主菜单设置面板
/// 游戏内：Space/Esc 暂停，显示重新开始/主菜单；主菜单：仅设置，无暂停，隐藏两小按钮，大按钮为「设置完成」
/// </summary>
public class ParsePanel : View
{
    public GameObject menuPanel;
    public GameObject pauseButton;
    [Header("加速开关（可选，Toggle 勾选=2x，未勾选=1x）")]
    public Toggle speedToggle;
    public GameObject continuLevelPanel;
    public GameObject confirmPanel;
    [Header("主菜单模式需隐藏：重新开始、主菜单按钮的父物体（或分别指定）")]
    public GameObject restartButton;
    public GameObject returnMenuButton;
    [Header("大按钮（返回游戏/设置完成）")]
    public Button confirmCloseButton;
    public string confirmTextReturnGame = "返回游戏";
    public string confirmTextSettingsDone = "设置完成";
    [Header("分辨率（手机模式隐藏）")]
    public GameObject resolutionDropdownRoot;
    public TMP_Dropdown resolutionDropdown;
    [Header("全屏开关（手机模式隐藏）")]
    public GameObject fullscreenToggleRoot;
    public Toggle fullscreenToggle;
    static readonly (int w, int h)[] ResolutionPresets = {
        (0, 0),         // 默认
        (1280, 720),    // 720p
        (1366, 768),    // 常见笔记本
        (1600, 900),
        (1920, 1080),   // 1080p
        (2560, 1440),  // 2K
        (3840, 2160),  // 4K
    };
    static readonly string[] ResolutionLabels = { "默认", "1280×720", "1366×768", "1600×900", "1920×1080", "2560×1440", "3840×2160" };

    [Header("金币显示：捡起银币时显示，5秒后隐藏")]
    public GameObject coinDisplayObject;
    [Header("本体图片")]
    public Image Mask;
    public TMP_Text coinText;
    public Slider BGM,AudioEffect;
    public AudioPlayer au;
    Coroutine coinDisplayHideCoroutine;
    bool pause;
    bool isFromMainMenu;
    public override void Init()
    {
        EventController.Instance.AddListener(EventName.WhenLeaveLevel.ToString(), Hide);
        EventController.Instance.AddListener(EventName.SelectState.ToString(), Show);
        au= GetComponent<AudioPlayer>();
        BGM.value = AudioManage.BgmValue;
        AudioEffect.value = AudioManage.SoundEffectValue;
        InitResolutionDropdown();
        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
        if (speedToggle != null)
            speedToggle.onValueChanged.AddListener(OnSpeedToggleChanged);
    }

    void InitResolutionDropdown()
    {
        if (resolutionDropdown == null) return;
        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(new List<string>(ResolutionLabels));
        resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
    }
    public override void Show()
    {
        base.Show();
        isFromMainMenu = false;
        au.audioSource.enabled = true;
        if (BGM != null) BGM.value = AudioManage.BgmValue;
        if (AudioEffect != null) AudioEffect.value = AudioManage.SoundEffectValue;
        ApplyConfirmButtonText(confirmTextReturnGame);
        if (restartButton != null) restartButton.SetActive(true);
        if (returnMenuButton != null) returnMenuButton.SetActive(true);
        if (pauseButton != null) pauseButton.SetActive(true);
        if (speedToggle != null) speedToggle.gameObject.SetActive(true);
        RefreshSpeedToggleState();
        RefreshPcOnlySettingsVisibility();
        SyncResolutionDropdownFromSave();
        SyncFullscreenToggleFromSave();
        UIManage.Show<DamagePanel>();//这一句要放在之后的开关里面使用 跟其他设置一样
    }

    /// <summary>从主菜单打开设置面板（不暂停，隐藏重新开始/主菜单，大按钮为「设置完成」）</summary>
    public void ShowAsMainMenuSettings()
    {
        isFromMainMenu = true;
        base.Show();
        au.audioSource.enabled = true;
        if (BGM != null) BGM.value = AudioManage.BgmValue;
        if (AudioEffect != null) AudioEffect.value = AudioManage.SoundEffectValue;
        ApplyConfirmButtonText(confirmTextSettingsDone);
        if (restartButton != null) restartButton.SetActive(false);
        if (returnMenuButton != null) returnMenuButton.SetActive(false);
        if (pauseButton != null) pauseButton.SetActive(false);
        if (speedToggle != null) speedToggle.gameObject.SetActive(false);
        RefreshPcOnlySettingsVisibility();
        SyncResolutionDropdownFromSave();
        SyncFullscreenToggleFromSave();
        if (menuPanel != null) menuPanel.SetActive(true);
        Mask.maskable = true;
        Mask.raycastTarget = true;
    }

    void ApplyConfirmButtonText(string text)
    {
        if (confirmCloseButton != null)
        {
            var t = confirmCloseButton.GetComponentInChildren<TMP_Text>();
            if (t != null) t.text = text;
        }
    }
    public override void Hide()
    {
        base.Hide();
        gameObject.SetActive(false);
        Mask.maskable = false;
        Mask.raycastTarget = false;
        if (coinDisplayHideCoroutine != null)
        {
            StopCoroutine(coinDisplayHideCoroutine);
            coinDisplayHideCoroutine = null;
        }
        if (coinDisplayObject != null)
            coinDisplayObject.SetActive(false);
        if (isFromMainMenu)
        {
            isFromMainMenu = false;
            if (menuPanel != null) menuPanel.SetActive(false);
        }
        else
        {
            CloseMenuPanel();
            LoadGameContinue();
        }
    }
    private void Update()
    {
        if (isFromMainMenu)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                CloseMenuPanel();
            return;
        }
        if (!pause)
        {
            if (Input.GetKeyDown(KeyCode.Space)|| Input.GetKeyDown(KeyCode.Escape))
                ShowMenuPanel();
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Space)|| Input.GetKeyDown(KeyCode.Escape))
                CloseMenuPanel();
        }
    }
    public void ShowMenuPanel()
    {
        if (!menuPanel.activeSelf)
        {
            pause = true;
            menuPanel.SetActive(true);
            if (!isFromMainMenu && GameManage.instance != null && GameManage.instance.timerManage != null)
            {
                GameManage.instance.timerManage.ChangeTimeSpeed(0);
                EventController.Instance.TriggerEvent(EventName.PauseGame.ToString());
            }
            if (pauseButton != null) pauseButton.gameObject.SetActive(false);
            if (speedToggle != null) speedToggle.gameObject.SetActive(false);
        }
    }
    public void ShowContinuePanel()
    {
        pause = true;
        continuLevelPanel.SetActive(true);
        GameManage.instance.timerManage.ChangeTimeSpeed(0);
        EventController.Instance.TriggerEvent(EventName.PauseGame.ToString());
        pauseButton.gameObject.SetActive(false);
        if (speedToggle != null) speedToggle.gameObject.SetActive(false);
    }
    public void LoadGameContinue()
    {
        pause = false;
        if (continuLevelPanel != null) continuLevelPanel.SetActive(false);
        if (!isFromMainMenu && GameManage.instance != null && GameManage.instance.timerManage != null)
        {
            float resumeSpeed = GameManage.instance.timerManage.GetResumeSpeed();
            GameManage.instance.timerManage.ChangeTimeSpeed(resumeSpeed);
            EventController.Instance.TriggerEvent(EventName.ResumeGame.ToString());
        }
        if (pauseButton != null) pauseButton.gameObject.SetActive(true);
        if (speedToggle != null) speedToggle.gameObject.SetActive(true);
        RefreshSpeedToggleState();
    }
    /// <summary>
    /// 可能有按钮控制
    /// </summary>
    public void CloseMenuPanel()
    {
        if (menuPanel != null && menuPanel.activeSelf)
        {
            pause = false;
            menuPanel.SetActive(false);
            if (!isFromMainMenu && GameManage.instance != null && GameManage.instance.timerManage != null)
            {
                float resumeSpeed = GameManage.instance.timerManage.GetResumeSpeed();
                GameManage.instance.timerManage.ChangeTimeSpeed(resumeSpeed);
                EventController.Instance.TriggerEvent(EventName.ResumeGame.ToString());
            }
            if (pauseButton != null) pauseButton.gameObject.SetActive(true);
            if (speedToggle != null) speedToggle.gameObject.SetActive(true);
            RefreshSpeedToggleState();
            if (confirmPanel != null) confirmPanel.SetActive(false);
            if (isFromMainMenu)
                Hide();
        }
    }
    /// <summary>加速 Toggle 变化：勾选=2x，未勾选=1x。仅在游戏运行且未暂停时生效。</summary>
    void OnSpeedToggleChanged(bool isOn)
    {
        if (GameManage.instance?.timerManage == null) return;
        if (LevelManage.instance == null || !LevelManage.instance.IfGameStart  )
        {
            RefreshSpeedToggleState(); // 选卡/暂停时点击无效，还原 Toggle 状态
            return;
        }
        GameManage.instance.timerManage.ChangeTimeSpeed(isOn ? 2f : 1f);
    }

    /// <summary>根据当前倍速同步 Toggle 状态（恢复时调用，避免触发 onValueChanged）</summary>
    void RefreshSpeedToggleState()
    {
        if (speedToggle == null || GameManage.instance?.timerManage == null) return;
        float s = GameManage.instance.timerManage.GetResumeSpeed();
        speedToggle.SetIsOnWithoutNotify(s > 1f);
    }

    /// <summary>
    /// 重新开始本关卡
    /// </summary>
    public void RestartGame()
    {
        Hide();
        LevelManage.instance.RestartLevel();
    }
    /// <summary>
    /// 返回主菜单
    /// </summary>
    public void ReturnMenu()
    {
        Hide();
        LevelManage.instance.ReturnMenu();
    }

    /// <summary>捡起银币时调用：显示金币数，5秒后隐藏</summary>
    public void ShowCoinDisplay(int totalCoins)
    {
        if (coinDisplayObject == null || coinText == null) return;
        if (coinDisplayHideCoroutine != null)
            StopCoroutine(coinDisplayHideCoroutine);
        coinDisplayObject.SetActive(true);
        coinText.text = totalCoins.ToString();
        coinDisplayHideCoroutine = StartCoroutine(HideCoinDisplayAfter(5f));
    }

    IEnumerator HideCoinDisplayAfter(float seconds)
    {
        yield return new WaitForSecondsRealtime(seconds);
        if (coinDisplayObject != null)
            coinDisplayObject.SetActive(false);
        coinDisplayHideCoroutine = null;
    }

    public void ChangeSoundEffectValue(float value)
    {
        AudioManage.ChangeSoundEffect(value);
        if (PlayerSaveContext.CurrentData != null)
        {
            PlayerSaveContext.CurrentData.sfxVolume = value;
            PlayerSaveContext.SaveCurrent();
        }
    }
    public void ChangeBgmValue(float value)
    {
        AudioManage.ChangeBGMValue(value);
        if (PlayerSaveContext.CurrentData != null)
        {
            PlayerSaveContext.CurrentData.bgmVolume = value;
            PlayerSaveContext.SaveCurrent();
        }
    }

    void RefreshPcOnlySettingsVisibility()
    {
        bool isPhone = GameManage.instance != null && GameManage.instance.mode == GameMode.Phone;
        if (resolutionDropdownRoot != null)
            resolutionDropdownRoot.SetActive(!isPhone);
        if (fullscreenToggleRoot != null)
            fullscreenToggleRoot.SetActive(!isPhone);
    }

    void SyncResolutionDropdownFromSave()
    {
        if (resolutionDropdown == null) return;
        var data = PlayerSaveContext.CurrentData;
        int idx = 0;
        if (data != null && data.screenWidth > 0 && data.screenHeight > 0)
        {
            for (int i = 1; i < ResolutionPresets.Length; i++)
            {
                if (ResolutionPresets[i].w == data.screenWidth && ResolutionPresets[i].h == data.screenHeight)
                {
                    idx = i;
                    break;
                }
            }
        }
        resolutionDropdown.SetValueWithoutNotify(idx);
    }

    void SyncFullscreenToggleFromSave()
    {
        if (fullscreenToggle == null) return;
        var data = PlayerSaveContext.CurrentData;
        fullscreenToggle.SetIsOnWithoutNotify(data?.fullscreen ?? true);
    }

    public void OnFullscreenChanged(bool value)
    {
        var data = PlayerSaveContext.CurrentData;
        if (data != null)
        {
            data.fullscreen = value;
            PlayerSaveContext.SaveCurrent();
        }
        if (data != null && data.screenWidth > 0 && data.screenHeight > 0)
            Screen.SetResolution(data.screenWidth, data.screenHeight, value);
        else
            Screen.fullScreen = value;
    }

    public void OnResolutionChanged(int index)
    {
        if (index < 0 || index >= ResolutionPresets.Length) return;
        var (w, h) = ResolutionPresets[index];
        var data = PlayerSaveContext.CurrentData;
        bool fullscreen = data?.fullscreen ?? Screen.fullScreen;
        if (data != null)
        {
            data.screenWidth = w;
            data.screenHeight = h;
            data.fullscreen = fullscreen;
            PlayerSaveContext.SaveCurrent();
        }
        if (w > 0 && h > 0)
            Screen.SetResolution(w, h, fullscreen);
        else
            Screen.fullScreen = fullscreen;
    }
}
