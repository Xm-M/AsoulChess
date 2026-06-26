using System.Collections.Generic;

/// <summary>休息房扩展选项提供者注册表；打开面板时由 <see cref="RoguelikeRestFlow"/> 遍历。</summary>
public static class RoguelikeRestOptionRegistry
{
    static readonly List<IRoguelikeRestOptionProvider> Providers = new List<IRoguelikeRestOptionProvider>();

    public static void Register(IRoguelikeRestOptionProvider provider)
    {
        if (provider == null || Providers.Contains(provider))
            return;
        Providers.Add(provider);
    }

    public static void Unregister(IRoguelikeRestOptionProvider provider)
    {
        if (provider == null)
            return;
        Providers.Remove(provider);
    }

    public static void AppendAll(int nodeId, RoguelikeRunState state, List<RoguelikeRestOption> options)
    {
        if (options == null || state == null)
            return;
        for (int i = 0; i < Providers.Count; i++)
        {
            var p = Providers[i];
            if (p == null)
                continue;
            p.AppendRestOptions(nodeId, state, options);
        }
    }
}
