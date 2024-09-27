using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 这是传送带模式的准备阶段 其实就没有准备阶段了 但是会看一眼僵尸 同时使用的栏会变成传送带
/// </summary>
public class LevelPrepare_Conveyor : ILevelPreparation
{
    public List<PropertyCreator> creators;
    public void Prepare(LevelData levelData)
    {
        UIManage.Show<ConveyorPanel>();
        UIManage.GetView<ConveyorPanel>().InitCreator(creators);
    }
}
