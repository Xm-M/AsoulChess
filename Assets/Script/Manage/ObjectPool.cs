using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
public class ObjectPool : MonoBehaviour
{
    public static ObjectPool instance;
    public Scene poolScene;
    Dictionary<string, Stack<GameObject>> objectPool;
    Dictionary<Type, Stack<object>> otherPool;
    private void Awake()
    {
        if (instance == null)
            instance = this;
        objectPool = new Dictionary<string, Stack<GameObject>>();
        poolScene = SceneManager.CreateScene(name);
        otherPool = new Dictionary<Type, Stack<object>>();
    }
    //
    IEnumerator AddMember(Chess c, Tile tile)
    {
        yield return null;
        tile.ChessEnter(c);

    }
    public GameObject Create(GameObject a)
    {
        GameObject creat;
        if (objectPool.ContainsKey(a.name))
        {
            if (objectPool[a.name].Count != 0)
            {
                creat= objectPool[a.name].Pop();
                //Debug.Log(objectPool[a.name].Count);
                creat.SetActive(true);
                return creat;
            }          
        }
        else
        {
            objectPool.Add(a.name, new Stack<GameObject>());
        }        
        creat = Instantiate(a);
        SceneManager.MoveGameObjectToScene(creat, poolScene);
        return creat;
    }
    public void Recycle(GameObject a)
    {
        string name = a.name.Replace("(Clone)", "");
        if (!objectPool[name].Contains(a))
        {           
            objectPool[name].Push(a);
        }
        else
        {
            Debug.LogWarning("没有这个物体" + a.name);
        }
        a.SetActive(false);
    }
    public object CreateObject(Type type)
    {
        //Type type = obj.GetType();
        if (otherPool.ContainsKey(type))
        {
            if (otherPool[type].Count != 0)
                return otherPool[type].Pop();
            else return Activator.CreateInstance(type);
        }
        else
        {
            otherPool.Add(type, new Stack<object>());
            return Activator.CreateInstance(type);
        }
    }
    public void ReycleObject(object obj)
    {
        Type type = obj.GetType();
        if (otherPool.ContainsKey(type))
        {
            otherPool[type].Push(obj);
        }
    }
}
