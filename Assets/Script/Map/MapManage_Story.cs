using UnityEngine;

/// <summary>
/// 肉鸽剧情壳场景 MapManage：最小化地图初始化，Start 时直接触发 EnterMap（不依赖 Timeline）。
/// </summary>
[DefaultExecutionOrder(-100)]
public class MapManage_Story : MapManage
{
    protected override void Awake()
    {
        base.Awake();
        EnsureLevelController();
    }

    protected override void Start()
    {
        EnsureLevelController();
        if (preTiles == null)
            preTiles = new System.Collections.Generic.List<Tile>();
        tiles = new Tile[1, 1];
        LevelManage.instance?.currentController?.EnterMap();
    }

    void EnsureLevelController()
    {
        if (LevelManage.instance?.currentLevel == null)
            return;

        if (LevelManage.instance.currentLevel.levelMode != LevelMode.StoryMode)
            Debug.LogWarning("[MapManage_Story] 当前 LevelData 不是 StoryMode");

        var all = GetComponents<LevelController>();
        LevelController kept = null;
        foreach (var c in all)
        {
            if (c is LevelController_Story && kept == null)
            {
                kept = c;
                continue;
            }

            DestroyImmediate(c);
        }

        if (kept == null)
            gameObject.AddComponent<LevelController_Story>();
    }
}
