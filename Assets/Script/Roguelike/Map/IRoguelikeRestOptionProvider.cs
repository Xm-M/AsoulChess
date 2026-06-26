using System.Collections.Generic;

/// <summary>向休息房注入额外选项（事件、遗物等）。</summary>
public interface IRoguelikeRestOptionProvider
{
    void AppendRestOptions(int nodeId, RoguelikeRunState state, List<RoguelikeRestOption> options);
}
