using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UIElements.UxmlAttributeDescription;

public class LevelController_HammerZombie : LevelController
{
    public PropertyCreator stone;
    [ShowInInspector, ReadOnly]
    [ShowIf("@UnityEngine.Application.isPlaying")]
    List<Chess> stones;
    [SerializeReference]
    public Buff_BaseValueBuff_AttackSpeed speedBuff;
    [SerializeReference]
    public Buff_DeathSunLight sunLightBuff;
    Timer timer;
    #region 这里的都不用改
    /// <summary>
    /// 这个不用改
    /// </summary>
    /// <param name="levelData"></param>
    public override void Init(LevelData levelData)
    {
        base.Init(levelData);
        
    }
    protected override void OnEnable()
    {
        base.OnEnable();
        if (zombies == null)
            zombies = new List<Chess>();
        if (stones == null) stones = new List<Chess>();
    }
    /// <summary>
    /// 这个也不用改
    /// </summary>
    public override void EnterMap()
    {
        base.EnterMap();
        maxtime = 21;
    }
    public void CreateStone(int n)
    {
        List<Vector2Int> random = GetRandomPositions(MapManage.instance.mapSize.x - 1, MapManage.instance.mapSize.y - 1, n);
        for (int i = 0; i < random.Count; i++)
        {
            Chess c = ChessTeamManage.Instance.CreateChess(stone, MapManage.instance.tiles[random[i].x, random[i].y], "Enemy");
            MapManage.instance.tiles[random[i].x, random[i].y].PlantChess(c);
            stones.Add(c);
            c.OnRemove.AddListener((chess) => stones.Remove(chess));
        }
    }

    public override void GamePrepare()
    {
        base.GamePrepare();
    }
    public override void GameOver(bool win)
    {
        base.GameOver(win);
    }
    public override void GameStart()
    {
        base.GameStart();
        CreateStone(9);
    }
    //这几个好像都用不改 都在插件里面搞就行
    #endregion
   
    /// <summary>
    /// 就这两个是最需要改的
    /// </summary>
    public override void CreateZombieWaves()
    {
        //base.CreateZombieWaves();
        if (zombies == null)
            zombies = new List<Chess>();
        else zombies.Clear();
        if (levelData == null)
        {
            Debug.LogError("没有关卡数据");
        }
        //在调用完所有准备阶段前的插件后，生成僵尸列表的所有数据 这里好像也不用改
        if (waveDatas == null)
            waveDatas = new List<WaveData>();
        else waveDatas.Clear();
        for (int i = 0; i < levelData.MaxWave; i++)
        {
           var waveData = new HammerZombieWave();
            waveData.level = this;
            waveData.InitWave(i + 1, levelData);
            waveDatas.Add(waveData);
        }
        //在MapManage的右侧生成所有可能出现的僵尸 小推车放在小推车插件里
        for (int i = 0; i < levelData.zombieList.Count; i++)
        {
            Chess zombie = ChessTeamManage.Instance.CreateChess(levelData.zombieList[i], (MapManage.instance as MapManage_PVZ).zombiePreTile[i], "Enemy");
            zombies.Add(zombie);
        }
        t = 0;
        currentWave = -1;
    }
    protected override void Update()
    {
        if (LevelManage.instance.IfGameStart && currentWave < levelData.MaxWave)
        {
            t += Time.deltaTime;
            if (currentWave == -1 && t > mintime)
            {
                waveDatas[currentWave + 1].EnterWave();
                currentWave++;
                t = 0;
                UIManage.Show<ProgressBar>();//进度条(其实也可以在第一只僵尸出来的时候刷)
                UIManage.GetView<ProgressBar>().SetFlag(levelData.MaxWave / 10);
            }
            else if (currentWave != -1 && (waveDatas[currentWave].CheckZombieHp() && t> maxtime && currentWave < levelData.MaxWave))
            {
                t = 0;
                UIManage.GetView<ProgressBar>().MoveBar(currentWave + 1, levelData.MaxWave);
                if (currentWave + 1 < levelData.MaxWave)
                {
                    waveDatas[currentWave + 1].EnterWave();
                    currentWave++;
                }
                //每一波都要检查墓碑数量 小于5则补满5 大于5则只加一
                if (stones.Count < 5)
                {
                    CreateStone(5-stones.Count);
                }
                else {
                    CreateStone(1);
                }
            }
        }
    }

    public static List<Vector2Int> GetRandomPositions(int width, int height, int n)
    {
        List<Vector2Int> all = new List<Vector2Int>();

        // 生成全部坐标
        for (int x = 2; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if(MapManage.instance.tiles[x, y].stander == null|| MapManage.instance.tiles[x, y].stander.CompareTag("Player"))
                    all.Add(new Vector2Int(x, y));
            }
        }

        // 打乱
        for (int i = 0; i < all.Count; i++)
        {
            int rand = Random.Range(i, all.Count);
            (all[i], all[rand]) = (all[rand], all[i]);
        }

        // 取前 n 个
        n = Mathf.Min(n, all.Count);
        return all.GetRange(0, n);
    }


    //**************************************************************************************************************************
    /// <summary>
    /// 锤僵尸专用的WaveData
    /// </summary>
    public class HammerZombieWave:WaveData
    {
        //public PropertyCreator gravestone;
        public LevelController_HammerZombie level;
        public float interval;//每小波的间隔时间 
        public List<PropertyCreator> creators;
        Timer timer;
        int current;
        [ShowInInspector, ReadOnly]
        [ShowIf("@UnityEngine.Application.isPlaying")]
        int waveNum;
        public override void InitWave(int wave, LevelData data)
        {
            int raritySum = 0;
            this.wave = wave;
            outcome = data.outcome;
            liveZombie = new List<Chess>();
            if (zombieList == null)
            {
                zombieList = new List<ZombieInWaveData>();
                fateList = new List<float>();
            }
            else
            {
                zombieList.Clear();
                fateList.Clear();
            }
            creators=new List<PropertyCreator>();
            for (int i = 0; i < data.zombieList.Count; i++)
            {
                if (data.zombieList[i].baseProperty.waveLimit <= wave)
                {
                    creators.Add(data.zombieList[i]);
                    raritySum += data.zombieList[i].baseProperty.rarity;
                }
            }
            //那么时间上 每小波的时间在(1.2~1.6 总波长是18s) 
            Debug.Log("筛选僵尸完毕");
            //这个还是僵尸的概率
            fateList.Add((float)(creators[0].baseProperty.rarity) / raritySum);
            for (int i = 1; i < creators.Count; i++)
            {
                fateList.Add(fateList[i - 1] + (float)(creators[i].baseProperty.rarity) / raritySum);
            }
            //这个是每大波含有n小波
            waveNum = (Random.Range(11, 16));
            interval = 18f / (waveNum);
            //生成每小波的僵尸
            for(int i = 0; i < waveNum;i++)
            {
                float fate = UnityEngine.Random.Range(0, 1f);
                for(int j = 0;j<fateList.Count;j++)
                {
                    ZombieInWaveData zombieInWaveData = new ZombieInWaveData();
                    if (fate < fateList[j])
                    {
                        zombieList.Add(zombieInWaveData);
                        zombieInWaveData.zombieCreate=creators[j];
                        break;
                    }
                }
                float k = Random.Range(0, 1f);
                //Debug.Log(wave+""+ k);
                if (wave > 8 && k < 0.2f)
                {
                    zombieList[i].zombieNum = 3;
                }
                else if (wave > 3 && k < 0.4)
                {
                    zombieList[i].zombieNum = 2;
                }
                else
                {
                    zombieList[i].zombieNum = 1;
                }
            }
            //Debug.Log(string.Format("第{0}波僵尸已经生成完毕,共生成{1}只僵尸", wave, num));
        }
        public override void EnterWave()
        {
            if(wave != LevelManage.instance.currentLevel.MaxWave)
                timer = GameManage.instance.timerManage.AddTimer(CreateZombie,interval,true);
            else
            {
                EventController.Instance.TriggerEvent(EventName.WaveZombieComming.ToString());
                UIManage.GetView<TextPanel>().LastWave() ;
                timer = GameManage.instance.timerManage.AddTimer(CreateZombie, 4);
            }
            
        }
        public void CreateZombie()
        {
            
            if (wave != LevelManage.instance.currentLevel.MaxWave)
            {
                if (current < waveNum)
                {
                    for (int i = 0; i < zombieList[current].zombieNum; i++)
                    {
                       Chess c=  CreateChess(zombieList[current].zombieCreate);
                        float x = Random.Range(0, 1f);
                        if (x <= 0.2f)
                        {
                            Debug.Log("生成阳光"+c.name);
                            c.buffController.AddBuff(level.sunLightBuff);
                        }
                    }
                    current++;
                }
                else
                {
                    timer.Stop();
                    timer = null;
                }
            }
            else
            {
                CreateChess(zombieList[0].zombieCreate);
            }
        }
        public override Chess CreateChess(PropertyCreator creator)
        {
            if (wave != LevelManage.instance.currentLevel.MaxWave)
            {
                int n = UnityEngine.Random.Range(0, level.stones.Count);
                Tile standTile = level.stones[n].moveController.standTile;
                Chess chess = ChessTeamManage.Instance.CreateChess(creator, standTile, "Enemy");
                chess.buffController.AddBuff(level.speedBuff);
                //Debug.Log(chess.name);
                return chess;
            }
            else
            {
                
                for(int i = 0;i< level.stones.Count; i++)
                {
                    creator = creators[Random.Range(0, creators.Count)];
                    Tile standTile = level.stones[i].moveController.standTile;
                    Chess chess = ChessTeamManage.Instance.CreateChess(creator, standTile, "Enemy");
                    chess.buffController.AddBuff(level.speedBuff);
                    liveZombie.Add(chess);
                }
                current = 1;
            }
            return null;
        }
        //锤僵尸的check就是单纯的计时了
        public override bool CheckZombieHp()
        {

            if (wave == LevelManage.instance.currentLevel.MaxWave&&current==1)
            {
                for (int i = liveZombie.Count - 1; i >= 0; i--)
                {
                    if (liveZombie[i].IfDeath)
                    {
                        liveZombie.RemoveAt(i);
                    }
                }
                if (liveZombie.Count == 0)
                {
                    if (outcome == null)
                    {
                        Item_Reward reward = UIManage.GetView<ItemPanel>().Create<Item_Reward>();
                        reward.SetRewardPos(level.stones[0].transform.position);
                    }
                    else
                    {
                        outcome.HandleOutcome(true);
                    }
                    current = 0;
                    return true;
                }return false;
            } 
            else return current >= waveNum;
        }
        public override void ClearWave()
        {
            base.ClearWave();
            timer?.Stop();
            timer = null;
        }
    }
}
public class Buff_DeathSunLight : Buff
{
    public int num=75;
    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        target.DeathEvent.AddListener(OnDeath);
    }
    public void OnDeath(Chess c)
    {
        SunLight lignt = UIManage.GetView<ItemPanel>().Create<SunLight>() as SunLight;
        lignt.InitSunLight(c.moveController.standTile, num, c.transform.position);
    }
    public override void BuffOver()
    {
        base.BuffOver();
        target.DeathEvent.RemoveListener(OnDeath);
    }
}

