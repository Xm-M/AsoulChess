using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BowlingArmor : CarArmor
{
    //int prey=-1;
    //float prePosy=-100;

    public Collider2D col;
    public override void InitArmor()
    {
        base.InitArmor();
        //prey = -1;
        user.WhenEnterGame.AddListener(ResetArmor);
        col = GetComponent<Collider2D>();
    }
    public override void ResetArmor(Chess chess)
    {
        base.ResetArmor(chess);
        //prey = -1;
        col.enabled = true;
    }
    //private void Update()
    //{
    //    if (Mathf.Abs(prePosy - user.transform.position.y) > 2.5f)
    //    {
    //        if (user.transform.position.y > prey)
    //        {
    //            prey += 1;
    //            prePosy += 2.5f;
    //        }
    //        else {
    //            prey -= 1;
    //            prePosy -= 2.5f;
    //        } 
    //    }
    //}
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!collision.CompareTag(user.tag))
        {
            Chess chess = collision.GetComponent<Chess>();
            //if (chess.moveController.standTile.mapPos.y != prey
            //    &&Mathf.Abs(chess.transform.position.y-user.transform.position.y)<0.1f)
            //Debug.Log(Mathf.Abs(chess.moveController.standTile.transform.position.y - user.transform.position.y));
            if (Mathf.Abs(chess.moveController.standTile.transform.position.y - user.transform.position.y) < 0.1f)
            {
                dm.damageFrom = user;
                dm.damageTo = chess;
                dm.damage = user.propertyController.GetAttack();
                //Debug.Log("Åö×²");
                onHit?.Invoke();
                user.propertyController.TakeDamage(dm);
                if (player != null)
                {
                    player.RandomPlay();
                }
                //col.enabled = false;
                //prey=chess.moveController.standTile.mapPos.y;
                //prePosy = chess.moveController.standTile.transform.position.y;
                StartCoroutine(ReHitAble());
            }
        }
    }
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag(user.tag))
        {
            Chess chess = collision.GetComponent<Chess>();
            //if (chess.moveController.standTile.mapPos.y != prey
            //    &&Mathf.Abs(chess.transform.position.y-user.transform.position.y)<0.1f)
            //Debug.Log(Mathf.Abs(chess.moveController.standTile.transform.position.y - user.transform.position.y));
            if(Mathf.Abs(chess.moveController.standTile.transform.position.y - user.transform.position.y) < 0.1f)
            {
                dm.damageFrom = user;
                dm.damageTo = chess;
                dm.damage = user.propertyController.GetAttack();
                //Debug.Log("Åö×²");
                onHit?.Invoke();
                user.propertyController.TakeDamage(dm);
                if (player != null)
                {
                    player.RandomPlay();
                }
                //col.enabled = false;
                //prey=chess.moveController.standTile.mapPos.y;
                //prePosy = chess.moveController.standTile.transform.position.y;
                StartCoroutine(ReHitAble());
            }
        }
        
    }
    
    IEnumerator ReHitAble()
    {
        col.enabled = false;
        //Debug.Log(3.1f / user.propertyController.GetMoveSpeed());
        yield return new WaitForSeconds(2.5f / user.propertyController.GetMoveSpeed());
        col.enabled = true;
        Debug.Log("Åö×²ÖØÐÂÆô¶¯");
    }
}
