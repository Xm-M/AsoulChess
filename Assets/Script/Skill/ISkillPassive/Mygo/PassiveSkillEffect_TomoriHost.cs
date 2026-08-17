using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>灯邻格 MyGO 成员 → 额外部署石头外观。</summary>
[Serializable]
public class TomoriMemberStoneSprite
{
    [Tooltip("与 fetterMemberId 一致，如 高松灯 / 千早爱音 / 要乐奈 / 长崎素世 / 椎名立希")]
    public string memberId;
    public Sprite sprite;
}

/// <summary>
/// 灯宿主被动：复用 MyGO 邻格计数；棋子模式下买卡种植播 attack、免费额外部署；死亡清棋并还原商店卡。
/// 宜作为 <see cref="PassiveSkill.effect"/>，或放入 Composite（须先跑 MyGO 计数）。
/// </summary>
[Serializable]
public class PassiveSkillEffect_TomoriHost : PassiveSkill_Mygo
{
    [Tooltip("棋子 PropertyCreator（与主动技换卡同一份）")]
    public PropertyCreator chessPieceCreator;

    [Tooltip("额外部署间隔（秒）；逐个种出，默认 0.1")]
    [Min(0f)]
    public float extraDeployInterval = 0.1f;

    [Tooltip("额外部署石头按 nearChess 成员换图；买卡主棋不改。未配置则保持 Prefab 默认")]
    public List<TomoriMemberStoneSprite> memberStoneSprites = new List<TomoriMemberStoneSprite>(5);

    UnityAction<Chess> _onPlant;
    UnityAction<Chess> _onRemove;
    PropertyCreator _lampCreator;
    readonly List<Tile> _neighborBuffer = new List<Tile>(4);
    readonly List<PendingExtra> _pendingExtras = new List<PendingExtra>(4);
    Timer _extraDeployTimer;

    struct PendingExtra
    {
        public Tile tile;
        public string memberId;
    }

    public override void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        base.SkillEffect(user, config, targets);
        if (user == null)
            return;

        _lampCreator = user.propertyController?.creator;
        _onPlant = OnPlantChess;
        _onRemove = OnHostRemove;
        EventController.Instance.AddListener<Chess>(EventName.WhenPlantChess.ToString(), _onPlant);
        user.OnRemove.AddListener(_onRemove);
    }

    void OnHostRemove(Chess chess)
    {
        StopExtraDeployTimer();
        CleanupListeners(chess);
        ClearAllPieces(chess);
        RestoreShopCard();
    }

    void CleanupListeners(Chess chess)
    {
        if (_onPlant != null)
            EventController.Instance.RemoveListener<Chess>(EventName.WhenPlantChess.ToString(), _onPlant);
        _onPlant = null;
        if (chess != null && _onRemove != null)
            chess.OnRemove.RemoveListener(_onRemove);
        _onRemove = null;
    }

    void OnPlantChess(Chess planted)
    {
        if (user == null || user.IfDeath || planted == null || chessPieceCreator == null)
            return;
        if (planted.propertyController?.creator != chessPieceCreator)
            return;
        if (TomoriChessKeys.IsExtraDeploy)
            return;
        if (user.skillController?.context == null
            || !user.skillController.context.TryGet(TomoriChessKeys.ChessMode, out bool mode)
            || !mode)
            return;

        TrackPiece(planted);
        user.animatorController?.PlayAttack();
        TryFreeExtraDeploy(planted);
    }

    void TrackPiece(Chess piece)
    {
        List<Chess> list = TomoriChessKeys.GetOrCreatePieceList(user);
        if (list == null || piece == null)
            return;
        if (!list.Contains(piece))
            list.Add(piece);

        float atk = 0f;
        if (user.skillController.context.TryGet(TomoriChessKeys.AtkSnapshot, out float snap))
            atk = snap;
        else if (user.propertyController != null)
            atk = user.propertyController.GetAttack();

        if (piece.skillController?.context != null)
            piece.skillController.context.Set(TomoriChessKeys.PieceDamageAtk, atk);
    }

    void TryFreeExtraDeploy(Chess mainPiece)
    {
        if (mainPiece?.moveController?.standTile == null)
            return;

        int want = 0;
        user.skillController.context.TryGet("mygo", out want);
        if (want <= 0)
            return;

        TomoriChessKeys.CollectFourNeighborTiles(mainPiece.moveController.standTile, _neighborBuffer);
        int remainingCap = TomoriChessKeys.CountMygoUnitsOnField()
            - TomoriChessKeys.CountFieldPieces(chessPieceCreator);
        int maxExtra = Mathf.Min(want, Mathf.Max(0, remainingCap));
        _pendingExtras.Clear();
        int memberIdx = 0;
        for (int i = 0; i < _neighborBuffer.Count && _pendingExtras.Count < maxExtra; i++)
        {
            Tile tile = _neighborBuffer[i];
            if (tile == null || chessPieceCreator == null)
                continue;
            if (!chessPieceCreator.IfCanPlant(tile))
                continue;
            string memberId = null;
            if (nearChess != null && memberIdx < nearChess.Count)
            {
                memberId = nearChess[memberIdx];
                memberIdx++;
            }
            _pendingExtras.Add(new PendingExtra { tile = tile, memberId = memberId });
        }

        if (_pendingExtras.Count == 0)
            return;

        // 只停旧定时器，勿 Clear 队列（否则刚填好的 pending 会被清掉）
        if (_extraDeployTimer != null)
        {
            _extraDeployTimer.Stop();
            _extraDeployTimer = null;
        }

        float interval = Mathf.Max(0f, extraDeployInterval);
        if (interval <= 0f || GameManage.instance?.timerManage == null)
        {
            while (TryPlaceNextExtra()) { }
            return;
        }

        // 首枚也延后 interval，避免与主棋同一帧「一口气」出来
        _extraDeployTimer = GameManage.instance.timerManage.AddTimer(OnExtraDeployTick, interval, true);
    }

    void OnExtraDeployTick()
    {
        if (user == null || user.IfDeath)
        {
            CancelExtraDeploy();
            return;
        }

        if (!TryPlaceNextExtra())
            CancelExtraDeploy();
    }

    /// <summary>从队列取下一格尝试种植；成功且还有剩余返回 true；结束返回 false。</summary>
    bool TryPlaceNextExtra()
    {
        while (_pendingExtras.Count > 0)
        {
            PendingExtra pending = _pendingExtras[0];
            _pendingExtras.RemoveAt(0);
            Tile tile = pending.tile;
            if (tile == null || chessPieceCreator == null)
                continue;
            if (!chessPieceCreator.IfCanPlant(tile))
                continue;
            if (TomoriChessKeys.CountFieldPieces(chessPieceCreator)
                >= TomoriChessKeys.CountMygoUnitsOnField())
            {
                _pendingExtras.Clear();
                return false;
            }

            TomoriChessKeys.BeginExtraDeploy();
            try
            {
                Chess extra = ChessTeamManage.Instance.CreateChess(
                    chessPieceCreator, tile, user.tag);
                if (extra != null)
                {
                    TrackPiece(extra);
                    ApplyMemberStoneSprite(extra, pending.memberId);
                }
            }
            finally
            {
                TomoriChessKeys.EndExtraDeploy();
            }

            return _pendingExtras.Count > 0;
        }

        return false;
    }

    void ApplyMemberStoneSprite(Chess piece, string memberId)
    {
        if (piece == null || string.IsNullOrEmpty(memberId) || memberStoneSprites == null)
            return;
        Sprite sp = null;
        for (int i = 0; i < memberStoneSprites.Count; i++)
        {
            TomoriMemberStoneSprite entry = memberStoneSprites[i];
            if (entry == null || entry.sprite == null)
                continue;
            if (entry.memberId == memberId)
            {
                sp = entry.sprite;
                break;
            }
        }
        if (sp == null)
            return;
        SpriteRenderer sr = piece.animatorController != null
            ? piece.animatorController.sprite
            : null;
        if (sr != null)
            sr.sprite = sp;
    }

    void CancelExtraDeploy()
    {
        if (_extraDeployTimer != null)
        {
            _extraDeployTimer.Stop();
            _extraDeployTimer = null;
        }
        _pendingExtras.Clear();
    }

    void StopExtraDeployTimer() => CancelExtraDeploy();

    void ClearAllPieces(Chess host)
    {
        List<Chess> list = null;
        host?.skillController?.context?.TryGet(TomoriChessKeys.SpawnedPieces, out list);

        if (list != null)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                Chess p = list[i];
                if (p != null && !p.IfDeath)
                    p.Death();
            }
            list.Clear();
        }

        if (chessPieceCreator == null || ChessTeamManage.Instance == null)
            return;
        List<Chess> team = ChessTeamManage.Instance.GetTeam(host != null ? host.tag : "Player");
        if (team == null)
            return;
        List<Chess> snapshot = new List<Chess>(team);
        for (int i = 0; i < snapshot.Count; i++)
        {
            Chess c = snapshot[i];
            if (c != null && !c.IfDeath
                && c.propertyController?.creator == chessPieceCreator)
                c.Death();
        }
    }

    void RestoreShopCard()
    {
        if (_lampCreator == null || chessPieceCreator == null)
            return;
        ShopIcon icon = TomoriChessKeys.FindShopIconByGood(chessPieceCreator);
        icon?.RefreshGood(_lampCreator);
    }
}
