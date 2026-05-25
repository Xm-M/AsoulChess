using UnityEngine;
using Sirenix.OdinInspector;
/// <summary>
/// 在 <see cref="InRangeTransition"/> 基础上增加普攻间隔：仅在「可普攻且有索敌目标」时累加 <c>_t</c>；
/// 当 <c>_t ≥ weapon.GetInterval()/GetAccelerate()</c> 时清零 <c>_t</c> 并返回 true 进入攻击态。
/// 无目标或 <see cref="AttackController.AttackAble"/> 为 false 时重置 <c>_t</c>，避免离怪游走把 CD 攒满。
/// 攻击动画阶段本边不评估，故间隔不包含在攻击态内流逝的时间（与策划约定一致）。
/// </summary>
public class InRangeTransition_Interval : InRangeTransition
{
    [ShowInInspector]
    float _t;

    public override bool ifReach(Chess chess)
    {
         
        float interval = chess.equipWeapon.weapon.GetInterval();
        if (interval <= 0f)
            return true;

        float accel = chess.propertyController.GetAccelerate();
        if (accel <= 0.0001f)
            accel = 1f;
        float need = interval / accel;

        _t += Time.deltaTime;
        if (_t >= need&&base.ifReach(chess))
        {
            _t = 0f;
            return true;
        }

        return false;
    }

    public override Transition Clone()
    {
        return new InRangeTransition_Interval();
    }
}
