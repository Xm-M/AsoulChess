using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadArmor :ArmorBase
{
    public float armorMax;
    public Animator animator;
    public string animName;
    float armorCurrent;
    float brokenPercent;
    /// <summary>
    /// ����ǳ�ʼ����ֻ�ᱻ����һ��
    /// </summary>
    public override void InitArmor()
    {
        armorCurrent = armorMax;
        user.WhenEnterGame.AddListener((Chess) => ResetArmor());
    }
    public override void GetDamage(DamageMessege dm)
    {
        if (armorCurrent >= dm.damage)
        {
            Debug.Log("���ߵ���");
            armorCurrent -= dm.damage;
            dm.damage = 0;
        }
        else
        {
            Debug.Log("�������");
            dm.damage -= armorCurrent;
            armorCurrent = 0;
            BrokenArmor();
        }
        brokenPercent = armorCurrent / armorMax;
        animator?.SetFloat(animName, brokenPercent);
    }
    /// <summary>
    ///  �������ú���
    /// </summary>
    /// <exception cref="System.NotImplementedException"></exception>
    public override void ResetArmor()
    {
        tag = user.tag;
        user.propertyController.onSetDamage.AddListener(GetDamage);
        armorCurrent = armorMax;
    }
    /// <summary>
    ///   �Ƿ����𻵺���
    /// </summary>
    /// <exception cref="System.NotImplementedException"></exception>
    public override void BrokenArmor()
    {
        //�����ǰ�����Ļ����Ҳ�ǻᱻ����� ���Կ��Բ���
        Debug.Log("�����ƻ�");
        user.propertyController.onSetDamage.RemoveListener(GetDamage);
    }
}
