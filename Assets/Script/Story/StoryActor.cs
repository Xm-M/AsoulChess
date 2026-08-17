using UnityEngine;

/// <summary>
/// stage 预制体内可选标记：策划对照场景演员与 <see cref="DialogueCharacter"/>。
/// 需要公式化位移时同物体挂 <see cref="StoryMotionPlayer"/>。
/// </summary>
public class StoryActor : MonoBehaviour
{
    public DialogueCharacter dialogueCharacter;
}
