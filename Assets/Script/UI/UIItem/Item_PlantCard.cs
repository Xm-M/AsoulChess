using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class Item_PlantCard : UIItem
{
    public PropertyCreator creator;
    public Image goodImage;
    bool ifselect;
    public UnityEvent WhenRecycle;
    public void InitCard(PropertyCreator p,UnityAction WhenReycle=null)
    {
        creator = p;
        ifselect = false;
        if(WhenReycle!=null)WhenRecycle.AddListener(WhenReycle);
    }
    //然后这里还要写一个闪烁的效果，和一个过了时间就消失的代码
    public override void OnPointerClick(PointerEventData eventData)
    {
        if (!ifselect)
        {
            ifselect = true;
            goodImage.color = new Color(1, 1, 1, 0);
            //这里调用PrePlantImage的效果
            PrePlantImage.instance.TryToPlant(creator, CancelPlant, Recycle);
        }
    }
    public void CancelPlant()
    {
        ifselect = false;
        goodImage.color = new Color(1, 1, 1, 1);
    }
    public override void Recycle()
    {
        UIManage.GetView<ItemPanel>().Recycle<Item_PlantCard>(this);
        WhenRecycle?.Invoke();
    }
}
