using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorController : MonoBehaviour,Controller
{
    public Animator animator;
    public SpriteRenderer sprite;
    protected Chess chess;
    public virtual void InitController(Chess chess)
    {
        this.chess = chess;
        if (animator == null)animator = GetComponent<Animator>();
        //throw new System.NotImplementedException();
    }
    public virtual void WhenControllerEnterWar()
    {
        ChangeColor(Color.white);
    }
    public bool IfAnimPlayOver()
    {
        AnimatorStateInfo animStateInfo = chess.animatorController.animator.GetCurrentAnimatorStateInfo(0);
        if (animStateInfo.normalizedTime >=1f && !animator.IsInTransition(0))
        {
            //Debug.Log("动画播放结束");
            return true;
        }
        return false;
    }
    public virtual void WhenControllerLeaveWar()
    {
        SetOutline(Color.white, 0);
    }
    public virtual void PlayIdle()
    {
        animator.Play("idle");
    }
    public virtual void PlayMove()
    {
        animator.Play("run");
    }
    public virtual void PlayAttack()
    {
        animator.Play("attack");
    }
    public virtual void PlayDeath()
    {
        animator.Play("death");
    }
     
    public virtual void OnGetDamage(DamageMessege dm)
    {
        sprite.material.SetFloat("_FlashAmount", Time.time);
    }
    public virtual void PlaySkill()
    {
   
        animator.Play("skill");
    }
    public virtual void ChangeSpeed(float value)
    {
      
        animator.speed = value;
    }
    public virtual void ChangeInt(int value)
    {
        animator.SetInteger("skill", value);
    }

    public virtual void ChangeColor(Color color)
    {
        sprite.color = color;
    }
    public virtual void ChangeFlash(float value)
    {
        sprite.material.SetFloat("_ColdDown", value);
    }
    public virtual void ChangeFloat(string vname,float value)
    {
        animator.SetFloat(vname, value);
    }
    public virtual void Freezy()
    {
        ChangeSpeed(0);
    }
    public virtual void ResumeSpeed()
    {
        ChangeSpeed(chess.propertyController.GetAccelerate());
    }
    public virtual void PlayDizzy()
    {
        animator.Play("idle");
    }
    public virtual string GetCurrentAnimName()
    {
        var anim = GetComponent<Animator>();
        AnimatorStateInfo info = anim.GetCurrentAnimatorStateInfo(0);

        string fullPathName = anim.GetCurrentAnimatorClipInfo(0)[0].clip.name;
        //Debug.Log("当前动画名：" + fullPathName);
        return fullPathName;
    }
    public void SetOutline(Color _OutlineColor,float _OutlineSize)
    {
        sprite.material.SetColor("_OutlineColor", _OutlineColor);
        sprite.material.SetFloat("_OutlineSize",_OutlineSize);
    }
}
