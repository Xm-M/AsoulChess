namespace AsoulChess.Game.Entity.Animation
{
    /// <summary>Animation hook — implement in the game project (Animator / Spine / etc.).</summary>
    public interface IAnimBridge
    {
        void PlayIdle();
        void PlayAttack();
        void PlaySkill();
        void PlayDeath();
        void PlayDizzy();
    }

    public sealed class NullAnimBridge : IAnimBridge
    {
        public static readonly NullAnimBridge Instance = new NullAnimBridge();
        public void PlayIdle() { }
        public void PlayAttack() { }
        public void PlaySkill() { }
        public void PlayDeath() { }
        public void PlayDizzy() { }
    }
}
