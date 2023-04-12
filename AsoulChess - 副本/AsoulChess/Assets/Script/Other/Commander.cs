using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Commander : MonoBehaviour
{
    public float moveSpeed;
    public bool facingRight=true;
    private void Update()
    {
        float i = Input.GetAxisRaw("Horizontal");
        transform.position = Vector2.MoveTowards(transform.position, new Vector2(transform.position.x+i, transform.position.y), moveSpeed * Time.deltaTime);
        if ((facingRight && i < 0) || (!facingRight && i > 0))
        {
            facingRight = !facingRight;
            transform.Rotate(0, 180, 0);
        }
        i = Input.GetAxisRaw("Vertical");
        transform.position = Vector2.MoveTowards(transform.position, new Vector2(transform.position.x, transform.position.y+i), moveSpeed * Time.deltaTime);

    }
}
