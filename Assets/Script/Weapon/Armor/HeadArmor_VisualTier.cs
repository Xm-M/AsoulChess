using UnityEngine;

/// <summary>
/// 头盔护甲：不驱动分体帽子 Sprite，按甲量写入本体 Animator VisualTier（3→4→5），破甲写 0。
/// 用于橄榄球等一体贴图僵尸；不改动路障用的 <see cref="HeadArmor"/>。
/// </summary>
public class HeadArmor_VisualTier : ArmorBase
{
    public const float TierHelmetFull = 3f;
    public const float TierHelmetDamaged1 = 4f;
    public const float TierHelmetDamaged2 = 5f;
    public const float TierHelmetOff = 0f;

    public float armorMax = 1400f;
    public GameObject effect;
    public AudioPlayer player;

    [SerializeField]
    float armorCurrent;

    bool _broken;

    public bool IsBroken => _broken;

    public override void InitArmor()
    {
        armorCurrent = armorMax;
        _broken = false;
        if (user != null)
            user.WhenEnterGame.AddListener(ResetArmor);
    }

    public override void ResetArmor(Chess chess)
    {
        if (user == null) return;
        tag = user.tag;
        _broken = false;
        armorCurrent = armorMax;
        user.propertyController.onSetDamage.RemoveListener(GetDamage);
        user.propertyController.onSetDamage.AddListener(GetDamage);
        ApplyHelmetVisualTier();
    }

    public override void GetDamage(DamageMessege dm)
    {
        if (_broken || user == null || dm == null) return;

        if (armorCurrent >= dm.damage)
        {
            dm.damage *= (1f - user.propertyController.GetExtraDefence());
            UIManage.GetView<DamagePanel>().ShowDamageMes(dm);
            armorCurrent -= dm.damage;
            if ((dm.damageElementType & ElementType.Explode) == 0)
                dm.damage = 0f;
            else if (dm.damage > user.propertyController.GetHp())
                BrokenArmor();

            FlashBody();
            player?.RandomPlay();
        }
        else
        {
            if (dm.damageElementType != ElementType.CloseAttack)
                dm.damage -= armorCurrent;
            else
                dm.damage = 0f;
            armorCurrent = 0f;
            if ((dm.damageElementType & ElementType.Explode) == 0 && effect != null && ObjectPool.instance != null)
            {
                GameObject fall = ObjectPool.instance.Create(effect);
                fall.transform.SetParent(user.transform, false);
                fall.transform.localPosition = Vector3.zero;
                player?.RandomPlay();
            }
            BrokenArmor();
            return;
        }

        if (!_broken)
            ApplyHelmetVisualTier();
    }

    public override void BrokenArmor()
    {
        if (_broken) return;
        _broken = true;
        if (user != null)
        {
            user.propertyController.onSetDamage.RemoveListener(GetDamage);
            user.animatorController?.SetVisualTierPublic(TierHelmetOff);
        }
        base.BrokenArmor();
    }

    void ApplyHelmetVisualTier()
    {
        if (user == null || user.animatorController == null) return;
        float tier = TierHelmetFull;
        if (armorCurrent < armorMax / 3f)
            tier = TierHelmetDamaged2;
        else if (armorCurrent < armorMax * 2f / 3f)
            tier = TierHelmetDamaged1;
        user.animatorController.SetVisualTierPublic(tier);
    }

    void FlashBody()
    {
        var sr = user != null && user.animatorController != null ? user.animatorController.sprite : null;
        if (sr != null && sr.material != null)
            sr.material.SetFloat("_FlashAmount", Time.time);
    }
}
