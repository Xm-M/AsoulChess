using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class UIItem : MonoBehaviour,IPointerClickHandler
{
    private void Awake()
    {
        EventController.Instance.AddListener(EventName.WhenLeaveLevel.ToString(), Recycle);
        EventController.Instance.AddListener(EventName.EnterMap.ToString(), Recycle);
    }
    private void OnDestroy()
    {
        EventController.Instance.RemoveListener(EventName.WhenLeaveLevel.ToString(), Recycle);
        EventController.Instance.RemoveListener(EventName.EnterMap.ToString(), Recycle);
    }
    public abstract void OnPointerClick(PointerEventData eventData);
    public abstract void Recycle();
     
    
}
