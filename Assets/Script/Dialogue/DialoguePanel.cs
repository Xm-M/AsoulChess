using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 对话面板：显示头像、名字、文本，点击推进，支持 eventOnEnter/Exit 和 waitForEvent。
/// </summary>
public class DialoguePanel : View
{
    [Header("UI 引用")]
    public Image avatarImage;
    public TMP_Text speakerNameText;
    public TMP_Text dialogueText;
    public Button clickArea;

    DialogueData currentData;
    int index;
    bool waiting;
    string waitingEvent;
    string resumeEvent;
    Action onComplete;
    UnityEngine.Events.UnityAction onWaitEvent;
    UnityEngine.Events.UnityAction onResumeEvent;

    public override void Init()
    {
        if (clickArea != null)
            clickArea.onClick.AddListener(OnClick);
        EventController.Instance.AddListener(EventName.WhenLeaveLevel.ToString(), OnLeaveLevel);
    }

    void OnLeaveLevel()
    {
        Hide();
        Cleanup();
    }

    /// <summary>显示对话，结束后调用 onComplete</summary>
    public void ShowDialogue(DialogueData data, Action onCompleteCallback)
    {
        Cleanup();
        if (data == null || data.entries == null || data.entries.Count == 0)
        {
            onCompleteCallback?.Invoke();
            return;
        }
        currentData = data;
        index = 0;
        onComplete = onCompleteCallback;
        base.Show();
        DisplayEntry(0);
    }

    void DisplayEntry(int i)
    {
        waiting = false;
        UnlistenWaitEvent();

        var entry = currentData.entries[i];
        if (entry.speaker != null)
        {
            if (avatarImage != null)
            {
                avatarImage.gameObject.SetActive(true);
                avatarImage.sprite = entry.speaker.avatar;
            }
            if (speakerNameText != null)
                speakerNameText.text = entry.speaker.characterName ?? "";
        }
        else
        {
            if (avatarImage != null) avatarImage.gameObject.SetActive(false);
            if (speakerNameText != null) speakerNameText.text = "";
        }
        if (dialogueText != null)
            dialogueText.text = entry.text ?? "";

        if (!string.IsNullOrEmpty(entry.eventOnEnter))
            EventController.Instance.TriggerEvent(entry.eventOnEnter);

        if (!string.IsNullOrEmpty(entry.waitForEvent))
        {
            waiting = true;
            waitingEvent = entry.waitForEvent;
            onWaitEvent = OnWaitEventReceived;
            EventController.Instance.AddListener(waitingEvent, onWaitEvent);
        }
    }

    void OnWaitEventReceived()
    {
        if (!waiting || string.IsNullOrEmpty(waitingEvent)) return;
        UnlistenWaitEvent();
        waiting = false;
    }

    void OnResumeEventReceived()
    {
        if (currentData == null || string.IsNullOrEmpty(resumeEvent)) return;
        UnlistenResumeEvent();
        index++;
        if (index >= currentData.entries.Count)
        {
            Hide();
            Cleanup();
            onComplete?.Invoke();
        }
        else
        {
            base.Show();
            DisplayEntry(index);
        }
    }

    void UnlistenResumeEvent()
    {
        if (!string.IsNullOrEmpty(resumeEvent) && onResumeEvent != null)
        {
            EventController.Instance.RemoveListener(resumeEvent, onResumeEvent);
            resumeEvent = null;
            onResumeEvent = null;
        }
    }

    void UnlistenWaitEvent()
    {
        if (!string.IsNullOrEmpty(waitingEvent) && onWaitEvent != null)
        {
            EventController.Instance.RemoveListener(waitingEvent, onWaitEvent);
            waitingEvent = null;
            onWaitEvent = null;
        }
    }

    void OnClick()
    {
        if (currentData == null) return;
        if (waiting) return;

        var entry = currentData.entries[index];
        if (!string.IsNullOrEmpty(entry.eventOnExit))
            EventController.Instance.TriggerEvent(entry.eventOnExit);

        if (entry.hideOnClick && !string.IsNullOrEmpty(entry.resumeEvent))
        {
            UnlistenResumeEvent(); // 防止重复点击时重复注册监听，导致 resumeEvent 触发多次
            Hide();
            resumeEvent = entry.resumeEvent;
            onResumeEvent = OnResumeEventReceived;
            EventController.Instance.AddListener(resumeEvent, onResumeEvent);
            return;
        }

        index++;
        if (index >= currentData.entries.Count)
        {
            Hide();
            Cleanup();
            onComplete?.Invoke();
        }
        else
        {
            DisplayEntry(index);
        }
    }

    void Cleanup()
    {
        UnlistenWaitEvent();
        UnlistenResumeEvent();
        currentData = null;
        index = 0;
        waiting = false;
        waitingEvent = null;
        resumeEvent = null;
        onComplete = null;
    }

    void OnDestroy()
    {
        UnlistenWaitEvent();
        UnlistenResumeEvent();
        EventController.Instance.RemoveListener(EventName.WhenLeaveLevel.ToString(), OnLeaveLevel);
    }
}
