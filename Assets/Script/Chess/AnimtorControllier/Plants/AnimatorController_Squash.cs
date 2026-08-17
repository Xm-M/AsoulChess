using UnityEngine;

/// <summary>
/// 窝瓜 / 黄瓜睦等下砸单位：动画事件可绑 <see cref="UseSkill"/> 触发圆形结算。
/// </summary>
public class AnimatorController_Squash : AnimatorController
{
    /// <summary>动画事件：触发 <see cref="Chess.UseSkill"/>（黄瓜睦被动监听后结算下砸）。</summary>
    public void UseSkill()
    {
        chess?.UseSkill();
    }

    /// <summary>可选别名，与 UseSkill 相同。</summary>
    public void OnSmashHit()
    {
        UseSkill();
    }
}
