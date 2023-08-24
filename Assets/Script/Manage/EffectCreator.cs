using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectCreator : MonoBehaviour
{
    public Vector3 offeset;
    public void DeathEffect(GameObject effect){
        GameObject a= ObjectPool.instance.Create(effect);
        a.transform.position=transform.position+offeset;
    }
}
