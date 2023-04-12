using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HolyLight : Weapon
{
    public float speed;
    public bool ifMove;
    
    public override void Attack()
    {
        GetComponent<Animator>().SetBool("attack",true);
        if(!ifMove)StartCoroutine(Move());  
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if(collision.GetComponent<Chess>() != null && !CompareTag(master.tag))
        {
            collision.GetComponent<Chess>().Death();
        }
    }
    IEnumerator Move()
    {
        ifMove=true;
        
        Chess t = master.target;
        while((transform.position-t.transform.position).magnitude > 0.01f&&t.enabled)
        {
            transform.position = Vector2.MoveTowards(transform.position, t.transform.position, speed * Time.deltaTime);
            yield return null;
        }
        ifMove=false;
    }
    public void End()
    {
        GetComponent<Animator>().SetBool("attack", false);
    }
    
}
