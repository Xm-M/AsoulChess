using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorController_SampleZombie : AnimatorController
{
    public SpriteRenderer arm, body;
    public bool deathfire;
    public GameObject leftarm, leftHead;
    public AudioPlayer player;
    [SerializeReference]
    public BloodBuff bloodBuff;//持续掉血buff
    public float randomSpeed=0.2f;//移速偏差值
    public override void WhenControllerEnterWar()
    {
        base.WhenControllerEnterWar();
        sprite.gameObject.SetActive(true);
        arm.gameObject.SetActive(false);
        body.gameObject.SetActive(false);
        ChangeColor(Color.white);
        deathfire = false;
        float n = UnityEngine.Random.Range(0, randomSpeed);
        chess.propertyController.ChangeAcceleRate(n);
    }
    public override void PlayIdle()
    {
        base.PlayIdle();
    }
    //override p
    public override void OnGetDamage(DamageMessege dm)
    {
        //base.OnGetDamage(dm);
        if ((dm.damageElementType & ElementType.Explode)!=0)
        {
            if (chess.propertyController.GetHpPerCent() <= 0)
            {
                //Debug.Log("death_fire");
                sprite.gameObject.SetActive(false);
                arm.gameObject.SetActive(false);
                body.gameObject.SetActive(true);
                deathfire = true;
            }
        }
        else
        {
            if((dm.damageElementType&ElementType.CloseAttack)==0)
                player?.RandomPlay();
            if (chess.propertyController.GetHpPerCent() > 0.6)
            {
                base.OnGetDamage(dm);
            }
            else
            {
                if (chess.propertyController.GetHpPerCent() <= 0.6f   )
                {
                    if (sprite.gameObject.activeSelf)
                    {
                        sprite.gameObject.SetActive(false);
                        arm.gameObject.SetActive(true);
                        ObjectPool.instance.Create(leftarm).transform.position = transform.position;
                    }
                    //生成一个手臂特效
                    if(chess.propertyController.GetHpPerCent() > 0.1f)
                        arm.material.SetFloat("_FlashAmount", Time.time);
                }
                if (chess.propertyController.GetHpPerCent() <= 0.25f)
                {
                    if (arm.gameObject.activeSelf)
                    {
                        arm.gameObject.SetActive(false);
                        body.gameObject.SetActive(true);
                        //这里要添加一个持续掉血buff
                        chess.buffController.AddBuff(bloodBuff);
                        GameObject lhead = ObjectPool.instance.Create(leftHead);
                        lhead.transform.SetParent(transform);
                        lhead.transform.localPosition = Vector3.zero;

                        //这里生成一个掉头特效
                    }
                    //body.material.SetFloat("_FlashAmount", Time.time);
                }
                //else 
            }
        }
    }
    public override void PlayDeath()
    {
        //base.PlayDeath();
        if (!deathfire)
            StartCoroutine(ReadyToDeath());
        else animator.Play("death_fire");
    }
    IEnumerator ReadyToDeath()
    {
        yield return null;
        animator.Play("death");
    }
    public override void ChangeColor(Color color)
    {
        base.ChangeColor(color);
        arm.color = color;
        body.color = color;
    }
}
