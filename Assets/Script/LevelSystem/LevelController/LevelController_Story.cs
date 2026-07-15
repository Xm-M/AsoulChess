using UnityEngine;

/// <summary>
/// 剧情关控制器：无波次、无刷怪、无 ProgressBar；Timeline + 对话由 <see cref="EnterMapPlugin_StoryStage"/> 驱动。
/// </summary>
public class LevelController_Story : LevelController_TestArena
{
    void Awake()
    {
        skipStartupUiAndSave = true;
    }

    public override void EnterMap()
    {
        if (levelData?.EnterMapPlugin != null)
        {
            for (int i = 0; i < levelData.EnterMapPlugin.Count; i++)
                levelData.EnterMapPlugin[i].StadgeEffect(this);
        }

        CreateZombieWaves();
        EventController.Instance.TriggerEvent(EventName.EnterMap.ToString());
    }
}
