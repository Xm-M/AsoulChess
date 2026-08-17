namespace AsoulChess.Game.Entity
{
    /// <summary>String event ids for entity framework (not AVZ EventName).</summary>
    public static class EntityEvents
    {
        public const string EnterCombat = "entity.enter_combat";
        public const string Damaged = "entity.damaged";
        public const string Healed = "entity.healed";
        public const string Death = "entity.death";
        public const string SkillUsed = "entity.skill_used";
        public const string BuffAdded = "entity.buff_added";
        public const string BuffRemoved = "entity.buff_removed";
    }
}
