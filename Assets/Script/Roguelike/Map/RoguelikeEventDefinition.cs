using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 肉鸽剧情事件定义：指向 StoryMode <see cref="LevelData"/> 与道具奖励。
/// </summary>
[CreateAssetMenu(fileName = "RoguelikeEvent", menuName = "Roguelike/Event Story Definition")]
public class RoguelikeEventDefinition : ScriptableObject
{
    [LabelText("事件 Id"), Tooltip("策划标识，不参与运行时逻辑")]
    public string eventId;

    [LabelText("剧情关卡")]
    public LevelData storyLevel;

    [LabelText("道具奖励"), Tooltip("剧情结束后写入 Run 的 propId 列表")]
    public List<string> propRewardIds = new List<string>();
}
