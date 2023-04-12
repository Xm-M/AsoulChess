using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class Tile : MonoBehaviour
{
    public Vector3Int cubePos;
    public Vector2 AstarValue;//对应的是G,H,F
    public Tile preTile;
    public Vector2Int mapPos;
    public Color baseColor;

    public bool ifPrePareTile;

    public bool ifMoveable;
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
        EventController.Instance.AddListener(EventName.GameOver.ToString(),ChessLeave);
    }

    public void InitAstarValue()
    {
        AstarValue = Vector3.zero;
        preTile = null;
    }

    public void ChessEnter(Chess chess,bool ifin=false)
    {
        if (Stander!=null&&Stander != chess) Debug.LogError("有人啦");
        if(!ifin)
            chess.transform.position = transform.position;
        IfMoveable = false;
        Stander = chess;
        chess.standTile=this;
        EventController.Instance.TriggerEvent<Tile>(chess.instanceID + EventName.WhenEnterTile.ToString(),this);
        //GetComponent<SpriteRenderer>().color = Color.black;
    }
    public void ChessLeave()
    {
        IfMoveable = true;
        Stander = null;
    }
    public void CantMove()
    {
        IfMoveable=false;
        SetColor(Color.black);
    }
    public void SetColor()
    {
        GetComponent<SpriteRenderer>().color = Color.red;
    }
    public void SetColor(Color color)
    {
        GetComponent<SpriteRenderer>().color = color;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.GetComponent<Chess>() != null)
        {
            baseColor = Color.green;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<Chess>() != null)
        {
            baseColor = Color.white;
        }
    }
}
