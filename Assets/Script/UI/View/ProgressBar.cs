using PixelsoftGames.PixelUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : View
{
    public UIStatBar uiBar;
    public Text stadgeName;//关卡名
    public List<GameObject> flags;
    [Tooltip("肉鸽战斗：mintime 过后显示并可点进下一波")]
    public Button earlyNextWaveButton;
    bool bossHpMode;
    bool earlyNextWaveHooked;

    public override void Init()
    {
        uiBar.SetValue(0, 1);
        EventController.Instance.AddListener(EventName.WhenLeaveLevel.ToString(),
            Hide);
        EnsureEarlyNextWaveHook();
        if (earlyNextWaveButton != null)
            earlyNextWaveButton.gameObject.SetActive(false);
    }
    public void SetFlag(int n)
    {
        for (int i = 0; i < n; i++)
        {
            flags[i].SetActive(true);
        }
    }
    public void MoveBar(float cur,float max)
    {
        bossHpMode = false;
        uiBar.SetValue(cur, max);
        RefreshStageName();
    }

    /// <summary>Boss 关：隐藏波次旗，进度条表示 Boss 剩余血量。</summary>
    public void ShowBossHp(float current, float max)
    {
        bossHpMode = true;
        ClearFlags();
        if (earlyNextWaveButton != null)
            earlyNextWaveButton.gameObject.SetActive(false);
        uiBar.SetValue(current, max);
    }

    public void UpdateBossHp(float current, float max)
    {
        if (!bossHpMode) return;
        uiBar.SetValue(current, max);
    }

    void ClearFlags()
    {
        if (flags == null) return;
        for (int i = 0; i < flags.Count; i++)
            flags[i].SetActive(false);
    }

    void EnsureEarlyNextWaveHook()
    {
        if (earlyNextWaveHooked || earlyNextWaveButton == null)
            return;
        earlyNextWaveHooked = true;
        earlyNextWaveButton.onClick.AddListener(OnEarlyNextWaveClicked);
    }

    void OnEarlyNextWaveClicked()
    {
        LevelManage.instance?.currentController?.TryManualAdvanceWave();
    }

    /// <summary>由 LevelController 每帧或进波后调用。</summary>
    public void RefreshEarlyNextWaveButton()
    {
        if (earlyNextWaveButton == null)
            return;
        if (bossHpMode)
        {
            earlyNextWaveButton.gameObject.SetActive(false);
            return;
        }
        var controller = LevelManage.instance?.currentController;
        if (controller == null || !controller.SupportsManualWaveAdvance())
        {
            earlyNextWaveButton.gameObject.SetActive(false);
            return;
        }
        bool show = controller.CanShowManualAdvanceWaveButton();
        earlyNextWaveButton.gameObject.SetActive(show);
        if (show)
            earlyNextWaveButton.interactable = true;
    }

    public override void Show()
    {
        base.Show();
        EnsureEarlyNextWaveHook();
        RefreshStageName();
        RefreshEarlyNextWaveButton();
    }

    void RefreshStageName()
    {
        if (stadgeName == null) return;
        var level = LevelManage.instance?.currentLevel;
        if (level == null)
        {
            stadgeName.text = string.Empty;
            return;
        }

        if (level.levelMode == LevelMode.SurvivalMode)
        {
            string name = level.levelName ?? string.Empty;
            if (LevelManage.instance?.currentController is LevelController_Endless endless)
            {
                int round = endless.GetRoundDisplay();
                int maxRounds = endless.GetMaxRoundsDisplay();
                stadgeName.text = maxRounds > 0
                    ? $"{name} · 第{round}/{maxRounds}轮"
                    : $"{name} · 第{round}轮";
            }
            else
            {
                stadgeName.text = name;
            }
            return;
        }

        stadgeName.text = RoguelikeRunInfoFormatter.FormatProgressBarStageName(level);
    }

    public override void Hide()
    {
        base.Hide();
        bossHpMode = false;
        ClearFlags();
        uiBar.SetValue(0, 1);
        stadgeName.text = "";
        if (earlyNextWaveButton != null)
            earlyNextWaveButton.gameObject.SetActive(false);
    }
}
