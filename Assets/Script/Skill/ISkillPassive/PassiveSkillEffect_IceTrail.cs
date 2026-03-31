using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 冰车被动：在 <see cref="PassiveSkill.UseSkill"/> 进场时注册 <see cref="MoveController.OnReachTile"/>，
/// 每次 <b>reach 新格</b>时在 <b>上一格</b>（刚离开的格）铺冰，与 <see cref="IceCell"/> 配合。
/// 在棋子预制体的被动里挂 <see cref="PassiveSkill"/>；冰块预制体由场景里 <see cref="Effect_Snow"/>（建议经 <see cref="GameStartPlugin_Snow"/> 生成）统一配置。
/// </summary>
[Serializable]
public class PassiveSkillEffect_IceTrail : ISkillEffect
{
    [Tooltip("≤0 表示不自动融化")]
    public float iceLifetimeSeconds = 25f;

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        if (user == null) return;
        IceTrailPassiveRuntime r = user.GetComponent<IceTrailPassiveRuntime>();
        if (r == null)
            r = user.gameObject.AddComponent<IceTrailPassiveRuntime>();
        r.Setup(user, iceLifetimeSeconds);
    }
}

/// <summary>
/// 由 <see cref="PassiveSkillEffect_IceTrail"/> 添加；订阅 <see cref="MoveController.OnReachTile"/>，在上一格生成冰。
/// </summary>
public class IceTrailPassiveRuntime : MonoBehaviour
{
    Chess _chess;
    float _lifetime;
    Tile _lastStandTile;

    public void Setup(Chess chess, float lifetime)
    {
        if (_chess != null && _chess.moveController != null)
            _chess.moveController.OnReachTile.RemoveListener(OnReachTile);

        _chess = chess;
        _lifetime = lifetime;

        if (_chess == null || _chess.moveController == null) return;

        _lastStandTile = _chess.moveController.standTile;
        _chess.moveController.OnReachTile.AddListener(OnReachTile);
    }

    void OnReachTile(Chess c, Tile newTile)
    {
        if (_chess == null || c != _chess || newTile == null) return;
        if (_lastStandTile != null)
            SpawnIceOnTile(_lastStandTile);
        _lastStandTile = newTile;
    }

    void SpawnIceOnTile(Tile tile)
    {
        if (tile == null || _chess == null) return;
        var snow = Effect_Snow.GetInstanceOrNull();
        if (snow == null) return;
        snow.PlaceOrRefreshIce(tile.mapPos, _lifetime, _chess.tag);
    }

    void OnDisable()
    {
        if (_chess != null && _chess.moveController != null)
            _chess.moveController.OnReachTile.RemoveListener(OnReachTile);
    }
}
