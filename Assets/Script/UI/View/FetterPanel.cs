using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 那么问题来了 Fetter系统要不要放在这里面呢 我的倾向是不要 
/// </summary>
public class FetterPanel : View
{
    public GameObject fetterIcon;//这个Icon是用来显示羁绊效果的
    public RectTransform fetterIconParent;//放icon的地方
    List<GameObject> icons;
    public override void Init()
    {
        icons = new List<GameObject>();
    }
    public override void Show()
    {
        base.Show();
    }
    public void ShowFetter(Fetter fetter)
    {
        GameObject icon= Instantiate(fetterIcon, fetterIconParent);
        icon.GetComponent<FetterIcon>().ShowFetterIcon(fetter);
        icons.Add(icon);
    }
    public override void Hide()
    {
        base.Hide();
        for(int i = icons.Count - 1; i >= 0; i--)
        {
            Destroy(icons[i]);
        }
        icons.Clear();
    }
}
