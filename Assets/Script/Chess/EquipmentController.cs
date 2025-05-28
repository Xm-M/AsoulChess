using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// head 优先承受伤害
/// body 就是有一类防具 二类防具的说法对吧
/// Front 只承受来自前方的伤害 并且有碰撞体积
/// back 只承受来自后方的伤害 并且有碰撞体积
/// </summary>
public class EquipmentController : MonoBehaviour,Controller
{
    public Transform head, body, Front,Back;//这个主要是放在哪的问题

    public void InitController(Chess chess)
    {
        //throw new System.NotImplementedException();
    }

    public void WhenControllerEnterWar()
    {
        //throw new System.NotImplementedException();
    }

    public void WhenControllerLeaveWar()
    {
        //throw new System.NotImplementedException();
    }
    /// <summary>
    /// 装备的时候 首先会触发装备的WhenEquip(Chess chess);的效果
    /// 然后 Equipment.transform.postion=对应位置.transform.position;
    /// 再接下来 好像就没有了 
    /// 现在的问题就是装备到底要不要额外制作一个承伤系统 还是直接当做chess来做 
    /// 我是觉得额外做个承伤比较好 但是很多findtargte 啊 子弹的ontriggerEnter什么的都是直接对那个生效的 
    /// 这就很烦了 算了 还是下周再做吧 
 
    /// </summary>
    public void EquipHead()
    {

    }
    public void EquipBody()
    {

    }
    public void EquipFront()
    {

    }
    public void EquipBack()
    {

    }
}
