using AsoulChess.Game.Board;
using AsoulChess.Game.Entity;
using AsoulChess.Game.Entity.Animation;
using AsoulChess.Game.Entity.Controllers;
using AsoulChess.Game.Entity.State;
using UnityEngine;

namespace AsoulChess.Game.Entity.Board
{
    /// <summary>Simple melee/ranged-by-manhattan attack with cooldown. Requires <see cref="MoveController"/>.</summary>
    public sealed class AttackController : IEntityController
    {
        GameEntity _entity;
        float _cooldownLeft;

        public int RangeInCells { get; set; } = 1;
        public float CooldownSeconds { get; set; } = 1f;
        public float DamageMultiplier { get; set; } = 1f;
        public IAnimBridge Anim { get; set; } = NullAnimBridge.Instance;

        MoveController Move => _entity?.GetController<MoveController>();

        public bool IsCoolingDown => _cooldownLeft > 0f;

        public void InitController(GameEntity entity) => _entity = entity;
        public void OnEnterCombat() { }
        public void OnLeaveCombat() { }

        public void Tick(float deltaTime)
        {
            if (_cooldownLeft > 0f)
                _cooldownLeft = Mathf.Max(0f, _cooldownLeft - deltaTime);
        }

        public bool IsInRange(GameEntity target)
        {
            var move = Move;
            var targetMove = target?.GetController<MoveController>();
            if (move == null || targetMove == null) return false;
            if (!move.IsOnBoard || !targetMove.IsOnBoard) return false;
            return GridBoard.Manhattan(move.X, move.Y, targetMove.X, targetMove.Y) <= RangeInCells;
        }

        public bool TryAttack(GameEntity target)
        {
            if (_entity == null || target == null || !_entity.IsAlive || !target.IsAlive) return false;
            if (IsCoolingDown || !IsInRange(target)) return false;

            _entity.State?.SetState(EntityStateId.Attack);
            Anim?.PlayAttack();

            float dmg = _entity.Property.CurrentAttack * DamageMultiplier;
            target.Property.ApplyDamage(dmg, _entity);

            _cooldownLeft = CooldownSeconds;
            if (_entity.IsAlive && _entity.State?.Current != EntityStateId.Dead)
                _entity.State?.SetState(EntityStateId.Idle);
            return true;
        }
    }
}
