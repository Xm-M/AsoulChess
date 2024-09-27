using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 运行中的暂停页面
/// 有两种暂停,一种是space 一种是esc
/// </summary>
public class ParsePanel : View
{
    public GameObject menuPanel;
    bool pause;
    public override void Init()
    {
        EventController.Instance.AddListener(EventName.WhenLeaveLevel.ToString(), Hide);
        EventController.Instance.AddListener(EventName.SelectState.ToString(), Show);
    }
    public override void Show()
    {
        base.Show();
        //Debug.Log("show");
        //GameManage.instance.timerManage.ChangeTimeSpeed(0);
    }
    public override void Hide()
    {
        base.Hide();
        gameObject.SetActive(false);
        //Debug.Log("隐藏");
        CloseMenuPanel();
        //GameManage.instance.timerManage.ChangeTimeSpeed(1);
    }
    private void Update()
    {
        if (!pause)
        {
            if (Input.GetKeyDown(KeyCode.Space)|| Input.GetKeyDown(KeyCode.Escape))
            {
                ShowMenuPanel();
            } 
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Space)|| Input.GetKeyDown(KeyCode.Escape))
            {
                CloseMenuPanel();
            }
             
        }
    }
    public void ShowMenuPanel()
    {
        if (!menuPanel.activeSelf)
        {
            pause = true;
            menuPanel.SetActive(true);
            GameManage.instance.timerManage.ChangeTimeSpeed(0);
            EventController.Instance.TriggerEvent(EventName.PauseGame.ToString());
        }
    }
    /// <summary>
    /// 可能有按钮控制
    /// </summary>
    public void CloseMenuPanel()
    {
        if (menuPanel.activeSelf)
        {
            pause = false;
            menuPanel.SetActive(false);
            GameManage.instance.timerManage.ChangeTimeSpeed(1);
            EventController.Instance.TriggerEvent(EventName.ResumeGame.ToString());
        }
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

    public void ChangeSoundEffectValue(float value)
    {
        AudioManage.ChangeSoundEffect(value);
    }
    public void ChangeBgmValue(float value)
    {
        AudioManage.ChangeBGMValue(value);
    }
}
