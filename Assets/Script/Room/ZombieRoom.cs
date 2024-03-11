using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
/// <summary>
/// 这个也要改
/// 每十波 出一大波
/// </summary>
[CreateAssetMenu(fileName ="NewZombieRoom",menuName ="RoomType/ZombieRoom")]
public class ZombieRoom : RoomType{
    public List<PropertyCreator> zombies;
    public ZombiesWave zombiesWave;
    public float zombieInterval=0.5f;
    public float startTime=-10;
    float t;
    float zt;
    bool gameStart;
    public List<Chess> liveZombies;
    public string preparePanelName;
    public override void WhenEnterRoom()
    {
        base.WhenEnterRoom();
        Debug.Log("谁调用的");
        t=startTime;
        zt=0;
        liveZombies =GameManage.instance.enemyManage.chesses;
        zombiesWave.InitWave();
        GameManage.instance.WhenGameStart.AddListener(EnterWar);
        for (int i = 0; i < zombies.Count; i++)
        {
            Chess zombie= GameManage.instance.enemyManage.CreateChess(zombies[i] , (MapManage.instance as MapManage_PVZ).zombiePreTile[i] );
            liveZombies.Add(zombie);
        }
        UIManage.Show(preparePanelName);
    }
    public void EnterWar()
    {
        //UIManage.Close(preparePanelName);
        for(int i=0;i<liveZombies.Count;i++)
        {
            liveZombies[i].RemoveChess();
        }
        liveZombies.Clear();
        UIManage.Show<ProgressBar>();
        //zombiesWave.EnterNextWave();
    }
    public void StartCreateZombie()
    {
        UIManage.Show<TextPanel>();
        gameStart = true;
    }
    public override void WhenLeaveRoom()
    {
        base.WhenLeaveRoom();
        GameManage.instance.WhenGameStart.RemoveListener(EnterWar);
        gameStart = false;
        UIManage.Close<TextPanel>() ;
        UIManage.Close<ProgressBar>();
        UIManage.Close(preparePanelName);
    }

    public override void WhenStayRoom()
    {
        base.WhenStayRoom();
        if(GameManage.instance.ifGameStart&&gameStart){
            if(zombiesWave.CurrentPrice>0){
                zt+=Time.deltaTime;
                if(zt>zombieInterval){
                    zt=0;
                    Tile tile=MapManage.instance.preTiles[UnityEngine.Random.Range(0,MapManage.instance.preTiles.Count)];
                    Chess c= zombiesWave.CreateChess(tile);
                    c.WhenChessEnterWar();
                }
            }
            t+=Time.deltaTime;
            if(t>zombiesWave.minTime){   
                if((t>zombiesWave.maxTime&&zombiesWave.currentWave<zombiesWave.LastWave)|| liveZombies.Count==0){
                    t=0;
                    zombiesWave.EnterNextWave();
                    if (liveZombies.Count == 0) Debug.Log("死完了");
                    for(int i=0;i<zombies.Count;i++){
                        if(zombies[i].baseProperty.price<=zombiesWave.CurrentPrice&&
                        !zombiesWave.WaveZombie.Contains(zombies[i])){
                            zombiesWave.AddNewZombie(zombies[i]);
                        }
                    }
                    zt = zombieInterval + 1;
                }

            }
        }
    }

}
[Serializable]
public class ZombiesWave{
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
    [HideInInspector]public float minTime;
    [HideInInspector]public float maxTime;
    float t;

    public void InitWave(){
        currentWave=0;
        CurrentPrice=0;
        minTime=UnityEngine.Random.Range(minTimeRange.x  ,minTimeRange.y);
        maxTime=UnityEngine.Random.Range(maxTimeRange.x,maxTimeRange.y);
        WaveZombie.Clear();
        fateList.Clear();
        //UIManage.instance.zombieBar.SetValue(currentWave, LastWave);
        UIManage.GetView<ProgressBar>().SetFlag(LastWave / 10);
    }

    public void EnterNextWave(){
        currentWave++;
        UIManage.GetView<ProgressBar>().MoveBar(currentWave, LastWave);
        if (currentWave > LastWave)
        {
            GameManage.instance.GameOver("Player");
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
    public void AddNewZombie(PropertyCreator zombie){
        if(!WaveZombie.Contains(zombie)){
            WaveZombie.Add(zombie);
            fateList.Add(0);
            int raritySum=0;
            for(int i=0;i<WaveZombie.Count;i++){
                raritySum+=WaveZombie[i].baseProperty.rarity;
            }
            fateList[0]= (float)WaveZombie[0].baseProperty.rarity / raritySum;
            for(int i=1;i<fateList.Count;i++){
                fateList[i]=fateList[i-1]+(float)WaveZombie[i].baseProperty.rarity/raritySum;
            }
        }
    }
    public Chess CreateChess(Tile standTile){
        float fate= UnityEngine.Random.Range(0,1f);
        //Debug.Log(fate);
        Chess c=WaveZombie[0].chessPre;
        //Debug.Log(fateList.Count);
        for(int i=0;i<fateList.Count;i++){
            if(fate<fateList[i]){
                c=WaveZombie[i].chessPre;
                CurrentPrice -= WaveZombie[i].baseProperty.price;
                break;
            }
        }
        //Debug.Log(c.propertyController.creator.chessName);
        return GameManage.instance.enemyManage.CreateChess(c.propertyController.creator,standTile );
    }
}