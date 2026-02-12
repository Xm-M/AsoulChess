using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStartPlugin_Smoke : ILevelPlugin
{
    public GameObject smoke;
    public float smokeDis = 5;
    GameObject sm;
    public void StadgeEffect(LevelController levelController)
    {
        //throw new System.NotImplementedException();
        sm=ObjectPool.instance.Create(smoke);
        sm.transform.position = Vector2.zero;
        sm.GetComponent<Effect_Smoke>().InitSmokes(smokeDis);
        //EventController.Instance.AddListener(EventName.WhenLeaveLevel.ToString(),OnGameOver);
    }
    public void OnGameOver()
    {
        ObjectPool.instance.Recycle(sm);
        //EventController.Instance.RemoveListener(EventName.WhenLeaveLevel.ToString(), OnGameOver);
    }
    public void OverPlugin(LevelController levelController)
    {
        OnGameOver();
    }
}
