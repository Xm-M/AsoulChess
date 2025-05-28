using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
/// <summary>
/// anno和soyo的彩蛋
/// 只有当场上有anon的时候种植soyo才会触发
/// </summary>
public class HideEgg_AnonSoyo : HideEgg
{
    public List<PropertyCreator> mujicaLists;
    [LabelText("概率")]
    public float rate=0.2f;//概率
    public override bool IfTriggerEgg(Chess chess, Chess target)
    {
        float n = Random.Range(0, 1f);
        //Debug.Log(target.propertyController.creator.chessName);
        //Debug.Log(n);
        //Debug.Log(target.moveController.standTile.mapPos.x - target.moveController.standTile.mapPos.x);
        if (target.propertyController.creator.chessName == "长崎素世" &&
            target.moveController.standTile.mapPos.x-self.moveController.standTile.mapPos.x==-1&&n<=rate)
        {
            Debug.Log("触发菜单_我没有说不去");
            TriggerEggEffect(chess, target);
            return true;
        }
        return false;
    }

    public override void InitEgg(Chess user)
    {
         this.self=user;
         rate = 1;
        mujicaLists = new List<PropertyCreator>();
        var playerChess = Resources.LoadAll<PropertyCreator>("ChessData/Player");//加载UIPrefab文件夹下的所有UI预制体
        foreach (PropertyCreator view in playerChess)
        {
            if (view.chessPre != null)
            {
                if (view.plantTags.Contains("AveMujica"))
                {
                    mujicaLists.Add(view);
                }

            }
        }
    }

    public override void TriggerEggEffect(Chess chess, Chess target)
    {
        target.animatorController.animator.Play("egg_anon");
        chess.animatorController.animator.Play("egg_soyo");
        target.stateController.ChangeState(StateName.EggState);
        chess.stateController.ChangeState(StateName.EggState);
    }

    public override void WhenEggOver()
    {
        //生成一张mujica的成员卡
        Item_PlantCard card= UIManage.GetView<ItemPanel>().Create<Item_PlantCard>();
        Vector2 pos = Camera.main.WorldToScreenPoint(self.transform.position);
        Debug.Log(pos);
        //card.InitCard(nailong, pos, pos, 1);
        card.InitCard(mujicaLists[Random.Range(0, mujicaLists.Count)],pos,pos,1);
        //具体怎么移动 之后再改吧
    }

    public override void WhenEnterWar(Chess user)
    {
         
    }

    public override void WhenLeaveWar(Chess user)
    {
         
    }
}
public class HideEgg_AnonTomorin : HideEgg
{
    public static int count;
    [LabelText("奶龙")]
    public PropertyCreator nailong;
    [LabelText("彩蛋")]
    public float rate = 1f;//概率
    public override bool IfTriggerEgg(Chess chess, Chess target)
    {
        float n=Random.Range(0, 1f);
        Debug.Log(target.propertyController.creator.chessName);
        if (n<=rate&&target.propertyController.creator.chessName == "高松灯" &&
            target.moveController.standTile.mapPos.x-chess.moveController.standTile.mapPos.x==1)
        {
            Debug.Log("触发彩蛋_TWGgroup");
            TriggerEggEffect(chess, target);
            return true;
        }
        return false;
    }

    public override void InitEgg(Chess user)
    {
        //throw new System.NotImplementedException();
        rate = 1;
        //mujicaLists = new List<PropertyCreator>();
        self = user;
        var playerChess = Resources.LoadAll<PropertyCreator>("ChessData/Player");//加载UIPrefab文件夹下的所有UI预制体
        foreach (PropertyCreator view in playerChess)
        {
            if (view.chessPre != null)
            {
                if (view.chessName=="迈巴赫")
                {
                    nailong = view;
                    return;
                }

            }
        }
    }

    public override void TriggerEggEffect(Chess chess, Chess target)
    {
        //chess.animatorController.ChangeFloat("egg",count);
        if (count == 0)
        {
            chess.animatorController.animator.Play("egg_tomorin");
        }
        else if(count == 1)
            chess.animatorController.animator.Play("egg_tomorin1");
        else if(count == 2)
            chess.animatorController.animator.Play("egg_tomorin2");
        else if(count==3)
            chess.animatorController.animator.Play("egg_tomorin3");
        count++;
        target.animatorController.animator.Play("egg_anon");
        ///chess.animatorController.animator.Play("egg_tomorin");
        //target.stateController.ChangeState(StateName.EggState);
        chess.stateController.ChangeState(StateName.EggState);
    }

    public override void WhenEggOver()
    {
        //count++;
        if (count == 4)
        {
            //Debug.Log("生成奶龙");
            count = 0;
            //生成一张粉色奶龙卡
            Item_PlantCard card = UIManage.GetView<ItemPanel>().Create<Item_PlantCard>();
            Vector2 pos = Camera.main.WorldToScreenPoint(self.transform.position);
            Debug.Log(pos);
            card.InitCard(nailong , pos, pos, 1);
            //具体怎么移动 之后再改吧
        }
    }

    public override void WhenEnterWar(Chess user)
    {
        //throw new System.NotImplementedException();
    }

    public override void WhenLeaveWar(Chess user)
    {
        //wdthrow new System.NotImplementedException();
    }
}
//之后可以做个彩蛋