using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Weapon : MonoBehaviour
{
    public Chess master;
    public AudioSource audioSouce;
    public Animator animator;
    public float ExtraMagic = 10f;
    public DamageMessege DM;
    protected float timer = 0;
    protected bool ifAttack = false; 

    public virtual void InitWeapon()
    {
        ifAttack = false;
        timer = 0;
    }
    public virtual void Attack() {
        Chess chess = master;
        if (ifAttack == false)
        {
            animator.Play("attack");
            ifAttack = true;
        }
    }
    public virtual void TakeDamage(Chess target)
    {
        if (target == null) return;
        DM.damageFrom = master;
        DM.damageTo = target;
        DM.damageType = DamageType.Physical;
        master.propertyController.TakeDamage(DM);
    }
    public virtual void TakeDamage()
    {
        TakeDamage(master.target);
    }
    private void Update()
    {
        if (ifAttack)
        {
            timer += Time.deltaTime;
            if (timer > master.propertyController.Data.attackSpeed)
            {
                timer = 0;
                Attack();
            }
        }
    }
    public void PlayAudio()
    {
        if (audioSouce != null)
            audioSouce.Play();
    }
}
//if(ifRoate)
//    chess.equipWeapon.transform.right = chess.target.transform.position - chess.transform.position;
//if (((chess.FacingRight && chess.target.transform.position.x < chess.transform.position.x) || (!chess.FacingRight && chess.target.transform.position.x > chess.transform.position.x))
//){
//    chess.equipWeapon.transform.localScale = new Vector3(1, -chess.equipWeapon.transform.localScale.y, 1);
//}