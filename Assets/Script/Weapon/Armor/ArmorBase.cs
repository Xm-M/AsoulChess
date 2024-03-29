using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Armor�ı�����ΪChess���ˣ�������������Ҫ�ô����Ǽ�����WhenGetDamage�¼���
/// WhenLeaveGame �м��������1.ʹ��������ʱ������˵���ű���ƨ�ɣ�
/// 2.�Լ�����(�����˺��ﵽ����)
/// 3.�������������ᣨ����������� 
/// </summary>
public abstract class ArmorBase : MonoBehaviour,IDamageable
{
    public Chess user;
    public ArmorType type;
    private void Awake()
    {
        InitArmor();
    }
    public abstract void InitArmor();
    public abstract void ResetArmor();
    public abstract void GetDamage(DamageMessege dm);
    public abstract void BrokenArmor();
    
}
public interface IDamageable
{
    public void GetDamage(DamageMessege dm);
}
[Flags]
public enum ArmorType
{
    None=0,
    Head=1<<0,//�Ƿ�ͷ�� ����һ�����
    Invincible=1<<1,//�Ƿ��޵�
    Metal=1<<2,//�Ƿ����
    PassDamage=1<<3,//�Ƿ񴫵��˺�
}
