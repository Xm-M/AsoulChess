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

    /// <summary>按钻石/金币/银币面额贪心拆分视觉硬币（仅展示，与 runGold 总额一致近似）。</summary>
    public static List<int> SplitGreedy(int amount, int diamondUnit, int goldUnit, int silverUnit)
    {
        var list = new List<int>();
        if (amount <= 0)
            return list;

        diamondUnit = Mathf.Max(1, diamondUnit);
        goldUnit = Mathf.Max(1, goldUnit);
        silverUnit = Mathf.Max(1, silverUnit);

        int[] units = { diamondUnit, goldUnit, silverUnit };
        System.Array.Sort(units, (a, b) => b.CompareTo(a));

        int remaining = amount;
        for (int u = 0; u < units.Length; u++)
        {
            int unit = units[u];
            while (remaining >= unit && list.Count < MaxVisualCoins)
            {
                list.Add(unit);
                remaining -= unit;
            }
        }

        if (remaining > 0 && list.Count < MaxVisualCoins)
            list.Add(silverUnit);

        return list;
    }

    static void ResolveVisualUnits(out int diamondUnit, out int goldUnit, out int silverUnit)
    {
        diamondUnit = 1000;
        goldUnit = 50;
        silverUnit = 10;
        var economy = RoguelikeRunService.ActiveRunConfig?.economyConfig;
        if (economy != null)
            economy.GetCoinVisualUnits(out diamondUnit, out goldUnit, out silverUnit);
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
        float scatterRadiusMax = 140f,
        float scatterDuration = 0.5f)
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

        ResolveVisualUnits(out int diamondUnit, out int goldUnit, out int silverUnit);
        int runGold = RoguelikeRunService.State?.runGold ?? 0;
        var batch = new RoguelikeRewardCoinBatch(host, runGold);
        Vector2 center = GetWidgetScreenCenter(widget);
        var amounts = SplitGreedy(goldAmount, diamondUnit, goldUnit, silverUnit);

        for (int i = 0; i < amounts.Count; i++)
        {
            var coin = itemPanel.Create<Item_Coin>();
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float radius = Random.Range(rMin, rMax);
            Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            coin.InitRoguelikeRewardScatter(amounts[i], center, offset, batch, scatterDuration);
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
    int _scatterFinished;
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
    }

    public void OnScatterFinished()
    {
        if (_collecting)
            return;

        _scatterFinished++;
        if (_scatterFinished >= _coins.Count && _coins.Count > 0)
            TryStartAutoCollectTimer();
    }

    void TryStartAutoCollectTimer()
    {
        if (_autoCollectRoutine != null || _host == null)
            return;

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
