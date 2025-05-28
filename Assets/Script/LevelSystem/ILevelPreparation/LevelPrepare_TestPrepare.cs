using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelPrepare_TestPrepare : ILevelPreparation
{
    public void Prepare(LevelData levelData)
    {
        UIManage.Show<TestScenePanel>();
        (MapManage_PVZ.instance as MapManage_PVZ).WhenGameStart();
    }
}
