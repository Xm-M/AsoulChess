namespace AsoulChess.Game.Entity
{
    /// <summary>Replaces AVZ <c>LevelManage.IfGameStart</c> for animation/skill gates. Optional in P1-0.</summary>
    public interface ISimulationGate
    {
        bool IsSimulationRunning { get; }
    }
}
