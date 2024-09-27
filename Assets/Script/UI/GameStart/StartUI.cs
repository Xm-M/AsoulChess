using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartUI : View
{
    //public GameObject starui;

    public override void Init()
    {
        Debug.Log("initstartUI");
    }

    public void LoadS()
    {
        gameObject.SetActive(false);
    }
    public void Quit()
    {
        GameManage.instance.QuitGame();
    }
    public void OpenPanel()
    {

    }
    public void LoadScene(LevelData roomType)
    {
        LevelManage.instance.ChangeLevel(roomType);
        Hide();
    }
}
