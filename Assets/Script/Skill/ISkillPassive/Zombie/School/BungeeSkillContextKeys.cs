/// <summary>蹦极僵尸在 <see cref="SkillContext"/> 中使用的键。</summary>
public static class BungeeSkillContextKeys
{
    /// <summary>本次抱走的有序单位列表（含霸凌主目标），与 <see cref="PassiveSkill_Bungee.targetSprites"/> 下标对应。</summary>
    public const string Victims = "蹦极抱走列表";

    /// <summary>莴苣保护伞打断下跳：主动抱走逻辑应跳过，仅播返回段离场。</summary>
    public const string InterruptedByUmbrella = "莴苣打断蹦极";

    /// <summary>返回动画协程进行中，避免重复触发。</summary>
    public const string RetreatInProgress = "蹦极返回中";
}
