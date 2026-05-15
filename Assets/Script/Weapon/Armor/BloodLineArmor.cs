using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// 红线防具：随机连线敌方，周期性由主人对目标造成伤害并抬高主人生命上限；线体用 <see cref="LineRenderer"/> + <see cref="EdgeCollider2D"/>。
/// 主人或目标侧受 <see cref="ElementType.Cutting"/> 会断线；目标侧多条红线合并为同一 <see cref="Buff_BloodLine"/>，切割时全部 <see cref="BreakLineOnly"/>。
/// </summary>
public class BloodLineArmor : ArmorBase
{
    [SerializeReference, LabelText("连线目标 Buff（血线连结）")]
    public Buff_BloodLine bloodLineBuff;

    [LabelText("红线起始点")]
    public Transform startPos;
    [LabelText("红线牵引目标")]
    public Chess targetChess;
    //public BloodBuff buff;//当然不可能是bloodBuff啦 
    public DamageMessege dm;//直接用dm就好
    [LabelText("红线伤害")]
    public float damage=10;//伤害
    [LabelText("伤害间隔")]
    public float interval = 1;//伤害间隔
    [LabelText("红线")]
    public LineRenderer bloodLine;
    public EdgeCollider2D edgeCollider;
    float t;

    /// <summary>
    /// 仅断线（不移除目标 Buff）。<paramref name="invokeArmorBroken"/> 为 false 时只做表现与碰撞清理，避免与 <see cref="BrokenArmor"/> 末尾的 <c>base.BrokenArmor()</c> 重复触发事件。
    /// </summary>
    public void BreakLineOnly(bool invokeArmorBroken = true)
    {
        targetChess = null;
        if (bloodLine != null)
            bloodLine.enabled = false;
        if (edgeCollider != null)
            edgeCollider.enabled = false;
        if (invokeArmorBroken)
            base.BrokenArmor();
    }

    /// <summary>断线并移除目标身上的 <see cref="Buff_BloodLine"/>（单条红线正常结束、主人受切割、目标死亡等）。</summary>
    public override void BrokenArmor()
    {
        Chess t = targetChess;
        List<BloodLineArmor> snap = null;
        if (t != null && t.buffController != null && t.buffController.buffDic.TryGetValue(Buff_BloodLine.BuffKey, out var blSnap) && blSnap is Buff_BloodLine bbl && bbl.sourceArmors != null)
            snap = new List<BloodLineArmor>(bbl.sourceArmors);

        targetChess = null;
        if (bloodLine != null)
            bloodLine.enabled = false;
        if (edgeCollider != null)
            edgeCollider.enabled = false;
        if (t != null && t.buffController != null && t.buffController.buffDic.TryGetValue(Buff_BloodLine.BuffKey, out var bl))
            bl.BuffOver();

        if (snap != null)
        {
            foreach (var a in snap)
            {
                if (a != null && a != this)
                    a.BreakLineOnly(true);
            }
        }
        base.BrokenArmor();
    }
    private void Update()
    {
        if(targetChess != null&&!targetChess.IfDeath)
        {
            t+=Time.deltaTime;
            if(t > interval)
            {
                dm.damageTo = targetChess;
                dm.damage = this.damage;
                dm.damageFrom = user;
                user.propertyController.TakeDamage(dm);
                user.propertyController.ChangeHPMax(damage);
                t = 0;
            }
            bloodLine.SetPosition(0, startPos.position);
            bloodLine.SetPosition(1, targetChess.transform.position);
            int pointCount = bloodLine.positionCount;
            Vector2[] colliderPoints = new Vector2[pointCount];
            for (int i = 0; i < pointCount; i++)
            {
                Vector3 worldPos = bloodLine.GetPosition(i);
                colliderPoints[i] = new Vector2(worldPos.x, worldPos.y); // 转换为2D
            }
            edgeCollider.points = colliderPoints;
        }
        else if(targetChess != null&&targetChess.IfDeath)
        {
            BrokenArmor();
        }
    }
    static Chess _selfCutHpUser;
    static int _selfCutHpFrame = -1;

    public override void GetDamage(DamageMessege dm)
    {
        if ((dm.damageElementType & ElementType.Cutting) == 0) return;

        bool hadLine = targetChess != null;
        BrokenArmor();

        if (!hadLine || user == null || user.IfDeath) return;

        if (user == _selfCutHpUser && Time.frameCount == _selfCutHpFrame)
            return;
        _selfCutHpUser = user;
        _selfCutHpFrame = Time.frameCount;

        float half = user.propertyController.GetHp() * 0.5f;
        if (half > 0f)
            user.propertyController.GetDamage(new DamageMessege(user, user, half, DamageType.Real));
    }

    public void UseLine()
    {
        List<Chess> enemys = ChessTeamManage.Instance.GetEnemyTeam(user.gameObject.tag);
        if (enemys.Count != 0)
        {
            int n = Random.Range(0, enemys.Count);
            targetChess = enemys[n];
            bloodLine.SetPosition(0, startPos.position);
            bloodLine.SetPosition(1, targetChess.transform.position);
            if (bloodLineBuff != null && targetChess != null && targetChess.buffController != null)
            {
                var inst = (Buff_BloodLine)bloodLineBuff.Clone();
                inst.lineOwner = user;
                inst.sourceArmors = new List<BloodLineArmor> { this };
                targetChess.buffController.AddBuff(inst);
            }
        }
        else
        {
            //后续应该会添加一个 断线的效果
            BrokenArmor();
        }
    }

    public override void InitArmor()
    {
        user.WhenEnterGame.AddListener(ResetArmor);
        
    }

    public override void ResetArmor(Chess chess)
    {
        if (bloodLine != null)
            bloodLine.enabled = true;
        if (edgeCollider != null)
            edgeCollider.enabled = true;
        targetChess = null;
        t = 0;
        user.propertyController.onGetDamage.AddListener(GetDamage);
        UseLine();
    }

    void OnDisable()
    {
        if (user != null && user.propertyController != null)
            user.propertyController.onGetDamage.RemoveListener(GetDamage);
    }
}
