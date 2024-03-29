using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPanel : View
{

    public static List<UIItem> itemPre;
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
    }
    public UIItem Create<T>() where T : UIItem
    {
        Type t=typeof(T);   
        if (itemPool.ContainsKey(t) && itemPool[t].Count>0)
        {
            return itemPool[t].Pop();
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
        return item.GetComponent<UIItem>();
    }
    public void Recycle<T>(UIItem item) where T : UIItem
    {
        if (itemPool.ContainsKey(typeof(T)))
        {
            item.gameObject.SetActive(false);
            itemPool[typeof(T)].Push(item);
        }
    }
}
