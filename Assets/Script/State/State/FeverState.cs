using UnityEngine;

/// <summary>
/// AveMujica 羁绊 Fever；状态图边与迁移由预制体上的 <see cref="StateGraph"/> 自行配置。
/// </summary>
public class FeverState : State
{
    [Tooltip("进入 Fever 时在棋子身上生成的特效预制体（需在 ObjectPool 注册）；Exit 时回收")]
    public GameObject feverEffect;

    [System.NonSerialized]
    GameObject _spawnedEffect;

    public FeverState()
    {
        stateName = StateName.FeverState;
    }

    public override void Enter(Chess chess)
    {
        base.Enter(chess);
        if (chess.animatorController != null && chess.animatorController.animator != null)
            chess.animatorController.animator.Play("fever");

        SpawnFeverEffect(chess);
    }

    public override void Exit(Chess chess)
    {
        RecycleFeverEffect();
        base.Exit(chess);
        if (chess?.stateController?.preState?.state != null &&
            chess.stateController.preState.state.stateName == StateName.SkillState)
        {
            chess.skillController.activeSkill?.ReturnCD();
        }
    }

    void SpawnFeverEffect(Chess chess)
    {
        RecycleFeverEffect();
        if (feverEffect == null || chess == null || ObjectPool.instance == null)
            return;

        _spawnedEffect = ObjectPool.instance.Create(feverEffect);
        if (_spawnedEffect == null)
            return;

        _spawnedEffect.transform.SetParent(chess.transform);
        _spawnedEffect.transform.localPosition = Vector3.zero;
    }

    void RecycleFeverEffect()
    {
        if (_spawnedEffect == null)
            return;
        if (ObjectPool.instance != null)
            ObjectPool.instance.Recycle(_spawnedEffect);
        else
            Object.Destroy(_spawnedEffect);
        _spawnedEffect = null;
    }

    public override State Clone()
    {
        var ans = new FeverState();
        ans.stateName = stateName;
        ans.feverEffect = feverEffect;
        return ans;
    }
}
