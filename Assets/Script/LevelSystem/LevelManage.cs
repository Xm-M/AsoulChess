using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 那么roommanage是用来干什么的呢
/// 也就是说所有的关卡进程都是在这里控制的 
/// 而不是在GameManage
/// 所以为什么这个脚本要是MoneBehavirour的
/// </summary>
public class LevelManage: MonoBehaviour
{
    public static LevelManage instance;
    public LevelData currentLevel;
    public LevelData menu;
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
    public void RestartLevel()
    {
        GameManage.instance.sceneManage.LoadScene(currentLevel.sceneName, null, () => { GameOver(); LeaveState(); });
    }
    public void ReturnMenu()
    {
        //GameOver();
        //LeaveState();
        GameManage.instance.sceneManage.LoadScene("开始",()=>
        UIManage.GetView<StartUI>().Show(), () => { GameOver();LeaveState(); });
        
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
        Debug.Log("GameOver");
        IfGameStart = false;
        currentLevel.OverGameStage();
        EventController.Instance.TriggerEvent(EventName.GameOver.ToString());
    }
    public void LeaveState()
    {
        Debug.Log("LeaveLevel");
        IfGameStart = false;
        currentLevel.LeaveStage();
        EventController.Instance.TriggerEvent(EventName.WhenLeaveLevel.ToString());
    }
}
