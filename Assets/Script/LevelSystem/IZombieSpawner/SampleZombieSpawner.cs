using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 我怎么看都是要用到update 而不是简单的计时器就能解决的问题...
/// </summary>
public class SampleZombieSpawner : IZombieSpawner
{
    public List<PropertyCreator> zombies;
    public ZombiesWave zombiesWave;
    public float zombieInterval = 0.5f;//每波僵尸 每只僵尸的间隔时间
    public float updateTime = 0.1f;//刷新时间
    public float startTime = -10;
    public List<Chess> liveZombies;
    Timer  waveTimer;
    float t, zt;
    bool win;
    public bool CheckWinCondition()
    {
        return win;
    }

    public void Prepare(LevelData levelData)
    {
        Debug.Log("准备阶段");
        t = startTime;zt = 0;
        liveZombies = ChessTeamManage.Instance.GetTeam("Enemy");
        zombiesWave.InitWave(this);
        for (int i = 0; i < zombies.Count; i++)
        {
            Chess zombie = ChessTeamManage.Instance.CreateChess(zombies[i], (MapManage.instance as MapManage_PVZ).zombiePreTile[i],"Enemy");
            Debug.Log("create"+zombie.name);
            //liveZombies.Add(zombie);
        }
        
    }
    /// <summary>
    /// 这个函数才是最重要的
    /// </summary>
    /// <param name="levelData"></param>
    public void StartSpawning(LevelData levelData)
    {
        List<Chess> chesses = new List<Chess>(liveZombies);
        for (int i = 0; i < chesses.Count; i++)
        {
            //Debug.Log(liveZombies[i].name);
            chesses[i].Death();
        }
        liveZombies.Clear();
        UIManage.Show<ProgressBar>();
        UIManage.Show<TextPanel>();
        GameManage.instance.timerManage.AddTimer(
            UpdateGameStage, updateTime, true);
    }
    public void OverSpawning(LevelData levelData)
    {
        UIManage.Close<TextPanel>();
        UIManage.Close<ProgressBar>();
        waveTimer.Stop();
    }
    public void CreateZombie()
    {
        Tile tile = MapManage.instance.preTiles[UnityEngine.Random.Range(0, MapManage.instance.preTiles.Count)];
        Chess c = zombiesWave.CreateChess(tile);
    }
    public  void UpdateGameStage()
    {
        if (zombiesWave.CurrentPrice > 0)
        {
            zt += updateTime;
            if (zt > zombieInterval)
            {
                zt = 0;
                CreateZombie();
            }
        }//上面一部分是生成僵尸的过程，zt是每只僵尸的间隔时间，这样就不会堆在一起了
        t += updateTime;
        //Debug.Log(t+" "+Time.time);
        if (t > zombiesWave.minTime)
        {
            if ((t > zombiesWave.maxTime && zombiesWave.currentWave < zombiesWave.LastWave) || liveZombies.Count == 0)
            {
                Debug.Log("进入下一波");
                t = 0;
                zombiesWave.EnterNextWave();
                if (liveZombies.Count == 0) Debug.Log("死完了");
                for (int i = 0; i < zombies.Count; i++)
                {
                    if (zombies[i].baseProperty.price <= zombiesWave.CurrentPrice &&
                    !zombiesWave.WaveZombie.Contains(zombies[i]))
                    {
                        zombiesWave.AddNewZombie(zombies[i]);
                    }
                }
                zt = zombieInterval + 1;
            }

        }
    }

    
}

[Serializable]
public class ZombiesWave
{
    public List<PropertyCreator> WaveZombie;
    public List<float> fateList;
    public float waitTime = 6;
    public Vector2 minTimeRange;
    public Vector2 maxTimeRange;
    public int waveIndex;//每间隔x波 翻一倍
    public int waveAdditionPrice;//每x波的增加金钱数量
    public int LastWave;
    public int currentWave;//当前波数
    public int CurrentPrice;
    public string tag = "Enemy";
    [HideInInspector] public float minTime;
    [HideInInspector] public float maxTime;
    float t;
    IZombieSpawner spawner;

    public void InitWave(IZombieSpawner spawner)
    {
        currentWave = 0;
        CurrentPrice = 0;
        minTime = UnityEngine.Random.Range(minTimeRange.x, minTimeRange.y);
        maxTime = UnityEngine.Random.Range(maxTimeRange.x, maxTimeRange.y);
        Debug.Log(minTime + " " + maxTime);
        WaveZombie.Clear();
        fateList.Clear();
        UIManage.GetView<ProgressBar>().SetFlag(LastWave / 10);
        this.spawner = spawner;
    }

    public void EnterNextWave()
    {
        currentWave++;
        UIManage.GetView<ProgressBar>().MoveBar(currentWave, LastWave);
        if (currentWave > LastWave)
        {
            spawner.OverSpawning(null);
        }
        else if (currentWave == 1)
        {
            CurrentPrice = ((currentWave / waveIndex) + 1) * waveAdditionPrice;
            UIManage.GetView<TextPanel>().FirstZombieCom();
            Debug.Log("first");
            EventController.Instance.TriggerEvent(EventName.FirstZombieComming.ToString());
        }
        else if (currentWave % 10 == 0)
        {
            Debug.Log("一大波僵尸即将来袭");
            EventController.Instance.TriggerEvent(EventName.WaveZombieComming.ToString());
            GameManage.instance.StartCoroutine(Wait());
            UIManage.GetView<TextPanel>().ZombieWave();
        }
        else
        {
            CurrentPrice = ((currentWave / waveIndex) + 1) * waveAdditionPrice;
        }
    }
    IEnumerator Wait()
    {
        yield return new WaitForSeconds(waitTime);
        CurrentPrice = ((currentWave / waveIndex) + 1) * waveAdditionPrice;
        CurrentPrice = (int)(CurrentPrice * 2.5f);
        if (currentWave == LastWave)
        {
            EventController.Instance.TriggerEvent(EventName.LastWaveZombie.ToString());
            UIManage.GetView<TextPanel>().LastWave();
        }
    }
    public void AddNewZombie(PropertyCreator zombie)
    {
        if (!WaveZombie.Contains(zombie))
        {
            WaveZombie.Add(zombie);
            fateList.Add(0);
            int raritySum = 0;
            for (int i = 0; i < WaveZombie.Count; i++)
            {
                raritySum += WaveZombie[i].baseProperty.rarity;
            }
            fateList[0] = (float)WaveZombie[0].baseProperty.rarity / raritySum;
            for (int i = 1; i < fateList.Count; i++)
            {
                fateList[i] = fateList[i - 1] + (float)WaveZombie[i].baseProperty.rarity / raritySum;
            }
        }
    }
    public Chess CreateChess(Tile standTile)
    {
        float fate = UnityEngine.Random.Range(0, 1f);
        //Debug.Log(fate);
        Chess c = WaveZombie[0].chessPre;
        //Debug.Log(fateList.Count);
        for (int i = 0; i < fateList.Count; i++)
        {
            if (fate < fateList[i])
            {
                c = WaveZombie[i].chessPre;
                CurrentPrice -= WaveZombie[i].baseProperty.price;
                break;
            }
        }
        //Debug.Log(c.propertyController.creator.chessName);
        return ChessTeamManage.Instance.CreateChess(c.propertyController.creator, standTile,tag);
    }
}

