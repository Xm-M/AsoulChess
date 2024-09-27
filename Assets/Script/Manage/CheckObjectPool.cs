using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 这里的回收机制应该放在哪呢？
/// </summary>
/// <typeparam name="T"></typeparam>
public class CheckObjectPool<T>
{
    private Stack<T[]> pool;
    private int arraySize;

    public CheckObjectPool(int arraySize, int initialCapacity)
    {
        this.arraySize = arraySize;
        pool = new Stack<T[]>(initialCapacity);

        // 预先填充池
        for (int i = 0; i < initialCapacity; i++)
        {
            pool.Push(new T[arraySize]);
        }
    }

    public T[] Get()
    {
        return pool.Count > 0 ? pool.Pop() : new T[arraySize];
    }

    public void Release(T[] array)
    {
        if (array.Length == arraySize)
        {
            pool.Push(array);
        }
        // 可以考虑添加一些错误处理，如果数组大小不正确
    }
}
public class CheckObjectPoolManage {
    public static Dictionary<int,CheckObjectPool<RaycastHit2D>> raycastPoolDic;
    public static Dictionary<int, CheckObjectPool<Collider2D>> colPoolDic;
    public CheckObjectPoolManage()
    {
        raycastPoolDic = new Dictionary<int, CheckObjectPool<RaycastHit2D>>();
        colPoolDic = new Dictionary<int, CheckObjectPool<Collider2D>>();
    }
    public static RaycastHit2D[] GetHitArray(int size)
    {
        if (!raycastPoolDic.ContainsKey(size))
        {
            raycastPoolDic.Add(size, new CheckObjectPool<RaycastHit2D>(size, 1));
        }
        return raycastPoolDic[size].Get();         
    }
    public static Collider2D[] GetColArray(int size)
    {
        if (!colPoolDic.ContainsKey(size))
        {
            colPoolDic.Add(size, new CheckObjectPool<Collider2D>(size, 1));
        }
        return colPoolDic[size].Get();
    }
    public static void ReleaseArray(int size,RaycastHit2D[] array)
    {
        if (raycastPoolDic.ContainsKey(size))
        {
            raycastPoolDic[size].Release(array);
        }
    }
    public static void ReleaseColArray(int size, Collider2D[] array)
    {
        if(colPoolDic.ContainsKey(size))
            colPoolDic[size].Release(array);
    }
    public static void WhenStadgeClear()
    {
        raycastPoolDic.Clear();
        colPoolDic.Clear();
    }
     
}
public interface IManager
{
    public void InitManage();
    public void OnGameStart();
    public void OnGameOver();
}
