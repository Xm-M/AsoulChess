using AsoulChess.Game.Entity.Controllers;

namespace AsoulChess.Game.Entity.State
{
    public sealed class IdleEntityState : IEntityState
    {
        public EntityStateId Id => EntityStateId.Idle;

        public void Enter(GameEntity entity, StateController controller) => controller.Anim.PlayIdle();
        public void Execute(GameEntity entity, StateController controller, float deltaTime) { }
        public void Exit(GameEntity entity, StateController controller) { }
        public IEntityState Clone() => new IdleEntityState();
    }

    public sealed class AttackEntityState : IEntityState
    {
        public EntityStateId Id => EntityStateId.Attack;

        public void Enter(GameEntity entity, StateController controller) => controller.Anim.PlayAttack();
        public void Execute(GameEntity entity, StateController controller, float deltaTime) { }
        public void Exit(GameEntity entity, StateController controller) { }
        public IEntityState Clone() => new AttackEntityState();
    }

    public sealed class SkillEntityState : IEntityState
    {
        public EntityStateId Id => EntityStateId.Skill;

        public void Enter(GameEntity entity, StateController controller) => controller.Anim.PlaySkill();
        public void Execute(GameEntity entity, StateController controller, float deltaTime) { }
        public void Exit(GameEntity entity, StateController controller) { }
        public IEntityState Clone() => new SkillEntityState();
    }

    public sealed class MoveEntityState : IEntityState
    {
        public EntityStateId Id => EntityStateId.Move;

        public void Enter(GameEntity entity, StateController controller) { }
        public void Execute(GameEntity entity, StateController controller, float deltaTime) { }
        public void Exit(GameEntity entity, StateController controller) { }
        public IEntityState Clone() => new MoveEntityState();
    }

    public sealed class PrepareEntityState : IEntityState
    {
        public EntityStateId Id => EntityStateId.Prepare;

        public void Enter(GameEntity entity, StateController controller) { }
        public void Execute(GameEntity entity, StateController controller, float deltaTime) { }
        public void Exit(GameEntity entity, StateController controller) { }
        public IEntityState Clone() => new PrepareEntityState();
    }

    public sealed class DeathEntityState : IEntityState
    {
        public EntityStateId Id => EntityStateId.Dead;

        public void Enter(GameEntity entity, StateController controller) => controller.Anim.PlayDeath();
        public void Execute(GameEntity entity, StateController controller, float deltaTime) { }
        public void Exit(GameEntity entity, StateController controller) { }
        public IEntityState Clone() => new DeathEntityState();
    }

    /// <summary>Timed stun using <see cref="Controllers.PropertyController.GetDizzinessTime"/> (AVZ <c>DizzinessState</c>).</summary>
    public sealed class DizzyEntityState : IEntityState
    {
        float _elapsed;

        public EntityStateId Id => EntityStateId.Dizzy;

        public void Enter(GameEntity entity, StateController controller)
        {
            _elapsed = 0f;
            controller.Anim.PlayDizzy();
        }

        public void Execute(GameEntity entity, StateController controller, float deltaTime)
        {
            _elapsed += deltaTime;
            var property = entity.Property;
            if (property == null)
            {
                controller.RevertToPreviousState();
                return;
            }

            if (_elapsed >= property.GetDizzinessTime())
                controller.RevertToPreviousState();
        }

        public void Exit(GameEntity entity, StateController controller)
        {
            _elapsed = 0f;
            entity.Property?.ResetDizzinessTime();
            controller.Anim.PlayIdle();
        }

        public IEntityState Clone() => new DizzyEntityState();
    }
}
