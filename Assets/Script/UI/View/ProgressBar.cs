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
    bool bossHpMode;

    public override void Init()
    {
        uiBar.SetValue(0, 1);
        EventController.Instance.AddListener(EventName.WhenLeaveLevel.ToString(),
            Hide);
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
    }

    /// <summary>Boss 关：隐藏波次旗，进度条表示 Boss 剩余血量。</summary>
    public void ShowBossHp(float current, float max)
    {
        bossHpMode = true;
        ClearFlags();
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
    public override void Show()
    {
        base.Show();
        stadgeName.text = LevelManage.instance.currentLevel.levelName;
    }

    public override void Hide()
    {
        base.Hide();
        bossHpMode = false;
        ClearFlags();
        uiBar.SetValue(0, 1);
        stadgeName.text = "";
    }
}
