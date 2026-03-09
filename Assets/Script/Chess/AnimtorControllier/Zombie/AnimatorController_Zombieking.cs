using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// </summary>
public class AnimatorController_Zombieking : AnimatorController
{
    bool stand;
    bool firstEnter;
    public override void InitController(Chess chess)
    {
        base.InitController(chess);
        chess.transform.right = Vector2.right;
        firstEnter = false;
        stand = true;
    }
    public override void PlayIdle()
    {
        if (!firstEnter)
        {
            Debug.Log("第一次播放");
            firstEnter = true;
            animator.Play("anim_enter");
            chess.skillController.context.Set<bool>("stand",true);
            stand = true;
            return;
        }
        bool s = false;
        chess.skillController.context.TryGet<bool>("stand",out s);
        if (stand==s) {
            if (stand)
            {
                animator.Play("anim_idle");
            } else
            {
                animator.Play("anim_head_idle");
            }
        } 
        else //如果不一样说明要切换形态
        {
            if (s)//s是战力 stand是俯身 那就是俯身->站立 head_leave
            {
                animator.Play("anim_head_leave");
            }
            else
            {
                animator.Play("anim_head_enter");
            }
            stand = s;
        }
    }
    public override void PlaySkill()
    {
        //base.PlaySkill();
        //int n=1;
        //animator.Play(string.Format("anim_spawn_{0}", n));
        //animator.Play(string.Format("anim_stomp_{0}", n));//脚踩 2*4格碾压伤害
        //animator.Play(string.Format("anim_RV_{0},n"));//砸车 2*3碾压伤害 这个也应该是多个动画来着
        //animator.Play("anim_bungeee_1_enter");//放蹦极(这个应该也有多个动画来着 不知道为什么都只有一个了)
        //animator.Play(string.Format("anim_head_attack_n"));
        //animator.Play(string.Format("anim_spawn_{0}",1));
        animator.Play(string.Format("anim_head_attack_1"));
    }
    public void UseSkill()
    {
        chess.UseSkill();
    }
    public override void PlayDeath()
    {
        animator.Play(("anim_death"));
    }
}
