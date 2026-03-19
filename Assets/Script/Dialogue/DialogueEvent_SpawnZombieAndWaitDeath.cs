using System;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 监听 SpawnZombie 生成僵尸，僵尸死亡时触发 ZombieDead。
/// 用于对话「点击后隐藏 → 消灭僵尸 → 显示下一句」流程。
/// </summary>
[Serializable]
public class DialogueEvent_SpawnZombieAndWaitDeath : IDialogueEvent
{
    [Tooltip("要生成的僵尸")]
    public PropertyCreator zombieCreator;
    [Tooltip("收到此事件时生成僵尸")]
    public string spawnEventName = "SpawnZombie";
    [Tooltip("僵尸死亡时触发此事件")]
    public string deathEventName = "ZombieDead";

    LevelController levelController;
    Chess spawnedZombie;
    UnityAction<Chess> onZombieDeath;
    UnityEngine.Events.UnityAction onSpawnZombie;

    public void Register(LevelController levelController)
    {
        this.levelController = levelController;
        onSpawnZombie = OnSpawnZombie;
        onZombieDeath = OnZombieDeath;
        EventController.Instance.AddListener(spawnEventName, onSpawnZombie);
    }

    public void Unregister()
    {
        EventController.Instance.RemoveListener(spawnEventName, onSpawnZombie);
        if (spawnedZombie != null && !spawnedZombie.IfDeath && onZombieDeath != null)
        {
            spawnedZombie.DeathEvent?.RemoveListener(onZombieDeath);
        }
        spawnedZombie = null;
        levelController = null;
    }

    void OnSpawnZombie()
    {
        if (zombieCreator == null) return;

        var mapPvz = MapManage.instance as MapManage_PVZ;
        if (mapPvz == null || mapPvz.preTiles == null || mapPvz.preTiles.Count == 0) return;

        var tiles = mapPvz.preTiles;
        var standTile = tiles[UnityEngine.Random.Range(0, tiles.Count)];
        spawnedZombie = ChessTeamManage.Instance.CreateChess(zombieCreator, standTile, "Enemy");
        if (spawnedZombie != null)
        {
            float dx = UnityEngine.Random.Range(0f, 3.75f);
            spawnedZombie.transform.position = standTile.transform.position + Vector3.right * dx;
            spawnedZombie.WhenChessEnterWar();
            spawnedZombie.DeathEvent.AddListener(onZombieDeath);
        }
    }

    void OnZombieDeath(Chess c)
    {
        if (c != spawnedZombie) return;
        spawnedZombie = null;
        EventController.Instance.TriggerEvent(deathEventName);
    }
}
