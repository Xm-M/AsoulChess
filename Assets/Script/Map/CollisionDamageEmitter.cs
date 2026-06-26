using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 非 Chess 绑定的碰撞伤害体：预制体带 Trigger，碰到目标 Tag 的棋子时按 <see cref="damageMessage"/> 造成伤害。
/// 位移由 Animator 等驱动 Transform；本组件不负责移动（弹道式移动见后续 P2）。
/// </summary>
public class CollisionDamageEmitter : MonoBehaviour
{
    [LabelText("伤害模板")]
    [Tooltip("运行时 damageFrom 默认 null；damageTo 为碰撞到的棋子")]
    public DamageMessege damageMessage = new DamageMessege
    {
        damage = 100f,
        damageType = DamageType.Magic,
        damageElementType = ElementType.None,
    };

    [LabelText("目标 Tag")]
    public string targetTag = "Enemy";

    [LabelText("同一目标冷却（秒）")]
    [MinValue(0f)]
    [Tooltip("0 表示每次 Stay/Enter 都可命中（需配合 maxHitCount 防刷）")]
    public float hitCooldownPerTarget = 0.15f;

    [LabelText("总命中次数上限")]
    [MinValue(0)]
    [Tooltip("0 表示不限制")]
    public int maxHitCount = 0;

    [LabelText("存活时间（秒）")]
    [MinValue(0f)]
    [Tooltip("0 表示不自动回收；可与动画结束时调用 Recycle() 配合")]
    public float lifetime = 1f;

    [LabelText("命中时")]
    public UnityEvent onHit;

    readonly Dictionary<Chess, float> _lastHitTime = new Dictionary<Chess, float>();
    int _totalHits;
    Timer _lifetimeTimer;
    bool _leaveListenerRegistered;

    void OnEnable()
    {
        _totalHits = 0;
        _lastHitTime.Clear();
        RegisterLeaveListener();
        ScheduleLifetime();
    }

    void OnDisable()
    {
        _lifetimeTimer?.Stop();
        _lifetimeTimer = null;
        UnregisterLeaveListener();
        _lastHitTime.Clear();
    }

    void RegisterLeaveListener()
    {
        if (_leaveListenerRegistered || EventController.Instance == null)
            return;
        EventController.Instance.AddListener(EventName.WhenLeaveLevel.ToString(), Recycle);
        _leaveListenerRegistered = true;
    }

    void UnregisterLeaveListener()
    {
        if (!_leaveListenerRegistered || EventController.Instance == null)
            return;
        EventController.Instance.RemoveListener(EventName.WhenLeaveLevel.ToString(), Recycle);
        _leaveListenerRegistered = false;
    }

    void ScheduleLifetime()
    {
        _lifetimeTimer?.Stop();
        if (lifetime <= 0f || GameManage.instance?.timerManage == null)
            return;
        _lifetimeTimer = GameManage.instance.timerManage.AddTimer(Recycle, lifetime);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        TryHit(collision);
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        TryHit(collision);
    }

    void TryHit(Collider2D collision)
    {
        if (collision == null)
            return;
        if (maxHitCount > 0 && _totalHits >= maxHitCount)
            return;

        if (!string.IsNullOrEmpty(targetTag) && !collision.CompareTag(targetTag))
            return;

        var chess = collision.GetComponent<Chess>();
        if (chess == null || chess.IfDeath)
            return;

        if (hitCooldownPerTarget > 0f)
        {
            float now = Time.time;
            if (_lastHitTime.TryGetValue(chess, out float last) && now - last < hitCooldownPerTarget)
                return;
            _lastHitTime[chess] = now;
        }

        ApplyDamage(chess);
        _totalHits++;
        onHit?.Invoke();

        if (maxHitCount > 0 && _totalHits >= maxHitCount)
            Recycle();
    }

    void ApplyDamage(Chess target)
    {
        if (target?.propertyController == null)
            return;

        var template = damageMessage ?? new DamageMessege();
        var mes = new DamageMessege(
            null,
            target,
            template.damage,
            template.damageType,
            template.damageElementType);
        mes.ifCrit = template.ifCrit;
        mes.suppressFloatingDamage = template.suppressFloatingDamage;
        mes.takeBuff = template.takeBuff;
        target.propertyController.GetDamage(mes);
    }

    /// <summary>回收到对象池；可在动画最后一帧通过 Animation Event 调用。</summary>
    public void Recycle()
    {
        _lifetimeTimer?.Stop();
        _lifetimeTimer = null;
        if (ObjectPool.instance != null)
            ObjectPool.instance.Recycle(gameObject);
        else
            gameObject.SetActive(false);
    }
}
