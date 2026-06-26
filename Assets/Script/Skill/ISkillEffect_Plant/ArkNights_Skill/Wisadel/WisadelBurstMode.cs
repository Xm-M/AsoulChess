using UnityEngine;

/// <summary>维什戴尔爆裂模式：仅换子弹 + 动画 + Buff，6 发打完后再转 CD。</summary>
public static class WisadelBurstMode
{
    public class BurstConfig
    {
        public GameObject burstBullet;
        public Buff_WisadelBurst burstBuff;
        public int burstAmmo = 6;
    }

    public static void EnterBurst(Chess user, BurstConfig config)
    {
        if (user == null || config == null)
            return;

        var weapon = user.equipWeapon?.weapon as Weapon_Sample;
        var shoot = weapon?.attackFunction as ShootBullet;
        if (weapon == null || shoot == null)
        {
            Debug.LogWarning("[WisadelBurstMode] 需要 Weapon_Sample + ShootBullet。");
            return;
        }

        var ctx = user.skillController.context;
        if (!ctx.TryGet<bool>(WisadelKeys.BurstActive, out bool active) || !active)
            ctx.Set(WisadelKeys.SavedBullet, shoot.bullet);

        if (config.burstBullet != null)
            shoot.bullet = config.burstBullet;

        user.animatorController.ChangeFloat(1);

        if (config.burstBuff != null)
            user.buffController.AddBuff(config.burstBuff);
        else
            user.buffController.AddBuff(new Buff_WisadelBurst());

        ctx.Set(WisadelKeys.BurstActive, true);
        ctx.Set(WisadelKeys.BurstAmmo, config.burstAmmo);
        ctx.Set(WisadelKeys.ExplodeProcRate, 1f);
    }

    public static void EndBurst(Chess user)
    {
        if (user == null)
            return;

        var ctx = user.skillController.context;
        if (!ctx.TryGet<bool>(WisadelKeys.BurstActive, out bool active) || !active)
            return;

        var shoot = (user.equipWeapon?.weapon as Weapon_Sample)?.attackFunction as ShootBullet;
        if (shoot != null
            && ctx.TryGet<GameObject>(WisadelKeys.SavedBullet, out GameObject savedBullet)
            && savedBullet != null)
            shoot.bullet = savedBullet;

        user.animatorController.ChangeFloat(0);
        if (user.buffController.buffDic.TryGetValue("WisadelBurst", out Buff burstBuff))
            burstBuff.BuffOver();

        ctx.Set(WisadelKeys.BurstActive, false);
        ctx.Set(WisadelKeys.BurstAmmo, 0);
        ctx.Set(WisadelKeys.ExplodeProcRate, 0.15f);

        user.skillController.activeSkill?.SkillOver(user);
    }

    public static void TryConsumeAmmo(Chess user)
    {
        if (user?.skillController?.context == null)
            return;
        if (!user.skillController.context.TryGet<bool>(WisadelKeys.BurstActive, out bool active) || !active)
            return;
        if (!user.skillController.context.TryGet<int>(WisadelKeys.BurstAmmo, out int ammo))
            ammo = 0;

        ammo--;
        user.skillController.context.Set(WisadelKeys.BurstAmmo, ammo);
        if (ammo <= 0)
            EndBurst(user);
    }
}
