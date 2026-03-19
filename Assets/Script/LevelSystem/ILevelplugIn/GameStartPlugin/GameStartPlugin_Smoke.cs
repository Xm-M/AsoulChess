using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStartPlugin_Smoke : ILevelPlugin
{
    public GameObject smoke;
    [Tooltip("隐藏 0~n 列的雾气，n 可配置。如 4 表示前 5 列（0~4）无雾")]
    public int hideColumns = 4;
    [Tooltip("隐藏持续时间，足够长则整局不显示")]
    public float hideTime = 99999f;
    GameObject sm;

    public void StadgeEffect(LevelController levelController)
    {
        if (smoke == null) return;
        sm = ObjectPool.instance.Create(smoke);
        sm.transform.position = Vector2.zero;
        var effect = sm.GetComponent<Effect_Smoke>();
        effect.InitSmokes();
        effect.HideSmokeInColumns(hideColumns, hideTime);
    }
    public void OnGameOver()
    {
        if (sm != null)
        {
            ObjectPool.instance.Recycle(sm);
            sm = null;
        }
        //EventController.Instance.RemoveListener(EventName.WhenLeaveLevel.ToString(), OnGameOver);
    }
    public void OverPlugin(LevelController levelController)
    {
        OnGameOver();
    }
}
