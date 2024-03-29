using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 那么roommanage是用来干什么的呢
/// </summary>
public class LevelManage: MonoBehaviour
{
    public static LevelManage instance;
    public LevelData currentLevel;
    public bool IfGameStart { get; private set; }
    private void Awake()
    {
        if(instance== null)
        {
            instance = this;
        }
        else
        {
           Destroy(gameObject);
        }
    }
    public void ChangeLevel(LevelData levelData)
    {
        currentLevel = levelData;
        GameManage.instance.sceneManage.LoadScene(currentLevel.sceneName);
    }
    public void PrepareLevel()
    {
        currentLevel.PrepareStage();
        EventController.Instance.TriggerEvent(EventName.SelectState.ToString());
    }
    public void GameStart()
    {
        IfGameStart = true;
        currentLevel.StartGameStage();
        EventController.Instance.TriggerEvent(EventName.GameStart.ToString());
    }
    public void GameOver()
    {
        IfGameStart = false;
        currentLevel.OverGameStage();
        EventController.Instance.TriggerEvent(EventName.GameOver.ToString());
    }
    public void LevelState()
    {
        IfGameStart = false;
        currentLevel.LeaveStage();
        EventController.Instance.TriggerEvent(EventName.WhenLeaveLevel.ToString());
    }
}
