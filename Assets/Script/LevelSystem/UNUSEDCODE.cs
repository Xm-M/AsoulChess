//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Runtime.ConstrainedExecution;
//using UnityEngine;

///// <summary>
///// 暂时没想好怎么改 真的很复杂啊
///// 我怎么看都是要用到update 而不是简单的计时器就能解决的问题...
///// 每个LevelData应该都有一个Clear方法 这个Clear方法应该调用每个Spwner的Clear函数
///// 这个Clear函数我还没写
///// 关于下一关：如果是冒险模式就进入下一关；如果是小游戏胜利就返回小游戏菜单其他模式同理
///// 有限制出怪：某种僵尸可能出现的波数是固定的 或者说 大部分僵尸都只能在一大波之后出现 
///// 无限制出怪：可能第一只僵尸就是巨人了
///// 一类出怪：（int((x-1)/3)+n）*t 
///// 二类出怪：(int((x-1)*2/5)+n)*t
///// 其中 x表示波数 n表示初始绿（第一波出现怪的值） t表示出怪倍率
///// 因为我有设定僵尸的阳光 所以公式算出来的值*25=僵尸的每波阳光
///// 
///// 我应该把出怪逻辑和具体数据剥离开来 然后插件可以一个个加进去 应该是这样的 
///// 僵尸怎么出 这个函数应该是放在 具体的类里面的
///// </summary>
//public class SampleZombieSpawner : IZombieSpawner
//{
//    public List<PropertyCreator> zombies;
//    public ZombiesWave zombiesWave;
//    public float zombieInterval = 0.5f;//每波僵尸 每只僵尸的间隔时间
//    public float updateTime = 0.1f;//刷新时间
//    public float startTime = -10;
//    public List<Chess> liveZombies;
//    Timer  waveTimer;
//    float t, zt;
//    bool win;
//    Vector3 lastPos;
//    public LevelData nextLevelData;
//    public bool CheckWinCondition()
//    {
//        return win;
//    }

//    public void Prepare(LevelData levelData)
//    {
//        Debug.Log("准备阶段");
//        win = false;
//        //但我说实话 所有的初始化阶段是不是都可以在Prepare阶段就做好
//        t = startTime;zt = 0;
//        liveZombies = ChessTeamManage.Instance.GetTeam("Enemy");
//        zombiesWave.InitWave(this);
//        for (int i = 0; i < zombies.Count; i++)
//        {
//            Chess zombie = ChessTeamManage.Instance.CreateChess(zombies[i], (MapManage.instance as MapManage_PVZ).zombiePreTile[i],"Enemy");
//        }
//        for (int i = 0; i < (MapManage_PVZ.instance as MapManage_PVZ).roomTile.Count; i++)
//        {
//            Chess cars = ChessTeamManage.Instance.CreateChess((MapManage_PVZ.instance as MapManage_PVZ).car,
//                 (MapManage_PVZ.instance as MapManage_PVZ).roomTile[i], "Player");
//            cars.gameObject.layer = 11;
//            //Debug.Log(cars.gameObject.layer);
//            //Debug.Log(LayerMask.LayerToName(cars.gameObject.layer));
//        }
//    }
//    /// <summary>
//    /// 这个函数才是最重要的
//    /// </summary>
//    /// <param name="levelData"></param>
//    public void StartSpawning(LevelData levelData)
//    {
        
//        List<Chess> chesses = new List<Chess>(liveZombies);
//        for (int i = 0; i < chesses.Count; i++)
//        {
//            //Debug.Log(liveZombies[i].name);
//            chesses[i].Death();
//        }
//        liveZombies.Clear();
//        UIManage.Show<ProgressBar>();
//        UIManage.Show<TextPanel>();
//        UIManage.GetView<TextPanel>().GameStart();
//        waveTimer = GameManage.instance.timerManage.AddTimer(
//            ()=>UpdateGameStage(levelData), updateTime, true);
//        //EventController.Instance.AddListener<Chess>(EventName.WhenDeath.ToString(), CheckLastZombie);
//    }
//    public void OverSpawning(LevelData levelData)
//    {
        
//        UIManage.Close<ProgressBar>();
//        //((MapManage_PVZ.instance) as MapManage_PVZ).au.SetLoop(false);
//        if (levelData.win)
//        {
//            //Debug.Log("游戏胜利");
//            //((MapManage_PVZ.instance) as MapManage_PVZ).au.PlayAudio("游戏胜利");
//            if(nextLevelData!=null)
//                LevelManage.instance.ChangeLevel(nextLevelData);
//            else
//            {
//                LevelManage.instance.ReturnMenu();
//            }
//        }
//        else
//        {
//            //((MapManage_PVZ.instance) as MapManage_PVZ).au.PlayAudio("游戏失败");
//            Debug.Log("游戏失败");
//            UIManage.GetView<TextPanel>().GameOver();
//        }
//        waveTimer?.Stop();
//    }
//    public void CreateZombie()
//    {
//        Tile tile = MapManage.instance.preTiles[UnityEngine.Random.Range(0, MapManage.instance.preTiles.Count)];
//        Chess c = zombiesWave.CreateChess(tile);
//    }
//    public  void UpdateGameStage(LevelData levelData)
//    {
//        if (zombiesWave.CurrentPrice > 0)
//        {
//            zt += updateTime;
//            if (zt > zombieInterval)
//            {
//                zt = 0;
//                CreateZombie();
//            }
//        }//上面一部分是生成僵尸的过程，zt是每只僵尸的间隔时间，这样就不会堆在一起了
//        t += updateTime;
//        //Debug.Log(t+" "+Time.time);
//        if (t > zombiesWave.minTime)
//        {
//            if ((t > zombiesWave.maxTime && zombiesWave.currentWave < zombiesWave.LastWave) || liveZombies.Count == 0)
//            {
//                Debug.Log("进入下一波");
//                t = 0;
//                zombiesWave.EnterNextWave(levelData);
//                if (liveZombies.Count == 0&&levelData.win)
//                {
//                    Debug.Log("死完了");
//                    if (nextLevelData != null)
//                        Debug.Log(nextLevelData.levelName);
//                    Item_Reward reward = UIManage.GetView<ItemPanel>().Create<Item_Reward>();
//                    reward.SetRewardPos(lastPos);
//                    win = true;
//                }
//                for (int i = 0; i < zombies.Count; i++)
//                {
//                    if (zombies[i].baseProperty.price <= zombiesWave.CurrentPrice &&
//                    !zombiesWave.WaveZombie.Contains(zombies[i]))
//                    {
//                        zombiesWave.AddNewZombie(zombies[i]);
//                    }
//                }
//                zt = zombieInterval + 1;
//            }
//            else
//            {
//                lastPos = liveZombies[0].transform.position;
//            }
//        }
//    }
//    public void CheckLastZombie(Chess chess)
//    {
//        if (zombiesWave.currentWave >= zombiesWave.LastWave&& liveZombies.Count == 0&&!win)
//        {
//            if(nextLevelData!=null)
//                Debug.Log(nextLevelData.levelName);
//            Item_Reward reward = UIManage.GetView<ItemPanel>().Create<Item_Reward>();
//            reward.SetRewardPos(chess.transform.position);
//            win = true;
//        }
//    }

//    public void LeaveSpawning(LevelData levelData)
//    {
//        Debug.Log("离开房间"+levelData.levelName);
//        waveTimer?.Stop();
//        waveTimer = null;
//        //EventController.Instance.RemoveListener<Chess>(EventName.WhenDeath.ToString(), CheckLastZombie);
//    }
//}

//[Serializable]
//public class ZombiesWave
//{
//    public List<PropertyCreator> WaveZombie;
//    public List<float> fateList;
//    public float waitTime = 6;
//    public Vector2 minTimeRange;
//    public Vector2 maxTimeRange;
//    public int waveIndex;//每间隔x波 翻一倍
//    public int waveAdditionPrice;//每x波的增加金钱数量
//    public int LastWave;
//    public int currentWave;//当前波数
//    public int CurrentPrice;
//    public string tag = "Enemy";
//    [HideInInspector] public float minTime;
//    [HideInInspector] public float maxTime;
//    float t;
//    IZombieSpawner spawner;

//    public void InitWave(IZombieSpawner spawner)
//    {
//        currentWave = 0;
//        CurrentPrice = 0;
//        minTime = UnityEngine.Random.Range(minTimeRange.x, minTimeRange.y);
//        maxTime = UnityEngine.Random.Range(maxTimeRange.x, maxTimeRange.y);
//        //Debug.Log(minTime + " " + maxTime);
//        WaveZombie.Clear();
//        fateList.Clear();
//        UIManage.GetView<ProgressBar>().SetFlag(LastWave / 10);
//        this.spawner = spawner;
//    }

//    public void EnterNextWave(LevelData levelData)
//    {
//        currentWave++;
//        UIManage.GetView<ProgressBar>().MoveBar(currentWave, LastWave);
//        if (currentWave > LastWave)
//        {
//            levelData.win = true;
//        }
//        else if (currentWave == 1)
//        {
//            CurrentPrice = ((currentWave / waveIndex) + 1) * waveAdditionPrice;
//            UIManage.GetView<TextPanel>().FirstZombieCom();
//            Debug.Log("first");
//            EventController.Instance.TriggerEvent(EventName.FirstZombieComming.ToString());
//        }
//        else if (currentWave % 10 == 0)
//        {
//            Debug.Log("一大波僵尸即将来袭");
//            EventController.Instance.TriggerEvent(EventName.WaveZombieComming.ToString());
//            GameManage.instance.StartCoroutine(Wait());
//            UIManage.GetView<TextPanel>().ZombieWave();
//        }
//        else
//        {
//            CurrentPrice = ((currentWave / waveIndex) + 1) * waveAdditionPrice;
//        }
//    }
//    IEnumerator Wait()
//    {
//        yield return new WaitForSeconds(waitTime);
//        CurrentPrice = ((currentWave / waveIndex) + 1) * waveAdditionPrice;
//        CurrentPrice = (int)(CurrentPrice * 2.5f);
//        if (currentWave == LastWave)
//        {
//            EventController.Instance.TriggerEvent(EventName.LastWaveZombie.ToString());
//            UIManage.GetView<TextPanel>().LastWave();
//        }
//    }
//    public void AddNewZombie(PropertyCreator zombie)
//    {
//        if (!WaveZombie.Contains(zombie))
//        {
//            WaveZombie.Add(zombie);
//            fateList.Add(0);
//            int raritySum = 0;
//            for (int i = 0; i < WaveZombie.Count; i++)
//            {
//                raritySum += WaveZombie[i].baseProperty.rarity;
//            }
//            fateList[0] = (float)WaveZombie[0].baseProperty.rarity / raritySum;
//            for (int i = 1; i < fateList.Count; i++)
//            {
//                fateList[i] = fateList[i - 1] + (float)WaveZombie[i].baseProperty.rarity / raritySum;
//            }
//        }
//    }
//    public Chess CreateChess(Tile standTile)
//    {
//        float fate = UnityEngine.Random.Range(0, 1f);
//        //Debug.Log(fate); 
//        Chess c = WaveZombie[0].GetPre();
//        //Debug.Log(fateList.Count);
//        for (int i = 0; i < fateList.Count; i++)
//        {
//            if (fate < fateList[i])
//            {
//                c = WaveZombie[i].GetPre();
//                CurrentPrice -= WaveZombie[i].baseProperty.price;
//                break;
//            }
//        }
//        //Debug.Log(c.propertyController.creator.chessName);
//        return ChessTeamManage.Instance.CreateChess(c.propertyController.creator, standTile,tag);
//    }
//}

