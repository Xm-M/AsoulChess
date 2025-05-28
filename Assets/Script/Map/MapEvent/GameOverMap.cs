using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOverMap : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy") && LevelManage.instance.currentLevel.levelMode!=LevelMode.TestMode)
        {
            //GameManage.instance.
            LevelManage.instance.GameOver(false);
        }
        else if(collision.gameObject.GetComponent<Chess>()!=null)
        {

            collision.gameObject.GetComponent<Chess>().Death();
        }
    }
}
