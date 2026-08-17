using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 棋子被动：僵尸进入以自身为中心的 3×3 范围即爆炸，伤害 = 快照 ATK × 倍率，随后自毁。
/// </summary>
[Serializable]
public class PassiveSkillEffect_TomoriChessPiece : ISkillEffect
{
    [Tooltip("伤害 = ATK × 此倍率（默认 22.5）")]
    public float damageMultiplier = 22.5f;

    [Tooltip("检测间隔（秒）")]
    public float checkInterval = 0.15f;

    [Tooltip("爆炸特效 Prefab（对象池 Create）；空则只结算伤害不播特效")]
    public GameObject explodeEffect;

    Chess _user;
    Timer _timer;
    readonly List<Tile> _tiles = new List<Tile>(9);
    readonly List<Chess> _enemies = new List<Chess>(16);
    bool _exploded;

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        if (user == null)
            return;
        _user = user;
        _exploded = false;

        if (user.equipWeapon != null)
            user.equipWeapon.AttackAble = false;

        user.OnRemove.AddListener(OnRemove);
        if (GameManage.instance?.timerManage != null)
            _timer = GameManage.instance.timerManage.AddTimer(CheckTrigger, checkInterval, true);
    }

    void OnRemove(Chess chess)
    {
        _timer?.Stop();
        _timer = null;
        if (chess != null)
            chess.OnRemove.RemoveListener(OnRemove);
    }

    void CheckTrigger()
    {
        if (_exploded || _user == null || _user.IfDeath)
            return;

        Tile center = _user.moveController?.standTile;
        if (center == null)
            return;

        WisadelGridHelper.CollectNineGridTiles(center, _tiles);
        WisadelGridHelper.CollectEnemiesOnTiles(_user, _tiles, _enemies);
        if (_enemies.Count == 0)
            return;

        Explode();
    }

    void Explode()
    {
        if (_exploded || _user == null)
            return;
        _exploded = true;
        _timer?.Stop();
        _timer = null;

        float atk = 0f;
        if (_user.skillController?.context != null
            && _user.skillController.context.TryGet(TomoriChessKeys.PieceDamageAtk, out float snap))
            atk = snap;

        float damage = atk * damageMultiplier;
        Tile center = _user.moveController?.standTile;
        if (center != null && damage > 0f)
        {
            WisadelGridHelper.CollectNineGridTiles(center, _tiles);
            WisadelGridHelper.CollectEnemiesOnTiles(_user, _tiles, _enemies);
            for (int i = 0; i < _enemies.Count; i++)
            {
                Chess enemy = _enemies[i];
                if (enemy == null || enemy.IfDeath)
                    continue;
                DamageMessege dm = _user.skillController.DM;
                dm.damageFrom = _user;
                dm.damageTo = enemy;
                dm.damage = damage;
                _user.propertyController.TakeDamage(dm);
            }
        }

        SpawnExplodeEffect();
        _user.Death();
    }

    void SpawnExplodeEffect()
    {
        if (explodeEffect == null || ObjectPool.instance == null || _user == null)
            return;
        GameObject fx = ObjectPool.instance.Create(explodeEffect);
        if (fx == null)
            return;
        Vector3 pos = _user.moveController?.standTile != null
            ? _user.moveController.standTile.transform.position
            : _user.transform.position;
        fx.transform.position = pos;
    }
}
