using UnityEngine;

/// <summary>
/// 僵王俯身吐气：单预制体用 Animator Blend（0 火 / 1 冰，与 <see cref="ZombieKingContextKeys.BallVisual"/> 一致）。
/// 动画事件绑 <see cref="EffectMiss.CreateEffectInCircle"/> 或 <see cref="EffectMiss.CreateEffect"/>。
/// </summary>
public class EffectMiss_ZombieKingBreath : EffectMiss
{
    [Tooltip("与口气特效 Animator 上 Blend 参数名一致")]
    public string blendParam = "Blend";

    int _blendParamId;

    void Awake()
    {
        _blendParamId = Animator.StringToHash(blendParam);
    }

    void OnValidate()
    {
        _blendParamId = Animator.StringToHash(blendParam);
    }

    protected override void ConfigureSpawnedEffect(GameObject obj)
    {
        if (obj == null) return;
        var anim = obj.GetComponent<Animator>();
        if (anim == null) return;
        anim.SetFloat(_blendParamId, ReadBallVisual());
    }

    int ReadBallVisual()
    {
        var chess = GetComponentInParent<Chess>();
        if (chess?.skillController?.context == null)
            return 0;
        chess.skillController.context.TryGet(ZombieKingContextKeys.BallVisual, out int ball);
        return Mathf.Clamp(ball, 0, 1);
    }
}
