using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BowlingArmor : CarArmor
{
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
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!collision.CompareTag(user.tag))
        {
            Chess chess = collision.GetComponent<Chess>();
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
