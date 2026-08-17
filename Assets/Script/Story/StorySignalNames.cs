/// <summary>Timeline Signal 推荐命名（与 SignalAsset 资源名对齐）。</summary>
public static class StorySignalNames
{
    public const string SequenceComplete = "Story.Sequence.Complete";

    /// <summary>示例：Story.Motion.Hero.Jump — 实际以 SignalAsset 资源名为准，经 StoryMotionBinder 绑定。</summary>
    public const string MotionPrefix = "Story.Motion.";
}
