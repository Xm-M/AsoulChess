using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect_Smoke : MonoBehaviour
{
    public static Effect_Smoke Instance;
    public GameObject smokeTile;
    public float movespeed;
    public List<Sprite> randomSmoke;
    List<LittleSmoke> smokes;
    float totalDis;
    float targetX;
    /// <summary>
    /// 在每个Tile都生成一个smokeTile 然后把自己向右移动n格 
    /// </summary>
    private void OnEnable()
    {
        Instance = this;
    }
    public void InitSmokes(float moveDis)
    {
        smokes = new List<LittleSmoke>();
        foreach(var tile in MapManage.instance.tiles)
        {
            GameObject smoke= ObjectPool.instance.Create(smokeTile);
            smoke.GetComponent<SpriteRenderer>().sprite = randomSmoke[Random.Range(0, randomSmoke.Count)];
            LittleSmoke LS = new LittleSmoke();
            LS.smoke = smoke;
            smokes.Add(LS);
            smoke.transform.position = tile.transform.position;
            smoke.transform.SetParent(transform);
        }
        totalDis = (MapManage.instance.mapSize.x * MapManage.instance.tileSize.x);
        targetX = transform.position.x +moveDis;
        transform.position = new Vector2(targetX + totalDis, transform.position.y);
        StartCoroutine(Move());
    }
    IEnumerator Move()
    {
        while (transform.position.x > targetX)
        {
            transform.position = Vector2.MoveTowards(transform.position,new Vector2(targetX,transform.position.y),movespeed*Time.deltaTime);
            yield return null;
        }
    }
    public void HideSmoke(Vector2 pos,float range,float hideTime)
    {
        foreach(var smoke in smokes)
        {
            if (smoke.ifInRange(pos, range)) smoke.Hide(hideTime);
        }
    }
    public class LittleSmoke
    {
        public GameObject smoke;
        public Timer hideTimer;
        bool hide;
        public void Hide(float hideTime)
        {
            //其实是播放烟雾慢慢消失的动画
            if (!hide)
            {
                hide = true;
                smoke.SetActive(false);
                hideTimer = GameManage.instance.timerManage.AddTimer(Show, hideTime, false);
            }
            else
            {
                //hide = false;
                hideTimer.ResetTime(hideTime);
            }
            
        }
        public void Show()
        {
            hide=false;
            smoke.SetActive(true);
        }
        public bool ifInRange(Vector2 center,float dis)
        {
            return Vector2.Distance(smoke.transform.position,center)<=dis;
        }
    }

}
