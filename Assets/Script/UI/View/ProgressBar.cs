using PixelsoftGames.PixelUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressBar : View
{
    public UIStatBar uiBar;
    public List<GameObject> flags;
    public override void Init()
    {
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
        uiBar.SetValue(cur, max);
    }
    public override void Show()
    {
        base.Show();
    }
    public override void Hide()
    {
        base.Hide();
        for(int i = 0; i < flags.Count; i++)
        {
            flags[i].SetActive(false);
        }
    }
}
