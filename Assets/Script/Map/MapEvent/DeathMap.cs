using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathMap : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Chess c=collision.GetComponent<Chess>();
        Debug.Log(collision.name);
        if (c != null) c.Death();
        else
        Destroy(collision.gameObject);
    }
}
