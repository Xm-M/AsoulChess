using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using System;

/// <summary>
/// 关卡流程
/// 1.进入关卡场景
/// (--关卡插件--)
/// 2.调用EnterMap插件
/// 3.调用Prepare插件
/// 4.调用GameStart插件
/// 5.调用所有插件的Over函数 掉用outcome插件
/// (--关卡插件--)
/// 6.离开关卡场景 WhenLeaveLevel;
/// </summary>
public class LevelController : MonoBehaviour
{
    public LevelData levelData;//关卡数据 但是是从LevelManage里设置的
    [ShowInInspector, ReadOnly]
    [ShowIf("@UnityEngine.Application.isPlaying")]
    protected int currentWave;//当前波数
    [ShowInInspector, ReadOnly]
    [ShowIf("@UnityEngine.Application.isPlaying")]
    protected float t;//还是要用独立的时间控制器 不然还是太混乱了
    [ShowInInspector, ReadOnly]
    [ShowIf("@UnityEngine.Application.isPlaying")]
    protected List<WaveData> waveDatas;//每波的数据
    [ShowInInspector, ReadOnly]
    [ShowIf("@UnityEngine.Application.isPlaying")]
    public float mintime;//每波的最小时间
    [ShowInInspector, ReadOnly]
    [ShowIf("@UnityEngine.Application.isPlaying")]
    public float maxtime;//每波的最大时间
    protected List<Chess> zombies;
    //插件也是放在LevelData里面的

    /// <summary>
    /// 供存档系统获取当前波次
    /// </summary>
    public virtual int GetCurrentWave() => currentWave;
    /// <summary>
    /// 供存档系统获取波次计时
    /// </summary>
    public virtual float GetWaveTime() => t;
    /// <summary>
    /// 供存档系统获取波次最小时间
    /// </summary>
    public virtual float GetMintime() => mintime;
    /// <summary>
    /// 供存档系统获取波次最大时间
    /// </summary>
    public virtual float GetMaxtime() => maxtime;

    /// <summary>
    /// 读档时恢复关卡进度
    /// </summary>
    public virtual void RestoreLevelProgress(LevelSaveData data)
    {
        if (data == null) return;
        currentWave = data.currentWave;
        t = data.t;
        mintime = data.mintime;
        maxtime = data.maxtime;
        TimerManage.SetGameTimeForLoad(data.gameTime);
    }

    /// <summary>
    /// 读档时恢复场上玩家植物
    /// </summary>
    public virtual void RestorePlayerPlants(List<ChessSaveData> plants)
    {
        if (plants == null || MapManage.instance == null) return;
        SkillContext.PendingChessRefs.Clear();
        foreach (var p in plants)
        {
            var creator = GetCreatorByChessName(p.creatorId);
            if (creator == null) continue;
            if (!MapManage.instance.IfInMapRange(p.tileX, p.tileY)) continue;
            var tile = MapManage.instance.tiles[p.tileX, p.tileY];
            var chess = ChessTeamManage.Instance.CreateChess(creator, tile, "Player", forRestore: true);
            if (chess.CompareTag("Player"))
                tile.PlantChess(chess);
            chess.propertyController.ChangeHPMax(p.hpMax - chess.propertyController.GetMaxHp());
            chess.propertyController.ChangeHp(p.hp);
            if (p.buffs != null)
            {
                foreach (var b in p.buffs)
                    chess.buffController.AddBuffFromSave(b);
            }
            var sc = chess.skillController;
            if (p.skillContextData != null && sc?.context != null)
                sc.context.RestoreFromSaveData(p.skillContextData, chess);
            if (p.skillStateData != null && sc?.activeSkill != null)
                sc.activeSkill.RestoreFromSaveData(p.skillStateData, chess);
            if (p.skillRuntimeData != null && sc?.activeSkill is IHasRuntimeInfo hi && hi.Runtime != null)
                hi.Runtime.RestoreFromSaveData(p.skillRuntimeData);
            var savedState = (StateName)p.stateName;
            if (savedState == StateName.SkillState)
            {
                if (p.skillEffectFired)
                    sc?.SkillOver(chess);
                else
                {
                    sc?.activeSkill?.ReturnCD();
                    sc?.SkillOver(chess);
                }
            }
            else if (savedState == StateName.ResumeState)
                chess.stateController.ChangeState(StateName.ResumeState);
        }
        RestorePendingChessRefsNextFrame();
    }

    void RestorePendingChessRefsNextFrame()
    {
        if (SkillContext.PendingChessRefs.Count == 0) return;
        StartCoroutine(RestorePendingChessRefsCoroutine());
    }
    System.Collections.IEnumerator RestorePendingChessRefsCoroutine()
    {
        yield return null;
        var team = ChessTeamManage.Instance?.GetTeam("Player");
        if (team == null) yield break;
        foreach (var (owner, key, chessData) in SkillContext.PendingChessRefs)
        {
            var parts = chessData.Split('|');
            if (parts.Length < 3) continue;
            int tx = int.TryParse(parts[1], out int x) ? x : -1;
            int ty = int.TryParse(parts[2], out int y) ? y : -1;
            if (tx < 0 || ty < 0 || MapManage.instance == null || !MapManage.instance.IfInMapRange(tx, ty)) continue;
            var tile = MapManage.instance.tiles[tx, ty];
            var target = tile?.stander;
            if (target != null && owner?.skillController?.context != null)
                owner.skillController.context.Set(key, target);
        }
        SkillContext.PendingChessRefs.Clear();
    }

    private static PropertyCreator GetCreatorByChessName(string chessName)
    {
        var playerChess = Resources.LoadAll<PropertyCreator>("ChessData/Player");
        if (playerChess == null) return null;
        foreach (var c in playerChess)
        {
            if (c != null && c.chessName == chessName)
                return c;
        }
        return null;
    }

    protected virtual  void OnEnable()
    {
        LevelManage.instance.SetController(this);
        
    }
    public virtual void Init(LevelData levelData)
    {
        this.levelData = levelData;
    }

    /// <summary>
    /// 进入地图时调用的函数 一般在地图加载完就调用 还是统一绑定在TimeLine上吧 比较好控制
    /// 如果我再次调用这个函数是不是就又能重来了
    /// </summary>
    public virtual void EnterMap()
    {
        if (levelData.EnterMapPlugin != null)
        {
            for(int i = 0; i < levelData.EnterMapPlugin.Count; i++)
            {
                levelData.EnterMapPlugin[i].StadgeEffect(this);//但是开始插件为什么放在这里呢
            }
        }
        CreateZombieWaves();
    }
    public virtual void CreateZombieWaves()
    {
        if (zombies == null)
            zombies = new List<Chess>();
        else zombies.Clear();
        if (levelData == null)
        {
            Debug.LogError("没有关卡数据");
        }
        //在调用完所有准备阶段前的插件后，生成僵尸列表的所有数据
        if (waveDatas == null)
            waveDatas = new List<WaveData>();
        else waveDatas.Clear();
        for (int i = 0; i < levelData.MaxWave; i++)
        {
            WaveData waveData = new WaveData();
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

    /// <summary>
    /// 游戏准备阶段 也就是选卡的时候调用
    /// 选卡和羁绊可以放一起 反正传送带没有羁绊
    /// </summary>
    public virtual void GamePrepare()
    {
        if (levelData == null)
        {
            Debug.LogError("没有关卡数据");
        }
        for(int i = 0; i < levelData.PreParePlugin.Count; i++)
        {
            levelData.PreParePlugin[i].StadgeEffect(this);
        }
        LevelManage.instance.PrepareLevel();
        //这里一定要做的..好像没有
        currentWave = -1;
        mintime = 16;
    }


    /// <summary>
    /// 游戏开始阶段 也就是选卡之后 出僵尸之前
    /// </summary>
    public virtual void GameStart()
    {
        if (levelData == null)
        {
            Debug.LogError("没有关卡数据");
        }
        bool isLoadFromSave = SaveLoadContext.IsLoadFromSave && SaveLoadContext.CurrentSaveData != null;
        if (isLoadFromSave)
        {
            RestoreLevelProgress(SaveLoadContext.CurrentSaveData.levelData);
        }
        for(int i = 0; i < zombies.Count; i++)
        {
            zombies[i].Death();
        }
        if (levelData.GameStartPlugin != null)
        {
            for (int i = 0; i < levelData.GameStartPlugin.Count; i++)
            {
                levelData.GameStartPlugin[i].StadgeEffect(this);
            }
        }
        if (isLoadFromSave)
        {
            BuffDatabase.RestoreRegistry(SaveLoadContext.CurrentSaveData.buffRegistry);
            RestorePlayerPlants(SaveLoadContext.CurrentSaveData.playerPlants);
        }
        LevelManage.instance.GameStart();
        if (!isLoadFromSave)
        {
            UIManage.Show<TextPanel>();
            UIManage.GetView<TextPanel>().GameStart();
            SaveSystem.SaveCurrentLevel();
        }
        if (isLoadFromSave && currentWave >= 0)
        {
            UIManage.Show<ProgressBar>();
            UIManage.GetView<ProgressBar>().SetFlag(levelData.MaxWave / 10);
            UIManage.GetView<ProgressBar>().MoveBar(currentWave + 1, levelData.MaxWave);
            if (t >= mintime)
                DoEnterNextWave();
        }
    }
    protected virtual void DoEnterNextWave()
    {
        t = -2;
        UIManage.GetView<ProgressBar>().MoveBar(currentWave + 1, levelData.MaxWave);
        if (currentWave + 1 < levelData.MaxWave)
            waveDatas[currentWave + 1].EnterWave();
        currentWave++;
        if (currentWave % 10 == 9)
        {
            mintime = 4;
            maxtime = 50;
        }
        else
        {
            mintime =4;
            maxtime = UnityEngine.Random.Range(0, 6) + 23;
        }
    }

    /// <summary>
    /// 接下来的生成僵尸才是最tm难的地方;
    /// 首先就是波次与生成僵尸的问题
    /// </summary>
    protected virtual void Update()
    {
        if (LevelManage.instance.IfGameStart && currentWave < levelData.MaxWave)
        {
            t += Time.deltaTime;
            if (currentWave == -1&&t>mintime)
            {
                SaveSystem.SaveCurrentLevel();
                waveDatas[currentWave+1].EnterWave();
                UIManage.GetView<TextPanel>().FirstZombieCom();
                EventController.Instance.TriggerEvent(EventName.FirstZombieComming.ToString());
                mintime =UnityEngine. Random.Range(0, 6);
                maxtime = mintime+23;
                currentWave++;
                t = 0;
                UIManage.Show<ProgressBar>();//进度条(其实也可以在第一只僵尸出来的时候刷)
                UIManage.GetView<ProgressBar>().SetFlag(levelData.MaxWave / 10);
            }
            else if (currentWave!=-1&&((waveDatas[currentWave].CheckZombieHp() && t > mintime) || (t > maxtime)))
            {
                if (waveDatas[currentWave].GetCurrentZombieHpSum() <= 0)
                    SaveSystem.SaveCurrentLevel();
                t = 0;
                DoEnterNextWave();
            }
        }
    }
    /// <summary>
    /// 游戏结束 生成胜利道具或者失败 生成失败文字
    /// 在LevelController里面不需要考虑LeaveLevel;
    /// </summary>
    /// <param name="win"></param>
    public virtual void GameOver(bool win)
    {
        UIManage.Close<ProgressBar>();
        OverPlugin();
        if (win)
        {
            if (levelData.nextLevel != null)
                LevelManage.instance.ChangeLevel(levelData.nextLevel);
            else
            {
                LevelManage.instance.ReturnMenu();
            }
        }
        else
        {
            UIManage.GetView<TextPanel>().GameOver();
        }
        UIManage.Close<ProgressBar>();
        foreach (var wave in waveDatas) { 
            wave.ClearWave();
        }
    }
    public void OverPlugin()
    {
        foreach (var plugin in levelData.GameStartPlugin)
            plugin.OverPlugin(this );
        foreach (var plugin in levelData.EnterMapPlugin)
            plugin.OverPlugin(this);
        foreach (var plugin in levelData.PreParePlugin)
            plugin.OverPlugin(this);
    }
}
/// <summary>
/// 保存每波僵尸的数据，
/// </summary>
[Serializable]
public class WaveData
{
    [Serializable]
    public class ZombieInWaveData
    {
        public PropertyCreator zombieCreate;
        public int zombieNum;
        //public float interval;//锤僵尸用的间隔时间
    }
    [FoldoutGroup("zombieData")]
    public  List<float> fateList;
    [FoldoutGroup("zombieData")]
     public List<ZombieInWaveData> zombieList;
    [ShowInInspector]
    protected List<Chess> liveZombie;
    protected float enterPecent;
    protected float hpmax;
    protected int wave;
    protected bool createOver;
    [FoldoutGroup("zombieData"),ShowInInspector]
    protected int maxZombieValue = 0;
    protected ILevelOutcome outcome;
    /// <summary>
    /// 总之先这样写吧
    /// </summary>
    /// <param name="wave"></param>
    /// <param name="data"></param>
    public virtual void InitWave(int wave,LevelData data)
    {
        int raritySum = 0;
        this.wave = wave;
        outcome=data.outcome;
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
        if (data.createZombieType == CreateZombieType.一类有限制 || data.createZombieType == CreateZombieType.二类有限制)
        {
            for (int i = 0; i < data.zombieList.Count; i++)
            {
                if (data.zombieList[i].baseProperty.waveLimit <= wave)
                {
                    ZombieInWaveData zombieInWaveData = new ZombieInWaveData();
                    zombieList.Add(zombieInWaveData);
                    zombieInWaveData.zombieCreate = data.zombieList[i];
                    zombieInWaveData.zombieNum = 0;
                    raritySum += data.zombieList[i].baseProperty.rarity;
                }

            }
        }
        else
        {
            for (int i = 0; i < data.zombieList.Count; i++)
            {
                ZombieInWaveData zombieInWaveData = new ZombieInWaveData();
                zombieList.Add(zombieInWaveData);
                zombieInWaveData.zombieCreate = data.zombieList[i];
                zombieInWaveData.zombieNum = 0;
                raritySum += data.zombieList[i].baseProperty.rarity;
            }
        }

        Debug.Log("筛选僵尸完毕");
        fateList.Add((float)(zombieList[0].zombieCreate.baseProperty.rarity) / raritySum);
        for (int i = 1; i < zombieList.Count; i++)
        {
            fateList.Add(fateList[i - 1] + (float)(zombieList[i].zombieCreate.baseProperty.rarity) / raritySum);
            //Debug.Log(fateList[i]);
        }

         maxZombieValue = 0;
        if (wave % 10 != 0)
        {
            if (data.createZombieType == CreateZombieType.一类有限制 || data.createZombieType == CreateZombieType.一类无限制)
                maxZombieValue = ((int)((wave - 1) / 3) + data.n) * data.t;
            else
                maxZombieValue = ((int)((wave - 1) * 2 / 5) + data.n) * data.t;
        }   
        else
        {
            maxZombieValue = (data.t + 1) * 5;
            Debug.Log("大波僵尸"+maxZombieValue);
        }
        maxZombieValue *= 25;
        Debug.Log("本波僵尸价值为" + maxZombieValue);
        int num = 0;
        while (maxZombieValue > 0)
        {
            float fate = UnityEngine.Random.Range(0, 1f);
            //Debug.Log(fateList.Count);
            //Debug.Log(fate);
            for (int i = 0; i < fateList.Count; i++)
            {
                if (fate < fateList[i])
                {
                    zombieList[i].zombieNum += 1;
                    maxZombieValue -= zombieList[i].zombieCreate.baseProperty.price;
                    //Debug.Log(maxZombieValue);
                    hpmax += zombieList[i].zombieCreate.baseProperty.HpMax;
                    break;
                }
            }
            num++;
            if (num > 500) break;
        }
        if (wave % 10 == 9||wave%10==0)
        {
            enterPecent = 0;
        }
        else enterPecent = UnityEngine.Random.Range(0.5f,0.65f);
        Debug.Log(string.Format("第{0}波僵尸已经生成完毕,共生成{1}只僵尸", wave,num));
    }

    /// <summary>
    /// 在这里生成这波的所有僵尸，并且生成的位置要符合规则
    /// 今天下午把他写完吧 其实可以用携程写 这样不会卡
    /// </summary>
    public virtual void EnterWave()
    {
        
        LevelManage.instance.StartCoroutine(Create());
    }
    IEnumerator Create()
    {
        liveZombie = new List<Chess>();
        //激活状态 2s后生成僵尸
        if (wave % 10 == 0)
        {
            EventController.Instance.TriggerEvent(EventName.WaveZombieComming.ToString());
            UIManage.GetView<TextPanel>().ZombieWave();
            yield return new WaitForSeconds(6);
            if (wave == LevelManage.instance.currentLevel.MaxWave)
            {
                EventController.Instance.TriggerEvent(EventName.LastWaveZombie.ToString());
                UIManage.GetView<TextPanel>().LastWave();
            }
        }
        else
        {
            yield return new WaitForSeconds(2);
        }
        for(int i = 0; i < zombieList.Count; i++)
        {
            for(int j = 0; j < zombieList[i].zombieNum; j++)
            {
                //Debug.Log(zombieList[i].zombieCreate.chessName);
                liveZombie.Add(CreateChess(zombieList[i].zombieCreate));
            }
            yield return null;
        }
        createOver = true;
    }
    public virtual Chess CreateChess(PropertyCreator creator)
    {
        Tile standTile = null;
        List<Tile> tiles = new List<Tile>();
        List<Tile> all= ((MapManage.instance) as MapManage_PVZ).preTiles;
        for (int i=0;i<all.Count; i++)
        {
            if((all[i].tileType & creator.chessTileType) != 0)
            {
                tiles.Add(all[i]);
            }
        }
        int n = UnityEngine.Random.Range(0, tiles.Count);
        standTile = tiles[n];
        Chess chess= ChessTeamManage.Instance.CreateChess(creator, standTile, "Enemy");
        float dx = UnityEngine.Random.Range(0, 3.75f);
        chess.transform.position = standTile.transform.position + Vector3.right * dx;//位置偏移
        //Debug.Log(chess.name);
        return chess;
    }

    public virtual bool CheckZombieHp()
    {
        if(!createOver)return false;
        if(liveZombie.Count==0)return true;
        float hpcurrent = 0;
        Chess last = liveZombie[0];
        for(int i = liveZombie.Count-1; i >= 0; i--)
        {
            if (!liveZombie[i].IfDeath)
                hpcurrent += liveZombie[i].propertyController.GetHp();
            else
            {
                liveZombie.RemoveAt(i);
            }
        }
        if (liveZombie.Count==0&&wave==LevelManage.instance.currentLevel.MaxWave)
        {
            //if (nextLevelData != null)
            //    Debug.Log(nextLevelData.levelName); 
            //也就是说我是在这里搞对吧
            
            if (outcome == null)
            {
                Item_Reward reward = UIManage.GetView<ItemPanel>().Create<Item_Reward>();
                reward.SetRewardPos(last.transform.position);
            }
            else
            {
                outcome.HandleOutcome(true);
            }
            //win = true;
            return true;
        }

        if (hpcurrent / hpmax < enterPecent)
        {
            return true;
        
        }
        else return false;
    }
    /// <summary>当前波僵尸生命值总和，用于判断是否全场已死（hp<=0 时虽未触发 Death 但僵尸已全死）</summary>
    public virtual float GetCurrentZombieHpSum()
    {
        if (!createOver || liveZombie == null) return float.MaxValue;
        float sum = 0;
        for (int i = 0; i < liveZombie.Count; i++)
        {
            if (!liveZombie[i].IfDeath)
                sum += liveZombie[i].propertyController.GetHp();
        }
        return sum;
    }
    public virtual void ClearWave()
    {

    }
}
