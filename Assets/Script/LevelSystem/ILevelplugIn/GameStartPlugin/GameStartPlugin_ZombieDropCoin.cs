using UnityEngine;

/// <summary>
/// 给所有 Enemy 单位添加 Buff_DeathCoin，僵尸死亡时掉落金币。
/// 仅当商店已解锁时生效，Test 模式视为已解锁。
/// </summary>
public class GameStartPlugin_ZombieDropCoin : ILevelPlugin
{
    static Buff_DeathCoin deathCoinBuff;

    static Buff_DeathCoin GetBuff()
    {
        if (deathCoinBuff == null)
            deathCoinBuff = new Buff_DeathCoin();
        return deathCoinBuff;
    }

    static bool IsShopUnlocked()
    {
        if (GameManage.instance == null) return false;
        if (GameManage.instance.mode == GameMode.Test) return true;
        return PlayerSaveContext.CurrentData?.shopUnlocked == true;
    }

    public void StadgeEffect(LevelController levelController)
    {
        if (!IsShopUnlocked()) return;
        EventController.Instance.AddListener<Chess>(EventName.WhenChessEnterWar.ToString(), OnEnemyEnterWar);
        EventController.Instance.AddListener(EventName.WhenLeaveLevel.ToString(), OnLeaveLevel);
    }

    void OnEnemyEnterWar(Chess chess)
    {
        if (chess != null && chess.CompareTag("Enemy"))
            chess.buffController.AddBuff(GetBuff());
    }

    void OnLeaveLevel()
    {
        EventController.Instance.RemoveListener<Chess>(EventName.WhenChessEnterWar.ToString(), OnEnemyEnterWar);
        EventController.Instance.RemoveListener(EventName.WhenLeaveLevel.ToString(), OnLeaveLevel);
    }

    public void OverPlugin(LevelController levelController)
    {
        OnLeaveLevel();
    }
}
