using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reward : MonoBehaviour
{
    public virtual void PickReward(){
        
    }
    void OnMouseDown()
    {
        PickReward();
    }
}
