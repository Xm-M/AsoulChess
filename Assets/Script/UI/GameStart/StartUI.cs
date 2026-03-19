using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;
public class StartUI : View
{
    public LevelData startLevelData;
    public List<GameObject> panels;
    public Transform miniParent, riddleParent, SurvivalParent;
    [LabelText("关卡预制体")]
    public GameObject levelPrefab;
    [Header("商店按钮，商店解锁前隐藏")]
    public GameObject shopButton;
    [Header("存档按钮：显示用户名，点击打开读档面板")]
    public Button saveButton;
    [Header("设置按钮：打开设置面板")]
    public Button settingsButton;
    [Header("退出按钮：退出游戏")]
    public Button exitButton;
    [Header("难度选择（首页选择，进入关卡前生效）")]
    public TMP_Dropdown difficultyDropdown;
    static readonly string[] DifficultyLabels = { "简单", "普通", "困难", "噩梦" };

    /// <summary>
    /// Init的时候读取Resource里的所有关卡信息
    /// </summary>
    public override void Init()
    {
        Debug.Log("initstartUI");
        if (saveButton != null)
            saveButton.onClick.AddListener(OnSaveButtonClick);
        if (settingsButton != null)
            settingsButton.onClick.AddListener(OnSettingsButtonClick);
        if (exitButton != null)
            exitButton.onClick.AddListener(OnExitButtonClick);
        InitDifficultyDropdown();
        RefreshShopButtonVisibility();
        var miniGame = Resources.LoadAll<LevelData>("LevelData/MiniMode");
        foreach(var data in miniGame)
        {
            Debug.Log(data.levelName);
            GameObject card= Instantiate(levelPrefab, miniParent);
            card.GetComponent<UI_LevelDataMesCard>().InitCard(data);
        }
    }
    public override void Show()
    {
        base.Show();
        RefreshSaveButtonText();
        RefreshShopButtonVisibility();
        SyncDifficultyDropdownFromSave();
        if (saveButton != null)
            saveButton.interactable = GameManage.instance == null || GameManage.instance.mode != GameMode.Test;
        if (exitButton != null)
            exitButton.interactable = GameManage.instance == null || GameManage.instance.mode != GameMode.Test;
    }

    void OnExitButtonClick()
    {
        if (GameManage.instance != null && GameManage.instance.mode == GameMode.Test)
            return;
        Quit();
    }

    void OnSaveButtonClick()
    {
        if (GameManage.instance != null && GameManage.instance.mode == GameMode.Test)
            return;
        UIManage.GetView<LoadSaveDataPanel>()?.Show();
    }

    void OnSettingsButtonClick()
    {
        var panel = UIManage.GetView<ParsePanel>();
        if (panel != null) panel.ShowAsMainMenuSettings();
    }

    void InitDifficultyDropdown()
    {
        if (difficultyDropdown == null) return;
        difficultyDropdown.ClearOptions();
        difficultyDropdown.AddOptions(new List<string>(DifficultyLabels));
        difficultyDropdown.onValueChanged.AddListener(OnDifficultyChanged);
    }

    void SyncDifficultyDropdownFromSave()
    {
        if (difficultyDropdown == null) return;
        var data = PlayerSaveContext.CurrentData;
        int lv = Mathf.Clamp(data?.difficultyLevel ?? 1, 0, 3);
        difficultyDropdown.SetValueWithoutNotify(lv);
        if (GameManage.instance != null && GameManage.instance.mode == GameMode.Test)
            difficultyDropdown.interactable = false;
        else
            difficultyDropdown.interactable = data != null;
    }

    void OnDifficultyChanged(int index)
    {
        if (index < 0 || index > 3) return;
        if (GameManage.instance != null && GameManage.instance.mode == GameMode.Test) return;
        var data = PlayerSaveContext.CurrentData;
        if (data != null)
        {
            data.difficultyLevel = index;
            PlayerSaveContext.SaveCurrent();
        }
    }

    /// <summary>刷新存档按钮显示的用户名</summary>
    public void RefreshSaveButtonText()
    {
        if (saveButton == null) return;
        var text = saveButton.GetComponentInChildren<TMP_Text>();
        if (text != null)
            text.text = PlayerSaveContext.CurrentData?.username ?? "创建存档";
    }

    /// <summary>供 LoadSaveDataPanel 等读档后调用，刷新难度显示</summary>
    public void RefreshDifficultyDropdown()
    {
        SyncDifficultyDropdownFromSave();
    }

    /// <summary>供 LoadSaveDataPanel 等读档后调用，刷新商店按钮显示状态</summary>
    public void RefreshShopButtonVisibility()
    {
        if (shopButton != null)
            shopButton.SetActive(IsShopUnlocked());
    }

    static bool IsShopUnlocked()
    {
        if (GameManage.instance != null && GameManage.instance.mode == GameMode.Test)
            return true;
        return PlayerSaveContext.CurrentData?.shopUnlocked == true;
    }

    public override void Hide()
    {
        base.Hide();
        foreach(var panel in panels)
        {
            panel.SetActive(false);
        }
    }
    public void LoadS()
    {
        gameObject.SetActive(false);
    }
    public void Quit()
    {
        GameManage.instance.QuitGame();
    }
    public void OpenPanel(int n)
    {
        panels[n].gameObject.SetActive(true);
    }
    public void ClosePanel(int n)
    {
        panels[n].gameObject.SetActive(false);
    }
    public void GameStart()
    {
        var level = GetNextUncompletedLevel(startLevelData);
        if (level != null) LoadScene(level);
    }

    /// <summary>从 start 沿 nextLevel 遍历，返回第一个未通关的关卡；全通关则返回最后一关</summary>
    static LevelData GetNextUncompletedLevel(LevelData start)
    {
        if (start == null) return null;
        var cur = start;
        LevelData last = start;
        while (cur != null)
        {
            if (!IsLevelCompleted(cur.levelName))
                return cur;
            last = cur;
            cur = cur.nextLevel;
        }
        return last;
    }

    static bool IsLevelCompleted(string levelName)
    {
        return PlayerSaveContext.CurrentData?.completedLevelIds?.Contains(levelName) ?? false;
    }
    public void LoadScene(LevelData roomType)
    {
        LevelManage.instance.ChangeLevel(roomType);
        Hide();
    }
    public void OpenShop()
    {
        UIManage.GetView<CoinShopPanel>().Show();
    }
}
