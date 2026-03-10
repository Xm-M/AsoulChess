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
    }


    protected override void Start()
    {
        base.Start();
        lightBase = GlobleLight.intensity;
        lightRate = 1;
        if (SaveLoadContext.IsLoadFromSave && dir != null && LevelManage.instance?.currentController != null)
        {
            StartCoroutine(SkipTimelineAndRunLoadFlow());
        }
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
        var controller = LevelManage.instance?.currentController;
        if (controller != null && SaveLoadContext.IsLoadFromSave)
        {
            SaveLoadContext.LoadFlowExecuted = true;
            controller.EnterMap();
            controller.GamePrepare();
            controller.GameStart();
        }
    }
  
    /// <summary>
    /// 下面的这几个函数都是为地图动画调用准备的 所以本身并没有被调用
    /// </summary>
    public void WhenGameStart()
    {
        dir.Play();
        
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
