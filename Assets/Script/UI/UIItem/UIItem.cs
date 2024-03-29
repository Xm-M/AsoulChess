using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class UIItem : MonoBehaviour,IPointerClickHandler
{

    public abstract void OnPointerClick(PointerEventData eventData);
    public abstract void Recycle();
     
    
}
