using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 单条可配置提示：<see cref="eventId"/> 须与 <see cref="SnakeGameEventId"/> 或事件载荷字符串一致。
/// <para><b>SnakeEatFood</b>：图标与时长用本行；正文来自事件载荷中的展示部分（棋子描述等），仍空才用 <see cref="tipMessage"/>；同种食物<strong>本关卡内</strong>仅首次吃掉时提示，关卡重开会重新计数。</para>
/// </summary>
[Serializable]
public class SnakeEventTipBinding
{
    [Tooltip("例如 SnakeStumble_OutOfBounds、SnakeEatFood、SnakeFoodSpawned 等，见 SnakeGameEventId")]
    public string eventId;

    public Sprite icon;

    [TextArea(1, 4)]
    [Tooltip("普通事件作正文；SnakeEatFood 行在无棋子描述时作兜底文案")]
    public string tipMessage;

    [Min(0.1f)]
    [Tooltip("TextPanel 提示停留秒数")]
    public float displaySeconds = 2.5f;
}

/// <summary>
/// 贪吃蛇关卡：注册蛇相关事件，并按 <see cref="tipBindings"/> 在 <see cref="TextPanel"/> 上排队提示。
/// 将本插件加入关卡 <see cref="LevelData.GameStartPlugin"/> 列表即可。吃食物提示按关卡重置，不在 SO 上持久化运行时状态。
/// </summary>
public class GameStartPlugin_SnakeEvent : ILevelPlugin
{
    [Tooltip("事件载荷 eventId → 图标与文案；未配置的 id 不会弹提示")]
    public List<SnakeEventTipBinding> tipBindings = new List<SnakeEventTipBinding>();

    [NonSerialized]
    HashSet<string> _eatFoodTipShownKeys;

    public void StadgeEffect(LevelController levelController)
    {
        // 插件挂在关卡 SO 上，不能用持久字段做「是否已注册」判断；每次关卡开始先卸监听再注册，避免重复订阅，且与 OverPlugin 成对安全。
        Unregister();
        if (_eatFoodTipShownKeys == null)
            _eatFoodTipShownKeys = new HashSet<string>();
        else
            _eatFoodTipShownKeys.Clear();
        Register();
    }

    public void OverPlugin(LevelController levelController)
    {
        Unregister();
    }

    void Register()
    {
        EventController.Instance.AddListener<string>(EventName.SnakeHitWall.ToString(), OnSnakeHitWall);
        EventController.Instance.AddListener<string>(EventName.SnakeEatFood.ToString(), OnSnakeEatFood);
        EventController.Instance.AddListener<string>(EventName.SnakeEatZombie.ToString(), OnSnakeEatZombie);
        EventController.Instance.AddListener<string>(EventName.SnakeFoodSpawned.ToString(), OnSnakeFoodSpawned);
    }

    void Unregister()
    {
        EventController.Instance.RemoveListener<string>(EventName.SnakeHitWall.ToString(), OnSnakeHitWall);
        EventController.Instance.RemoveListener<string>(EventName.SnakeEatFood.ToString(), OnSnakeEatFood);
        EventController.Instance.RemoveListener<string>(EventName.SnakeEatZombie.ToString(), OnSnakeEatZombie);
        EventController.Instance.RemoveListener<string>(EventName.SnakeFoodSpawned.ToString(), OnSnakeFoodSpawned);
    }

    void OnSnakeHitWall(string eventId) => TryEnqueueTipForEventId(eventId);

    /// <summary>载荷为 <see cref="SnakeEatFoodPayload"/>；每种棋子名在本关卡内仅第一次吃掉时弹提示。</summary>
    void OnSnakeEatFood(string payload) => TryEnqueueEatFoodTip(payload);

    void OnSnakeEatZombie(string eventId) => TryEnqueueTipForEventId(eventId);

    void OnSnakeFoodSpawned(string eventId) => TryEnqueueTipForEventId(eventId);

    /// <summary>供子类或扩展：在入队前可改写 eventId 或做额外逻辑。</summary>
    protected virtual void TryEnqueueTipForEventId(string eventId)
    {
        if (string.IsNullOrEmpty(eventId) || tipBindings == null || tipBindings.Count == 0)
            return;

        var binding = FindBinding(eventId);
        if (binding == null)
            return;

        if (string.IsNullOrEmpty(binding.tipMessage) && binding.icon == null)
            return;

        var panel = UIManage.GetView<TextPanel>();
        if (panel == null)
            return;

        float d = binding.displaySeconds > 0f ? binding.displaySeconds : 2.5f;
        panel.EnqueueTip(binding.icon, binding.tipMessage ?? string.Empty, d);
    }

    /// <summary>吃食物：文字用载荷中的展示部分；图与时长用 <c>eventId == SnakeEatFood</c> 的绑定；去重键在本关卡内仅提示一次。</summary>
    protected virtual void TryEnqueueEatFoodTip(string payload)
    {
        SnakeEatFoodPayload.Split(payload, out string dedupeKey, out string displayText);

        if (string.IsNullOrEmpty(dedupeKey))
            return;
        if (_eatFoodTipShownKeys != null && _eatFoodTipShownKeys.Contains(dedupeKey))
            return;

        var binding = FindBinding(SnakeGameEventId.EatFood);
        string text = !string.IsNullOrWhiteSpace(displayText)
            ? displayText
            : (binding != null ? binding.tipMessage : string.Empty);

        if (string.IsNullOrEmpty(text) && (binding == null || binding.icon == null))
            return;

        var panel = UIManage.GetView<TextPanel>();
        if (panel == null)
            return;

        _eatFoodTipShownKeys?.Add(dedupeKey);

        Sprite icon = binding != null ? binding.icon : null;
        float d = binding != null && binding.displaySeconds > 0f ? binding.displaySeconds : 2.5f;
        panel.EnqueueTip(icon, text ?? string.Empty, d);
    }

    SnakeEventTipBinding FindBinding(string eventId)
    {
        if (string.IsNullOrEmpty(eventId) || tipBindings == null)
            return null;
        for (int i = 0; i < tipBindings.Count; i++)
        {
            var b = tipBindings[i];
            if (b == null || string.IsNullOrEmpty(b.eventId))
                continue;
            if (string.Equals(b.eventId, eventId, StringComparison.Ordinal))
                return b;
        }

        return null;
    }
}
