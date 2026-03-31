using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 地形类植物被动：种下时把所在格 <see cref="Tile.tileType"/> 改为 <see cref="afterPlantTileType"/>（如睡莲→草地）；
/// 离场时（铲除、直接 <see cref="Chess.Death"/> 等）恢复地形：监听 <see cref="Chess.OnRemove"/>，勿用 <see cref="Chess.DeathEvent"/>（后者仅 DeathState 亡语）。
/// 若格上仍有 <see cref="PlantType.MainPlant"/> 则先令其死亡。
/// 实际状态挂在运行时组件 <see cref="PotTerrainTileRuntime"/> 上；可选在格上有 MainPlant 时地形植物 <see cref="Chess.UnSelectable"/>。
/// </summary>
[Serializable]
public class PassiveSkillEffect_TerrainPot : ISkillEffect
{
    [Tooltip("种下后该格的有效 tileType（睡莲一般为 Grass；水盆等可配其它组合）")]
    public TileType afterPlantTileType = TileType.Grass;

    [Tooltip("格上已有 MainPlant（stander）时，地形植物不可选中")]
    public bool unselectableWhenMainPlantOnTile = true;

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        if (user == null) return;
        PotTerrainTileRuntime r = user.GetComponent<PotTerrainTileRuntime>();
        if (r == null)
            r = user.gameObject.AddComponent<PotTerrainTileRuntime>();
        r.Apply(user, afterPlantTileType, unselectableWhenMainPlantOnTile);
    }
}

/// <summary>
/// 由 <see cref="PassiveSkillEffect_TerrainPot"/> 在进场时添加；勿在同一预制体上手动重复挂多个。
/// <para>
/// <see cref="Chess.Death"/> 会先 <see cref="ChessTeamManage.RecycleChess"/> 再 <c>OnRemove</c> 广播；
/// 回收会 <c>SetActive(false)</c>，本组件 <see cref="OnDisable"/> 先于 <c>OnRemove.Invoke</c> 执行。
/// 因此不得在 <see cref="OnDisable"/> 里对 <see cref="Chess.OnRemove"/> 做 <c>RemoveListener</c>，否则铲除时回调已被摘掉，地形无法还原。
/// </para>
/// </summary>
public class PotTerrainTileRuntime : MonoBehaviour
{
    Tile _tile;
    TileType _savedType;
    bool _applied;
    Chess _chess;
    bool _unselectWhenMainOnTile;

    public void Apply(Chess chess, TileType afterPlantTileType, bool unselectableWhenMainPlantOnTile = true)
    {
        if (chess == null || _applied) return;
        _chess = chess;
        _unselectWhenMainOnTile = unselectableWhenMainPlantOnTile;
        Tile t = chess.moveController?.standTile;
        if (t == null) return;
        _tile = t;
        _savedType = t.tileType;
        t.tileType = afterPlantTileType;
        _applied = true;
        chess.OnRemove.AddListener(OnOwnerRemove);
    }

    void LateUpdate()
    {
        if (!_applied || !_unselectWhenMainOnTile || _chess == null || _chess.IfDeath) return;
        Tile t = _chess.moveController?.standTile;
        if (t == null) return;
        if (t.stander != null && t.stander != _chess)
            _chess.UnSelectable();
        else
            _chess.ResumeSelectable();
    }

    void OnOwnerRemove(Chess removed)
    {
        TryRestoreTerrain(removed);
    }

    void OnDisable()
    {
        TryRestoreTerrain(_chess);
    }

    /// <summary>可重复调用：先回收关对象再 <see cref="Chess.OnRemove"/> 时，第一次即还原地形。</summary>
    void TryRestoreTerrain(Chess removed)
    {
        if (!_applied || _tile == null) return;
        Chess main = _tile.stander;
        if (main != null && main != removed && main.propertyController != null &&
            main.propertyController.creator.plantType == PlantType.MainPlant)
            main.Death();
        _tile.tileType = _savedType;
        _applied = false;
        _tile = null;
    }
}
