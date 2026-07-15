using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

/// <summary>
/// 剧情关编排：Instantiate stage → Play Timeline → Signal 暂停对话 → Complete → Outcome。
/// stage 预制体内已含 PlayableDirector、StoryActor、Timeline 绑定。
/// </summary>
[Serializable]
public class EnterMapPlugin_StoryStage : ILevelPlugin
{
    public GameObject stagePrefab;
    public DialogueData dialogue;
    public List<StoryDialogueRangeBinding> dialogueRanges = new List<StoryDialogueRangeBinding>();
    [Tooltip("整段演出结束 Signal；为空则按 StorySignalNames.SequenceComplete 名称匹配")]
    public SignalAsset sequenceCompleteSignal;

    GameObject _stageRoot;
    PlayableDirector _director;
    StoryTimelineBridge _bridge;
    LevelController _controller;
    Coroutine _runCoroutine;
    bool _completed;
    bool _waitingDialogue;

    public void StadgeEffect(LevelController levelController)
    {
        _controller = levelController;
        _completed = false;
        _waitingDialogue = false;
        if (_controller != null)
            _runCoroutine = _controller.StartCoroutine(RunStory());
    }

    IEnumerator RunStory()
    {
        if (stagePrefab == null)
        {
            CompleteSequence();
            yield break;
        }

        _stageRoot = UnityEngine.Object.Instantiate(stagePrefab);
        _director = _stageRoot.GetComponentInChildren<PlayableDirector>(true);
        _bridge = _stageRoot.GetComponentInChildren<StoryTimelineBridge>(true);
        if (_bridge == null && _director != null)
            _bridge = _director.gameObject.AddComponent<StoryTimelineBridge>();

        RegisterSignals();

        if (_director == null)
        {
            CompleteSequence();
            yield break;
        }

        _director.Play();
        while (!_completed && _director != null)
        {
            if (_waitingDialogue || _director.state == PlayState.Paused)
            {
                yield return null;
                continue;
            }

            if (_director.state == PlayState.Playing)
            {
                if (_director.duration > 0.01f && _director.time >= _director.duration - 0.02f)
                    break;
                yield return null;
                continue;
            }

            break;
        }

        if (!_completed)
            CompleteSequence();
    }

    void RegisterSignals()
    {
        if (_bridge == null)
            return;

        _bridge.ClearHandlers();
        if (sequenceCompleteSignal != null)
            _bridge.Register(sequenceCompleteSignal, CompleteSequence);
        else
            _bridge.Register(StorySignalNames.SequenceComplete, CompleteSequence);

        if (dialogueRanges == null)
            return;

        for (int i = 0; i < dialogueRanges.Count; i++)
        {
            var binding = dialogueRanges[i];
            if (binding?.signal == null)
                continue;
            _bridge.Register(binding.signal, () => OnDialogueSignal(binding));
        }
    }

    void OnDialogueSignal(StoryDialogueRangeBinding binding)
    {
        if (binding == null)
            return;

        _director?.Pause();
        _waitingDialogue = true;

        var panel = UIManage.GetView<DialoguePanel>();
        if (panel == null || dialogue == null)
        {
            ResumeAfterDialogue();
            return;
        }

        panel.ShowDialogueRange(
            dialogue,
            binding.startIndex,
            binding.endIndexInclusive,
            ResumeAfterDialogue);
    }

    void ResumeAfterDialogue()
    {
        _waitingDialogue = false;
        if (_director == null)
            return;

        if (_director.state == PlayState.Paused)
            _director.Resume();
        else if (_director.state != PlayState.Playing)
            _director.Play();
    }

    void CompleteSequence()
    {
        if (_completed)
            return;

        _completed = true;
        _director?.Stop();

        var outcome = LevelManage.instance?.currentLevel?.outcome;
        if (outcome != null)
            outcome.HandleOutcome(true, Vector3.zero);
        else
            RoguelikeRunService.LeaveStoryEventNode();
    }

    public void OverPlugin(LevelController levelController)
    {
        if (_runCoroutine != null && _controller != null)
        {
            _controller.StopCoroutine(_runCoroutine);
            _runCoroutine = null;
        }

        _bridge?.ClearHandlers();
        if (_stageRoot != null)
            UnityEngine.Object.Destroy(_stageRoot);
        _stageRoot = null;
        _director = null;
        _bridge = null;
        _controller = null;
        _completed = false;
        _waitingDialogue = false;
    }
}
