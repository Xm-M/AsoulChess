using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartUI : View
{
    //public GameObject starui;

    public override void Init()
    {
         
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
    public void LoadScene(RoomType roomType)
    {
        GameManage.instance.sceneManage.LoadScene(roomType);
        Hide();
    }
}
