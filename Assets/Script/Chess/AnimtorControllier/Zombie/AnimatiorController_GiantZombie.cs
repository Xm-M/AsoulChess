using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatiorController_GiantZombie : AnimatorController
{
    public SpriteRenderer bengdai, littleZombie;
    public bool deathfire;
    [LabelText("受伤播放器")]
    public AudioPlayer player;
    public float randomSpeed = 0.2f;//移速偏差值
    public override void WhenControllerEnterWar()
    {
        base.WhenControllerEnterWar();
        sprite.gameObject.SetActive(true);
        littleZombie.gameObject.SetActive(true);
        bengdai.gameObject.SetActive(false);
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
            bengdai.gameObject.SetActive(false);
            littleZombie.gameObject.SetActive(false);
            deathfire = true;
        }
        else
        {
            base.OnGetDamage(dm);
            if ((dm.damageElementType & ElementType.Bullet) != 0)
            {
                player?.RandomPlay();
            }
            Debug.Log(chess.propertyController.GetHpPerCent());
            if (chess.propertyController.GetHpPerCent() <= 0.6f)
            {
                Debug.Log("绷带");
                if (!bengdai.gameObject.activeSelf)
                {
                    bengdai.gameObject.SetActive(true);
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
        bengdai.color = color;
        littleZombie.color = color;
    }
    public void HideLittle()
    {
        littleZombie.gameObject.SetActive(false);
    }
}
