using UnityEngine;

/// <summary>
/// 雪橇防具：与 <see cref="IceCarArmor"/> 相同的<strong>二类</strong>承伤规则（Buff 不进本体、溢出不进本体、非爆炸伤害先由甲承担）。
/// <see cref="ElementType.Explode"/>：<b>不扣甲、不将本段伤害传给本体</b>（甲未破时直接吞掉），与冰车不同；破甲仅来自非爆炸伤害、离开冰道等。
/// 无冰车式<strong>碾压</strong>（不对碰撞到的植物 <see cref="PropertyController.TakeDamage"/>）。
/// 在防具内监听 <see cref="MoveController.OnReachTile"/>：落地格无冰则 <see cref="BrokenArmor"/>（<see cref="breakEffect"/>）。
/// </summary>
public class Armor_Sled : ArmorBase
{
    public float armorMax = 300f;

    [Tooltip("护甲比例低于该值时从 state1 切到 state2；与 HeadArmor 默认一致（2/3）")]
    [Range(0.05f, 1f)]
    public float stage2SwitchRatio = 2f / 3f;

    [Tooltip("护甲比例低于该值时从 state2 切到 state3；与 HeadArmor 默认一致（1/3），须小于上一档")]
    [Range(0.05f, 1f)]
    public float stage3SwitchRatio = 1f / 3f;

    [Tooltip("可选：雪橇整体模型，破损或静默拆除时隐藏")]
    public GameObject sledVisualRoot;
    /// <summary>三档外观：满甲 state1 → 中损 state2 → 重损 state3，切换比例与 <see cref="HeadArmor"/> 相同。</summary>
    public SpriteRenderer state1, state2, state3;
    public GameObject breakEffect;
    public GameObject explodeEffect;
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

    /// <summary>二类：非爆炸伤害由甲承担；爆炸不扣甲且不传给本体（避免本体先死导致无法召唤）。</summary>
    public override void GetDamage(DamageMessege dm)
    {
        if (_broken || user == null || dm == null) return;
        //Debug.Log("?");
        dm.takeBuff = null;
         

        if (armorCurrent >= dm.damage)
        {
            dm.damage *= (1f - user.propertyController.GetExtraDefence());
            if (!dm.suppressFloatingDamage)
            {
                if (dm.damageTo == null)
                    dm.damageTo = user;
                var damagePanel = UIManage.GetView<DamagePanel>();
                if (damagePanel != null)
                    damagePanel.ShowDamageMes(dm);
            }
            armorCurrent -= dm.damage;
            if ((dm.damageElementType & ElementType.Explode) == 0)
                 dm.damage = 0;
            
            if (currentRender != null)
                currentRender.material.SetFloat("_FlashAmount", Time.time);
            player?.RandomPlay();

            if (!_broken && armorCurrent <= 0f)
            {
                dm.damage = 0;
                BrokenArmor();
            }
        }
        else
        {
            
            armorCurrent = 0f;
            if ((dm.damageElementType & ElementType.Explode) == 0)
            {
                if (breakEffect != null)
                {
                    GameObject fall = ObjectPool.instance.Create(breakEffect);
                    //fall.transform.SetParent(user.transform, false);
                    fall.transform.position= transform.position;
                }
            }
            else
            {
                if (explodeEffect != null)
                {
                    GameObject fall = ObjectPool.instance.Create(explodeEffect);
                    fall.transform.position = transform.position;
                }
            }
            if ((dm.damageElementType & ElementType.Explode) == 0)
                dm.damage = 0f;
            BrokenArmor();
        }

        UpdateSledStageSprites();
    }

    /// <summary>与 <see cref="HeadArmor.GetDamage"/> 末尾两段判断一致：<c>armorCurrent &lt; armorMax * 2/3</c> → state2，<c>&lt; armorMax * 1/3</c> → state3。</summary>
    void UpdateSledStageSprites()
    {
        if (_broken) return;
        float max = armorMax;
        if (max <= 0f) return;

        float r2 = Mathf.Clamp01(stage2SwitchRatio);
        float r3 = Mathf.Clamp01(stage3SwitchRatio);
        if (r3 >= r2)
            r3 = Mathf.Max(0.05f, r2 - 0.01f);

        // 对齐 HeadArmor：先判二档，再判三档
        if (state1 != null && state2 != null && state3 != null && armorCurrent < max * r2)
        {
            if (state1.gameObject.activeSelf)
            {
                state1.gameObject.SetActive(false);
                state2.gameObject.SetActive(true);
                currentRender = state2;
            }
        }
        if (state2 != null && state3 != null && armorCurrent < max * r3)
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
        chess.animatorController.sprite.gameObject.SetActive(false);
        chess.GetComponent<BoxCollider2D>().size = new Vector2(6.5f, 1);
        user.equipWeapon.AttackAble=false;
        tag = user.tag;
        _broken = false;
        armorCurrent = armorMax;
        user.propertyController.onSetDamage.AddListener(GetDamage);
        SubscribeReachIceCheck();
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
        user.animatorController.sprite.gameObject.SetActive(true);
        user.GetComponent<BoxCollider2D>().size = new Vector2(1f, 1);
        user.equipWeapon.AttackAble = true ;
        if (_broken) return;
        _broken = true;
        UnsubscribeReachIceCheck();
        user.propertyController.onSetDamage.RemoveListener(GetDamage);
        HideVisuals();
        base.BrokenArmor();
    }

    void SubscribeReachIceCheck()
    {
        if (user?.moveController == null) return;
        user.moveController.OnReachTile.RemoveListener(OnReachTileLeaveIce);
        user.moveController.OnReachTile.AddListener(OnReachTileLeaveIce);
    }

    void UnsubscribeReachIceCheck()
    {
        if (user?.moveController == null) return;
        user.moveController.OnReachTile.RemoveListener(OnReachTileLeaveIce);
    }

    /// <summary>落地格无冰则视为离开冰道，破甲（原在被动里，现由防具承担）。</summary>
    void OnReachTileLeaveIce(Chess c, Tile newTile)
    {
        if (_broken || user == null || c != user || newTile == null) return;
        var snow = Effect_Snow.GetInstanceOrNull();
        if (snow != null && snow.HasIceAt(newTile.mapPos)) return;

        BrokenArmor();
        if (breakEffect != null)
        {

            GameObject fall = ObjectPool.instance.Create(breakEffect);
            fall.transform.SetParent(user.transform, false);
            //fall.transform.localPosition = Vector3.zero;
        }
    }

    void OnDestroy()
    {
        UnsubscribeReachIceCheck();
    }

    /// <summary>与冰车相同：子弹命中雪橇碰撞体时走二类承伤；无碾压植物。</summary>
    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (_broken || user == null || user.IfDeath) return;
        if (collision.CompareTag(user.tag)) return;

        Bullet bullet = collision.GetComponent<Bullet>();
        if (bullet != null && bullet.Dm != null)
        {
            ElementType elementSave = bullet.Dm.damageElementType;
            GetDamage(bullet.Dm);
            if ((elementSave & ElementType.AOE) == 0)
                bullet.RecycleBullet();
        }
    }
}
