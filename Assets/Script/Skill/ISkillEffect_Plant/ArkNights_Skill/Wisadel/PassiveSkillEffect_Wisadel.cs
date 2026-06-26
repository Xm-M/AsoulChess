using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 维什戴尔被动：入场召影 + Bullet 命中后余震/好礼（onTakeDamage）。
/// </summary>
public class PassiveSkillEffect_Wisadel : ISkillEffect
{
    [LabelText("魂灵之影 Creator（可空，后续配置）")]
    public PropertyCreator shadowCreator;

    [LabelText("入场召唤数量")]
    public int entrySummonCount = 1;

    [SerializeField, LabelText("残影标记")]
    Buff_WisadelMark markBuff;

    [SerializeField, LabelText("爆炸眩晕")]
    DizznessBuff stunBuff;

    [SerializeField, LabelText("标记增伤 ATK 比例")]
    float markBonusRatio = 0.15f;

    [SerializeField, LabelText("余震 ATK 比例")]
    float aftershockRatio = 0.5f;

    [SerializeField, LabelText("爆炸 ATK 比例")]
    float explodeRatio = 1.5f;

    [SerializeField, LabelText("召唤特效")]
    GameObject summonEffect;

    Chess user;
    readonly List<Tile> _tileBuffer = new List<Tile>();
    readonly List<Chess> _aftershockBuffer = new List<Chess>();
    readonly List<Chess> _explodeBuffer = new List<Chess>();

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        this.user = user;
        user.propertyController.onTakeDamage.AddListener(OnBulletHit);
        user.OnRemove.AddListener(OnChessRemove);
        user.skillController.context.Set(WisadelKeys.ExplodeProcRate, 0.15f);

        WisadelShadowPlacer.TrySummon(user, shadowCreator, entrySummonCount, summonEffect);
        WisadelStealthHelper.Refresh(user);
    }

    void OnBulletHit(DamageMessege dm)
    {
        if (user == null || dm.damageFrom != user)
            return;
        if ((dm.damageElementType & ElementType.Bullet) == 0)
            return;

        Chess main = dm.damageTo;
        if (main == null || main.IfDeath)
            return;

        float atk = WisadelDamageHelper.GetAttack(user);

        if (Buff_WisadelMark.HasMarkFrom(main, user))
        {
            WisadelDamageHelper.DealDamage(user, main, atk * markBonusRatio, ElementType.AOE);
        }

        WisadelGridHelper.CollectAftershockVictims(user, main, _tileBuffer, _aftershockBuffer);

        float aftershockDmg = atk * aftershockRatio;
        for (int i = 0; i < _aftershockBuffer.Count; i++)
        {
            WisadelDamageHelper.DealDamage(user, _aftershockBuffer[i], aftershockDmg, ElementType.AOE);
        }

        float procRate = WisadelDamageHelper.GetExplodeProcRate(user);
        float explodeDmg = atk * explodeRatio;
        var explodeElement = ElementType.AOE | ElementType.Explode;

        for (int i = 0; i < _aftershockBuffer.Count; i++)
        {
            Chess victim = _aftershockBuffer[i];
            if (!Buff_WisadelMark.HasMarkFrom(victim, user))
                continue;
            if (Random.value > procRate)
                continue;

            if (!WisadelGridHelper.TryGetStandOrEstimatedTile(victim, out Tile victimTile))
                continue;

            WisadelGridHelper.CollectNineGridTiles(victimTile, _tileBuffer);
            WisadelGridHelper.CollectEnemiesOnTiles(user, _tileBuffer, _explodeBuffer);
            for (int j = 0; j < _explodeBuffer.Count; j++)
            {
                WisadelDamageHelper.DealDamage(
                    user, _explodeBuffer[j], explodeDmg, explodeElement, DamageType.Physical, stunBuff);
            }
        }

        Buff_WisadelMark.ApplyOrRefresh(main, user);

        WisadelBurstMode.TryConsumeAmmo(user);
    }

    void OnChessRemove(Chess chess)
    {
        if (user != null)
            user.propertyController.onTakeDamage.RemoveListener(OnBulletHit);

        Buff_WisadelMark.ClearAllFromOwner(user);
        WisadelShadowPlacer.DestroyAllShadows(user);
        if (user.buffController.buffDic.TryGetValue("WisadelCamouflage", out Buff camouflage))
            camouflage.BuffOver();
    }
}
