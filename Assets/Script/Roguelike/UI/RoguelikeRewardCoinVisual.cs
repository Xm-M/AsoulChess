using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 肉鸽搜刮领取金币时的 <see cref="Item_Coin"/> 视觉：面额拆分、UI scatter、飞向 Run 金币条。
/// </summary>
public static class RoguelikeRewardCoinVisual
{
    public const float AutoCollectSeconds = 3f;
    const int MaxVisualCoins = 8;

    /// <summary>按大/小面额贪心拆分视觉硬币数量。</summary>
    public static List<int> SplitGreedy(int amount, int largeUnit, int smallUnit)
    {
        var list = new List<int>();
        if (amount <= 0)
            return list;

        largeUnit = Mathf.Max(1, largeUnit);
        smallUnit = Mathf.Max(1, smallUnit);
        if (smallUnit > largeUnit)
            (smallUnit, largeUnit) = (largeUnit, smallUnit);

        int remaining = amount;
        while (remaining >= largeUnit && list.Count < MaxVisualCoins)
        {
            list.Add(largeUnit);
            remaining -= largeUnit;
        }

        while (remaining >= smallUnit && list.Count < MaxVisualCoins)
        {
            list.Add(smallUnit);
            remaining -= smallUnit;
        }

        if (remaining > 0 && list.Count < MaxVisualCoins)
            list.Add(smallUnit);

        return list;
    }

    static void ResolveVisualUnits(out int largeUnit, out int smallUnit)
    {
        largeUnit = 100;
        smallUnit = 10;
        var economy = RoguelikeRunService.ActiveRunConfig?.economyConfig;
        if (economy != null)
            economy.GetCoinVisualUnits(out largeUnit, out smallUnit);
    }

    public static void EnsureRecycleTarget()
    {
        var parsePanel = UIManage.GetView<ParsePanel>();
        if (parsePanel == null)
            return;

        parsePanel.PrepareCoinDisplayForRoguelikeReward();
        if (parsePanel.coinDisplayObject != null)
            Item_Coin.RecycleTarget = parsePanel.coinDisplayObject.transform;
    }

    public static void ClearRecycleTarget()
    {
        Item_Coin.RecycleTarget = null;
        UIManage.GetView<ParsePanel>()?.RestoreCoinDisplayAfterRoguelikeReward();
    }

    public static void Spawn(
        MonoBehaviour host,
        int goldAmount,
        RoguelikeRewardEntryWidget widget,
        float scatterRadiusMin = 56f,
        float scatterRadiusMax = 140f)
    {
        if (host == null || goldAmount <= 0)
            return;

        EnsureRecycleTarget();

        var itemPanel = UIManage.GetView<ItemPanel>();
        if (itemPanel == null)
            return;

        UIManage.Show<ItemPanel>();

        float rMin = Mathf.Max(0f, scatterRadiusMin);
        float rMax = Mathf.Max(rMin, scatterRadiusMax);

        ResolveVisualUnits(out int largeUnit, out int smallUnit);
        int runGold = RoguelikeRunService.State?.runGold ?? 0;
        var batch = new RoguelikeRewardCoinBatch(host, runGold);
        Vector2 center = GetWidgetScreenCenter(widget);
        var amounts = SplitGreedy(goldAmount, largeUnit, smallUnit);

        for (int i = 0; i < amounts.Count; i++)
        {
            var coin = itemPanel.Create<Item_Coin>();
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float radius = Random.Range(rMin, rMax);
            Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            coin.InitRoguelikeRewardScatter(amounts[i], center, offset, batch);
        }
    }

    static Vector2 GetWidgetScreenCenter(RoguelikeRewardEntryWidget widget)
    {
        if (widget == null)
            return new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

        var rt = widget.transform as RectTransform;
        if (rt == null)
            return widget.transform.position;

        var canvas = rt.GetComponentInParent<Canvas>();
        Camera cam = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? canvas.worldCamera
            : null;
        return RectTransformUtility.WorldToScreenPoint(cam, rt.TransformPoint(rt.rect.center));
    }
}

/// <summary>一次领取 spawned 的多枚硬币：3s 自动或点击收束，全部飞抵后刷新 Run 金币条。</summary>
public class RoguelikeRewardCoinBatch
{
    readonly MonoBehaviour _host;
    readonly int _runGoldToDisplay;
    readonly List<Item_Coin> _coins = new List<Item_Coin>();
    bool _collecting;
    int _flying;
    Coroutine _autoCollectRoutine;
    bool _displayUpdated;

    public RoguelikeRewardCoinBatch(MonoBehaviour host, int runGoldToDisplay)
    {
        _host = host;
        _runGoldToDisplay = runGoldToDisplay;
    }

    public void Register(Item_Coin coin)
    {
        if (coin == null)
            return;

        _coins.Add(coin);
        if (_autoCollectRoutine == null && _host != null)
            _autoCollectRoutine = _host.StartCoroutine(AutoCollectAfterDelay());
    }

    IEnumerator AutoCollectAfterDelay()
    {
        yield return new WaitForSecondsRealtime(RoguelikeRewardCoinVisual.AutoCollectSeconds);
        CollectAll();
    }

    public void CollectAll()
    {
        if (_collecting)
            return;

        _collecting = true;
        if (_autoCollectRoutine != null && _host != null)
        {
            _host.StopCoroutine(_autoCollectRoutine);
            _autoCollectRoutine = null;
        }

        _flying = 0;
        for (int i = 0; i < _coins.Count; i++)
        {
            var coin = _coins[i];
            if (coin != null && coin.gameObject.activeInHierarchy && coin.BeginRoguelikeFly(this))
                _flying++;
        }

        if (_flying <= 0)
            ShowRunGoldDisplay();
    }

    public void OnCoinFlyFinished()
    {
        _flying--;
        if (_flying <= 0)
            ShowRunGoldDisplay();
    }

    void ShowRunGoldDisplay()
    {
        if (_displayUpdated)
            return;

        _displayUpdated = true;
        var parsePanel = UIManage.GetView<ParsePanel>();
        parsePanel?.ShowRunGoldDisplay(_runGoldToDisplay);

        var itemPanel = UIManage.GetView<ItemPanel>();
        if (itemPanel?.player != null)
            itemPanel.player.PlayAudio("Coin");
    }
}
