using UnityEngine;

/// <summary>
/// 对话角色：名字、头像等基本信息，供 DialogueEntry 引用复用。
/// </summary>
[CreateAssetMenu(fileName = "NewCharacter", menuName = "Dialogue/Character")]
public class DialogueCharacter : ScriptableObject
{
    public string characterName;
    public Sprite avatar;
}
