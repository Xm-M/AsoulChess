using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 这个的作用是在特定事件发生后调用特定对话事件的 事件插件
/// </summary>
public class DialogueEvent_GameEventTrigger : IDialogueEvent
{
    public EventName EventName;
    public string triggerName;
    public void Register(LevelController levelController)
    {
        EventController.Instance.AddListener(EventName.ToString(), TriggerEvent);
    }
    public void TriggerEvent()
    {
        EventController.Instance.TriggerEvent(triggerName);
    }
    public void Unregister()
    {
        EventController.Instance.RemoveListener(EventName.ToString(), TriggerEvent);
    }
}
/// <summary>
/// 这个的作用是在特定事件发生后调用特定对话事件的 事件插件 但是多了一个Chess参数
/// </summary>
public class DialogueEvent_ChessEventTrigger : IDialogueEvent
{
    public EventName EventName;
    public string triggerName;
    public void Register(LevelController levelController)
    {
        EventController.Instance.AddListener<Chess>(EventName.ToString(), TriggerEvent);
    }
    public void TriggerEvent(Chess chess)
    {
        EventController.Instance.TriggerEvent(triggerName);
    }
    public void Unregister()
    {
        EventController.Instance.RemoveListener<Chess>(EventName.ToString(), TriggerEvent);
    }
}
public class DialogueEvent_PauseDir : IDialogueEvent
{
    public void Register(LevelController levelController)
    {
        EventController.Instance.AddListener("Timeline暂停",PauseDir);
    }
    public void PauseDir()
    {
        (MapManage.instance as MapManage_PVZ).dir.Pause();
        Debug.Log("暂停Timeline");
    }

    public void Unregister()
    {
        EventController.Instance.RemoveListener("Timeline暂停", PauseDir);
    }
}
public class DialogueEvent_ContinueDir : IDialogueEvent
{
    public void Register(LevelController levelController)
    {
        EventController.Instance.AddListener("Timeline播放", PauseDir);
    }
    public void PauseDir()
    {
        (MapManage.instance as MapManage_PVZ).dir.Play();
        Debug.Log("继续播放Timeline");
    }

    public void Unregister()
    {
        EventController.Instance.RemoveListener("Timeline播放", PauseDir);
    }
}
public class DialogueEvent_PauseGame : IDialogueEvent
{
    public void Register(LevelController levelController)
    {
        EventController.Instance.AddListener("游戏暂停", PauseGame);
    }
    public void PauseGame()
    {
        LevelManage.instance.GamePause();
        Debug.Log("暂停游戏");
    }

    public void Unregister()
    {
        EventController.Instance.RemoveListener("游戏暂停", PauseGame);
    }
}
public class DialogueEvent_ContinueGame : IDialogueEvent
{
    public void Register(LevelController levelController)
    {
        EventController.Instance.AddListener("游戏继续", PauseGame);
    }
    public void PauseGame()
    {
        LevelManage.instance.GameContinue();
        Debug.Log("继续游戏");
    }

    public void Unregister()
    {
        EventController.Instance.RemoveListener("游戏继续", PauseGame);
    }
}
