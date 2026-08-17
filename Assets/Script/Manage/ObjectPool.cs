using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using AsoulChess.Game.Core.Pooling;
using AsoulChess.Game.Core.Services;

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool instance;
    public Scene poolScene;

    IGameObjectPool CorePool
    {
        get
        {
            if (GameServices.Pool == null)
                GameServices.RegisterDefaults();
            return GameServices.Pool;
        }
    }

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        if (GameServices.Pool == null)
            GameServices.RegisterDefaults();

        EventController.Instance.AddListener(EventName.GameStart.ToString(), ClearPool);
    }

    void SyncForceDestroy()
    {
        CorePool.ForceDestroyOnRelease = GameManage.instance != null && GameManage.instance.IsDestroy;
    }

    public GameObject Create(GameObject prefab)
    {
        SyncForceDestroy();
        return CorePool.Get(prefab);
    }

    public void Recycle(GameObject go)
    {
        SyncForceDestroy();
        CorePool.Release(go);
    }

    public object CreateObject(Type type) => Activator.CreateInstance(type);

    public void ReycleObject(object obj) { }

    public void ClearPool()
    {
        Debug.Log("ObjectPool清理完成");
        CorePool.Clear(true);
    }
}
