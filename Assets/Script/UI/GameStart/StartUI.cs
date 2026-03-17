using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

    /// <summary>
    /// Init的时候读取Resource里的所有关卡信息
    /// </summary>
    public override void Init()
    {
        Debug.Log("initstartUI");
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
        RefreshShopButtonVisibility();
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
