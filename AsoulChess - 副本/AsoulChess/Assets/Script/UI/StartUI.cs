using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartUI : MonoBehaviour
{
    public GameObject starui;
    public void LoadS()
    {
        FindObjectOfType<SceneManage>().LoadScene();
        gameObject.SetActive(false);
    }
    public void Quit()
    {
        GameManage.instance.QuitGame();
    }
}
