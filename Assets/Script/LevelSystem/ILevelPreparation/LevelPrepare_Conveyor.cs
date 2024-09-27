using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ���Ǵ��ʹ�ģʽ��׼���׶� ��ʵ��û��׼���׶��� ���ǻῴһ�۽�ʬ ͬʱʹ�õ������ɴ��ʹ�
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
