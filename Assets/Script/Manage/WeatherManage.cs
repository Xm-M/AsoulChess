using UnityEngine;

/// <summary>
/// 气候效果：在本组件 Inspector 配置烟雾/雪地预制体；统一创建、持有与回收 <see cref="Effect_Smoke"/> / <see cref="Effect_Snow"/>。
/// 挂在 <see cref="GameManage"/> 同一物体上（或由 <see cref="GameManage.Awake"/> 自动添加）。
/// 关卡内可由 <see cref="GameStartPlugin_Smoke"/> / <see cref="GameStartPlugin_Snow"/> 触发初始化；离开关卡时回收。
/// </summary>
public class WeatherManage : IManager
{
    [Header("气候预制体")]
    [Tooltip("根物体挂 Effect_Smoke，从对象池创建")]
    [SerializeField]
    GameObject weatherSmokePrefab;
    [Tooltip("根物体挂 Effect_Snow（组件上再配 iceCellPrefab）")]
    [SerializeField]
    GameObject weatherSnowPrefab;

    GameObject _smokeRoot;
    Effect_Smoke _smoke;
    GameObject _snowRoot;
    Effect_Snow _snow;

    public void InitManage()
    {
        EventController.Instance.AddListener(EventName.WhenLeaveLevel.ToString(), OnLeaveLevel);
    }

    public void OnGameStart() { }

    public void OnGameOver() { }

    void OnLeaveLevel()
    {
        RecycleAll();
    }

    /// <summary>若本关尚未创建则从对象池生成并定位到原点。</summary>
    public Effect_Smoke GetOrCreateSmoke()
    {
        if (weatherSmokePrefab == null) return null;
        if (_smoke != null && _smoke.gameObject != null)
            return _smoke;

        _smokeRoot = ObjectPool.instance.Create(weatherSmokePrefab);
        _smokeRoot.transform.position = Vector2.zero;
        _smoke = _smokeRoot.GetComponent<Effect_Smoke>();
        return _smoke;
    }

    /// <summary>若本关尚未创建则生成。</summary>
    public Effect_Snow GetOrCreateSnow()
    {
        if (weatherSnowPrefab == null) return null;
        if (_snow != null && _snow.gameObject != null)
            return _snow;

        _snowRoot = ObjectPool.instance.Create(weatherSnowPrefab);
        _snowRoot.transform.position = Vector2.zero;
        _snow = _snowRoot.GetComponent<Effect_Snow>();
        return _snow;
    }

    public Effect_Smoke GetSmokeOrNull() => _smoke != null && _smoke.gameObject != null ? _smoke : null;

    public Effect_Snow GetSnowOrNull() => _snow != null && _snow.gameObject != null ? _snow : null;

    /// <summary>
    /// 若本关尚未创建 <see cref="Effect_Snow"/> 则生成并 <see cref="Effect_Snow.InitSnow"/>。
    /// 无 <see cref="GameStartPlugin_Snow"/> 时，初雪/冰车等仍可通过 <see cref="Effect_Snow.GetInstanceOrNull"/> 按需创建。
    /// </summary>
    public Effect_Snow EnsureSnow()
    {
        Effect_Snow s = GetOrCreateSnow();
        if (s != null)
            s.InitSnow();
        return s;
    }

    /// <summary>离开关卡时回收烟雾与雪地对象池实例。</summary>
    public void RecycleAll()
    {
        if (_smokeRoot != null)
        {
            ObjectPool.instance.Recycle(_smokeRoot);
            _smokeRoot = null;
            _smoke = null;
        }

        if (_snowRoot != null)
        {
            if (_snow != null)
                _snow.ClearAllIce();
            ObjectPool.instance.Recycle(_snowRoot);
            _snowRoot = null;
            _snow = null;
        }
    }
}
