using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 进入关卡时显示对话，对话结束后可继续 Timeline 或下一步。
/// 将 dialogue 赋值为关卡对应的 DialogueData，onComplete 时执行后续逻辑。
/// dialogueEvents 中的处理器会在显示对话前注册，离开关卡时统一注销。
/// </summary>
public class EnterMapPlugin_Dialogue : ILevelPlugin
{
    public DialogueData dialogue;
    [Tooltip("对话结束后是否继续 Timeline（若在 Timeline 中调用需配合 Plot 等）")]
    public bool resumeTimelineOnComplete;

    [Tooltip("对话相关事件处理器，进入时注册离开时注销")]
    [SerializeReference]
    public List<IDialogueEvent> dialogueEvents = new List<IDialogueEvent>();

    public void StadgeEffect(LevelController levelController)
    {
        if (dialogueEvents != null)
        {
            foreach (var e in dialogueEvents)
                e?.Register(levelController);
        }
        if (dialogue == null) return;

        var panel = UIManage.GetView<DialoguePanel>();
        if (panel == null) return;

        panel.ShowDialogue(dialogue, OnDialogueComplete);
    }

    void OnDialogueComplete()
    {
        if (resumeTimelineOnComplete)
        {
            var mapPvz = MapManage.instance as MapManage_PVZ;
            if (mapPvz?.dir != null)
                mapPvz.dir.Play();
        }
    }

    public void OverPlugin(LevelController levelController)
    {
        if (dialogueEvents != null)
        {
            foreach (var e in dialogueEvents)
                e?.Unregister();
        }
    }
}
