using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessFactory : MonoBehaviour
{
    public Dictionary<string,ChessTeam> teams;
    public Dictionary<Chess,ChessDate> chessMessages;
    public static ChessFactory instance;
    
    public Color[] colors;
    public int instanceID;
    void Awake()
    {
        if(instance==null){
            instance=this;
        }else{
            Destroy(gameObject);
            return;
        }
        teams=new Dictionary<string, ChessTeam>();
        chessMessages=new Dictionary<Chess, ChessDate>();
        teams.Add("Player",new ChessTeam());
        teams.Add("Enemy",new ChessTeam());
        EventController.Instance.AddListener(EventName.GameStart.ToString(),SaveAllChessMessage);
        EventController.Instance.AddListener(EventName.GameOver.ToString(),LoadAllChessMessage);
        EventController.Instance.AddListener<Chess>(EventName.ChessEnterDesk.ToString(),WhenChessEnterDesk);
        EventController.Instance.AddListener<Chess>(EventName.ChessLeaveDesk.ToString(),WhenChessLeaveDesk);
    }
    public Chess ChessCreate(Chess chessPre,Tile standTile,string tag){
        GameObject c=Instantiate(chessPre.gameObject,transform);
        Chess chess=c.GetComponent<Chess>();
        if(!GameManage.instance.ifGameStart){
        if(!teams.ContainsKey(tag))teams.Add(tag,new ChessTeam());
        teams[tag].teamHold.Add(chess);
        }
        standTile.ChessEnter(chess);
        chess.instanceID=instanceID;
        //根据tile的不同来
        instanceID++;
        c.tag=tag;
        //后面还有更改颜色什么的 一堆东西
        if(tag=="Player")chess.GetComponent<SpriteRenderer>().material.SetColor("_color",colors[0]);
        else if(tag=="Enemy")chess.GetComponent<SpriteRenderer>().material.SetColor("_color",colors[1]);
        else chess.GetComponent<SpriteRenderer>().material.SetColor("_color",colors[2]);
        return chess;
    }
    public void WhenChessEnterDesk(Chess chess){
        teams[chess.tag].teamInDesk.Add(chess);
        Debug.Log(chess.name+"进入棋盘");
    }
    public void WhenChessLeaveDesk(Chess chess){
         teams[chess.tag].teamInDesk.Remove(chess);
         Debug.Log(chess.name+"离开棋盘");
    }
   
    public void RecycleChess(Chess c){
        c.gameObject.SetActive(false);
        if(teams[c.tag].teamHold.Contains(c))
            teams[c.tag].DeathPool.Add(c);
    }
    public bool CheckAllDeath(string tag){
        foreach(var chess in teams[tag].teamInDesk){
            if(!chess.ifDeath)return false;
        }
        return true;
    }
    public List<Chess> FindEnemyList(string tag){
        if(tag=="Player")return teams["Enemy"].teamInDesk;
        else return teams["Player"].teamInDesk;
    }
    public void SaveMessage(Chess chess){
        if(!chessMessages.ContainsKey(chess)){
            chessMessages.Add(chess,new ChessDate());
        }
        chessMessages[chess].SaveMessage(chess);
    }
    public void LoadMessage(Chess chess){
        if(!chessMessages.ContainsKey(chess))Destroy(chess.gameObject);
        else {
            chess.gameObject.SetActive(true);
            teams[chess.tag].teamInDesk.Add(chess);
            chessMessages[chess].LoadMessage(chess);
        }
    }
    public void SaveAllChessMessage(){
        Debug.Log("SaveMessage");
        foreach(var chess in teams["Player"].teamInDesk){
            SaveMessage(chess);
        }
        foreach(var team in teams){
            foreach(var chess in team.Value.teamInDesk){{
                chess.stateController.EnterWar();
            }}
        }
    }
    public void LoadAllChessMessage(){
        teams["Player"].DeathPool.Clear();
        for(int i=0;i<teams["Player"].teamInDesk.Count;i++){
            Chess c=teams["Player"].teamInDesk[i];
            if(!teams["Player"].teamHold.Contains(c)){
                Destroy(c.gameObject);
            }
        }
        teams["Player"].teamInDesk.Clear();
        foreach(var chess in chessMessages){
            LoadMessage(chess.Key);
        }
    }
    public void ClearTeam(string tag){

        if(!teams.ContainsKey(tag)){
            Debug.Log("不存在这队伍");
            return;
        }
        foreach(var chess in teams[tag].teamHold){
            //Debug.Log(chess);
            Destroy(chess.gameObject);
        }
        EventController.Instance.TriggerEvent<string>(EventName.TeamDeath.ToString(),tag);
        teams[tag].Clear();
    }
}
public class ChessDate{
    public Tile standTile;   
    public void SaveMessage(Chess chess){{
        standTile=chess.standTile;
    }}
    public void LoadMessage(Chess chess){
        Debug.Log(chess.name);
        chess.ResetAll();
        standTile.ChessEnter(chess);
    }
}
public class ChessTeam{
    public List<Chess> teamHold;
    public List<Chess> teamInDesk;
    public List<Chess> DeathPool;
    

    public ChessTeam(){
        teamHold=new List<Chess>();
        teamInDesk=new List<Chess>();
        DeathPool=new List<Chess>();
    }
    public void Clear(){
        teamHold.Clear();
        teamInDesk.Clear();
        DeathPool.Clear();
    }
}