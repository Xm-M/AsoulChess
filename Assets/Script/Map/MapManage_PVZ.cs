using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Playables;
using UnityEngine.Rendering.Universal;

[DefaultExecutionOrder(-100)]
//类似PVZ类型的地图 都是向右走啦 
public class MapManage_PVZ : MapManage
{
    public Transform sunLightRecyclePos;
    public List<Tile> zombiePreTile;
    public List<Tile> roomTile;
    public Tile deathTile;
    //public AudioPlayer au;
    public PlayableDirector dir;
    public PropertyCreator car;
    public Light2D GlobleLight;
    //public LevelData room;
    public float lightBase, lightRate;
    protected override void Awake()
    {
        base.Awake();
        if (SaveLoadContext.IsLoadFromSave && dir != null)
            dir.playOnAwake = false;
        EnsureLevelController();
    }


    protected override void Start()
    {
        EnsureLevelController();
        base.Start();
        lightBase = GlobleLight.intensity;
        lightRate = 1;
        if (SaveLoadContext.IsLoadFromSave && dir != null)
        {
            StartCoroutine(SkipTimelineAndRunLoadFlow());
        }
    }

    /// <summary>
    /// 按关卡模式挂载对应 LevelController；生存模式使用 LevelController_Endless。
    /// 先移除已有（含错误类型/重复挂载），再 Add，避免 Destroy 延迟导致同帧叠多个 Controller。
    /// </summary>
    public void EnsureLevelController()
    {
        if (LevelManage.instance?.currentLevel == null)
            return;

        var mode = LevelManage.instance.currentLevel.levelMode;
        bool survival = mode == LevelMode.SurvivalMode;
        bool boss = mode == LevelMode.BossMode;
        var all = GetComponents<LevelController>();
        LevelController kept = null;

        foreach (var c in all)
        {
            bool correct = survival
                ? c is LevelController_Endless
                : boss
                    ? c is LevelController_Boss
                    : c is not LevelController_Endless && c is not LevelController_Boss;
            if (correct && kept == null)
            {
                kept = c;
                continue;
            }
            RemoveLevelControllerImmediate(c);
        }

        if (kept != null)
            return;

        if (survival)
            gameObject.AddComponent<LevelController_Endless>();
        else if (boss)
            gameObject.AddComponent<LevelController_Boss>();
        else
            gameObject.AddComponent<LevelController>();
    }

    static void RemoveLevelControllerImmediate(LevelController controller)
    {
        if (controller == null)
            return;
        // EnsureLevelController 可能在 Awake/Start/Timeline 同帧连调，Destroy 延迟会导致叠多个 Controller
        Object.DestroyImmediate(controller);
    }

    public void Timeline_EnterMap()
    {
        EnsureLevelController();
        LevelManage.instance?.currentController?.EnterMap();
    }

    public void Timeline_GamePrepare()
    {
        EnsureLevelController();
        LevelManage.instance?.currentController?.GamePrepare();
    }

    public void Timeline_GameStart()
    {
        EnsureLevelController();
        LevelManage.instance?.currentController?.GameStart();
    }

    private System.Collections.IEnumerator SkipTimelineAndRunLoadFlow()
    {
        yield return null;
        if (SaveLoadContext.LoadFlowExecuted) yield break;
        if (dir == null || LevelManage.instance?.currentLevel == null) yield break;
        float skipTime = LevelManage.instance.currentLevel.loadSkipToTime;
        for (int i = 0; i < 3 && dir.duration <= 0; i++)
            yield return null;
        double duration = dir.duration;
        if (duration > 0)
        {
            dir.Stop();
            dir.time = Mathf.Clamp((float)skipTime, 0, (float)duration);
            dir.Evaluate();
        }
        else if (dir.playableAsset != null)
        {
            dir.Stop();
            dir.time = skipTime;
            dir.Evaluate();
        }
        EnsureLevelController();
        var controller = LevelManage.instance?.currentController;
        if (controller != null && SaveLoadContext.IsLoadFromSave)
        {
            SaveLoadContext.LoadFlowExecuted = true;
            controller.EnterMap();
            controller.GamePrepare();
            bool survival = LevelManage.instance.currentLevel.levelMode == LevelMode.SurvivalMode;
            if (!survival)
                controller.GameStart();
        }
    }
  
    /// <summary>
    /// 下面的这几个函数都是为地图动画调用准备的 所以本身并没有被调用
    /// </summary>
    public void WhenGameStart()
    {
        if (dir != null)
            dir.Play();
        else
            Timeline_GameStart();
    }
    public void WhenGameOver()
    {
        //au.Stop();

    }
     
    private void OnDestroy()
    {
        //EventController.Instance.RemoveListener(EventName.GameOver.ToString(), WhenGameOver);
        //EventController.Instance.RemoveListener(EventName.GameStart.ToString(), WhenGameStart);
    }
    public void ChangeLight(float light)
    {
        lightRate += light;
        if (GlobleLight != null) {
            GlobleLight.intensity =lightBase*lightRate;
        }
    }
    public void ResumeLight()
    {
        lightRate = 1;
        GlobleLight.intensity = lightBase;
    }

}
