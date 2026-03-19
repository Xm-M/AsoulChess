using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Playables;
/// <summary>
/// 这里放的都是最基本的插件
/// </summary>
public interface ILevelPlugin  
{
     public void StadgeEffect(LevelController levelController);
     public void OverPlugin(LevelController levelController);
}

/// <summary>
/// 需要参与存档的插件实现此接口，SaveSystem 会统一调用 CaptureTo 采集状态
/// 读档时插件在 StadgeEffect 中自行检查 SaveLoadContext 并恢复
/// </summary>
public interface ISaveableLevelPlugin : ILevelPlugin
{
    void CaptureTo(GameSaveData saveData);
}




/// <summary>
/// 制作小推车
/// </summary>
public class EnterWarPlugin_CarCreate : ISaveableLevelPlugin
{
    public PropertyCreator car;
    List<Chess> carses;
    public void StadgeEffect(LevelController levelController)
    {
        var mapPvz = MapManage.instance as MapManage_PVZ;
        if (mapPvz == null) return;

        if (SaveLoadContext.IsLoadFromSave && SaveLoadContext.CurrentSaveData?.carsSaveData != null)
        {
            RestoreCars(SaveLoadContext.CurrentSaveData.carsSaveData);
            return;
        }

        carses = new List<Chess>();
        int carCount = DifficultyManager.GetCarCount();
        if (carCount >= 0)
        {
            var indices = new List<int>();
            for (int i = 0; i < mapPvz.roomTile.Count; i++) indices.Add(i);
            for (int k = indices.Count - 1; k > 0; k--)
            {
                int j = Random.Range(0, k + 1);
                (indices[k], indices[j]) = (indices[j], indices[k]);
            }
            int take = Mathf.Min(carCount, indices.Count);
            for (int i = 0; i < take; i++)
            {
                var tile = mapPvz.roomTile[indices[i]];
                Chess cars = ChessTeamManage.Instance.CreateChess(car, tile, "Player");
                cars.gameObject.layer = 11;
                ChessTeamManage.Instance.GetTeam("Player").Remove(cars);
                carses.Add(cars);
            }
        }
        else
        {
            for (int i = 0; i < mapPvz.roomTile.Count; i++)
            {
                Chess cars = ChessTeamManage.Instance.CreateChess(car, mapPvz.roomTile[i], "Player");
                cars.gameObject.layer = 11;
                ChessTeamManage.Instance.GetTeam("Player").Remove(cars);
                carses.Add(cars);
            }
        }
    }

    public void CaptureTo(GameSaveData saveData)
    {
        if (saveData != null)
            saveData.carsSaveData = CaptureCarsState();
    }

    public System.Collections.Generic.List<CarSaveData> CaptureCarsState()
    {
        var list = new System.Collections.Generic.List<CarSaveData>();
        if (carses == null || car == null) return list;
        var mapPvz = MapManage.instance as MapManage_PVZ;
        if (mapPvz?.roomTile == null) return list;

        foreach (var c in carses)
        {
            if (c == null || c.IfDeath) continue;
            var tile = c.moveController?.standTile;
            if (tile == null) continue;
            int idx = mapPvz.roomTile.IndexOf(tile);
            if (idx < 0) continue;
            list.Add(new CarSaveData
            {
                roomTileIndex = idx,
                hp = c.propertyController.GetHp(),
                hpMax = c.propertyController.GetMaxHp(),
                creatorId = car.chessName
            });
        }
        return list;
    }

    private void RestoreCars(System.Collections.Generic.List<CarSaveData> data)
    {
        var mapPvz = MapManage.instance as MapManage_PVZ;
        if (mapPvz == null || data == null) return;

        carses = new List<Chess>();
        var creator = GetCreatorByChessName(data.Count > 0 ? data[0].creatorId : car?.chessName);
        if (creator == null) creator = car;
        if (creator == null) return;

        foreach (var d in data)
        {
            if (d.roomTileIndex < 0 || d.roomTileIndex >= mapPvz.roomTile.Count) continue;
            var tile = mapPvz.roomTile[d.roomTileIndex];
            var chess = ChessTeamManage.Instance.CreateChess(creator, tile, "Player");
            chess.gameObject.layer = 11;
            ChessTeamManage.Instance.GetTeam("Player").Remove(chess);
            chess.propertyController.ChangeHPMax(d.hpMax - chess.propertyController.GetMaxHp());
            chess.propertyController.ChangeHp(d.hp);
            carses.Add(chess);
        }
    }

    private static PropertyCreator GetCreatorByChessName(string chessName)
    {
        if (string.IsNullOrEmpty(chessName) || GameManage.instance?.allChess == null) return null;
        foreach (var c in GameManage.instance.allChess)
        {
            if (c != null && c.chessName == chessName) return c;
        }
        return null;
    }
    public void DestroyCars()
    {
        for(int i = 0; i < carses.Count; i++)
        {
            GameObject.Destroy(carses[i].gameObject);
        }
        carses.Clear();
        //EventController.Instance.RemoveListener(EventName.WhenLeaveLevel.ToString(), DestroyCars);
    }
    public void OverPlugin(LevelController levelController)
    {
        DestroyCars();
    }
}


/// <summary>
/// 这个其实是坚果保龄球的插件 因为会生成一条红线 红线外不可种植
/// </summary>
public class PreParePlugin_Conveyor : ILevelPlugin
{
    public List<PropertyCreator> creators;
    public GameObject redLine;
    //GameObject gameObject;
    GameObject line;
    public void StadgeEffect(LevelController levelController)
    {
        UIManage.Show<ConveyorPanel>();
        UIManage.GetView<ConveyorPanel>().InitCreator(creators, 6);
        (MapManage_PVZ.instance as MapManage_PVZ).WhenGameStart();
        for (int i = 3; i < MapManage_PVZ.instance.mapSize.x; i++)
            for (int j = 0; j < MapManage.instance.mapSize.y; j++)
            {
                MapManage.instance.tiles[i, j].gameObject.layer = 0;
            }
        //line = ObjectPool.instance.Create(redLine);
        line = GameObject.Instantiate(redLine);
        line.transform.position = MapManage.instance.tiles[2, 2].transform.position;
        //EventController.Instance.AddListener(EventName.WhenLeaveLevel.ToString(), GameOver);
    }
    public void OverPlugin(LevelController levelController)
    {
        GameOver();
    }
    public void GameOver()
    {
        //ObjectPool.instance.Recycle(line);
        GameObject.Destroy(line);
        UIManage.Close<ConveyorPanel>();
        //EventController.Instance.RemoveListener(EventName.WhenLeaveLevel.ToString(), GameOver);
    }
}

//还要做一个白天生成阳光的插件
public class GameStartPlugin_CreateSunlight : ISaveableLevelPlugin
{
    public int sunlight = 25;
    Timer timer;
    int n = 0;

    public void CaptureTo(GameSaveData saveData)
    {
        if (saveData == null || timer == null) return;
        timer.GetSaveState(out float remaining, out bool isLoop);
        var e = saveData.GetOrCreateTimerEntry("GameStartPlugin_CreateSunlight");
        e.remainingTime = remaining;
        e.isLoop = isLoop;
        e.extraInt = n;
    }

    public void StadgeEffect(LevelController levelController)
    {
        float delay;
        if (SaveLoadContext.IsLoadFromSave && SaveLoadContext.CurrentSaveData != null)
        {
            var e = SaveLoadContext.CurrentSaveData.GetTimerEntry("GameStartPlugin_CreateSunlight");
            if (e != null)
            {
                n = e.extraInt;
                delay = e.remainingTime;
                timer = GameManage.instance.timerManage.AddTimer(CreateSunlight, delay, true);
                return;
            }
        }
        delay = CaculateDelayTime();
        timer = GameManage.instance.timerManage.AddTimer(CreateSunlight, delay, true);
    }
    public void CreateSunlight()
    {
        SunLight lignt = UIManage.GetView<ItemPanel>().Create<SunLight>() as SunLight;
        int sun = sunlight;
        Vector2Int size = MapManage.instance.mapSize;
        Tile tile = MapManage.instance.tiles[(Random.Range(0,size.x-1)),Random.Range(0,size.y-1)];
        Vector3 startpos = tile.transform.position + Vector3.up * 20;
        lignt.InitSunLight(tile,sun,startpos);
        n++;
        //Debug.Log("下一次生成时间为："+CaculateDelayTime());
        timer.ChangeDelayTime(CaculateDelayTime());
    }
    public void OverPlugin(LevelController levelController)
    {
        WhenLeave();
    }
    public void WhenLeave()
    {
        timer?.Stop();
        timer = null;
        //EventController.Instance.RemoveListener(EventName.WhenLeaveLevel.ToString(), WhenLeave);
    }
    public float CaculateDelayTime()
    {
        return Mathf.Min(n * 0.1f + 4.25f, 9.5f) + Random.Range(0,2.75f);
    }
}

/// <summary>
/// 加速进场插件
/// </summary>
public class GameStartPlugin_FastCreateZombie : ILevelPlugin
{
    public float fastEnterTime = 4f;
    public void StadgeEffect(LevelController levelController)
    {
        if (SaveLoadContext.IsLoadFromSave) return;
        Debug.Log("加速进场");
        levelController.mintime = fastEnterTime;
    }
    public void OverPlugin(LevelController levelController)
    {

    }
}

//也就是说这个buff 1.要对场上的所有单位生效 2.要对出场的僵尸生效
/// <summary>
/// 校园铃声插件
/// 现在的问题是这个随机生成僵尸的任务要不要放在这里；还有一个问题是那个Tile要怎么放呢？
/// 用GameObject.Find() 也不是不行
/// </summary>
public class GameStartPlugin_PlaySchoolAudio : ISaveableLevelPlugin
{
    [LabelText("上课铃")]
    public float interval_classBegin=10;//上课铃声间隔
    [LabelText("下课铃")]
    public float interval_classOver=50;//下课铃
    [LabelText("校铃")]
    public GameObject audioEffect;
    [LabelText("上课buff")]
    [SerializeReference]
    public Buff_ClassBegin beginBuff;//上课buff 虚弱
    [LabelText("下课buff")]
    [SerializeReference]
    public Buff_ClassOver overBuff;//下课buff 进攻
    [LabelText("门里可能出现的随机僵尸")]
    public List<PropertyCreator> zombieList;
    Timer timer;
    Transform center;
    bool classBegin;
    Buff currentBuff;
    DoorTile[] doorTiles;

    public void CaptureTo(GameSaveData saveData)
    {
        if (saveData == null || timer == null) return;
        timer.GetSaveState(out float remaining, out bool isLoop);
        var e = saveData.GetOrCreateTimerEntry("GameStartPlugin_PlaySchoolAudio");
        e.remainingTime = remaining;
        e.isLoop = isLoop;
        e.extraInt = classBegin ? 1 : 0;
    }

    public void StadgeEffect(LevelController levelController)
    {
        classBegin = false;
        currentBuff=beginBuff;
        center = MapManage.instance.tiles[MapManage.instance.mapSize.x/2,MapManage.instance.mapSize.y/2].transform;
        EventController.Instance.AddListener<Chess>(EventName.WhenChessEnterWar.ToString(),AddBuff);
        doorTiles = GameObject.FindObjectsOfType<DoorTile>();
        foreach(DoorTile doorTile in doorTiles)
        {
            doorTile.zombieList = zombieList;
        }
        float delay;
        if (SaveLoadContext.IsLoadFromSave && SaveLoadContext.CurrentSaveData != null)
        {
            var e = SaveLoadContext.CurrentSaveData.GetTimerEntry("GameStartPlugin_PlaySchoolAudio");
            if (e != null)
            {
                classBegin = e.extraInt != 0;
                //currentBuff = classBegin ? beginBuff : overBuff;
                if (classBegin) currentBuff = beginBuff;
                else currentBuff = overBuff;
                if (!classBegin)
                {
                    foreach (var door in doorTiles)
                        door.OpenDoor();
                }
                foreach (var zombie in ChessTeamManage.Instance.GetTeam("Enemy"))
                {
                    if (zombie != null && zombie.propertyController?.creator?.plantTags?.Contains("学生") == true)
                        zombie.buffController.AddBuff(currentBuff);
                }
                delay = e.remainingTime;
                timer = GameManage.instance.timerManage.AddTimer(CreateAudio, delay, true);
                return;
            }
        }
        timer = GameManage.instance.timerManage.AddTimer(CreateAudio,interval_classBegin,true);
    }
    public void CreateAudio()
    {
        GameObject audio= ObjectPool.instance.Create(audioEffect);
        audio.transform.position = center.transform.position;
        if (!classBegin)
        {
            Debug.Log("开始上课");
            classBegin = true;
            foreach(var zombie in ChessTeamManage.Instance.GetTeam("Enemy"))
            {
                if (zombie.propertyController.creator.plantTags.Contains("学生"))
                {
                    zombie.buffController.AddBuff(beginBuff);
                }
            }
            currentBuff = beginBuff;
            timer.ChangeDelayTime(interval_classOver);
        }
        else
        {
            Debug.Log("下课了");
            classBegin = false;
            foreach(var door in doorTiles)
            {
                door.OpenDoor();
            }
            foreach (var zombie in ChessTeamManage.Instance.GetTeam("Enemy"))
            {
                if (zombie.propertyController.creator.plantTags.Contains("学生"))
                {
                    zombie.buffController.AddBuff(overBuff);
                }
            }
            timer.ChangeDelayTime(interval_classBegin);
            currentBuff =overBuff;
        }
    }
    public void AddBuff(Chess chess)
    {
        if(chess.propertyController.creator.plantTags.Contains("学生"))
            chess.buffController.AddBuff(currentBuff);
    }
    public void WhenLeave()
    {
        timer?.Stop();
        timer = null;
        EventController.Instance.RemoveListener<Chess>(EventName.WhenChessEnterWar.ToString(),AddBuff);
    }
    public void OverPlugin(LevelController levelController)
    {
        WhenLeave();
    }
}
//EventController.Instance.AddListener(EventName.GameStart.ToString(),CheckFetter);
//EventController.Instance.AddListener(EventName.GameOver.ToString(), ClearFetter);
//EventController.Instance.AddListener(EventName.WhenLeaveLevel.ToString(), ClearFetter);
public class GameStartPlugin_Fetter : ILevelPlugin
{
    public void StadgeEffect(LevelController levelController)
    {
        //throw new System.NotImplementedException();
        Debug.Log("羁绊系统调用");
        GameManage.instance.fetterManage.CheckFetter();
        UIManage.Show<FetterPanel>();
       
    }
    public void OverPlugin(LevelController levelController)
    {
        GameManage.instance.fetterManage.ClearFetter();
        UIManage.Close<FetterPanel>();
    
    }
}
