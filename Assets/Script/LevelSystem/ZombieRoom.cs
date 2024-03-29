using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
/// <summary>
/// 这个也要改
/// 每十波 出一大波
/// </summary>
//[CreateAssetMenu(fileName ="NewZombieRoom",menuName ="RoomType/ZombieRoom")]
//public class ZombieRoom : LevelData{
//    public List<PropertyCreator> zombies;
//    public ZombiesWave zombiesWave;
//    public float zombieInterval=0.5f;
//    public float startTime=-10;
//    float t;
//    float zt;
//    bool gameStart;
//    public List<Chess> liveZombies;
//    public string preparePanelName;
//    public override void StartGameStage()
//    {
//        base.StartGameStage();
//        Debug.Log("谁调用的");
//        t=startTime;
//        zt=0;
//        liveZombies =GameManage.instance.enemyManage.chesses;
//        zombiesWave.InitWave();
//        GameManage.instance.WhenGameStart.AddListener(EnterWar);
//        for (int i = 0; i < zombies.Count; i++)
//        {
//            Chess zombie= GameManage.instance.enemyManage.CreateChess(zombies[i] , (MapManage.instance as MapManage_PVZ).zombiePreTile[i] );
//            liveZombies.Add(zombie);
//        }
//    }
//    public void EnterWar()
//    {
//        //UIManage.Close(preparePanelName);
//        for(int i=0;i<liveZombies.Count;i++)
//        {
//            liveZombies[i].Death();
//        }
//        liveZombies.Clear();
//        UIManage.Show<ProgressBar>();
//        //zombiesWave.EnterNextWave();
//    }
//    public void StartCreateZombie()
//    {
//        UIManage.Show<TextPanel>();
//        gameStart = true;
//    }
//    public override void OverGameStage()
//    {
//        base.OverGameStage();
//        GameManage.instance.WhenGameStart.RemoveListener(EnterWar);
//        gameStart = false;
//        UIManage.Close<TextPanel>() ;
//        UIManage.Close<ProgressBar>();
//        UIManage.Close(preparePanelName);
//    }

//    public override void UpdateGameStage()
//    {
//        base.UpdateGameStage();
//        if(GameManage.instance.ifGameStart&&gameStart){
//            if(zombiesWave.CurrentPrice>0){
//                zt+=Time.deltaTime;
//                if(zt>zombieInterval){
//                    zt=0;
//                    Tile tile=MapManage.instance.preTiles[UnityEngine.Random.Range(0,MapManage.instance.preTiles.Count)];
//                    Chess c= zombiesWave.CreateChess(tile);
//                    c.WhenChessEnterWar();
//                }
//            }
//            t+=Time.deltaTime;
//            if(t>zombiesWave.minTime){   
//                if((t>zombiesWave.maxTime&&zombiesWave.currentWave<zombiesWave.LastWave)|| liveZombies.Count==0){
//                    t=0;
//                    zombiesWave.EnterNextWave();
//                    if (liveZombies.Count == 0) Debug.Log("死完了");
//                    for(int i=0;i<zombies.Count;i++){
//                        if(zombies[i].baseProperty.price<=zombiesWave.CurrentPrice&&
//                        !zombiesWave.WaveZombie.Contains(zombies[i])){
//                            zombiesWave.AddNewZombie(zombies[i]);
//                        }
//                    }
//                    zt = zombieInterval + 1;
//                }

//            }
//        }
//    }

//}
