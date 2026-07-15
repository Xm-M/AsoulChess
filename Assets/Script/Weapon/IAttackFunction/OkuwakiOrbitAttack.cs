using System.Collections.Generic;

/// <summary>
/// 老仓育普攻占位：实际伤害由常驻公式弹承担，此处不发射子弹。
/// 保留以驱动索敌与攻击动画循环。
/// </summary>
public class OkuwakiOrbitAttack : IAttackFunction
{
    public void Attack(Chess user, List<Chess> targets) { }
}
