//using Sirenix.OdinInspector;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class SkillEffect_Pingun : ISkill
//{
//    public bool IfSkillReady(Chess user)
//    {
//        //throw new System.NotImplementedException();
//        return false;
//    }

//    public void InitSkill(Chess user)
//    {
         
//    }

//    public void LeaveSkill(Chess user)
//    {
         
//    }

//    public void UseSkill(Chess user)
//    {
//        user.stateController.ChangeState(StateName.MoveState);
//        Tile t = user.moveController.standTile;
//        user.moveController.standTile.ChessLeave(user);
//        user.moveController.standTile = t;
//    }

//    public void WhenEnter(Chess user)
//    {
         
//    }
//}


//public class PassiveSkill_HitExplode : ISkill
//{

//    [SerializeReference]
//    [LabelText("寻敌方式")]
//    public IFindTarget findTarget;
//    [LabelText("爆炸伤害倍率")]
//    public float explodeRate;
//    public CarArmor armor;
//    List<Chess> targets;
//    float t;
//    Chess user;
//    public bool IfSkillReady(Chess user)
//    {
//        return false;
//    }
//    public void InitSkill(Chess user)
//    {
//        t = 0;
//        targets = new List<Chess>();
//        this.user = user;
//        armor.onHit.AddListener(Explode);
        
//    }

//    public void LeaveSkill(Chess user)
//    {
//        t = 0;
//    }
//    public void UseSkill(Chess user)
//    {
        
//    }
//    public void Explode()
//    {
        
//        findTarget.FindTarget(user, targets);
//        for (int i = 0; i < targets.Count; i++)
//        {
//            user.skillController.DM.damageFrom = user;
//            user.skillController.DM.damageTo = targets[i];
//            user.skillController.DM.damageElementType = ElementType.Explode;
//            user.skillController.DM.damage =
//                user.propertyController.GetAttack() * explodeRate;
//            user.propertyController.TakeDamage(user.skillController.DM);
//        }
//        user.Death();
//    }
//    public void WhenEnter(Chess user)
//    {
//        t = 0;
//        targets.Clear();
//    }
//}
//public class PassiveSkill_Uik : ISkill
//{
//    Chess user;
//    public bool IfSkillReady(Chess user)
//    {
//        //throw new System.NotImplementedException();
//        return false;
//    }

//    public void InitSkill(Chess user)
//    {
//        //throw new System.NotImplementedException();
//        this.user = user;
//    }

//    public void LeaveSkill(Chess user)
//    {
//        EventController.Instance.RemoveListener<Chess>(EventName.WhenPlantChess.ToString(), WhenSakiPlant);
//    }

//    public void UseSkill(Chess user)
//    {
//        //throw new System.NotImplementedException();
//        EventController.Instance.AddListener<Chess>(EventName.WhenPlantChess.ToString(), WhenSakiPlant);
//    }

//    public void WhenEnter(Chess user)
//    {
//        //throw new System.NotImplementedException();
//    }

//    public void WhenSakiPlant(Chess saki)
//    {
//        if (saki.propertyController.creator.chessName.Contains("丰川祥子")&&
//            saki.moveController.standTile.mapPos.y==user.moveController.standTile.mapPos.y
//            && user.stateController.currentState.state.stateName != StateName.MoveState)
//        {
//            user.stateController.ChangeState(StateName.MoveState);
//            Tile t = user.moveController.standTile;
//            user.moveController.standTile.ChessLeave(user);
//            user.moveController.standTile = t;
//        }
//    }
//}