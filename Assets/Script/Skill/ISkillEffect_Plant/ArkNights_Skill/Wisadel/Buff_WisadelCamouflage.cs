using System;

/// <summary>维什戴尔隐匿：四邻有己方魂灵之影时无法选中（<see cref="Chess.UnSelectable"/>）。</summary>
[Serializable]
public class Buff_WisadelCamouflage : Buff
{
    public Buff_WisadelCamouflage()
    {
        buffName = "WisadelCamouflage";
    }

    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        target.UnSelectable();
    }

    public override void BuffOver()
    {
        base.BuffOver();
        target?.ResumeSelectable();
    }

    public override Buff Clone() => new Buff_WisadelCamouflage();
}
