using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// �����е���ͣҳ��
/// ��������ͣ,һ����space һ����esc
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
        //Debug.Log("����");
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
    /// �����а�ť����
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
    /// ���¿�ʼ���ؿ�
    /// </summary>
    public void RestartGame()
    {
        Hide();
        LevelManage.instance.RestartLevel();
    }
    /// <summary>
    /// �������˵�
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
