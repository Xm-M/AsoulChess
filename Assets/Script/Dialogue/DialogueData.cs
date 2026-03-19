using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DialogueEntry
{
    public DialogueCharacter speaker;
    [TextArea(2, 4)]
    public string text;

    [Header("事件")]
    [Tooltip("显示这句时触发")]
    public string eventOnEnter;
    [Tooltip("点击离开这句时触发")]
    public string eventOnExit;
    [Tooltip("不空则需收到此事件后才允许点击进入下一句")]
    public string waitForEvent;
    [Tooltip("勾选后：点击时隐藏对话框并触发 eventOnExit，收到 resumeEvent 后再显示下一句")]
    public bool hideOnClick;
    [Tooltip("与 hideOnClick 配合：收到此事件后显示对话框并继续下一句")]
    public string resumeEvent;
}

/// <summary>
/// 对话序列，存放多句对话。
/// </summary>
[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue/Sequence")]
public class DialogueData : ScriptableObject
{
    public List<DialogueEntry> entries = new List<DialogueEntry>();
}
