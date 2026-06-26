using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 金币/银币/钻石掉落物，曲线掉落动画，点击后加钱并飞向左下角。
/// </summary>
public class Item_Coin : UIItem, IPointerEnterHandler
{
    public int coinAmount;
    public Image iconImage;
    [Header("银币10 / 金币100 / 钻石1000")]
    public Sprite silverIcon, goldIcon, diamondIcon;
    [Header("掉落曲线动画")]
    public AnimationCurve curve;
    public float dropTotalTime = 1f;
    public float dropTimeSpeed = 2f;
    public float dropHeight = 1.25f;
    public float dropMoveSpeed = 1f;
    public float fallSpeed = 400f;
    Vector2 recyclePos;
    bool ifPick;
    bool _roguelikeVisualOnly;
    RoguelikeRewardCoinBatch _roguelikeBatch;

    /// <summary>回收目标位置（屏幕坐标），为空则用左下角</summary>
    public static Transform RecycleTarget { get; set; }

    /// <summary>初始化：从世界坐标 startPos 曲线掉落，点击后获得 amount 金币并飞向左下角</summary>
    public void InitCoin(int amount, Vector3 startPos)
    {
        _roguelikeVisualOnly = false;
        _roguelikeBatch = null;
        coinAmount = amount;
        ifPick = false;
        ApplyIcon(amount);
        recyclePos = GetRecyclePos();
        float speed = dropMoveSpeed;
        if (MapManage_PVZ.instance != null && MapManage.instance != null)
        {
            Vector2 center = MapManage_PVZ.instance.tiles[MapManage_PVZ.instance.mapSize.x / 2, MapManage.instance.mapSize.y / 2].transform.position;
            if (startPos.x > center.x) speed *= -1;
        }
        StartCoroutine(CurveDropThenIdle(startPos, speed));
    }

    /// <summary>肉鸽搜刮：UI 位置 scatter，不加主线 coins；3s 或点击后飞向 Run 金币条。</summary>
    public void InitRoguelikeRewardScatter(int faceValue, Vector2 screenPos, Vector2 scatterOffset,
        RoguelikeRewardCoinBatch batch)
    {
        _roguelikeVisualOnly = true;
        _roguelikeBatch = batch;
        coinAmount = faceValue;
        ifPick = false;
        ApplyIcon(faceValue);
        recyclePos = GetRecyclePos();
        transform.position = screenPos + scatterOffset;
        batch?.Register(this);
    }

    void ApplyIcon(int amount)
    {
        if (iconImage == null)
            return;

        Sprite s = amount >= 1000 ? diamondIcon : amount >= 100 ? goldIcon : silverIcon;
        if (s != null)
            iconImage.sprite = s;
    }

    IEnumerator CurveDropThenIdle(Vector3 startPos, float moveSpeed)
    {
        if (curve != null && Camera.main != null)
        {
            float elapsed = 0f;
            float t = 0f;
            float y0 = curve.Evaluate(t);
            float starty = startPos.y;
            while (elapsed < dropTotalTime && !ifPick)
            {
                t = elapsed;
                float yOffset = curve.Evaluate(t) - y0;
                startPos = new Vector2(startPos.x + moveSpeed * Time.deltaTime, starty + yOffset * dropHeight);
                transform.position = Camera.main.WorldToScreenPoint(startPos);
                elapsed += Time.deltaTime * dropTimeSpeed;
                yield return null;
            }
        }
        else if (Camera.main != null)
        {
            transform.position = Camera.main.WorldToScreenPoint(startPos);
        }
        while (!ifPick)
            yield return null;
    }

    Vector2 GetRecyclePos()
    {
        if (RecycleTarget != null)
            return RecycleTarget.position;
        return new Vector2(80f, 80f);
    }

    public bool BeginRoguelikeFly(RoguelikeRewardCoinBatch batch)
    {
        if (ifPick)
            return false;

        ifPick = true;
        _roguelikeBatch = batch;
        recyclePos = GetRecyclePos();
        StartCoroutine(FlyRoguelikeRoutine());
        return true;
    }

    IEnumerator FlyRoguelikeRoutine()
    {
        while (Vector2.Distance((Vector2)transform.position, recyclePos) > 1f)
        {
            transform.position = Vector2.MoveTowards(transform.position, recyclePos, fallSpeed * Time.deltaTime * 3f);
            yield return null;
        }

        _roguelikeBatch?.OnCoinFlyFinished();
        Recycle();
    }

    public IEnumerator Recycles()
    {
        var panel = UIManage.GetView<ItemPanel>();
        if (panel?.player != null)
            panel.player.PlayAudio(coinAmount >= 1000 ? "Diamond" : "Coin");
        while (Vector2.Distance((Vector2)transform.position, recyclePos) > 1f)
        {
            transform.position = Vector2.MoveTowards(transform.position, recyclePos, fallSpeed * Time.deltaTime * 3f);
            yield return null;
        }
        Recycle();
    }

    void AddCoinsAndRecycle()
    {
        if (ifPick || _roguelikeVisualOnly)
            return;
        ifPick = true;

        var data = PlayerSaveContext.CurrentData;
        if (data == null && GameManage.instance != null && GameManage.instance.mode == GameMode.Test)
        {
            PlayerSaveContext.CurrentData = PlayerSaveSystem.CreateNew();
            data = PlayerSaveContext.CurrentData;
        }
        if (data != null)
        {
            data.coins += coinAmount;
            PlayerSaveContext.SaveCurrent();
        }

        var parsePanel = UIManage.GetView<ParsePanel>();
        if (parsePanel != null)
            parsePanel.ShowCoinDisplay(PlayerSaveContext.CurrentData?.coins ?? 0);

        StartCoroutine(Recycles());
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        if (_roguelikeVisualOnly)
        {
            _roguelikeBatch?.CollectAll();
            return;
        }

        AddCoinsAndRecycle();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_roguelikeVisualOnly)
            return;

        AddCoinsAndRecycle();
    }

    public override void Recycle()
    {
        _roguelikeVisualOnly = false;
        _roguelikeBatch = null;
        UIManage.GetView<ItemPanel>().Recycle<Item_Coin>(this);
    }
}
