using System;
using UnityEngine;
using UnityEngine.Timeline;

/// <summary>
/// Timeline Signal → 谁执行、哪套 Preset、去哪个锚点。
/// 配置在 stage 的 <see cref="StoryMotionBinder"/> 上，保证引用的是预制体实例侧对象。
/// </summary>
[Serializable]
public class StoryMotionSignalBinding
{
    [Tooltip("与 Timeline SignalAsset 资源一致")]
    public SignalAsset signal;

    public StoryMotionPreset preset;

    [Tooltip("本 stage 内挂了 StoryMotionPlayer 的角色")]
    public StoryMotionPlayer player;

    [Tooltip("MoveTo/JumpTo/DashTo 终点；Knockback 可空（用玩家朝向反方向）")]
    public Transform targetAnchor;
}
