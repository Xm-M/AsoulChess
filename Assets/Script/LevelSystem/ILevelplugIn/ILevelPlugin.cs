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
/// 剧情插件 等待剧情通过再继续
/// </summary>
public class EnterWarPlugin_Plot : ILevelPlugin
{
    public PlayableDirector plotDirector;
    PlayableDirector mapDir;
    bool over;
    public void StadgeEffect(LevelController levelController)
    {
        //throw new System.NotImplementedException();
        mapDir = (MapManage.instance as MapManage_PVZ).dir;
        mapDir.Pause();
        EventController.Instance.AddListener(EventName.WhenLeaveLevel.ToString(), WhenLeave);
    }
    IEnumerator WaitPlot()
    {
        while (!over)
        {
           if (plotDirector.state != PlayState.Playing &&plotDirector.time >= plotDirector.duration)
            {
                over = true;
            }
            yield return null;
        }
        if (mapDir != null) mapDir.Play();
        WhenLeave();
    }
    public void WhenLeave()
    {
        over = true;
        EventController.Instance.RemoveListener(EventName.WhenLeaveLevel.ToString(), WhenLeave);
    }
    public void OverPlugin(LevelController levelController)
    {
        //暂时不需要
    }
}


/// <summary>
/// 制作小推车
/// </summary>
public class EnterWarPlugin_CarCreate : ILevelPlugin
{
    public PropertyCreator car;
    List<Chess> carses;
    public void StadgeEffect(LevelController levelController)
    {
        carses=new List<Chess>();
        for (int i = 0; i < (MapManage_PVZ.instance as MapManage_PVZ).roomTile.Count; i++)
        {
            Chess cars = ChessTeamManage.Instance.CreateChess(car,
                 (MapManage_PVZ.instance as MapManage_PVZ).roomTile[i], "Player");
            cars.gameObject.layer = 11;
            ChessTeamManage.Instance.GetTeam("Player").Remove(cars);
            carses.Add(cars);
            //Debug.Log(cars.gameObject.layer);
            //Debug.Log(LayerMask.LayerToName(cars.gameObject.layer)); 
        }
        //EventController.Instance.AddListener(EventName.WhenLeaveLevel.ToString(), DestroyCars);
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
public class GameStartPlugin_CreateSunlight : ILevelPlugin
{
    public int sunlight = 25;
    Timer timer;
    int n = 0;
    public void StadgeEffect(LevelController levelController)
    {
        float delay = CaculateDelayTime();
        timer = GameManage.instance.timerManage.AddTimer(CreateSunlight,delay, true);
        //EventController.Instance.AddListener(EventName.WhenLeaveLevel.ToString(),WhenLeave);
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
        //throw new System.NotImplementedException();
        Debug.Log("加速进场");
        levelController.mintime = fastEnterTime;
    }
    public void OverPlugin(LevelController levelController)
    {

    }
}

/// <summary>
/// 播放音乐插件
/// </summary>
public class GameStartPlugin_PlayAudio : ILevelPlugin
{
    public string audioName;
    public void StadgeEffect(LevelController levelController)
    {
        MapManage.instance.BGMPlayer.ChangeAudio(audioName);
        MapManage.instance.BGMPlayer.Play();
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
public class GameStartPlugin_PlaySchoolAudio : ILevelPlugin
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
        GameManage.instance.fetterManage.CheckFetter();
        UIManage.Show<FetterPanel>();
       
    }
    public void OverPlugin(LevelController levelController)
    {
        GameManage.instance.fetterManage.ClearFetter();
        UIManage.Close<FetterPanel>();
    
    }
}
