using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public enum EventName
{
    WhenChessDestroy,
    CreateChess,
    WhenAttackTakeDamages,
    WhenBeAttack,
    WhenDeath,
    GameOver,
    RestartGame,
    GameStart,
    WhenSceneLoad,
    SelectState,
    WhenAddMember,
    CheckFetter,
    DamageFlatAmount,
    DamagePersentAmout,
    WhenHurt,
    AfterTakeDamage,
    WhenUseSkill,
    WhenSkillOver,
    WhenHealOther,
    WhenSummon,
    WhenEnterTile,
    WhenBuyChess,
    WhenSelectChess,
    TeamDeath,
    EnterNewRoom,
    WhenMoveTile,
    ChessEnterDesk,
    ChessLeaveDesk,
    ResetChess,
    WhenChessEnterWar,
    WhenSunLightChange,
    WaveZombieComming,
    LastWaveZombie,
    FirstZombieComming,
    GameSuccess,
    WhenPlantChess,
    WhenSweetChange,
    WhenLeaveLevel,
    PauseGame,
    ResumeGame,
    /// <summary>贪吃蛇：触发眩晕时统一派发（越界 / 咬自己 / 撞大体型敌）；载荷见 <see cref="SnakeGameEventId"/>。</summary>
    SnakeHitWall,
    /// <summary>贪吃蛇：吃掉关卡食物；载荷为 <see cref="SnakeEatFoodPayload"/>（棋子名 + 分隔符 + 描述文案），UI 图标仍由插件里 SnakeEatFood 绑定提供。</summary>
    SnakeEatFood,
    /// <summary>贪吃蛇：吃掉更小体型敌方；载荷为 <see cref="SnakeGameEventId.EatZombie"/>。</summary>
    SnakeEatZombie,
    /// <summary>贪吃蛇：场上新生成食物；载荷为 <see cref="SnakeGameEventId.FoodSpawned"/>。</summary>
    SnakeFoodSpawned,
}


public interface IEventAction
{
}
public class EventAction:IEventAction
{
    public UnityAction action;
}
public class EventAction<T> : IEventAction
{
    public UnityAction<T> action;
}
public class EventController 
{
    static EventController instance;
    public static EventController Instance
    {
        get
        {
            if (instance == null) instance = new EventController();
            return instance;
        }
    }
    public Dictionary<string, IEventAction> eventActionDic=new Dictionary<string, IEventAction>();
    //���Ӽ���
    public void AddListener(string name,UnityAction action)
    {
        if (!eventActionDic.ContainsKey(name)) eventActionDic.Add(name, new EventAction());
        (eventActionDic[name] as EventAction).action += action;
    }
    public void AddListener<T>(string name, UnityAction<T> action)
    {
        if (!eventActionDic.ContainsKey(name)) eventActionDic.Add(name, new EventAction<T>());
        (eventActionDic[name] as EventAction<T>).action += action;
    }
    //�Ƴ�����
    public void RemoveListener(string name,UnityAction action)
    {
        if(eventActionDic.ContainsKey(name))
        (eventActionDic[name] as EventAction).action -= action;
    }
    public void RemoveListener<T>(string name, UnityAction<T> action)
    {
        if (eventActionDic.ContainsKey(name))
            (eventActionDic[name] as EventAction<T>).action -= action;
    }

    public void TriggerEvent(string name)
    {
        if (eventActionDic.ContainsKey(name))
            (eventActionDic[name] as EventAction).action?.Invoke();
    }
    public void TriggerEvent<T>(string name,T message)
    {
        
        if (eventActionDic.ContainsKey(name))
        {
            (eventActionDic[name] as EventAction<T>).action?.Invoke(message);
        }
    }
}
