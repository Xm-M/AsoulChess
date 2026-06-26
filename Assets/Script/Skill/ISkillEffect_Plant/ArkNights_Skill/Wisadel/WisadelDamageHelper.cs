using UnityEngine;

public static class WisadelDamageHelper
{
    public static void DealDamage(
        Chess from,
        Chess to,
        float damage,
        ElementType elementType,
        DamageType damageType = DamageType.Physical,
        Buff takeBuff = null)
    {
        if (from?.propertyController == null || to == null || to.IfDeath)
            return;

        var mes = new DamageMessege(from, to, damage, damageType, elementType);
        if (takeBuff != null)
            mes.takeBuff = takeBuff;
        from.propertyController.TakeDamage(mes);
    }

    public static float GetAttack(Chess chess) =>
        chess?.propertyController != null ? chess.propertyController.GetAttack() : 0f;

    public static float GetExplodeProcRate(Chess owner)
    {
        if (owner?.skillController?.context == null)
            return 0.15f;
        if (owner.skillController.context.TryGet<float>(WisadelKeys.ExplodeProcRate, out float rate))
            return rate;
        return 0.15f;
    }
}
