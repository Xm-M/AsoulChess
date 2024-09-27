using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// roomtype���Ǿ����ÿ������԰�
/// ��ʵ����ÿ��������
/// һ��roomType��Ӧһ������
/// </summary>

[CreateAssetMenu(fileName = "NewLevel", menuName = "Message/Level")]
public class LevelData : ScriptableObject
{
    public string sceneName;
    public string levelName;
    public LevelMode levelMode;
    [SerializeReference]
    public ILevelPreparation preparation;
    [SerializeReference]
    public IZombieSpawner spawner;
    [SerializeReference]
    public ILevelOutcome outcome;
    /// <summary>
    /// ׼���׶�Ҫ��������
    /// </summary>
    public  void PrepareStage()
    {
        preparation.Prepare(this);
        spawner.Prepare(this);
    }
    /// <summary>
    /// �ս�����Ϸʱ���¼�
    /// </summary>
    public  void StartGameStage(){
        spawner?.StartSpawning(this);
    }
    /// <summary>
    /// ��Ϸ����ʱ���¼�
    /// </summary>
    public void OverGameStage(){
        spawner.OverSpawning(this);
        outcome?.HandleOutcome(spawner.CheckWinCondition());
    }
    /// <summary>
    /// �뿪�ؿ��ᴥ�����¼�
    /// </summary>
    public  void LeaveStage()
    {

    }
}
/// <summary>
/// ����ӿ���Ҫ�Ǹ���ѡ��ģʽ�����п�����Ϸ��ʼ
/// </summary>
public interface ILevelPreparation
{
    public void Prepare(LevelData levelData);
}
/// <summary>
/// ����ӿ��ǽ�ʬ�����ɷ�ʽ
/// </summary>
public interface IZombieSpawner
{
    public void Prepare(LevelData levelData);
    public void StartSpawning(LevelData levelData);
    public void OverSpawning(LevelData levelData);
    bool CheckWinCondition();
}
/// <summary>
/// �����ʤ����Ľ���׶�
/// </summary>
public interface ILevelOutcome
{
    public void HandleOutcome(bool win);
}

