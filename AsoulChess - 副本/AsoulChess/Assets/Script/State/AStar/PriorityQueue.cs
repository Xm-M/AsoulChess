using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PriorityQueue<T>//是传进来的数据类型obok
{
    //首先我们知道这里的优先队列满足list[n*2]<list[n]的，我们要做的就是满足这一条件
    Dictionary<T,float> dic=new Dictionary<T, float>();
    List<T> list=new List<T>();
    public void Add(T node,float val)
    {
        dic.Add(node,val);
        list.Add(node);
        SifiUp();
    }
    public T Remove()
    {
        T node = list[0];
        list[0]=list[list.Count-1];
        list[list.Count - 1] = list[0];
        list.Remove(list[list.Count-1]);
        SifiDown();
        return node;
    }
    public void SifiUp()
    {
        int n = list.Count - 1;
        while (n > 1)
        {
            if (dic[list[n]] > dic[list[n/2]])
            {
                float val = dic[list[n / 2]];
                dic[list[n / 2]] = dic[list[n]];
                dic[list[n]] = val;
            }
            else return;
            n /= 2;
        }
    }
    public void SifiDown()
    {
        int n = 1;
        while (n < list.Count)
        {
            n *= 2;
            if(n+1<list.Count&&dic[list[n]]>dic[list[n+1]])n=n+1;
            if (dic[list[n]] < dic[list[n / 2]])
            {
                float val = dic[list[n / 2]];
                dic[list[n / 2]] = dic[list[n]];
                dic[list[n]] = val;
            }
            else return;
        }
    }
}
