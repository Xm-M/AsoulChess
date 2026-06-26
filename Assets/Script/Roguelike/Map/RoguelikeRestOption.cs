using System;

/// <summary>休息房单个可选项（运行时构建，不进存档）。</summary>
public sealed class RoguelikeRestOption
{
    public const string BuiltinRestId = "builtin.rest";
    public const string BuiltinExpandId = "builtin.expand";

    public string id;
    public string title;
    public string description;
    public bool enabled;
    public Func<RoguelikeRunState, int, bool> execute;
}
