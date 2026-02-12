using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelController_HammerZombie : LevelController
{
    /// <summary>
    /// 这个不用改
    /// </summary>
    /// <param name="levelData"></param>
    public override void Init(LevelData levelData)
    {
        base.Init(levelData);
    }
    /// <summary>
    /// 这个也不用改
    /// </summary>
    public override void EnterMap()
    {
        base.EnterMap();
    }
    public override void GamePrepare()
    {
        base.GamePrepare();
    }
    public override void GameOver(bool win)
    {
        base.GameOver(win);
    }
    //这几个好像都用不改 都在插件里面搞就行


    /// <summary>
    /// 就这两个是最需要改的
    /// </summary>
    public override void CreateZombieWaves()
    {
        base.CreateZombieWaves();
    }
    protected override void Update()
    {
        base.Update();
    }
}
