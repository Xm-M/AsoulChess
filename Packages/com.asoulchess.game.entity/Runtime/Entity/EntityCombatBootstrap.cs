using AsoulChess.Game.Entity.Combat;
using AsoulChess.Game.Entity.Controllers;

namespace AsoulChess.Game.Entity
{
    /// <summary>Optional helper for samples and AVZ migration — not called by <see cref="GameEntity.Init"/>.</summary>
    public static class EntityCombatBootstrap
    {
        public static PropertyController RegisterCoreControllers(GameEntity entity, EntityStats template)
        {
            var property = new PropertyController(template);
            entity.RegisterController(property);
            entity.RegisterController(new StateController());
            entity.RegisterController(new SkillController());
            entity.RegisterController(new BuffController());
            return property;
        }

        public static PropertyController RegisterCoreControllers(GameEntity entity, float maxHp, float attack) =>
            RegisterCoreControllers(entity, new EntityStats { Hp = maxHp, HpMax = maxHp, Attack = attack });
    }
}
