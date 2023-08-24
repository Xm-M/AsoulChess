using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class Tile : MonoBehaviour
{
    public Vector3Int cubePos;
    public Vector2Int mapPos;
    //public Color baseColor;

    public AudioSource au ;
    public bool ifPrePareTile;

    [SerializeField]bool ifMoveable;
    public bool IfMoveable { get
        {
            return ifMoveable;
        }
        protected set
        {
            ifMoveable = value;
        }
    }
    public Chess Stander { get;private set; }
    private void Awake()
    {
        //baseColor = Color.white;
        GetComponent<Collider2D>().enabled = false;
        IfMoveable = true;
        EventController.Instance.AddListener(EventName.GameOver.ToString(), () =>
        {
            IfMoveable = true;
            Stander = null;
        });
    }
    public void ChessMoveIn(Chess chess)
    {
        chess.moveController.standTile = this;
        chess.transform.position = transform.position;
    }
 

    public void ChessEnter(Chess chess )
    {
        IfMoveable = false;
        if (Stander == null) 
            Stander = chess;
        chess.moveController.standTile=this;
        chess.transform.position = transform.position;
    }
    public void ChessLeave(Chess chess)
    {
        if (chess == Stander)
        {
            Debug.Log("chessLeave"+chess.name);
            IfMoveable = true;
            Stander = null;
        }
    }
    public void SetColor()
    {
        GetComponent<SpriteRenderer>().color = Color.red;
    }
    public void SetColor(Color color)
    {
        GetComponent<SpriteRenderer>().color = color;
    }
    void OnMouseDown()
    {
        if(Stander==null){
            MapManage.instance.SleepTile();
            PlantsShop.instance.BuyPlant(this);
            au?.Play();
            //要改的其实是这里，就是能不能占用的判断依据不是直接用ifMoveAble
        } 
    }
}
