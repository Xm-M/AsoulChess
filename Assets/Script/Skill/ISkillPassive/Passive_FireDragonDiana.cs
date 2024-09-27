using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
/// <summary>
/// attack->startloop(һ֡)->(����weaponֵ)->loop->startLoop ����
/// </summary>
public class Passive_FireDragonDiana : ISkill
{
    [SerializeField]
    public IFindTarget straightFindSingle,straightFindMuty, triangleFind;
    [SerializeField]
    public IAttackFunction shoot, close, laser;
    Chess user;
    //Init �ǳ�ʼ�� ֻ�����һ��
    public void InitSkill(Chess chess)
    {
        user =chess;
    }
    public void ChangeWeapon(int n)
    {
        user.animator.SetInteger("weapon", n);
        switch (n)
        {
            case 0:
                //user.equipWeapon.ChangeWeapon(shoot,straightFindSingle);
                //user.equipWeapon.attack = shoot;
                break;
            case 1:
                //user.equipWeapon.ChangeWeapon(close,straightFindSingle) ;
                //user.equipWeapon.attack = close;
                break;
            case 2:
                //user.equipWeapon.ChangeWeapon(laser,straightFindMuty);
                //user.equipWeapon.attack = laser;
                break;
            default:
                //user.equipWeapon.ChangeWeapon(laser, triangleFind);
                //user.equipWeapon.attack = laser;
                break;
        }
    }
     

    public void UseSkill(Chess user)
    {
        ChangeWeapon(PowerBarPanel.GetView<SweetBar>().GetStage());
        EventController.Instance.AddListener<int>(EventName.WhenSweetChange.ToString(),
            ChangeWeapon);
    }

    public void LeaveSkill(Chess user)
    {
        EventController.Instance.RemoveListener<int>(EventName.WhenSweetChange.ToString(),
            ChangeWeapon);
    }

    public bool IfSkillReady(Chess user)
    {
        return false;
    }
}
