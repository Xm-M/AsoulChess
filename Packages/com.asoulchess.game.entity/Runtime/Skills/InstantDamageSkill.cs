using AsoulChess.Game.Entity;

namespace AsoulChess.Game.Entity.Skills
{
    /// <summary>Minimal instant damage skill for demos and scaffolding.</summary>
    public sealed class InstantDamageSkill : ISkill
    {
        readonly float _damageMultiplier;
        readonly ISkillEffect[] _effects;

        public string Id { get; }

        public InstantDamageSkill(string id, float damageMultiplier = 1f, params ISkillEffect[] effects)
        {
            Id = id;
            _damageMultiplier = damageMultiplier;
            _effects = effects ?? System.Array.Empty<ISkillEffect>();
        }

        public void Init(GameEntity user) { }
        public void OnEnter(GameEntity user) { }
        public void OnLeave(GameEntity user) { }
        public void Tick(GameEntity user, float deltaTime) { }
        public bool IsReady(GameEntity user) => user != null && user.IsAlive;

        public void Use(GameEntity user, GameEntity target)
        {
            if (user == null || target == null || !target.IsAlive) return;
            float dmg = user.Property.CurrentAttack * _damageMultiplier;
            target.Property.ApplyDamage(dmg, user);
            var ctx = user.Skills.Context;
            for (int i = 0; i < _effects.Length; i++)
                _effects[i]?.Apply(user, target, ctx);
        }
    }
}
