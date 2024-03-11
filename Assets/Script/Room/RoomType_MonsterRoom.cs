using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="NewMonsterRoom",menuName ="RoomType/MonsterRoom")]
public class RoomType_MonsterRoom : RoomType
{
   //public List<Chess> monsters;
   //public int maxX,minX;

   //public GameObject Reward;
   //int[] dy = { 4, 2, 6, 0, 3, 5, 7, 1 };
   // public override void WhenEnterRoom()
   // {
   //     //base.WhenEnterRoom();
   //     //foreach(var chess in monsters){
   //     //    Vector2Int pos=SelectPos(chess);
   //     //    Tile standTile=MapManage.instance.tiles[pos.x,pos.y];
   //     //    Chess c=GameManage.instance.enemyManage.CreateChess(chess,standTile,"Enemy");
   //     //    EventController.Instance.TriggerEvent<Chess>(EventName.ChessEnterDesk.ToString(),c);
   //     //}
   //     ////EventController.Instance.AddListener(EventName.TeamDeath.ToString(),RoomClear);   
   //     //EventController.Instance.AddListener(EventName.GameOver.ToString(),WhenGameOver);
   //     //UIManage.instance.RoomButton.AddListener(EnterWar,"ATTACK!");    
   // }
   // public void EnterWar(){
   //     Debug.Log("EnterWar");
   //     GameManage.instance.GameStart();
   //     //UIManage.instance.RoomButton.BanButton();
   // }
   // public override void WhenLeaveRoom()
   // {
   //     base.WhenLeaveRoom();
   //     //ChessFactory.instance.ClearTeam("Enemy");
   // }

   // public void WhenGameOver(){
   //     //BaseButton button=UIManage.instance.RoomButton;
   //     GameObject reward=Instantiate(Reward);
   //     EventController.Instance.RemoveListener(EventName.GameOver.ToString(),WhenGameOver);
   //     reward.transform.position=MapManage.instance.tiles[8,3].transform.position;
   //     button.ClearButton();
   //     button.ReUse();
   //     button.AddListener(GameOverButton,"GO!");
   // }
   // public void GameOverButton(){{
   //     UIManage.instance.RoomButton.ClearButton();
        
   //     RoomManage.instance.RandomRoom();
   // }}

   // public Vector2Int SelectPos(Chess chess)
   // {
   //     int x = (int)chess.propertyController.GetAttackRange()+5;
   //     if (x > maxX) x = maxX;
   //     if (x < minX) x = minX;
   //     int y=0;
        
   //     for(int i = 0; i < dy.Length; i+=1)
   //     {
   //         if (MapManage.instance.tiles[x, dy[i]].IfMoveable) { 
   //             y=dy[i];
   //             break;
   //         }
   //     }
   //     return new Vector2Int(x, y);
   // }
}
