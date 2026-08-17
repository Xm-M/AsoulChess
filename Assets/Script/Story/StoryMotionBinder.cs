using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 挂在 stage 预制体根（或 Director 同级）：集中配置本事件的 Motion Signal 绑定。
/// <see cref="EnterMapPlugin_StoryStage"/> Instantiate 后读取并注册到 <see cref="StoryTimelineBridge"/>。
/// </summary>
public class StoryMotionBinder : MonoBehaviour
{
    public List<StoryMotionSignalBinding> bindings = new List<StoryMotionSignalBinding>();
}
