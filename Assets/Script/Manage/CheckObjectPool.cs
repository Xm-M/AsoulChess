using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ����Ļ��ջ���Ӧ�÷������أ�
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

        // Ԥ������
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
        // ���Կ������һЩ��������������С����ȷ
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
