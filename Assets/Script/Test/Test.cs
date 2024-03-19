using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class Test : MonoBehaviour
{
    public PropertyCreator creator;
    public LayerMask mask;
    public Collider2D[] c;
    private void Start()
    {
        c = new Collider2D[1000];
    }
    public void Create()
    {
        Chess c = GameManage.instance.enemyManage.CreateChess(creator, MapManage.instance.tiles[0, 0]);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < 100; i++)
            {
                Create();
            }
            stopwatch.Stop();
            UnityEngine.Debug.Log(stopwatch.ElapsedMilliseconds + "ms");
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < GameManage.instance.enemyManage.chesses.Count; i++)
            {
                float dis = Vector2.Distance
                    (Vector2.zero, GameManage.instance.enemyManage.chesses[i].transform.position);
            }
            stopwatch.Stop();
            UnityEngine.Debug.Log(GameManage.instance.enemyManage.chesses.Count);
            UnityEngine.Debug.Log(stopwatch.ElapsedMilliseconds+"ms");
        }
        else if(Input.GetKeyDown(KeyCode.D))
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            
            for (int i = 0; i < GameManage.instance.enemyManage.chesses.Count; i++)
            {
                RaycastHit2D hit = Physics2D.Raycast(
                    GameManage.instance.enemyManage.chesses[i].transform.position,
                    Vector2.right,1,mask
                    );
                //float dis = hit.distance;
            }
            stopwatch.Stop();
            UnityEngine.Debug.Log(GameManage.instance.enemyManage.chesses.Count);
            UnityEngine.Debug.Log(stopwatch.ElapsedMilliseconds + "ms");
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            DamageMessege damage = new DamageMessege();
            damage.damage = 1000;
            List<Chess> chesss = GameManage.instance.enemyManage.chesses;
            for (int i = 0; i < chesss.Count; i++)
            {
                float dis = Vector2.Distance
                    (Vector2.zero,chesss[i].transform.position);
                if (dis <= 1)
                {
                    chesss[i].propertyController.GetDamage(damage);
                }
            }
            stopwatch.Stop();
            UnityEngine.Debug.Log(GameManage.instance.enemyManage.chesses.Count);
            UnityEngine.Debug.Log(stopwatch.ElapsedMilliseconds + "ms");
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            DamageMessege damage = new DamageMessege();
            damage.damage = 1000;
            int num= Physics2D.OverlapCircleNonAlloc(Vector2.zero, 10, c,mask);
            for(int i=0;i<num; i++)
            {
                c[i].GetComponent<Chess>()?.propertyController.GetDamage(damage);
            }
            
            stopwatch.Stop();
            UnityEngine.Debug.Log(num);
            UnityEngine.Debug.Log(GameManage.instance.enemyManage.chesses.Count);
            UnityEngine.Debug.Log(stopwatch.ElapsedMilliseconds + "ms");
        }
    }
}
