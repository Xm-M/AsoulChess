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
        if (saveButton != null)
            saveButton.interactable = GameManage.instance == null || GameManage.instance.mode != GameMode.Test;
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

    /// <summary>刷新存档按钮显示的用户名</summary>
    public void RefreshSaveButtonText()
    {
        if (saveButton == null) return;
        var text = saveButton.GetComponentInChildren<TMP_Text>();
        if (text != null)
            text.text = PlayerSaveContext.CurrentData?.username ?? "创建存档";
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
        LoadScene(startLevelData);
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
