using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPanel : View
{

    public static List<UIItem> itemPre;
    public AudioPlayer player;
    Dictionary<Type, Stack<UIItem>> itemPool;
    public override void Init()
    {
        itemPre = new List<UIItem>();
        itemPool = new Dictionary<Type, Stack<UIItem>>();
        var prefabs = Resources.LoadAll<Transform>("Item");//加载所有PowerBar;
        foreach (Transform view in prefabs)
        {
            //Transform prefab = Instantiate(view, transform);
            itemPre.Add(view.GetComponent<UIItem>());
        }
        EventController.Instance.AddListener(EventName.WhenLeaveLevel.ToString(), Hide);
        EventController.Instance.AddListener(EventName.GameStart.ToString(), Show);
    }
    public override void Hide()
    {
        base.Hide();
        foreach(var stack in itemPool) {
            while (stack.Value.Count > 0)
            {
                Destroy(stack.Value.Pop().gameObject);
            }
        }
        itemPool.Clear();
    }
    public T Create<T>() where T : UIItem
    {
        Type t=typeof(T);   
        if (itemPool.ContainsKey(t) && itemPool[t].Count>0)
        {
            T target = itemPool[t].Pop() as T;
            target.gameObject.SetActive(true);
            return target;
        }else if (!itemPool.ContainsKey(t))
        {
            itemPool.Add(t, new Stack<UIItem>());
        }
        GameObject pre=null;
        for(int i = 0; i < itemPre.Count; i++)
        {
            if (itemPre[i].GetType() == t)
            {
                pre = itemPre[i].gameObject;
                break;
            }
        }
        if (!pre) Debug.LogError("没有这个Item");
        GameObject item= GameObject.Instantiate(pre, transform);
        return item.GetComponent<T>();
    }
    public void Recycle<T>(UIItem item) where T : UIItem
    {
        if (itemPool.ContainsKey(typeof(T)))
        {
            if (item.transform.parent != transform)
            {
                item.transform.SetParent(transform);
                //Debug.Log("回收并重新设置父节点");
            }
            item.gameObject.SetActive(false);
            itemPool[typeof(T)].Push(item);
        }
    }
}
