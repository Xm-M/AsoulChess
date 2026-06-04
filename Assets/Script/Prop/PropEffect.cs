using System;

/// <summary>道具进局效果（配置在 <see cref="PropItemData"/> 上，勿挂运行时专用棋子 Mono）。</summary>
[Serializable]
public abstract class PropEffect
{
    public abstract void Apply(PropContext ctx);
    public abstract void Remove(PropContext ctx);
}
