using UnityEngine;

/// <summary>
/// 老仓育压力 Buff：不低于 30；压力增加时向八邻格传导等量压力（一层、不连锁）。
/// </summary>
public class Buff_StressBuff_Okuwaki : Buff_StressBuff_Death
{
    [Tooltip("邻格无「压力」Buff 时施加的模板（与 Tomo 一致）")]
    public Buff_StressBuff_Death guestStressBuff;

    public override void BuffReset(Buff resetBuff)
    {
        int oldStress = stress;
        base.BuffReset(resetBuff);

        if (stress < OkuwakiCurveKeys.MinStress)
        {
            stress = OkuwakiCurveKeys.MinStress;
            if (user?.skillController?.context != null)
                user.skillController.context.Set<int>("stress", stress);
        }

        int delta = stress - oldStress;
        if (delta > 0)
            OkuwakiStressSpread.ApplyDelta(user, delta, guestStressBuff);
    }
}
