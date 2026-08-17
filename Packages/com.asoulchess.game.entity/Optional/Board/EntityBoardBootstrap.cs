using AsoulChess.Game.Board;
using AsoulChess.Game.Entity;
using AsoulChess.Game.Entity.Controllers;

namespace AsoulChess.Game.Entity.Board
{
    /// <summary>Registers core + board-coupled controllers for grid combat demos.</summary>
    public static class EntityBoardBootstrap
    {
        public static MoveController RegisterBoardCombat(
            GameEntity entity,
            IBoard board,
            float maxHp,
            float attack,
            int attackRangeCells = 1,
            float attackCooldown = 1f)
        {
            EntityCombatBootstrap.RegisterCoreControllers(entity, maxHp, attack);

            var move = new MoveController();
            move.BindBoard(board);
            entity.RegisterController(move);

            entity.RegisterController(new AttackController
            {
                RangeInCells = attackRangeCells,
                CooldownSeconds = attackCooldown
            });

            return move;
        }
    }
}
