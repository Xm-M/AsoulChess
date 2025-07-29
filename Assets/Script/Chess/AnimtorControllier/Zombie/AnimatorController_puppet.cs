using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;
using Sirenix.OdinInspector;
public class AnimatorController_puppet : AnimatorController
{
    public int TypeMax = 2;
    int current;
    [LabelText("Ä¾Í·ÇÃ»÷")]
    public AudioPlayer BeHit;
    [LabelText("×ÆÉÕÉù")]
    public AudioPlayer Fire;
    [LabelText("Ä¾Í·É¢¼Ü")]
    public AudioPlayer Death;
    public override void InitController(Chess chess)
    {
        base.InitController(chess);
    }
    public override void WhenControllerEnterWar()
    {
        base.WhenControllerEnterWar();
        current = Random.Range(0, TypeMax);
    }
    public override void OnGetDamage(DamageMessege dm)
    {
        base.OnGetDamage(dm);
        if ((dm.damageElementType & ElementType.Fire) != 0)
        {
            Fire?.RandomPlay();
        }
        else if ((dm.damageElementType & ElementType.Bullet) != 0)
        {
            BeHit?.RandomPlay();
        }
    }
    public override void PlayAttack()
    {
        if(current == 0)
            base.PlayAttack();
        else
        {
            animator.Play(string.Format("attack{0}",current));
        }
    }
    public override void PlayMove()
    {
        if(current==0)
            base.PlayMove();
        else
        {
            animator.Play(string.Format("run{0}", current));
        }
    }
    public override void PlaySkill()
    {
        if(current==0)
            base.PlaySkill();
        else
        {
            animator.Play(string.Format("skill{0}", current));
        }
    }
    public override void PlayDeath()
    {
        Death?.RandomPlay();
        if(current==0)
            base.PlayDeath();
        else
        {
            animator.Play(string.Format("death{0}",current));
        }
    }
}
