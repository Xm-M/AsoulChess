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
    public LevelController currentController;
    public LevelData currentLevel;
    public LevelData menu;
    public bool IfGameStart {  get;  set; }
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
        LeaveState();
        GameManage.instance.sceneManage.LoadScene(currentLevel.sceneName);
    }
    public void RestartLevel()
    {
        GameManage.instance.sceneManage.LoadScene(currentLevel.sceneName, null, () => {  LeaveState(); });
    }
    public void ReturnMenu()
    {
        //GameOver();
        //LeaveState();
        GameManage.instance.sceneManage.LoadScene("开始",()=>
        UIManage.GetView<StartUI>().Show(), () => { LeaveState(); });
    }
    public void PrepareLevel()
    {
        //currentLevel.PrepareStage();
        EventController.Instance.TriggerEvent(EventName.SelectState.ToString());
    }
  
    public void GameStart()
    {
        IfGameStart = true;
        //((MapManage_PVZ.instance) as MapManage_PVZ).au.SetLoop(true);
        EventController.Instance.TriggerEvent(EventName.GameStart.ToString());
    }
    /// <summary>
    /// 这个gameover是专门给游戏结束使用的 就是失败的是通用的
    /// </summary>
    public void GameOver(bool win)
    {
        IfGameStart=false;
        currentController.GameOver(win);
        EventController.Instance.TriggerEvent(EventName.GameOver.ToString());
    }
    public void GamePause()
    {
        IfGameStart = false;
    }
    public void GameContinue()
    {
        IfGameStart = true;
    }
    public void LeaveState()
    {
        Debug.Log("LeaveLevel");
        IfGameStart = false;
        //currentLevel.LeaveStage();
        EventController.Instance.TriggerEvent(EventName.WhenLeaveLevel.ToString());
    }
    public void SetController(LevelController levelController)
    {
        this.currentController = levelController;
        levelController.levelData = this.currentLevel;
    }


}
