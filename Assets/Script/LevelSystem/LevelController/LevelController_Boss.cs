using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// Boss 关：<see cref="LevelData.zombieList"/>[0] 为 Boss；GameStart 生成于最右列中间格；
/// 进度条显示 Boss 血量；无波次/右侧预览/开局文案；胜负与普关一致（Boss 死亡胜利，进家失败）。
/// </summary>
public class LevelController_Boss : LevelController
{
    [ShowInInspector, ReadOnly]
    [ShowIf("@UnityEngine.Application.isPlaying")]
    Chess boss;

    bool victoryHandled;

    public override void CreateZombieWaves()
    {
        ClearZombiePreviews();
        if (waveDatas == null)
            waveDatas = new List<WaveData>();
        else
            waveDatas.Clear();

        t = 0;
        mintime = 16;
        maxtime = 30;
        currentWave = levelData != null ? levelData.MaxWave : 0;
    }

    public override void GameStart()
    {
        if (levelData == null)
        {
            Debug.LogError("[LevelController_Boss] 没有关卡数据");
            return;
        }

        for (int i = 0; i < zombies.Count; i++)
            zombies[i].Death();

        if (levelData.GameStartPlugin != null)
        {
            for (int i = 0; i < levelData.GameStartPlugin.Count; i++)
                levelData.GameStartPlugin[i].StadgeEffect(this);
        }

        LevelManage.instance.GameStart();
        SpawnBoss();
    }

    void SpawnBoss()
    {
        victoryHandled = false;
        if (levelData.zombieList == null || levelData.zombieList.Count == 0)
        {
            Debug.LogError("[LevelController_Boss] zombieList[0] 未配置 Boss");
            return;
        }

        PropertyCreator creator = levelData.zombieList[0];
        Vector2Int size = MapManage.instance.mapSize;
        int x = size.x - 1;
        int y = size.y / 2;
        if (!MapManage.instance.IfInMapRange(x, y))
        {
            Debug.LogError($"[LevelController_Boss] Boss 出生格越界 ({x},{y})");
            return;
        }

        Tile tile = MapManage.instance.tiles[x, y];
        boss = ChessTeamManage.Instance.CreateChess(creator, tile, "Enemy");
        if (boss == null) return;

        UIManage.Show<ProgressBar>();
        var bar = UIManage.GetView<ProgressBar>();
        bar.ShowBossHp(boss.propertyController.GetHp(), boss.propertyController.GetMaxHp());
    }

    protected override void Update()
    {
        if (!LevelManage.instance.IfGameStart || boss == null || victoryHandled)
            return;

        if (boss.IfDeath)
        {
            OnBossDefeated();
            return;
        }

        float hp = boss.propertyController.GetHp();
        UIManage.GetView<ProgressBar>().UpdateBossHp(hp, boss.propertyController.GetMaxHp());
        if (hp <= 0f)
            OnBossDefeated();
    }

    void OnBossDefeated()
    {
        if (victoryHandled) return;
        victoryHandled = true;
        Vector3 pos = boss != null ? boss.transform.position : Vector3.zero;
        SaveSystem.DeleteSave(LevelManage.instance.currentLevel);
        (levelData.outcome ?? new LevelOutCome_Trophy()).HandleOutcome(true, pos);
    }

    public override void GameOver(bool win)
    {
        boss = null;
        victoryHandled = false;
        base.GameOver(win);
    }
}
