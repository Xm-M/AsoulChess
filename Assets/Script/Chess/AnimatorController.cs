using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorController : MonoBehaviour,Controller
{
    public Animator animator;

    /// <summary>
    /// 判断 Animator 是否包含指定参数，避免 GetInteger 等调用不存在的参数时产生警告
    /// </summary>
    public static bool HasParameter(Animator anim, string paramName)
    {
        if (anim == null) return false;
        for (int i = 0; i < anim.parameterCount; i++)
        {
            if (anim.GetParameter(i).name == paramName) return true;
        }
        return false;
    }
    public SpriteRenderer sprite;
    protected Chess chess;
    float mapMinX=-10;
    float mapMaxX=25;
    float mapMinY=-5;
    float mapMaxY=12;
    float zMin = -5f;
    float zMax = 0f;
    float xWeight = 1f;
    float yWeight = 2f;
    public virtual void InitController(Chess chess)
    {
        this.chess = chess;
        if (animator == null)animator = GetComponent<Animator>();
        if (sprite != null) sprite.sortingOrder = 2;
        //mapMinX=MapManage.instance
    }
    public virtual void WhenControllerEnterWar()
    {
        ChangeColor(Color.white);
    }
    protected virtual void Update(){
        if(chess==null)chess = GetComponent<Chess>();
        Vector3 currentPos= chess.transform.position;
        Vector3 spritpos = sprite.transform.position;
        chess.transform.position=new Vector3(currentPos.x,currentPos.y,-GetZByPosition(currentPos));
    }
    public virtual bool IfAnimPlayOver()
    {
        AnimatorStateInfo animStateInfo = chess.animatorController.animator.GetCurrentAnimatorStateInfo(0);
        if (animStateInfo.normalizedTime >=1f && !animator.IsInTransition(0))
        {
 
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
        return fullPathName;
    }
    public void SetOutline(Color _OutlineColor,float _OutlineSize)
    {
        sprite.material.SetColor("_OutlineColor", _OutlineColor);
        sprite.material.SetFloat("_OutlineSize",_OutlineSize);
    }
    public virtual void ChangeRandom(int max)
    {
        int n=Random.Range(0,max);
        animator.SetInteger("random", n);
    }
    public   float GetZByPosition(Vector3 worldPos)
    {
        // 归一化X：越右越大
        float x01 = Mathf.InverseLerp(mapMinX, mapMaxX, worldPos.x);

        // 归一化Y：越下越大，所以这里反过来
        float y01 = Mathf.InverseLerp(mapMaxY, mapMinY, worldPos.y);

        // 加权分数
        float score = x01 * xWeight + y01 * yWeight;

        // 最大理论值
        float maxScore = xWeight + yWeight;

        // 再归一化到 0~1
        float t = maxScore <= 0f ? 0f : score / maxScore;

        // 映射到 zMin ~ zMax
        return Mathf.Lerp(zMin, zMax, t);
    }
}
