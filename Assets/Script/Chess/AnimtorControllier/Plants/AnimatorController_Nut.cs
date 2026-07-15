using UnityEngine;

/// <summary>
/// 坚果类植物：按当前生命百分比写入 Animator Float（默认 <c>VisualTier</c>，可改为 <c>Blend</c>），
/// 与 <see cref="AnimatorController_SampleZombie"/> 断档规则一致，但不包含音效、掉件、冰车等逻辑。
/// Animator 需在 idle/attack 等状态用 Blend Tree 或同名 Float 参数消费 <see cref="visualTierParameterName"/>：0 完整、1 中破、2 大破。
/// 受伤与 <see cref="DamageType.Heal"/>（经 <c>GetDamage</c>）均会 <see cref="SyncMorphFromHp"/>；直接 <c>Heal(float)</c> 不经过此路径。
/// </summary>
public class AnimatorController_Nut : AnimatorController
{
    [Tooltip("血量比例高于此值 → 档位 0（完整）。默认与 SampleZombie 一致；天素罗用 0.66。")]
    [Range(0.01f, 0.99f)]
    [SerializeField]
    float hpTier0Above = 0.6f;

    [Tooltip("血量比例高于此值且不超过 hpTier0Above → 档位 1；否则为 2。应小于 hpTier0Above。天素罗用 0.33。")]
    [Range(0.01f, 0.99f)]
    [SerializeField]
    float hpTier1Above = 0.25f;

    [Tooltip("Animator Float 参数名，与 Blend Tree 一致（如 VisualTier 或 Blend）。")]
    [SerializeField]
    string visualTierParameterName = DefaultVisualTierParam;

    public override void WhenControllerEnterWar()
    {
        base.WhenControllerEnterWar();
        SyncMorphFromHp();
    }

    public override void WhenControllerLeaveWar()
    {
        SetVisualTier(0f, visualTierParameterName);
        base.WhenControllerLeaveWar();
    }

    public override void OnGetDamage(DamageMessege dm)
    {
        base.OnGetDamage(dm);
        SyncMorphFromHp();
    }

    /// <summary>外部（如治疗）若会改变血量比例且需立刻对齐外观，可调用。</summary>
    public void SyncMorphFromHp()
    {
        if (chess == null || chess.propertyController == null)
            return;

        float high = Mathf.Clamp01(hpTier0Above);
        float low = Mathf.Clamp01(hpTier1Above);
        if (low >= high)
            low = Mathf.Max(0.01f, high - 0.01f);

        float hp = chess.propertyController.GetHpPerCent();
        float tier = hp > high ? 0f : hp > low ? 1f : 2f;
        SetVisualTier(tier, visualTierParameterName);
    }
}
