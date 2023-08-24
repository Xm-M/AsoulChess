using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Elf : Fetter
{
    public int a;
    public float extraMagic=0;
    public Elf(){
        fetterName = "Elf";
    }
    public override void Start()
    {
        base.Start();
        
        extraMagic = 0;
        Debug.Log(self.instanceID  );
        EventController.Instance.AddListener(self.instanceID.ToString() + EventName.WhenSkillOver.ToString(),
           UseSkillAgent);
        EventController.Instance.AddListener(self.instanceID.ToString() + EventName.WhenChessDestroy.ToString(), OnDestroy);
    }
    public void UseSkillAgent()
    {
        //Debug.Log("useskillagent");
        //if(extraMagic!=0)
        //    self.skill.SkillEffect(self);
    }
    public override void FetterEffect(int num)
    {
        base.FetterEffect(num);
        //self.equipWeapon.attack -= extraAttack;
        switch (num/2)
        {
            case 0: extraMagic = 0; break;
            case 1: extraMagic = 1; break;
            case 2: extraMagic = 2; break;
            default: extraMagic = 3; break;
        }
        Debug.Log(self.name);
        //self.equipWeapon.attack += extraAttack;
    }
    public override void FetterReSet()
    {
        //base.FetterReSet();
        Debug.Log(self.name + "���ʧ");
        //self.equipWeapon.attack -= extraAttack;
        //EventController.Instance.RemoveListener(self.instanceID.ToString() + EventName.WhenSkillOver.ToString(),
        //   UseSkillAgent);
        extraMagic = 0;
    }
    public override void ResetFetter()
    {
        base.ResetFetter();        
    }
    public void OnDestroy()
    {
        Debug.Log("�����ٵľ���"+self.name);
        EventController.Instance.RemoveListener(self.instanceID.ToString() + EventName.WhenSkillOver.ToString(),
           UseSkillAgent);
        EventController.Instance.RemoveListener(self.instanceID.ToString() + EventName.WhenChessDestroy.ToString(),
            OnDestroy);
    }
}
