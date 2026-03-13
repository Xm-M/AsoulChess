using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 这个气球是个MoneBehavior 所以说是可以挂在BoxCollider的
/// 现在的重点问题是 tm爆炸类的怎么办 貌似除了修改exlode的函数 没有什么别的办法了呢
/// </summary>
public class BallonArmor : ArmorBase
{
    public BoxCollider2D ballonCol;
    /// <summary>
    /// 意思是这个防具被破坏的时候会发生的事情
    /// </summary>
    public override void BrokenArmor()
    {
        user.animatorController.animator.SetInteger("fly", 0);
        //user.SetCol(true);
        Debug.Log("气球 爆炸");
        user.ResumeSelectable();
        ballonCol.enabled = false;
        
    }
    
    public override void GetDamage(DamageMessege dm)
    {
        if ((dm.damageElementType & ElementType.Explode) != 0)
        {
            ballonCol.enabled = false;
        }
    }
  
    public override void InitArmor()
    {
        
        user.WhenEnterGame.AddListener(ResetArmor);
        ballonCol.enabled = true;
        user.UnSelectable();
        //Debug.Log("无法选中");
        //user.DeathEvent.AddListener
    }
    /// <summary>
    /// ResetArmor是重置的时候用的 所以每次进入游戏就会调用一次
    /// </summary>
    public override void ResetArmor(Chess chess)
    {
        user.animatorController.animator.SetInteger("fly", 1);
        this.tag = user.tag;
        //chess.SetCol(false);
        chess.UnSelectable();
        ballonCol.enabled = true;
        chess.propertyController.onGetDamage.AddListener(GetDamage);
        Debug.Log("重置气球");
    }
    /// <summary>
    /// 这个layer只跟Bullet相关 
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Bullet bullet = collision.GetComponent<Bullet>();
        //
        if ((bullet.Dm.damageElementType & ElementType.Puncture)!=0)
        {
            BrokenArmor();
            bullet.RecycleBullet();
        }
    }
}
