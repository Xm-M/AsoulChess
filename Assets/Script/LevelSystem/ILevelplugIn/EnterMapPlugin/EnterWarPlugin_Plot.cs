using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

/// <summary>
/// 剧情插件 等待剧情通过再继续
/// </summary>
public class EnterWarPlugin_Plot : ILevelPlugin
{
    public PlayableDirector plotDirector;
    PlayableDirector mapDir;
    bool over;
    public void StadgeEffect(LevelController levelController)
    {
        //throw new System.NotImplementedException();
        mapDir = (MapManage.instance as MapManage_PVZ).dir;
        mapDir.Pause();
        EventController.Instance.AddListener(EventName.WhenLeaveLevel.ToString(), WhenLeave);
    }
    IEnumerator WaitPlot()
    {
        while (!over)
        {
            if (plotDirector.state != PlayState.Playing && plotDirector.time >= plotDirector.duration)
            {
                over = true;
            }
            yield return null;
        }
        if (mapDir != null) mapDir.Play();
        WhenLeave();
    }
    public void WhenLeave()
    {
        over = true;
        EventController.Instance.RemoveListener(EventName.WhenLeaveLevel.ToString(), WhenLeave);
    }
    public void OverPlugin(LevelController levelController)
    {
        //暂时不需要
    }
}
