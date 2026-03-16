using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorController_SpecielZombie : AnimatorController
{
    public SpriteRenderer arm, head;
    public bool deathfire;
    public GameObject leftarm, leftHead;
    [LabelText("受伤播放器")]
    public AudioPlayer player;
    //[LabelText("火焰受伤播放器")]
    //public AudioPlayer player2;
    [SerializeReference]
    public BloodBuff bloodBuff;//持续掉血buff
    public float randomSpeed = 0.2f;//移速偏差值
    public override void WhenControllerEnterWar()
    {
        base.WhenControllerEnterWar();
        sprite.gameObject.SetActive(true);
        arm.gameObject.SetActive(true);
        head.gameObject.SetActive(true);
        ChangeColor(Color.white);
        deathfire = false;
        float n = UnityEngine.Random.Range(0, randomSpeed);
        chess.propertyController.ChangeAcceleRate(n);
    }
    public override void PlayIdle()
    {
        base.PlayIdle();
    }
    public override void ChangeFlash(float value)
    {
        base.ChangeFlash(value);
    }
    
    //override p
    public override void OnGetDamage(DamageMessege dm)
    {
        //base.OnGetDamage(dm);
        if ((dm.damageElementType & ElementType.Explode) != 0&& chess.propertyController.GetHpPerCent() <= 0)
        {
            //Debug.Log("death_fire");
            sprite.gameObject.SetActive(true);
            arm.gameObject.SetActive(false);
            head.gameObject.SetActive(false);
            deathfire = true;
        }
        else
        {
            base.OnGetDamage(dm);
            if ((dm.damageElementType & ElementType.Bullet) != 0)
            {
                player?.RandomPlay();
            }
            if (chess.propertyController.GetHpPerCent() <= 0.6f)
            {
                if (arm.gameObject.activeSelf)
                {
                    //sprite.gameObject.SetActive();
                    arm.gameObject.SetActive(false);
                    ObjectPool.instance.Create(leftarm).transform.position = transform.position;
                }
                 
            }
            if (chess.propertyController.GetHpPerCent() <= 0.25f)
            {
                if (head.gameObject.activeSelf)
                {
                    arm.gameObject.SetActive(false);
                    head.gameObject.SetActive(false);
                    //这里要添加一个持续掉血buff
                    chess.buffController.AddBuff(bloodBuff);
                    GameObject lhead = ObjectPool.instance.Create(leftHead);
                    lhead.transform.position = transform.position;
                    //lhead.transform.SetParent(transform);
                    //lhead.transform.localPosition = Vector3.zero;
                    //这里生成一个掉头特效
                }

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
        head.color = color;
    }
    
}
