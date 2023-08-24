using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhenMove : MonoBehaviour
{
    public GameObject effect;
    public Transform pos;
    public void WhenMoving()
    {
        ObjectPool.instance.Create(effect).transform.position=pos.transform.position;
    }
}
