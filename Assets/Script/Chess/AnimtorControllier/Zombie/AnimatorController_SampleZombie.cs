using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class AnimatorController_SampleZombie : AnimatorController
{
    public SpriteRenderer arm, body;
    public bool deathfire;
    public GameObject leftarm, leftHead;
    [LabelText("受伤播放器")]
    public AudioPlayer player;
    //[LabelText("火焰受伤播放器")]
    //public AudioPlayer player2;
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
            if ((dm.damageElementType & ElementType.Bullet) != 0)
            {
                player?.RandomPlay();
            }
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
                        lhead.transform.position = transform.position;
                        //lhead.transform.SetParent(transform);
                        //lhead.transform.localPosition = Vector3.zero;

                        //这里生成一个掉头特效
                    }
 
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
        body.color = color;
    }
}
/// <summary>
/// 僵尸的 自扣血buff
/// </summary>
public class BloodBuff : Buff
{
    public DamageMessege dm;
    //public float damage = 70;
    float speed;
    Timer timer;
    float leftHp;
    public override void WriteToSaveData(BuffSaveData data)
    {
        base.WriteToSaveData(data);
        if (data != null && timer != null) data.remainingTime = timer.LeftTime();
    }
    public override void WriteExtraToSaveData(BuffSaveData data)
    {
        base.WriteExtraToSaveData(data);
        if (data == null) return;
        data.SetExtra("Speed", speed);
        data.SetExtra("LeftHp", leftHp);
    }
    public override void RestoreExtraFromSaveData(BuffSaveData data)
    {
        base.RestoreExtraFromSaveData(data);
        if (data == null) return;
        speed = data.GetExtraFloat("Speed", 0);
        leftHp = data.GetExtraFloat("LeftHp", 0);
    }
    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        dm.damageTo = target;
        dm.damageFrom = target;
        this.target = target;
        target.propertyController.ChangeAttack(-target.propertyController.GetAttack());
        if (speed <= 0 || leftHp <= 0)
        {
            speed = UnityEngine.Random.Range(1, 1.5f);
            leftHp = target.propertyController.GetHp();
        }
        float delay = _restoreRemainingTime >= 0 ? _restoreRemainingTime : 0.03f;
        _restoreRemainingTime = -1f;
        timer = GameManage.instance.timerManage.AddTimer(BloodDamage, delay, true);
    }
    public void BloodDamage()
    {
        dm.damage =leftHp * 0.03f/speed;
        //Debug.Log("造成" + dm.damage);
        if (!target.IfDeath)
            target.propertyController.GetDamage(dm);
    }
    public override void BuffOver()
    {
        base.BuffOver();
        timer.Stop();
        timer = null;

    }
}
