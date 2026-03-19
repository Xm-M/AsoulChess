using System.Collections.Generic;
using UnityEngine;

public class Effect_Smoke : MonoBehaviour
{
    public static Effect_Smoke Instance;
    public GameObject smokeTile;
    //public List<Sprite> randomSmoke;
    //public Animator  animator;
    public int n;
    List<LittleSmoke> smokes;

    private void OnEnable()
    {
        Instance = this;
    }
    private void OnDisable()
    {
        if (Instance == this)
            Instance = null;
    }
    /// <summary>
    /// 在地图每个 Tile 生成雾气，铺满整张地图
    /// </summary>
    public void InitSmokes()
    {
        smokes = new List<LittleSmoke>();
        foreach (var tile in MapManage.instance.tiles)
        {
            GameObject smoke = ObjectPool.instance.Create(smokeTile);
            smoke.GetComponent<Animator>().SetInteger("n", Random.Range(0, n));
            LittleSmoke LS = new LittleSmoke();
            LS.smoke = smoke;
            LS.mapPos = tile.mapPos;
            smokes.Add(LS);
            smoke.transform.position = tile.transform.position;
            smoke.transform.SetParent(transform);
        }
    }

    /// <summary>
    /// 隐藏 0~maxColumn 列的雾气
    /// </summary>
    /// <param name="maxColumn">要隐藏的最大列索引（含），如 4 表示隐藏第 0、1、2、3、4 列</param>
    /// <param name="hideTime">隐藏持续时间，设足够长可视为整局不显示</param>
    public void HideSmokeInColumns(int maxColumn, float hideTime = 99999f)
    {
        if (smokes == null) return;
        foreach (var ls in smokes)
        {
            if (ls.mapPos.x <= maxColumn)
                ls.Hide(hideTime);
        }
    }
    public void HideSmoke(Vector2 pos,float range,float hideTime)
    {
        foreach(var smoke in smokes)
        {
            if (smoke.ifInRange(pos, range)) smoke.Hide(hideTime);
        }
    }

    /// <summary>
    /// 在指定格子区域生成/显示雾气。用于僵尸技能等在当前位置制造雾气。
    /// </summary>
    /// <param name="centerTile">中心格子（如僵尸的 standTile）</param>
    /// <param name="size">区域大小：1=单格，3=3x3，5=5x5</param>
    public void ShowSmoke(Tile centerTile, int size)
    {
        if (centerTile == null || smokes == null) return;
        ShowSmokeInRange(centerTile.mapPos, size);
    }

    /// <summary>
    /// 在指定世界坐标区域生成/显示雾气。
    /// </summary>
    /// <param name="pos">世界坐标（会换算到最近格子）</param>
    /// <param name="size">区域大小：1=单格，3=3x3，5=5x5</param>
    public void ShowSmoke(Vector2 pos, int size)
    {
        if (smokes == null) return;
        var map = MapManage.instance;
        int cx = Mathf.Clamp(Mathf.RoundToInt(pos.x / map.tileSize.x), 0, map.mapSize.x - 1);
        int cy = Mathf.Clamp(Mathf.RoundToInt(pos.y / map.tileSize.y), 0, map.mapSize.y - 1);
        ShowSmokeInRange(new Vector2Int(cx, cy), size);
    }

    void ShowSmokeInRange(Vector2Int centerMapPos, int size)
    {
        int half = (size - 1) / 2;
        int minX = Mathf.Max(0, centerMapPos.x - half);
        int maxX = Mathf.Min(MapManage.instance.mapSize.x - 1, centerMapPos.x + half);
        int minY = Mathf.Max(0, centerMapPos.y - half);
        int maxY = Mathf.Min(MapManage.instance.mapSize.y - 1, centerMapPos.y + half);

        foreach (var ls in smokes)
        {
            if (ls.mapPos.x >= minX && ls.mapPos.x <= maxX && ls.mapPos.y >= minY && ls.mapPos.y <= maxY)
                ls.ForceShow();
        }
    }
    public class LittleSmoke
    {
        public GameObject smoke;
        public Vector2Int mapPos;
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
            hide = false;
            hideTimer = null;
            smoke.SetActive(true);
        }
        /// <summary>
        /// 强制显示雾气，取消正在进行的隐藏计时
        /// </summary>
        public void ForceShow()
        {
            if (hide && hideTimer != null)
            {
                hideTimer.Stop();
                hideTimer = null;
            }
            hide = false;
            smoke.SetActive(true);
        }
        public bool ifInRange(Vector2 center,float dis)
        {
            return Vector2.Distance(smoke.transform.position,center)<=dis;
        }
    }

}
