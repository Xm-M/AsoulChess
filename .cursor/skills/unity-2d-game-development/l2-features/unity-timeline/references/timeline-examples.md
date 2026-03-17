# Timeline实战案例集

## 案例1：简单过场动画

### 场景描述
角色从A点走到B点，镜头跟随，背景音乐淡入。

### 实现步骤

**步骤1：创建Timeline资源**
```
Project窗口右键 → Create → Timeline
命名：Level01_Cutscene_01
```

**步骤2：配置轨道**
```
1. 添加Animation Track → 绑定Player对象
2. 添加Cinemachine Track → 绑定Main Camera
3. 添加Audio Track → 绑定AudioSource
```

**步骤3：添加剪辑**
```
Animation Track:
- 0s-2s: Idle动画
- 2s-5s: Walk动画

Cinemachine Track:
- 0s-5s: vcam_Follow（跟随镜头）

Audio Track:
- 0s-5s: Background_Music（音量曲线：0→1）
```

---

## 案例2：教程序列编排

### 场景描述
新手引导：高亮移动按钮 → 显示提示文字 → 播放角色移动动画。

### 实现代码

```csharp
using UnityEngine;
using UnityEngine.Playables;

public class TutorialTimeline : MonoBehaviour
{
    public PlayableDirector director;
    public GameObject[] tutorialSteps;
    
    private int currentStep = 0;
    
    public void StartTutorial()
    {
        currentStep = 0;
        PlayStep(currentStep);
    }
    
    void PlayStep(int stepIndex)
    {
        if (stepIndex >= tutorialSteps.Length) return;
        
        // 高亮当前步骤UI
        HighlightStep(tutorialSteps[stepIndex]);
        
        // 播放对应Timeline片段
        director.time = stepIndex * 5f; // 每步5秒
        director.Play();
    }
    
    public void OnStepComplete()
    {
        currentStep++;
        
        if (currentStep < tutorialSteps.Length)
        {
            PlayStep(currentStep);
        }
        else
        {
            // 教程完成
            OnTutorialComplete();
        }
    }
    
    void HighlightStep(GameObject step)
    {
        // 高亮逻辑
    }
    
    void OnTutorialComplete()
    {
        // 教程完成逻辑
    }
}
```

### Timeline配置

```
Timeline结构：
0s-5s: 步骤1（移动教学）
  - Signal: OnShowMoveHint
  - Activation: 高亮移动按钮
  - Animation: 角色移动动画

5s-10s: 步骤2（跳跃教学）
  - Signal: OnShowJumpHint
  - Activation: 高亮跳跃按钮
  - Animation: 角色跳跃动画
```

---

## 案例3：Boss战斗阶段切换

### 场景描述
Boss血量低于50%进入第二阶段，播放转场动画。

### 实现代码

```csharp
using UnityEngine;
using UnityEngine.Playables;

public class BossPhaseController : MonoBehaviour
{
    public PlayableDirector director;
    public TimelineAsset phase1Timeline;
    public TimelineAsset phase2Timeline;
    public TimelineAsset deathTimeline;
    
    private float maxHealth = 1000f;
    private float currentHealth;
    private int currentPhase = 1;
    
    void Update()
    {
        CheckPhaseTransition();
    }
    
    void CheckPhaseTransition()
    {
        float healthPercent = currentHealth / maxHealth;
        
        if (healthPercent <= 0.5f && currentPhase == 1)
        {
            TransitionToPhase(2);
        }
        else if (healthPercent <= 0.2f && currentPhase == 2)
        {
            TransitionToPhase(3);
        }
    }
    
    void TransitionToPhase(int phase)
    {
        currentPhase = phase;
        
        // 停止当前Timeline
        director.Stop();
        
        // 播放阶段转场Timeline
        TimelineAsset transitionTimeline = null;
        
        switch (phase)
        {
            case 2:
                transitionTimeline = phase2Timeline;
                break;
            case 3:
                transitionTimeline = deathTimeline;
                break;
        }
        
        if (transitionTimeline != null)
        {
            director.playableAsset = transitionTimeline;
            director.Play();
        }
    }
    
    // 由Timeline Signal调用
    public void OnPhaseTransitionComplete()
    {
        // 恢复Boss战斗控制
        EnableBossAI();
    }
    
    void EnableBossAI()
    {
        // 启用Boss AI逻辑
    }
}
```

### Timeline Signal配置

**Phase2转场Timeline**：
```
0s-2s: Boss怒吼动画
  - Animation Track: Boss_Roar
  - Audio Track: Boss_Roar_Sound
  - Signal: OnDisablePlayerInput

2s-4s: 环境变化
  - Control Track: 粒子特效（火焰）
  - Activation Track: 战斗场地变化
  - Signal: OnChangeEnvironment

4s-5s: 转场完成
  - Signal: OnPhaseTransitionComplete
```

---

## 案例4：对话系统Timeline驱动

### 场景描述
NPC对话，角色表情动画，镜头切换。

### 实现代码

```csharp
using UnityEngine;
using UnityEngine.Playables;
using TMPro;

public class TimelineDialogueSystem : MonoBehaviour
{
    public static TimelineDialogueSystem Instance { get; private set; }
    
    public PlayableDirector director;
    public GameObject dialoguePanel;
    public TextMeshProUGUI speakerNameText;
    public TextMeshProUGUI dialogueText;
    
    private Queue<DialogueLine> dialogueQueue = new Queue<DialogueLine>();
    
    void Awake()
    {
        Instance = this;
        dialoguePanel.SetActive(false);
    }
    
    public void StartDialogue(DialogueData data)
    {
        dialogueQueue.Clear();
        
        foreach (var line in data.lines)
        {
            dialogueQueue.Enqueue(line);
        }
        
        dialoguePanel.SetActive(true);
        PlayNextLine();
    }
    
    void PlayNextLine()
    {
        if (dialogueQueue.Count == 0)
        {
            EndDialogue();
            return;
        }
        
        DialogueLine line = dialogueQueue.Dequeue();
        
        // 更新UI
        speakerNameText.text = line.speakerName;
        StartCoroutine(TypeText(line.text));
        
        // 播放对应的Timeline片段
        director.time = line.timelineStartTime;
        director.Play();
    }
    
    System.Collections.IEnumerator TypeText(string text)
    {
        dialogueText.text = "";
        
        foreach (char letter in text)
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(0.03f);
        }
    }
    
    void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        director.Stop();
    }
    
    // 由Timeline Signal调用
    public void OnDialogueLineComplete()
    {
        PlayNextLine();
    }
}

[System.Serializable]
public class DialogueLine
{
    public string speakerName;
    [TextArea] public string text;
    public float timelineStartTime;
}

[CreateAssetMenu(menuName = "Dialogue/DialogueData")]
public class DialogueData : ScriptableObject
{
    public DialogueLine[] lines;
}
```

### Timeline配置

**对话Timeline**：
```
0s-3s: 第一句对话
  - Cinemachine Track: vcam_NPC（NPC特写）
  - Animation Track: NPC_Talk（说话动画）
  - Signal: OnDialogueLineComplete

3s-6s: 第二句对话
  - Cinemachine Track: vcam_Player（玩家特写）
  - Animation Track: Player_Listen（倾听动画）
  - Signal: OnDialogueLineComplete
```

---

## 案例5：游戏开始序列

### 场景描述
游戏启动 → Logo动画 → 主菜单显示。

### 实现代码

```csharp
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class GameStartupSequence : MonoBehaviour
{
    public PlayableDirector introDirector;
    public TimelineAsset introTimeline;
    public float skipDelay = 1f;
    
    private bool canSkip = false;
    
    void Start()
    {
        // 播放开场序列
        introDirector.playableAsset = introTimeline;
        introDirector.Play();
        
        // 延迟允许跳过
        Invoke(nameof(EnableSkip), skipDelay);
    }
    
    void EnableSkip()
    {
        canSkip = true;
    }
    
    void Update()
    {
        if (canSkip && Input.anyKeyDown)
        {
            SkipIntro();
        }
    }
    
    void SkipIntro()
    {
        introDirector.Stop();
        LoadMainMenu();
    }
    
    // 由Timeline stopped事件调用
    void OnIntroComplete(PlayableDirector director)
    {
        LoadMainMenu();
    }
    
    void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
```

### Timeline配置

**开场序列Timeline**：
```
0s-2s: 黑屏渐亮
  - Activation Track: BlackScreen（淡出）

2s-5s: Logo显示
  - Activation Track: GameLogo
  - Animation Track: Logo_Scale（缩放动画）
  - Audio Track: Logo_Sound

5s-8s: Logo淡出
  - Animation Track: Logo_FadeOut

8s: 序列完成
  - Signal: OnIntroComplete
```

---

## 最佳实践总结

### 1. Timeline资源管理

```csharp
// 使用Addressables动态加载
using UnityEngine.AddressableAssets;

public class TimelineLoader : MonoBehaviour
{
    public async void LoadTimeline(string address)
    {
        var handle = Addressables.LoadAssetAsync<TimelineAsset>(address);
        await handle.Task;
        
        director.playableAsset = handle.Result;
    }
}
```

### 2. Timeline状态保存

```csharp
public class TimelineStateSaver : MonoBehaviour
{
    public PlayableDirector director;
    public string saveKey = "Timeline_Progress";
    
    public void SaveProgress()
    {
        PlayerPrefs.SetFloat(saveKey, (float)director.time);
    }
    
    public void LoadProgress()
    {
        if (PlayerPrefs.HasKey(saveKey))
        {
            director.time = PlayerPrefs.GetFloat(saveKey);
            director.Evaluate();
        }
    }
}
```

### 3. Timeline调试工具

```csharp
#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(TimelineController))]
public class TimelineControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        var controller = (TimelineController)target;
        
        if (GUILayout.Button("Play"))
        {
            controller.Play();
        }
        
        if (GUILayout.Button("Pause"))
        {
            controller.Pause();
        }
        
        if (GUILayout.Button("Stop"))
        {
            controller.Stop();
        }
    }
}
#endif
```
