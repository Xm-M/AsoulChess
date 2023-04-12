using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Round : Weapon
{
    public float moveSpeed=1;
    public override void Attack()
    {
        if (ifAttack == false)
        {
            GetComponent<Animator>().Play("attack");
            ifAttack = true;
            StartCoroutine(Move());
        }
    }
    IEnumerator Move()
    {
        while (master.target&&Mathf.Abs(transform.position.x - master.target.transform.position.x) > 0.1)
        {
            timer = 0;
            transform.position = Vector2.MoveTowards(transform.position, master.target.transform.position, moveSpeed * Time.deltaTime);
            yield return null;
        }
        while(Mathf.Abs(transform.position.x - master.transform.position.x) > 0.1)
        {
            timer = 0;
            transform.position = Vector2.MoveTowards(transform.position, master.transform.position, moveSpeed * Time.deltaTime);
            yield return null;
        }
        //Stand();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Chess>() && !collision.CompareTag(master.tag))
        {
            TakeDamage(collision.GetComponent<Chess>());
        }
    }
}
