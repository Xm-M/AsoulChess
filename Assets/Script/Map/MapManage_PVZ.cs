using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

//类似PVZ类型的地图 都是向右走啦 
public class MapManage_PVZ : MapManage
{
    public Transform sunLightRecyclePos;
    public List<Tile> zombiePreTile;
    public Tile roomTile;
    public AudioSource au;
    public PlayableDirector dir;
    //public LevelData room;
    protected override void Start()
    {
        base.Start();
        //EventController.Instance.AddListener(EventName.GameOver.ToString(), WhenGameOver);
        //EventController.Instance.AddListener(EventName.GameStart.ToString(), WhenGameStart);
    }//
  
    public void WhenGameStart()
    {
        dir.Play();
    }
    public void WhenGameOver()
    {
        au.Stop();
    }
     
    private void OnDestroy()
    {
        //EventController.Instance.RemoveListener(EventName.GameOver.ToString(), WhenGameOver);
        //EventController.Instance.RemoveListener(EventName.GameStart.ToString(), WhenGameStart);
    }

    public void GamePrepare()
    {
        LevelManage.instance.PrepareLevel();
    }
    public void GameStart()
    {
        LevelManage.instance.GameStart();
    }

}
