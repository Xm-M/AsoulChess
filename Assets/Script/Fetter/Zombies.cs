using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zombies : Fetter
{
    public float comeBackHp=0.5f;
    bool ifcomeBack;
    Tile standTile;
    public Zombies(){
        fetterName = "Zombies";
    }
    public override void Start()
    {
        base.Start();
        
        //Debug.Log(self.name + "�����ǽ�ʬ��"+self.instanceID);
        //EventController.Instance.AddListener<>
        // EventController.Instance.AddListener<Tile>(self.instanceID.ToString() + EventName.WhenEnterTile.ToString(),
        //    SetTile);
        // EventController.Instance.AddListener(self.instanceID.ToString() + EventName.WhenDeath.ToString(),
        //     ComeBackToLife);
        // EventController.Instance.AddListener(self.instanceID.ToString() + EventName.WhenChessDestroy.ToString(), OnDestroy);
        // standTile=self.standTile;
    }
    
    public void SetTile(Tile tile)
    {
        //Debug.Log(self + "����tile");
        standTile = tile;
    }
    public void ComeBackToLife()
    {
        //Debug.Log(self.name + "���");
        if (ifcomeBack||!GameManage.instance.ifGameStart)
        {
            ifcomeBack = false;
            return;
        }
        
        //self.IfDeath = false;
        //self.unAttackable = true;
        //self.unSelectable = true;
        self.gameObject.SetActive(true);
        //self.stateController.ChangeState(dizziness.instance);
        //DizzinessState.instance.AddChess(self,2);
        standTile.ChessEnter(self);
        //GameManage.instance.teams[self.tag].AddMember(self);
        //self.property.GetValue(ValueType.Hp).currentValue = self.property.GetValue(ValueType.Hp).startValue*comeBackHp;
        ifcomeBack = true;
        self.StartCoroutine(ImmuneTime());
        //self.animator.Play("revive");
    }
    IEnumerator ImmuneTime()
    {
        yield return new WaitForSeconds(2);
        //self.unAttackable = false;
        //self.unSelectable=false;
    }
    public override void FetterEffect(int num)
    {
        base.FetterEffect(num);
    }
    public void OnDestroy()
    {
        EventController.Instance.RemoveListener<Tile>(self.instanceID.ToString() + EventName.WhenEnterTile.ToString(),
           SetTile);
        EventController.Instance.RemoveListener(self.instanceID.ToString() + EventName.WhenHurt.ToString(),
            ComeBackToLife);
        EventController.Instance.RemoveListener(self.instanceID.ToString() + EventName.WhenChessDestroy.ToString(), OnDestroy);

    }
}
