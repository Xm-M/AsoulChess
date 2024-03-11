using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class ChessFactory : MonoBehaviour
{
    public Dictionary<string,List<Chess>> teams;
    Dictionary<string, Stack<Chess>> chessPool;
    public Dictionary<Chess,ChessDate> chessMessages;
    public static ChessFactory instance;
    public LayerMask playerLayer, enemyLayer;

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
        chessPool = new Dictionary<string, Stack<Chess>>();
        teams=new Dictionary<string, List<Chess>>();
        chessMessages=new Dictionary<Chess, ChessDate>();
        teams.Add("Player",new List<Chess>());
        teams.Add("Enemy",new List<Chess>());
        //EventController.Instance.AddListener(EventName.GameStart.ToString(),SaveAllChessMessage);
        //EventController.Instance.AddListener(EventName.GameStart.ToString(),SaveAllChessMessage);
        //EventController.Instance.TriggerEvent(EventName.GameStart.ToString);
    }
    public Chess ChessCreate(Chess chessPre,Tile standTile,string tag){
        Chess chess=Create(chessPre);
        if (!teams.ContainsKey(tag)) teams.Add(tag, new List<Chess>());
        teams[tag].Add(chess);
        chess.tag = tag;
        if (chess.CompareTag("Enemy"))
        {
            chess.transform.right=Vector2.left;
            chess.gameObject.layer = LayerMask.NameToLayer(tag);
            chess.GetComponent<SpriteRenderer>().material.SetColor("_color", colors[1]);
            standTile.ChessMoveIn(chess);
        }
        else
        {
            chess.GetComponent<SpriteRenderer>().material.SetColor("_color", colors[0]);
            chess.gameObject.layer = LayerMask.NameToLayer(tag);
            standTile.ChessEnter(chess);
        }
        chess.instanceID=instanceID;
        instanceID++;
        return chess;
    }
   
    public void RecycleChess(Chess c){
        //如果调用了这个 说明c已经寄了
        if(teams[c.tag].Contains(c))
            teams[c.tag].Remove(c);
        Recycle(c);
        
         
    }
    public Chess Create(Chess c)
    {
        Chess creat;
        if (chessPool.ContainsKey(c.name))
        {
            if (chessPool[c.name].Count != 0)
            {
                creat = chessPool[c.name].Pop();
                creat.gameObject.SetActive(true);
                return creat;
            }
        }
        else
        {
            chessPool.Add(c.name, new Stack<Chess>());
        }
        creat = Instantiate(c.gameObject,transform).GetComponent<Chess>();
        creat.InitChess();
 
        return creat;
    }
    public void Recycle(Chess a)
    {
        string name = a.name.Replace("(Clone)", "");
        if (!chessPool[name].Contains(a))
        {
            chessPool[name].Push(a);
        }
        else
        {
            Debug.LogWarning("bug了" + a.name);
        }
        a.gameObject.SetActive(false);
    }
    public List<Chess> FindEnemyList(string tag){
        if(tag=="Player")return teams["Enemy"] ;
        else return teams["Player"] ;
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
            teams[chess.tag] .Add(chess);
            chessMessages[chess].LoadMessage(chess);
        }
    }
 
    public void ClearTeam(string tag){

        if(!teams.ContainsKey(tag)){
            Debug.Log("不存在这队伍");
            return;
        }
        for(int i = 0; i < teams[tag].Count;i++){
            //Debug.Log(chess);
            RecycleChess(teams[tag][i]);
        }
        EventController.Instance.TriggerEvent<string>(EventName.TeamDeath.ToString(),tag);
        teams[tag].Clear();
    }
}
public class ChessDate{
    public Tile standTile;   
    public void SaveMessage(Chess chess){{
        standTile=chess.moveController.standTile;
    }}
    public void LoadMessage(Chess chess){
        Debug.Log(chess.name);
        //chess.ResetAll();
        standTile.ChessEnter(chess);
    }
}
 