using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadArmor :ArmorBase
{
    public float armorMax;
    //public Animator animator;
    //public string animName;
    public SpriteRenderer state1,state2,state3;
    public GameObject effect;
    SpriteRenderer currentRender;
    [SerializeField]
    float armorCurrent;
    public AudioPlayer player;
    //float brokenPercent;

    /// <summary>
    /// 这个是初始化，只会被调用一次
    /// </summary>
    public override void InitArmor()
    {
        armorCurrent = armorMax;
        user.WhenEnterGame.AddListener(ResetArmor);
    }
    public override void GetDamage(DamageMessege dm)
    {
        //Debug.Log(dm.damage);
        if (armorCurrent >= dm.damage)
        {
            //Debug.Log("防具抵消");
            
            dm.damage *= (1 - user.propertyController.GetExtraDefence() );
            UIManage.GetView<DamagePanel>().ShowDamageMes(dm);
            armorCurrent -= dm.damage;
            dm.damage = 0;

            currentRender.material.SetFloat("_FlashAmount", Time.time);
            if ((dm.damageElementType & ElementType.CloseAttack) == 0)
                player?.RandomPlay();
        }
        else
        {
            //Debug.Log("防具溢出");
            dm.damage -= armorCurrent;
            armorCurrent = 0;
            if ((dm.damageElementType & ElementType.Explode) == 0)
            {
                GameObject fall = ObjectPool.instance.Create(effect);
                fall.transform.SetParent(user.transform, false);
                fall.transform.localPosition = Vector3.zero;
                if ((dm.damageElementType & ElementType.CloseAttack) == 0)
                    player?.RandomPlay();
            }
            BrokenArmor();
           
        }
        if (armorCurrent < armorMax * 2 / 3)
        {
            if (state1.gameObject.activeSelf)
            {
                state1.gameObject.SetActive(false);
                state2.gameObject.SetActive(true);
                currentRender = state2;
            }
        }
        if (armorCurrent < armorMax / 3)
        {
            if (state2.gameObject.activeSelf)
            {
                state2.gameObject.SetActive(false);
                state3.gameObject.SetActive(true);
                currentRender = state3;
            }
        }
         
        //brokenPercent = armorCurrent / armorMax;
        //animator?.SetFloat(animName, brokenPercent);
    }
    /// <summary>
    ///  就是重置函数
    /// </summary>
    /// <exception cref="System.NotImplementedException"></exception>
    public override void ResetArmor(Chess chess)
    {
        tag = user.tag;
        user.propertyController.onSetDamage.AddListener(GetDamage);
        armorCurrent = armorMax;
        state1.gameObject.SetActive(true);
        state2.gameObject.SetActive(false);
        state3.gameObject.SetActive(false);
        currentRender = state1;
    }
    /// <summary>
    ///   是防具损坏函数
    /// </summary>
    /// <exception cref="System.NotImplementedException"></exception>
    public override void BrokenArmor()
    {
        //如果提前死亡的话这个也是会被清除的 所以可以不管
        //Debug.Log("防具破坏");
        state2.gameObject.SetActive(false);
        state1.gameObject.SetActive(false);
        state3.gameObject.SetActive(false);
        user.propertyController.onSetDamage.RemoveListener(GetDamage);
        
        //gameObject.SetActive(false);
        //这里还要生成一个防具掉落的特效
    }
}
