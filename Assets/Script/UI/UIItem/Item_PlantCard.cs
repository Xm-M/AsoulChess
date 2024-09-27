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
    //Ȼ�����ﻹҪдһ����˸��Ч������һ������ʱ�����ʧ�Ĵ���
    public override void OnPointerClick(PointerEventData eventData)
    {
        if (!ifselect)
        {
            ifselect = true;
            goodImage.color = new Color(1, 1, 1, 0);
            //�������PrePlantImage��Ч��
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
