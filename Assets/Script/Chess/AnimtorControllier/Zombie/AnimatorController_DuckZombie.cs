using UnityEngine;

/// <summary>
/// 鸭子僵尸：陆地动画与 <see cref="AnimatorController_SampleZombie"/> 一致；站在 <see cref="TileType.Water"/> 格时使用水中移动/攻击。
/// Animator 可选两种配法：① 仅增加 Bool 参数 <see cref="inWaterParameterName"/>，在 run/attack 的 Blend Tree 里按参数混合；② 另做状态 <see cref="moveStateWater"/> / <see cref="attackStateWater"/>，由脚本切换播放。
/// </summary>
public class AnimatorController_DuckZombie : AnimatorController_SampleZombie
{
    [Tooltip("与 Animator 中 Bool 一致；未配置该参数则忽略。用于 Blend Tree 区分水陆。")]
    [SerializeField]
    string inWaterParameterName = "InWater";

    [Tooltip("非空且在水中时 Play 此状态名；留空则只依赖 InWater + 基类 Play(\"run\")（Blend Tree 模式）。")]
    [SerializeField]
    string moveStateWater = "run_water";

    [Tooltip("非空且在水中时 Play 此状态名；留空则只依赖 InWater + 基类 Play(\"attack\")。")]
    [SerializeField]
    string attackStateWater = "attack_water";

    [Tooltip("水中攻击且 attackVariant>0 时播放的状态名；留空则水中仍用 attackStateWater + attackVariant 整型参数。")]
    [SerializeField]
    string attackStateNameAltWater = "";

    [Tooltip("站在水格时显示的水波等子物体；非水格或离场时关闭。")]
    [SerializeField]
    GameObject waterRippleEffect;

    bool _inWater;
    Tile _lastStandTile;

    public override void InitController(Chess chess)
    {
        base.InitController(chess);
        _lastStandTile = null;
    }

    public override void WhenControllerEnterWar()
    {
        base.WhenControllerEnterWar();
        if (chess?.moveController == null) return;
        chess.moveController.OnReachTile.RemoveListener(OnReachTile);
        chess.moveController.OnReachTile.AddListener(OnReachTile);
        _lastStandTile = chess.moveController.standTile;
        SyncInWaterFromStandTile();
    }

    public override void WhenControllerLeaveWar()
    {
        if (chess?.moveController != null)
            chess.moveController.OnReachTile.RemoveListener(OnReachTile);
        SetWaterRippleActive(false);
        base.WhenControllerLeaveWar();
    }

    void OnReachTile(Chess c, Tile tile)
    {
        _lastStandTile = tile;
        SyncInWaterFromStandTile();
        TryRefreshLocomotionAnim();
    }

    void Update()
    {
        if (chess == null || chess.moveController == null) return;
        Tile st = chess.moveController.standTile;
        if (st != _lastStandTile)
        {
            _lastStandTile = st;
            SyncInWaterFromStandTile();
            TryRefreshLocomotionAnim();
        }
    }

    static bool IsWaterTile(Tile t) => t != null && (t.tileType & TileType.Water) != 0;

    void SyncInWaterFromStandTile()
    {
        _inWater = IsWaterTile(chess != null ? chess.moveController?.standTile : null);
        ApplyInWaterParameter();
        SetWaterRippleActive(_inWater);
    }

    void SetWaterRippleActive(bool on)
    {
        if (waterRippleEffect != null)
            waterRippleEffect.SetActive(on);
    }

    void ApplyInWaterParameter()
    {
        if (animator == null || string.IsNullOrEmpty(inWaterParameterName)) return;
        if (HasParameter(animator, inWaterParameterName))
            animator.SetBool(inWaterParameterName, _inWater);
    }

    void TryRefreshLocomotionAnim()
    {
        if (animator == null || string.IsNullOrEmpty(moveStateWater)) return;
        var info = animator.GetCurrentAnimatorStateInfo(0);
        if (info.IsName("run") || info.IsName(moveStateWater))
            PlayMove();
    }

    public override void PlayIdle()
    {
        ApplyInWaterParameter();
        base.PlayIdle();
    }

    public override void PlayMove()
    {
        if (animator == null) return;
        ApplyInWaterParameter();
        if (_inWater && !string.IsNullOrEmpty(moveStateWater))
            animator.Play(moveStateWater);
        else
            base.PlayMove();
    }

    public override void PlayAttack()
    {
        if (animator == null) return;
        ApplyInWaterParameter();
        if (_inWater && !string.IsNullOrEmpty(attackStateWater))
        {
            if (attackAnimationVariant > 0 && !string.IsNullOrEmpty(attackStateNameAltWater))
                animator.Play(attackStateNameAltWater);
            else
            {
                if (HasParameter(animator, "attackVariant"))
                    animator.SetInteger("attackVariant", attackAnimationVariant);
                animator.Play(attackStateWater);
            }
        }
        else
            base.PlayAttack();
    }
}
