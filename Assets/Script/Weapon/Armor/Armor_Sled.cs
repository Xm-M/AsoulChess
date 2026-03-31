using UnityEngine;

/// <summary>
/// 雪橇防具：吸收伤害直至归零；<see cref="BrokenArmor"/> 时触发 <see cref="ArmorBase.OnArmorBroken"/>。
/// 离开冰道由被动在 <see cref="MoveController.OnReachTile"/> 中调用 <see cref="BrokenArmor"/>，与被打爆共用同一套破损与刷怪。
/// 数值在 Inspector 配置（设计：防具 300）；本体血量在 <see cref="PropertyCreator"/>。
/// </summary>
public class Armor_Sled : ArmorBase
{
    public float armorMax = 300f;
    [Tooltip("可选：雪橇整体模型，破损或静默拆除时隐藏")]
    public GameObject sledVisualRoot;
    public SpriteRenderer state1, state2, state3;
    public GameObject breakEffect;
    public AudioPlayer player;

    [SerializeField] float armorCurrent;
    SpriteRenderer currentRender;
    bool _broken;

    public bool IsBroken => _broken;

    public override void InitArmor()
    {
        armorCurrent = armorMax;
        user.WhenEnterGame.AddListener(ResetArmor);
    }

    public override void GetDamage(DamageMessege dm)
    {
        if (armorCurrent >= dm.damage)
        {
            dm.damage *= (1 - user.propertyController.GetExtraDefence());
            UIManage.GetView<DamagePanel>().ShowDamageMes(dm);
            armorCurrent -= dm.damage;
            if ((dm.damageElementType & ElementType.Explode) == 0)
                dm.damage = 0;
            else if (dm.damage > user.propertyController.GetHp())
                BrokenArmor();

            if (currentRender != null)
                currentRender.material.SetFloat("_FlashAmount", Time.time);
            player?.RandomPlay();
        }
        else
        {
            if ((dm.damageElementType != ElementType.CloseAttack))
                dm.damage -= armorCurrent;
            else
                dm.damage = 0;
            armorCurrent = 0;
            if ((dm.damageElementType & ElementType.Explode) == 0 && breakEffect != null)
            {
                GameObject fall = ObjectPool.instance.Create(breakEffect);
                fall.transform.SetParent(user.transform, false);
                fall.transform.localPosition = Vector3.zero;
            }
            BrokenArmor();
        }

        if (state1 != null && state2 != null && state3 != null && armorCurrent < armorMax * 2f / 3f)
        {
            if (state1.gameObject.activeSelf)
            {
                state1.gameObject.SetActive(false);
                state2.gameObject.SetActive(true);
                currentRender = state2;
            }
        }
        if (state2 != null && state3 != null && armorCurrent < armorMax / 3f)
        {
            if (state2.gameObject.activeSelf)
            {
                state2.gameObject.SetActive(false);
                state3.gameObject.SetActive(true);
                currentRender = state3;
            }
        }
    }

    public override void ResetArmor(Chess chess)
    {
        tag = user.tag;
        _broken = false;
        user.propertyController.onSetDamage.AddListener(GetDamage);
        armorCurrent = armorMax;
        if (state1 != null)
        {
            state1.gameObject.SetActive(true);
            if (state2 != null) state2.gameObject.SetActive(false);
            if (state3 != null) state3.gameObject.SetActive(false);
            currentRender = state1;
        }
        if (sledVisualRoot != null)
            sledVisualRoot.SetActive(true);
    }

    void HideVisuals()
    {
        if (state1 != null) state1.gameObject.SetActive(false);
        if (state2 != null) state2.gameObject.SetActive(false);
        if (state3 != null) state3.gameObject.SetActive(false);
        if (sledVisualRoot != null)
            sledVisualRoot.SetActive(false);
    }

    public override void BrokenArmor()
    {
        if (_broken) return;
        _broken = true;
        user.propertyController.onSetDamage.RemoveListener(GetDamage);
        HideVisuals();
        base.BrokenArmor();
    }
}
