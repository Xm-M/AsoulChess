using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 对话面板：显示头像、名字、文本，点击推进，支持 eventOnEnter/Exit 和 waitForEvent。
/// 文本逐字显示，点击可立即显示完整文本，再次点击进入下一条。
/// </summary>
public class DialoguePanel : View
{
    [Header("UI 引用")]
    public Image avatarImage;
    public TMP_Text speakerNameText;
    public TMP_Text dialogueText;
    public Button clickArea;
    [Header("逐字显示")]
    [Tooltip("每秒显示字符数，0 则一次性显示")]
    public float charsPerSecond = 40f;

    DialogueData currentData;
    int index;
    bool waiting;
    string waitingEvent;
    string resumeEvent;
    Action onComplete;
    UnityEngine.Events.UnityAction onWaitEvent;
    UnityEngine.Events.UnityAction onResumeEvent;
    Coroutine typewriterCoroutine;
    bool isTypewriterComplete;
    string currentFullText;

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
        currentFullText = entry.text ?? "";
        if (dialogueText != null)
        {
            if (charsPerSecond <= 0 || string.IsNullOrEmpty(currentFullText))
            {
                dialogueText.text = currentFullText;
                isTypewriterComplete = true;
            }
            else
            {
                dialogueText.text = "";
                isTypewriterComplete = false;
                if (typewriterCoroutine != null) StopCoroutine(typewriterCoroutine);
                typewriterCoroutine = StartCoroutine(TypewriterEffect());
            }
        }
        else
        {
            isTypewriterComplete = true;
        }

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

    IEnumerator TypewriterEffect()
    {
        float delay = 1f / charsPerSecond;
        for (int i = 0; i < currentFullText.Length; i++)
        {
            dialogueText.text = currentFullText.Substring(0, i + 1);
            yield return new WaitForSecondsRealtime(delay);
        }
        dialogueText.text = currentFullText;
        isTypewriterComplete = true;
        typewriterCoroutine = null;
    }

    void SkipTypewriter()
    {
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
        }
        if (dialogueText != null && !string.IsNullOrEmpty(currentFullText))
            dialogueText.text = currentFullText;
        isTypewriterComplete = true;
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

        // 若正在逐字显示，点击则立即显示完整文本（含 waiting 时也可跳过）
        if (!isTypewriterComplete)
        {
            SkipTypewriter();
            return;
        }
        if (waiting) return; // waiting 时不能进入下一条

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
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
        }
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
