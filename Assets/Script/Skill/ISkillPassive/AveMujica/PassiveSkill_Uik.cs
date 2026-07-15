using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 初音小推车被动：种植后不可选且关闭 CarArmor；本行存在名字含「丰川祥子」的 plant 时，
/// 恢复可选与 CarArmor，朝其<strong>当时所在格</strong>移动，抵达后 <see cref="Chess.Death"/>。
/// </summary>
[Serializable]
public class PassiveSkill_Uik : ISkill
{
    const string SakiNameKey = "丰川祥子";

    Chess _user;
    CarArmor _carArmor;
    FindTileMethod_SeekMapPos _seekMethod;
    Vector2Int? _targetMapPos;
    bool _armed;
    UnityAction<Chess, Tile> _onReachTile;
    UnityAction<Chess> _onPlantChess;
    UnityAction<Chess> _onUserRemove;
    Coroutine _deferredActivate;

    public void InitSkill(Chess user)
    {
        _user = user;
        _carArmor = user != null ? user.GetComponentInChildren<CarArmor>() : null;
    }

    public void UseSkill(Chess user)
    {
        _user = user;
        if (_user == null)
            return;

        _carArmor ??= _user.GetComponentInChildren<CarArmor>();
        EnterWaitingState();
        ScheduleTryActivateFromExistingSakiOnRow();

        _onPlantChess = OnPlantChess;
        EventController.Instance.AddListener<Chess>(EventName.WhenPlantChess.ToString(), _onPlantChess);

        _onUserRemove = OnUserRemove;
        _user.OnRemove.AddListener(_onUserRemove);
    }

    public void LeaveSkill(Chess user) => Cleanup();

    public bool IfSkillReady(Chess user) => false;

    public void WhenEnter(Chess user) { }

    public bool IsSkillFinished(Chess user) => false;

    public void SkillOver(Chess user) { }

    public SkillConfig GetSkillConfig() => null;

    public void ReturnCD() { }

    public void WriteToSaveData(SkillStateSaveData data) { }

    public void RestoreFromSaveData(SkillStateSaveData data, Chess user) { }

    void EnterWaitingState()
    {
        _armed = false;
        _targetMapPos = null;
        _user.UnSelectable();
        SetCarArmorEnabled(false);
        UnregisterReachTile();
    }

    void ScheduleTryActivateFromExistingSakiOnRow()
    {
        StopDeferredActivate();
        _deferredActivate = _user.StartCoroutine(DeferredTryActivateFromExistingSakiOnRow());
    }

    IEnumerator DeferredTryActivateFromExistingSakiOnRow()
    {
        // skillController 先于 stateController 进战，需等 currentState 初始化后再切 MoveState
        yield return null;
        _deferredActivate = null;
        if (_user == null || _user.IfDeath)
            yield break;
        TryActivateFromExistingSakiOnRow();
    }

    void StopDeferredActivate()
    {
        if (_user != null && _deferredActivate != null)
        {
            _user.StopCoroutine(_deferredActivate);
            _deferredActivate = null;
        }
    }

    void TryActivateFromExistingSakiOnRow()
    {
        if (_armed)
            return;
        if (TryFindSakiTileOnSameRow(out Vector2Int targetPos))
            ActivateAndMoveTo(targetPos);
    }

    void OnPlantChess(Chess planted)
    {
        if (_user == null || _user.IfDeath || _armed || planted == null || planted == _user)
            return;
        if (!IsSaki(planted))
            return;
        if (!IsSameRow(_user, planted))
            return;

        Tile tile = planted.moveController?.standTile;
        if (tile == null)
            return;

        ActivateAndMoveTo(tile.mapPos);
    }

    void ActivateAndMoveTo(Vector2Int targetMapPos)
    {
        if (_user?.moveController == null || _user.stateController == null)
            return;

        _armed = true;
        _targetMapPos = targetMapPos;
        _user.ResumeSelectable();
        SetCarArmorEnabled(true);

        Tile stand = _user.moveController.standTile;
        if (stand != null && stand.mapPos == targetMapPos)
        {
            _user.Death();
            return;
        }

        _seekMethod ??= new FindTileMethod_SeekMapPos();
        _seekMethod.targetMapPos = targetMapPos;
        _user.moveController.tileMethod = _seekMethod;
        FaceTowardTarget(targetMapPos);

        UnregisterReachTile();
        _onReachTile = OnReachTile;
        if (_user.moveController.OnReachTile == null)
            _user.moveController.OnReachTile = new UnityEvent<Chess, Tile>();
        _user.moveController.OnReachTile.AddListener(_onReachTile);

        var state = _user.stateController.currentState.state;
        if (state == null || state.stateName != StateName.MoveState)
            _user.stateController.ChangeState(StateName.MoveState);
    }

    void FaceTowardTarget(Vector2Int targetMapPos)
    {
        Tile stand = _user?.moveController?.standTile;
        if (stand == null)
            return;
        _user.UpdateFacingFromHorizontalMove(new Vector2(targetMapPos.x - stand.mapPos.x, 0f));
    }

    void OnReachTile(Chess chess, Tile tile)
    {
        if (!_armed || !_targetMapPos.HasValue || tile == null)
            return;
        if (tile.mapPos == _targetMapPos.Value)
            _user.Death();
    }

    bool TryFindSakiTileOnSameRow(out Vector2Int targetPos)
    {
        targetPos = default;
        if (_user?.moveController?.standTile == null)
            return false;

        int rowY = _user.moveController.standTile.mapPos.y;
        var team = GameManage.instance?.chessTeamManage?.GetTeam(_user.tag);
        if (team == null)
            return false;

        for (int i = 0; i < team.Count; i++)
        {
            Chess c = team[i];
            if (c == null || c.IfDeath || c == _user || !IsSaki(c))
                continue;
            Tile tile = c.moveController?.standTile;
            if (tile == null || tile.mapPos.y != rowY)
                continue;

            targetPos = tile.mapPos;
            return true;
        }

        return false;
    }

    static bool IsSaki(Chess chess)
    {
        string name = chess?.propertyController?.creator?.chessName;
        return !string.IsNullOrEmpty(name) && name.Contains(SakiNameKey);
    }

    static bool IsSameRow(Chess a, Chess b)
    {
        var ta = a?.moveController?.standTile;
        var tb = b?.moveController?.standTile;
        return ta != null && tb != null && ta.mapPos.y == tb.mapPos.y;
    }

    void SetCarArmorEnabled(bool enabled)
    {
        if (_carArmor != null)
            _carArmor.collisionEnabled = enabled;
    }

    void UnregisterReachTile()
    {
        if (_user?.moveController?.OnReachTile != null && _onReachTile != null)
            _user.moveController.OnReachTile.RemoveListener(_onReachTile);
        _onReachTile = null;
    }

    void OnUserRemove(Chess _) => Cleanup();

    void Cleanup()
    {
        StopDeferredActivate();

        if (_onPlantChess != null)
        {
            EventController.Instance.RemoveListener<Chess>(EventName.WhenPlantChess.ToString(), _onPlantChess);
            _onPlantChess = null;
        }

        if (_user != null && _onUserRemove != null)
        {
            _user.OnRemove.RemoveListener(_onUserRemove);
            _onUserRemove = null;
        }

        UnregisterReachTile();
        _armed = false;
        _targetMapPos = null;
        _user = null;
    }
}
