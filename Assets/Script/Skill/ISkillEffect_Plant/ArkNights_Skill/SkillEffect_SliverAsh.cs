using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 这个是银灰的技能
/// 几个点1.yy部署后 对应后台位变为风暴中心
/// 2.部署后后台费用按高低排序
/// 3.之后才是对应的各种效果
/// 4.部署撤回后后台对应位置变回yy
/// 那就是把plantsshop的currentSelect遍历一下 然后按费用排序就好 对应的是shopicon
/// </summary>
/// 

//public class SkillEffect_SliverAsh : ISkill
//{
//    public PropertyCreator stormEye;//风暴之眼
//    public Dictionary<PropertyCreator, int> StormDic;
//    public GameObject stormSlide;
//    public int dePrice=100;//减少的费用 应该是100
//    public float coldDown;
//    public MouseDownSkill ifMouseDown;
//    float t;
//    ShopIcon self;
//    Chess user;
//    public bool IfSkillReady(Chess user)
//    {
//        t += Time.deltaTime;
//        //这里先大于0 实际是大于1 就是四个成员在场时才能使用的技能
//        if (t > coldDown)
//        {
//            user.animatorController.ChangeFlash(-1);
//            if (ifMouseDown.IfDown)
//            {
//                user.animatorController.ChangeFlash(1);
//                t = 0;
//                return true;
//            }
//            return false;
//        }
//        else
//        {
//            return false;
//        }
//    }
//    //Init是全局只用一次吗 忘了
//    public void InitSkill(Chess user)
//    {
//        StormDic = new Dictionary<PropertyCreator, int>();
//        this.user = user;
//    }

//    public void LeaveSkill(Chess user)
//    {
//        self.RefreshGood(user.propertyController.creator);
//        EventController.Instance.RemoveListener<Chess>(EventName.WhenPlantChess.ToString(), CreateSlide);
//    }
//    //这个技能的效果
//    public void UseSkill(Chess user)
//    {
//        CreateSlide(user);//用一次真银斩
//        List<ShopIcon> icons = UIManage.GetView<PlantsShop>().currentShopIcons;
//        int n=icons.IndexOf(self);
//        if (n + 1 < icons.Count)
//        {
//            icons[n + 1].ChangePrice(-dePrice);
//        }
//        else if(n-1>=0)
//        {
//            icons[n - 1].ChangePrice(-dePrice);
//        }
//        SortShopIcon();
//        //之后要给所有左边的单位增加一次真银斩
//        n=icons.IndexOf(self);
//        for(int i = 0; i < n; i++)
//        {
//            if (!StormDic.ContainsKey(icons[i].good))StormDic.Add(icons[i].good, 1);
//            else StormDic[icons[i].good]=Mathf.Min(2,StormDic[icons[i].good]+1);
//        }
//        StormDic.Add(user.propertyController.creator,1);
//    }

//    /// <summary>
//    /// 这里要改成 两段的情况
//    /// </summary>
//    /// <param name="chess"></param>
//    public void CreateSlide(Chess chess)
//    {
//        PropertyCreator c = chess.propertyController.creator;
//        //这里应该用一个循环 或者用一个携程
//        if ((StormDic.ContainsKey(c))) {
//            GameObject b = ObjectPool.instance.Create(stormSlide);
//            Bullet zidan = b.GetComponent<Bullet>();
//            zidan.InitBullet(chess, chess.equipWeapon.weaponPos.position, chess, chess.transform.right);
//            StormDic[c]--;
//            if (StormDic[c] <= 0) StormDic.Remove(c);
//        }
//    }
//    /// <summary>
//    /// 那么enter之后 第一件事是吧自己的位置变成风暴之眼 然后第二件事是排序
//    /// </summary>
//    /// <param name="user"></param>
//    public void WhenEnter(Chess user)
//    {
//        //这里是找到yh的位置 然后替换为眼
//        List<ShopIcon> icons = UIManage.GetView<PlantsShop>().currentShopIcons;
//        self = null;
//        for(int i = 0; i < icons.Count; i++)
//        {
//            if (icons[i].good = user.propertyController.creator)
//            {
//                self = icons[i];
//                break;
//            }
//        }
//        self.RefreshGood(stormEye);
//        //替换之后根据费用排序
//        SortShopIcon();
//        //然后就是各种事件的订阅对吧 
//        EventController.Instance.AddListener<Chess>(EventName.WhenPlantChess.ToString(), CreateSlide);
//    }
//    public void SortShopIcon()
//    {
//        List<ShopIcon> icons = UIManage.GetView<PlantsShop>().currentShopIcons;
//        for (int i = 0; i < icons.Count; i++)
//        {
//            for (int j = i; j < icons.Count; j++)
//            {
//                //妈的 交换位置的函数怎么写 试一下吧
//                if (icons[j].good.baseProperty.price < icons[i].good.baseProperty.price)
//                {
//                    SwapChildren(UIManage.GetView<PlantsShop>().shopIconParent, icons[j].transform, icons[i].transform);
//                    ShopIcon icon = icons[j];
//                    icons[j] = icons[i];
//                    icons[i] = icon;

//                }
//            }
//        }
//    }
//    public static void SwapChildren(Transform parent, Transform a, Transform b)
//    {
//        if (parent == null || a == null || b == null) return;
//        if (a.parent != parent || b.parent != parent) return; // 确保同一个父节点

//        int indexA = a.GetSiblingIndex();
//        int indexB = b.GetSiblingIndex();

//        // 先把A设成B的位置，再把B设成A原来的位置
//        a.SetSiblingIndex(indexB);
//        b.SetSiblingIndex(indexA);
//    }
//}
///从结果论上看 应该要分成被动技能和主动技能几个部分来