using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HpEventController : MonoBehaviour,Controller
{
    public Chess chess;
    [Serializable]
    public class HpEvent
    {
        public UnityEvent<float> WhenDamageAt;
        public Vector2 hpRange;
    }
    public List<HpEvent> hpEvents;
    public void InitController(Chess chess)
    {
        this.chess = chess;
        chess.propertyController.onGetDamage.AddListener(OnTakeDamage);
    }

    public void OnTakeDamage(DamageMessege mes)
    {
        float hpCurrent = chess.propertyController.GetHp();
        for(int i=0; i < hpEvents.Count; i++)
        {
            if (hpCurrent > hpEvents[i].hpRange.x && hpCurrent < hpEvents[i].hpRange.y)
            {
                hpEvents[i].WhenDamageAt?.Invoke(hpCurrent);
            }
        }
    }

    public void WhenControllerEnterWar()
    {
        //throw new System.NotImplementedException();
    }

    public void WhenControllerLeaveWar()
    {
        //throw new System.NotImplementedException();
    }
}
