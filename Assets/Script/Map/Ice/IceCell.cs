using UnityEngine;

/// <summary>
/// 单格冰块：不改 <see cref="Tile.tileType"/>；用 <see cref="plantBlocker"/>（Layer IcePlantBlock）挡种植射线，
/// <see cref="fireCollider"/>（Layer Ice）供火焰 Overlap 融化。敌方冰（<see cref="iceOwnerTag"/> != Player）才启用种植挡板。
/// 由 <see cref="Effect_Snow"/> 按格管理，同一 mapPos 不会叠多层。
/// </summary>
public class IceCell : MonoBehaviour
{
    public AudioPlayer player;
    [SerializeField] Collider2D fireCollider;
    [SerializeField] Collider2D plantBlocker;
    [SerializeField] SpriteRenderer spriteRenderer;

    [Tooltip("与生成冰的冰车/来源 Chess.tag 一致；Player 则不对己方挡种植")]
    [SerializeField]
    string iceOwnerTag = "Enemy";

    Timer _lifeTimer;
    /// <summary><see cref="int.MinValue"/> 表示未纳入 <see cref="Effect_Snow"/>（不应再出现）。</summary>
    Vector2Int _mapPos = new Vector2Int(int.MinValue, 0);

    /// <summary>世界坐标、来源 tag、存活秒数（≤0 表示不自动消失）、所在格（用于 <see cref="Effect_Snow"/> 注销）。</summary>
    public void Init(Vector3 worldPosition, string ownerTag, float lifetimeSeconds, Vector2Int mapPos)
    {
        transform.position = worldPosition;
        iceOwnerTag = ownerTag;
        _mapPos = mapPos;
        ApplyLayerToColliders();
        RefreshPlantBlocker();
        RefreshSpriteAlpha();
        ApplyLifetime(lifetimeSeconds);
    }
    private void OnEnable()
    {
        player?.SetAudioUniqueLimit(2);
        player?.PlayAudioUniqueLimit("ice");
    }
    void ApplyLifetime(float lifetimeSeconds)
    {
        if (_lifeTimer != null)
        {
            _lifeTimer.Stop();
            _lifeTimer = null;
        }
        if (lifetimeSeconds > 0f && GameManage.instance != null && GameManage.instance.timerManage != null)
            _lifeTimer = GameManage.instance.timerManage.AddTimer(Melt, lifetimeSeconds);
    }

    /// <summary>刷新融化计时（已存在冰时延长持续时间）。</summary>
    public void ResetLifetime(float lifetimeSeconds)
    {
        ApplyLifetime(lifetimeSeconds);
    }

    /// <summary>由 <see cref="Effect_Snow"/> 在己方覆盖敌方冰时调用：改归属、挡板与计时。</summary>
    public void SetOwnerAndLifetime(string ownerTag, float lifetimeSeconds)
    {
        iceOwnerTag = ownerTag;
        RefreshPlantBlocker();
        RefreshSpriteAlpha();
        ResetLifetime(lifetimeSeconds);
    }

    public string IceOwnerTag => iceOwnerTag;

    void Awake()
    {
        ApplyLayerToColliders();
        RefreshPlantBlocker();
        RefreshSpriteAlpha();
    }

    void ApplyLayerToColliders()
    {
        int ice = LayerMask.NameToLayer("Ice");
        int pb = LayerMask.NameToLayer("IcePlantBlock");
        if (fireCollider != null && ice >= 0)
            fireCollider.gameObject.layer = ice;
        if (plantBlocker != null && pb >= 0)
            plantBlocker.gameObject.layer = pb;
    }

    /// <summary>仅敌方冰挡己方种植：Player 冰不启用 plantBlocker。</summary>
    void RefreshPlantBlocker()
    {
        if (plantBlocker == null) return;
        plantBlocker.enabled = iceOwnerTag != "Player";
    }

    /// <summary>己方冰半透明，敌方冰不透明。</summary>
    void RefreshSpriteAlpha()
    {
        if (spriteRenderer == null) return;
        Color c = spriteRenderer.color;
        c.a = iceOwnerTag == "Player" ? 0.5f : 1f;
        spriteRenderer.color = c;
    }

    /// <summary>火焰等调用：先关碰撞体再关表现。</summary>
    public void Melt()
    {
        if (_mapPos.x != int.MinValue)
            Effect_Snow.Instance?.UnregisterIce(_mapPos);

        if (_lifeTimer != null)
        {
            _lifeTimer.Stop();
            _lifeTimer = null;
        }

        if (plantBlocker != null)
            plantBlocker.enabled = false;
        if (fireCollider != null)
            fireCollider.enabled = false;
        if (spriteRenderer != null)
            spriteRenderer.enabled = false;
        Destroy(gameObject, 0.02f);
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        ApplyLayerToColliders();
        RefreshSpriteAlpha();
    }
#endif

    /// <summary>
    /// 己方点选种植前调用：敌方冰应阻止种植。
    /// 先查 IcePlantBlock 层；若工程未建该层或点在子碰撞体上，再用 OverlapPointAll + IceCell 兜底（Ice 层火碰撞体也可关联到 IceCell）。
    /// </summary>
    public static bool BlocksPlayerPlantAt(Vector2 worldPos)
    {
        int iceBlockLayer = LayerMask.NameToLayer("IcePlantBlock");
        if (iceBlockLayer >= 0 && Physics2D.OverlapPoint(worldPos, 1 << iceBlockLayer) != null)
            return true;
        foreach (Collider2D col in Physics2D.OverlapPointAll(worldPos))
        {
            IceCell cell = col.GetComponentInParent<IceCell>();
            if (cell != null && cell.IceOwnerTag != "Player")
                return true;
        }
        return false;
    }

    /// <summary>火焰 Buff 等：在 Ice 层上 Overlap 并融化。</summary>
    public static void MeltIceInRadius(Vector2 worldPos, float radius)
    {
        int ice = LayerMask.NameToLayer("Ice");
        if (ice < 0 || radius <= 0f) return;
        Collider2D[] hits = Physics2D.OverlapCircleAll(worldPos, radius, 1 << ice);
        for (int i = 0; i < hits.Length; i++)
        {
            IceCell c = hits[i] != null ? hits[i].GetComponent<IceCell>() : null;
            c?.Melt();
        }
    }
}
