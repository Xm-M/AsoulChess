using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorController : MonoBehaviour,Controller
{
    public Animator animator;
    public AudioPlayer BeAttack;
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

    /// <summary>与 Animator 中 1D Blend Tree 常用档位参数名一致（完整/断手/断头 三套序列帧）。</summary>
    public const string DefaultVisualTierParam = "VisualTier";

    /// <summary>与 death 状态内 Blend Tree 一致：0=普通死亡，1=火烧等变体。</summary>
    public const string DefaultDeathVariantParam = "DeathVariant";

    /// <summary>
    /// 设置外观档位（0=完整，1=断手，2=断头）。若 Controller 未配置该 Float 参数则静默跳过，兼容旧预制体。
    /// </summary>
    protected virtual void SetVisualTier(float tier, string paramName = DefaultVisualTierParam)
    {
        if (animator != null && HasParameter(animator, paramName))
            animator.SetFloat(paramName, tier);
    }

    /// <summary>
    /// 设置死亡动画变体（通常 0=默认死亡，1=火烧）。若未配置参数则跳过。
    /// </summary>
    protected virtual void SetDeathVariant(float variant, string paramName = DefaultDeathVariantParam)
    {
        if (animator != null && HasParameter(animator, paramName))
            animator.SetFloat(paramName, variant);
    }

    /// <summary>无 AnimatorController 子类时（或仅写 Float）可由外部（如 IceCarArmor）写入 VisualTier。</summary>
    public void SetVisualTierPublic(float tier, string paramName = DefaultVisualTierParam)
    {
        if (animator != null && HasParameter(animator, paramName))
            animator.SetFloat(paramName, tier);
    }

    /// <summary>无子类时由外部写入 DeathVariant。</summary>
    public void SetDeathVariantPublic(float variant, string paramName = DefaultDeathVariantParam)
    {
        if (animator != null && HasParameter(animator, paramName))
            animator.SetFloat(paramName, variant);
    }
    public SpriteRenderer sprite;
    public string dizzynesss;
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
        OnBeAttack();
    }
    protected virtual void Update(){
        if(chess==null)chess = GetComponent<Chess>();
        Vector3 currentPos= chess.transform.position;
        Vector3 spritpos = sprite.transform.position;
        chess.transform.position=new Vector3(currentPos.x,currentPos.y,-GetZByPosition(currentPos));
    }
    public void OnBeAttack()
    {
        if (BeAttack != null)
        {
            chess.propertyController.onGetDamage.AddListener((chess) =>
            {
                BeAttack.RandomPlay();
            });
        }
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
        if (LevelManage.instance.IfGameStart)
        {
            SetOutline(Color.white, 0);
            ChangeColor(Color.white);
            PlayIdle();
        }
    }
    public virtual void PlayIdle()
    {
        animator.Play("idle");
    }
    public virtual void PlayMove()
    {
        animator.Play("run");
    }
    /// <summary>攻击动画变体，由技能/被动在切换武器等时机调用（如 0=普攻，1=发呆治疗攻击）。</summary>
    protected int attackAnimationVariant;

    [Tooltip("Mecanim：variant>0 时播放该 Animator 状态名（如 attack_heal）；留空则仍播 \"attack\"，仅写入 attackVariant 整型参数（用于 BlendTree）。")]
    public string attackStateNameAlt = "";

    public virtual void SetAttackAnimationVariant(int variant)
    {
        attackAnimationVariant = variant;
    }

    public virtual int GetAttackAnimationVariant() => attackAnimationVariant;

    public virtual void PlayAttack()
    {
        if (animator == null) return;
        if (attackAnimationVariant > 0 && !string.IsNullOrEmpty(attackStateNameAlt))
        {
            animator.Play(attackStateNameAlt);
        }
        else
        {
            if (HasParameter(animator, "attackVariant"))
                animator.SetInteger("attackVariant", attackAnimationVariant);
            animator.Play("attack");
        }
    }
    public virtual void PlayDeath()
    {
        animator.Play("death");
    }
     
    /// <summary>与 <c>Unlit/GameUnitShader</c> 等一致：材质含 <c>_FlashAmount</c> 时写入 <see cref="Time.time"/> 触发闪白。</summary>
    public static void FlashSpriteRendererMaterial(SpriteRenderer sr)
    {
        if (sr?.material == null) return;
        if (sr.material.HasProperty("_FlashAmount"))
            sr.material.SetFloat("_FlashAmount", Time.time);
    }

    public virtual void OnGetDamage(DamageMessege dm)
    {
        FlashSpriteRendererMaterial(sprite);
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
    public virtual void ChangeFloat(float  value)
    {
        animator.SetFloat("Blend", value);
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
        if (string.IsNullOrEmpty(dizzynesss))
            Freezy();
        else animator.Play(dizzynesss);
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
