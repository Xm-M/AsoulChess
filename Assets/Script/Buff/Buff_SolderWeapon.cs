using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 覆盖作用对象 <see cref="Weapon_Sample"/> 的索敌与攻击方式，并写入目标 Animator 的 Blend（Float）。
/// 首次 <see cref="BuffEffect"/> 会缓存原始 findTarget / attackFunction / Blend；<see cref="BuffOver"/> 恢复。
/// <see cref="BuffReset"/>：叠层/刷新时按「新传入 buff」的配置重新覆盖武器与 Blend（不把武器恢复成旧层再改）。
/// </summary>
public class Buff_SolderWeapon : Buff
{
    [SerializeReference, LabelText("索敌覆盖")]
    public IFindTarget findTargetOverride;

    [SerializeReference, LabelText("攻击方式覆盖")]
    public IAttackFunction attackFunctionOverride;

    [LabelText("Blend 数值"), Tooltip("写入 Animator Float 参数（默认名 Blend）。")]
    public float blendValue;

    [LabelText("Blend 参数名")]
    public string blendParameterName = "Blend";

    [Tooltip("留空则使用棋子 animatorController.animator；可指定武器子物体等上的 Animator。")]
    public Animator targetAnimator;

    IFindTarget _savedFind;
    IAttackFunction _savedAttack;
    float _savedBlend;
    bool _savedWeapon;
    bool _savedBlendParam;

    public Buff_SolderWeapon()
    {
        buffName = "SolderWeapon";
    }

    Animator GetTargetAnimator()
    {
        if (targetAnimator != null) return targetAnimator;
        return target != null ? target.animatorController != null ? target.animatorController.animator : null : null;
    }

    void CacheOriginalsIfNeeded()
    {
        if (_savedWeapon) return;
        if (target?.equipWeapon?.weapon is Weapon_Sample ws)
        {
            _savedFind = ws.findTarget;
            _savedAttack = ws.attackFunction;
            _savedWeapon = true;
        }
        var anim = GetTargetAnimator();
        if (anim != null && !string.IsNullOrEmpty(blendParameterName) && AnimatorController.HasParameter(anim, blendParameterName))
        {
            _savedBlend = anim.GetFloat(blendParameterName);
            _savedBlendParam = true;
        }
    }

    void ApplyConfig(IFindTarget find, IAttackFunction attack, float blend, string blendName, Animator animField)
    {
        if (target?.equipWeapon?.weapon is Weapon_Sample ws)
        {
            if (find != null) ws.findTarget = find;
            if (attack != null) ws.attackFunction = attack;
        }
        var anim = animField != null ? animField : GetTargetAnimator();
        if (anim != null && !string.IsNullOrEmpty(blendName) && AnimatorController.HasParameter(anim, blendName))
            anim.SetFloat(blendName, blend);
    }

    void RestoreOriginals()
    {
        if (target?.equipWeapon?.weapon is Weapon_Sample ws && _savedWeapon)
        {
            ws.findTarget = _savedFind;
            ws.attackFunction = _savedAttack;
        }
        var anim = GetTargetAnimator();
        if (anim != null && _savedBlendParam && !string.IsNullOrEmpty(blendParameterName) && AnimatorController.HasParameter(anim, blendParameterName))
            anim.SetFloat(blendParameterName, _savedBlend);
    }

    public override void BuffEffect(Chess t)
    {
        base.BuffEffect(t);
        CacheOriginalsIfNeeded();
        ApplyConfig(findTargetOverride, attackFunctionOverride, blendValue, blendParameterName, targetAnimator);
    }

    public override void BuffOver()
    {
        RestoreOriginals();
        _savedWeapon = false;
        _savedBlendParam = false;
        base.BuffOver();
    }

    public override void BuffReset(Buff resetBuff)
    {
        base.BuffReset(resetBuff);
        if (resetBuff is not Buff_SolderWeapon other) return;

        // 与当前卡上配置同步为「新 buff」的数据，便于读档/调试与后续逻辑一致
        findTargetOverride = other.findTargetOverride;
        attackFunctionOverride = other.attackFunctionOverride;
        blendValue = other.blendValue;
        blendParameterName = other.blendParameterName;
        targetAnimator = other.targetAnimator;
        if (!string.IsNullOrEmpty(other.buffName)) buffName = other.buffName;

        // 按新 buff 的效果再次覆盖武器与 Blend（不恢复为无 buff 再改）
        CacheOriginalsIfNeeded();
        ApplyConfig(other.findTargetOverride, other.attackFunctionOverride, other.blendValue, other.blendParameterName, other.targetAnimator);
    }
}
