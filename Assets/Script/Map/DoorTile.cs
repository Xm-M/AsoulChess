using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorTile : Tile
{
    //override 
    public List<PropertyCreator> zombieList;
    Animator animator;
    protected override void Awake()
    {
        base.Awake();
        animator = GetComponent<Animator>();
    }
    public void CreateZombie()
    {
        if (zombieList.Count > 0)
        {
            int n =Random.Range(0,zombieList.Count);
            Chess c = CreateChess(zombieList[n], this);
        }
    }
    public void OpenDoor()
    {
        animator.Play("open");
    }
    public Chess CreateChess(PropertyCreator creator,Tile standTile)
    {
        Chess chess = ChessTeamManage.Instance.CreateChess(creator, standTile, "Enemy");
        float dx = Random.Range(0, 3.75f);
        chess.transform.position = standTile.transform.position;//Î»ÖÃÆ«ÒÆ
        return chess;
    }
}
