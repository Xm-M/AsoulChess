using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class BaseHandPanel
{
    //public HandItemType type;
    //protected bool stop;
    public abstract IEnumerator Plants(UnityAction CancelPlant, UnityAction<Chess> Plant, PrePlantImage_Data data);

    public virtual void CancleUse() { }
     
}
