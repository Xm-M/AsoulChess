using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
/// <summary>
/// 所有Mygo的被动
/// 思考一下要怎么写好吧 
/// 怎么检测四个呢？ 
/// </summary>
public class PassiveSkill_Mygo : ISkillEffect
{
    [ShowInInspector, ReadOnly]
    [ShowIf("@UnityEngine.Application.isPlaying")]
    protected List<string> nearChess;
    protected Chess user;
    public float interval = 0.25f;
    Timer timer;

    public virtual void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        if(nearChess==null)
            nearChess = new List<string>();
        nearChess.Clear();
        this.user = user;
        CheckFriend();
        timer = GameManage.instance.timerManage.AddTimer(CheckFriend, interval, true);
        user.OnRemove.AddListener(OnDeath);
    }
    public virtual void CheckFriend( )
    {
        int[] dx = new int[4] { 0, 0, 1, -1 };
        Tile tile = user.moveController.standTile;
        nearChess.Clear();
        //bool triggerEgg = false;
        for (int i = 0; i < dx.Length; i++)
        {
            //如果不在地图范围内 忽视这一格
            if (!MapManage.instance.IfInMapRange(tile.mapPos.x + dx[i], tile.mapPos.y + dx[3 - i]))
            {
                continue;
            }
            Tile near = MapManage_PVZ.instance.tiles[tile.mapPos.x + dx[i], tile.mapPos.y + dx[3 - i]];
            if (near.stander != null) {
                string chessName = near.stander.propertyController.creator.chessName;
                if (chessName.Contains("高松灯")) chessName = "高松灯";
                if ( near.stander.propertyController.creator.plantTags.Contains("Mygo")
                    && chessName != user.propertyController.creator.chessName
                    && !nearChess.Contains(chessName))
                {
                    nearChess.Add(chessName);
                }
            }
        }
        user.skillController.context.Set<int>("mygo", nearChess.Count);

    }
    public void OnDeath(Chess chess)
    {
        timer?.Stop();
        timer = null;
    }
}


/// <summary>
/// anon的被动 主要是要多一个抱头
/// </summary>
public class PassiveSkill_MygoPassive_Anon : PassiveSkill_Mygo
{
    [SerializeReference]
    public Buff ResumeBuff;
    public override void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        base.SkillEffect(user, config, targets);
        user.propertyController.onGetDamage.AddListener(OnGetDamage);
    }
    public void OnGetDamage(DamageMessege dm)
    {
        Chess user = dm.damageTo;
        int mygo = 0;
        user.skillController.context.TryGet<int>("mygo",out mygo);
        if (user.propertyController.GetHp() <= 0 &&mygo > 0)
        {
            user.propertyController.ChangeHp(1);
            user.stateController.ChangeState(StateName.ResumeState);
            user.GetComponent<AudioPlayer>().PlaySubN(1);
            user.buffController.AddBuff(ResumeBuff);
        }
    }
}
public class PassiveSkill_MygoPassive_Tomorin : PassiveSkill_Mygo
{
    public float extraAttack = 0.5f;
    float current=0;
    public List<GameObject> bullets;
    [SerializeReference]
    [LabelText("tomorin照亮")]
    public LightBuff lightBuff;
    ShootBullet shoot;
    public override void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        Weapon_Sample weapon = user.equipWeapon.weapon as Weapon_Sample;
        shoot = weapon.attackFunction as ShootBullet;
        user.skillController.context.OnValueChange.AddListener(OnMygoChange);
        user.equipWeapon.OnAttack.AddListener(OnAttack);
        base.SkillEffect(user, config, targets);   
    }
    public void OnMygoChange()
    {
        int mygo = 0;
        user.skillController.context.TryGet<int>("mygo",out mygo);
        float extra = extraAttack * mygo;
        if (extra != current)
        {
            user.propertyController.ChangeAttack(-current);
            current = extra;
            user.propertyController.ChangeAttack(current);
        }
    }
    public void OnAttack(Chess user)
    {
        int n = Random.Range(0, bullets.Count);
        shoot.bullet = bullets[n];
    }
    public override void CheckFriend()
    {
        base.CheckFriend();
        //int mygo = 0;
        //user.skillController.context.TryGet<int>("mygo", out mygo);
        if(nearChess.Count>0)
            user.buffController.AddBuff(lightBuff);
    }
}
 
public class ShootBullet_Taki : IAttackFunction
{
    public Armor_TakiArmor armor;
    [LabelText("普通子弹")]
    public GameObject bullet;
    [LabelText("大号子弹")]
    public GameObject bigBullet;
    public Transform shooter;

    public void Attack(Chess user, List<Chess> targets)
    {
        if ((targets.Count == 0)) return;
          int mygo = 0;
        user.skillController.context.TryGet<int>("mygo", out mygo);
        GameObject bu = mygo == 4 ? bigBullet : bullet;
        GameObject b = ObjectPool.instance.Create(bu);
        Bullet zidan = b.GetComponent<Bullet>();
        int n = armor.animator.GetInteger("tomorin");
        zidan.GetComponent<Animator>().SetInteger("type", n);
        zidan.InitBullet(user, shooter.transform.position, targets[0], shooter.transform.right);
        zidan.Dm.damageTo = targets[0];
        zidan.Dm.takeBuff = armor.currentbuff;
    }
}
public class PassiveSkill_MygoPassive_Taki : PassiveSkill_Mygo
{



    public float extraRange = 10f;
    bool isChange;
    

    public override void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        isChange = false;
        user.skillController.context.OnValueChange.AddListener(OnMygoChange);
        base.SkillEffect(user, config, targets);
    }
    public void OnMygoChange()
    {
        int mygo = 0;
        user.skillController.context.TryGet<int>("mygo", out mygo);
        if (mygo == 4&&!isChange)
        {
            isChange=true;
            user.propertyController.ChangeAttackRange(extraRange);
        }else if (mygo!=4&&isChange)
        {
            isChange = false;
            user.propertyController.ChangeAttackRange(-extraRange);
        }
    }
}
/// <summary>
/// 修改子弹和寻敌方式
/// </summary>
public class PassiveSkill_MygoPassive_Rana : PassiveSkill_Mygo
{

    [LabelText("基础寻敌"),FoldoutGroup("基础攻击方式"),SerializeReference]
    public IFindTarget baseFindtagret;
    [LabelText("基础子弹"),FoldoutGroup("基础攻击方式")]
    public GameObject baseBullet;
    [LabelText("晋升寻敌"), FoldoutGroup("晋升攻击方式"), SerializeReference]
    public IFindTarget upFindtarget;
    [LabelText("晋升子弹"), FoldoutGroup("晋升攻击方式")]
    public GameObject upBullet;

    Weapon_Sample weapon;
    ShootBullet shoot;
    [ShowInInspector, ReadOnly]
    [ShowIf("@UnityEngine.Application.isPlaying")]
    bool isChange;
    [ShowInInspector, ReadOnly]
    [ShowIf("@UnityEngine.Application.isPlaying")]
    int mygo;

    public override void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        isChange = false;
        weapon = user.equipWeapon.weapon as Weapon_Sample;
        shoot = weapon.attackFunction as ShootBullet;
        weapon.findTarget =baseFindtagret;
        shoot.bullet = baseBullet;
        user.skillController.context.OnValueChange.AddListener(OnMygoChange);
        base.SkillEffect(user, config, targets);
    }
    public void OnMygoChange()
    {
        int mygo = 0;
        user.skillController.context.TryGet<int>("mygo", out mygo);
        this.mygo = mygo;
        if (mygo == 4 && !isChange)
        {
            isChange = true;
            Debug.Log("修改攻击方式");
            weapon.findTarget = upFindtarget;
            shoot.bullet = upBullet;
        }
        else if (mygo != 4 && isChange)
        {
            isChange = false;
            weapon.findTarget = baseFindtagret;
            shoot.bullet = baseBullet;
        }
    }
}