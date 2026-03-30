using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 关卡文字/波次提示面板。内含右上角「提示面板」：Image + TMP，按队列逐条显示，到时关闭。
/// </summary>
public class TextPanel : View
{
    public Animator animator;
    public AudioPlayer manage;
    public GameObject zombieWave;
    public GameObject lastWave;
    public GameObject gameOver;

    [Header("提示面板（队列）")]
    [SerializeField] GameObject tipPanelRoot;
    [SerializeField] Image tipIconImage;
    [SerializeField] TMP_Text tipMessageText;

    const float MinTipDuration = 0.1f;

    readonly Queue<TipMessage> tipQueue = new Queue<TipMessage>();
    Coroutine tipRoutine;
    bool tipRoutineRunning;

    struct TipMessage
    {
        public Sprite Icon;
        public string Text;
        public float Duration;
    }

    public override void Init()
    {
        EventController.Instance.AddListener(EventName.WhenLeaveLevel.ToString(), Hide);
        if (tipPanelRoot != null)
            tipPanelRoot.SetActive(false);
    }

    public override void Show()
    {
        base.Show();
        TryStartTipRoutine();
    }

    public override void Hide()
    {
        ClearTipQueue();
        base.Hide();
    }

    /// <summary>仅文字，不显示图标。</summary>
    public void EnqueueTip(string message, float durationSeconds)
    {
        EnqueueTip(null, message, durationSeconds);
    }

    /// <summary>图标 + 文字，加入队列按顺序显示，不会打断当前一条。</summary>
    public void EnqueueTip(Sprite icon, string message, float durationSeconds)
    {
        float d = Mathf.Max(MinTipDuration, durationSeconds);
        tipQueue.Enqueue(new TipMessage
        {
            Icon = icon,
            Text = message ?? string.Empty,
            Duration = d
        });
        TryStartTipRoutine();
    }

    /// <summary>清空队列并立即隐藏提示条（不隐藏整个 TextPanel）。</summary>
    public void ClearTipQueue()
    {
        tipQueue.Clear();
        if (tipRoutine != null)
        {
            StopCoroutine(tipRoutine);
            tipRoutine = null;
        }
        tipRoutineRunning = false;
        if (tipPanelRoot != null)
            tipPanelRoot.SetActive(false);
    }

    void TryStartTipRoutine()
    {
        if (tipRoutineRunning || tipQueue.Count == 0)
            return;
        if (!gameObject.activeInHierarchy)
            return;
        tipRoutine = StartCoroutine(RunTipQueue());
    }

    IEnumerator RunTipQueue()
    {
        tipRoutineRunning = true;
        while (tipQueue.Count > 0)
        {
            var item = tipQueue.Dequeue();
            ApplyTip(item);
            yield return new WaitForSeconds(item.Duration);
            if (tipPanelRoot != null)
                tipPanelRoot.SetActive(false);
        }
        tipRoutineRunning = false;
        tipRoutine = null;
        // 在最后一条显示期间若又入队，需再次启动
        TryStartTipRoutine();
    }

    void ApplyTip(TipMessage item)
    {
        if (tipPanelRoot == null)
            return;
        if (tipIconImage != null)
        {
            tipIconImage.sprite = item.Icon;
            tipIconImage.enabled = item.Icon != null;
        }
        if (tipMessageText != null)
            tipMessageText.text = item.Text;
        tipPanelRoot.SetActive(true);
    }

    public void FirstZombieCom()
    {
        manage.PlayAudio("firstZombie");
    }

    public void ZombieWave()
    {
        zombieWave.SetActive(true);
    }

    public void LastWave()
    {
        lastWave.SetActive(true);
    }

    public void GameOver()
    {
        animator.Play("gameover");
        MapManage.instance.BGMPlayer.PlayAudio("游戏失败");
        MapManage.instance.BGMPlayer.SetLoop(false);
        gameOver.SetActive(true);
    }

    public void GameStart()
    {
        manage.PlayAudio("准备种植");
        animator.Play("gamestart");
    }

    public void RestartGame()
    {
        LevelManage.instance.RestartLevel();
    }
}
