using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 老仓育被动：入场挂压力 Buff、初始化公式参数、生成常驻李萨如弹道；离场回收弹。
/// </summary>
public class PassiveSkillEffect_Okuwaki : ISkillEffect
{
    public GameObject orbitBulletPrefab;

    [SerializeReference]
    public Buff_StressBuff_Okuwaki stressBuff;

    [SerializeReference]
    public Buff_StressBuff_Death guestStressBuff;

    Chess _user;
    Bullet_OkuwakiOrbit _orbit;

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        if (user == null) return;
        _user = user;

        var buff = stressBuff != null
            ? (Buff_StressBuff_Okuwaki)stressBuff.Clone()
            : new Buff_StressBuff_Okuwaki();
        buff.guestStressBuff = guestStressBuff;
        user.buffController.AddBuff(buff);

        OkuwakiCurveState.ApplyInitial(user.skillController.context);
        SpawnOrbit(user);
        user.OnRemove.AddListener(OnRemove);
    }

    void SpawnOrbit(Chess user)
    {
        if (orbitBulletPrefab == null || user == null) return;

        if (_orbit != null)
            _orbit.RecycleBullet();

        GameObject go = ObjectPool.instance.Create(orbitBulletPrefab);
        if (go == null) return;

        _orbit = go.GetComponent<Bullet_OkuwakiOrbit>();
        if (_orbit == null)
        {
            ObjectPool.instance.Recycle(go);
            return;
        }

        Vector3 pos = OkuwakiGridHelper.GetAnchorWorldPos(user);
        _orbit.InitBullet(user, pos, null, user.transform.right);
    }

    void OnRemove(Chess chess)
    {
        if (_orbit != null)
        {
            _orbit.RecycleBullet();
            _orbit = null;
        }

        if (_user != null)
            _user.OnRemove.RemoveListener(OnRemove);
        _user = null;
    }
}
