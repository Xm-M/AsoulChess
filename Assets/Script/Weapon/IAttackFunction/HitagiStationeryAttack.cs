using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 战场原黑仪普攻：等概率随机文具；发数 = 1 + floor(stress/20)，连发有间隔。
/// </summary>
public class HitagiStationeryAttack : IAttackFunction
{
    [Tooltip("铅笔 / 尺子 / 订书机 / 橡皮擦 Prefab，等概率")]
    public List<GameObject> stationeryBullets = new List<GameObject>();

    [Min(0.01f)]
    [Tooltip("多发之间的间隔（秒）")]
    public float salvoInterval = 0.2f;

    [Min(1)]
    public int stressPerExtraShot = 20;

    const string SalvoKey = "hitagi_stationery_salvo";

    public void Attack(Chess user, List<Chess> targets)
    {
        if (user == null || stationeryBullets == null || stationeryBullets.Count == 0)
            return;

        int stress = 0;
        user.skillController?.context?.TryGet("stress", out stress);
        int shots = 1 + Mathf.Max(0, stress) / Mathf.Max(1, stressPerExtraShot);

        var targetCopy = new List<Chess>();
        if (targets != null)
        {
            for (int i = 0; i < targets.Count; i++)
            {
                if (targets[i] != null && !targets[i].IfDeath)
                    targetCopy.Add(targets[i]);
            }
        }

        var ctx = user.skillController?.context;
        if (ctx != null && ctx.TryGet(SalvoKey, out Coroutine prev) && prev != null)
            user.StopCoroutine(prev);

        Coroutine running = user.StartCoroutine(FireSalvo(user, targetCopy, shots));
        ctx?.Set(SalvoKey, running);
    }

    IEnumerator FireSalvo(Chess user, List<Chess> targets, int shots)
    {
        for (int i = 0; i < shots; i++)
        {
            if (user == null || user.IfDeath)
                yield break;

            SpawnOne(user, targets);

            if (i < shots - 1 && salvoInterval > 0f)
                yield return new WaitForSeconds(salvoInterval);
        }

        user?.skillController?.context?.Remove(SalvoKey);
    }

    void SpawnOne(Chess user, List<Chess> targets)
    {
        GameObject prefab = stationeryBullets[Random.Range(0, stationeryBullets.Count)];
        if (prefab == null) return;

        Transform muzzle = user.equipWeapon?.weaponPos != null
            ? user.equipWeapon.weaponPos
            : user.transform;

        GameObject go = ObjectPool.instance.Create(prefab);
        if (go == null) return;

        Bullet bullet = go.GetComponent<Bullet>();
        if (bullet == null)
        {
            ObjectPool.instance.Recycle(go);
            return;
        }

        Chess target = targets != null && targets.Count > 0 ? targets[0] : null;
        bullet.InitBullet(user, muzzle.position, target, user.transform.right);
        if (target != null)
            bullet.Dm.damageTo = target;
    }
}
