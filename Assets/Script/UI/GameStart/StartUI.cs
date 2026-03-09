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
     
    /// <summary>
    /// Init的时候读取Resource里的所有关卡信息
    /// </summary>
    public override void Init()
    {
        Debug.Log("initstartUI");
        var miniGame = Resources.LoadAll<LevelData>("LevelData/MiniMode");
        foreach(var data in miniGame)
        {
            Debug.Log(data.levelName);
            GameObject card= Instantiate(levelPrefab, miniParent);
            card.GetComponent<UI_LevelDataMesCard>().InitCard(data);
        }
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
}
