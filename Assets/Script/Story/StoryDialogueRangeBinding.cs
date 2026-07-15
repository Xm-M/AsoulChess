using System;
using UnityEngine;
using UnityEngine.Timeline;

/// <summary>
/// Timeline Signal ↔ 对话 entry 区间绑定；Signal 名取自 <see cref="SignalAsset"/> 资源名。
/// </summary>
[Serializable]
public class StoryDialogueRangeBinding
{
    public SignalAsset signal;
    [Min(0)] public int startIndex;
    [Min(0)] public int endIndexInclusive;
}
