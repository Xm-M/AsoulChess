using System.Collections.Generic;
using AsoulChess.Game.Entity;
using AsoulChess.Game.Entity.Skills;
using AsoulChess.Game.Entity.State;

namespace AsoulChess.Game.Entity.Controllers
{
    public sealed class SkillController : IEntityController
    {
        GameEntity _entity;
        readonly SkillContext _context = new SkillContext();
        readonly List<ISkill> _skills = new List<ISkill>();

        public SkillContext Context => _context;
        public IReadOnlyList<ISkill> Skills => _skills;

        public void InitController(GameEntity entity) => _entity = entity;

        public void OnEnterCombat()
        {
            for (int i = 0; i < _skills.Count; i++)
                _skills[i].OnEnter(_entity);
        }

        public void OnLeaveCombat()
        {
            for (int i = 0; i < _skills.Count; i++)
                _skills[i].OnLeave(_entity);
        }

        public void Tick(float deltaTime)
        {
            for (int i = 0; i < _skills.Count; i++)
                _skills[i].Tick(_entity, deltaTime);
        }

        public void AddSkill(ISkill skill)
        {
            if (skill == null || _skills.Contains(skill)) return;
            _skills.Add(skill);
            skill.Init(_entity);
        }

        public bool TryUse(int index, GameEntity target = null)
        {
            if (index < 0 || index >= _skills.Count) return false;
            var skill = _skills[index];
            if (!skill.IsReady(_entity)) return false;
            _entity.State.SetState(EntityStateId.Skill);
            skill.Use(_entity, target);
            _entity.Events?.Trigger(EntityEvents.SkillUsed, _entity);
            if (_entity.IsAlive && _entity.State.Current != EntityStateId.Dead)
                _entity.State.SetState(EntityStateId.Idle);
            return true;
        }
    }
}
