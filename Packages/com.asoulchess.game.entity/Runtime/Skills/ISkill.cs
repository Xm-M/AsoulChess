using AsoulChess.Game.Entity;

namespace AsoulChess.Game.Entity.Skills
{
    /// <summary>
    /// Skill contract for the entity package.
    /// Do not AddComponent runtime-only MonoBehaviours for skill logic — use effects + entity events/coroutines on GameEntity.
    /// </summary>
    public interface ISkill
    {
        string Id { get; }
        void Init(GameEntity user);
        void OnEnter(GameEntity user);
        void OnLeave(GameEntity user);
        void Tick(GameEntity user, float deltaTime);
        bool IsReady(GameEntity user);
        void Use(GameEntity user, GameEntity target);
    }

    public interface ISkillEffect
    {
        void Apply(GameEntity user, GameEntity target, SkillContext context);
    }
}
