# Event 模块

## 基本信息

**定位**: 全局事件系统，实现模块间解耦通信
**复杂度**: 低
**脚本数**: 1个
**核心类**: EventController, EventName (枚举)

## 核心类详解

### EventController

**类型**: 单例类
**职责**: 管理所有游戏事件的订阅和触发

```csharp
public class EventController 
{
    static EventController instance;
    public static EventController Instance { get; }
    
    Dictionary<string, IEventAction> eventActionDic;
    
    // 订阅事件
    public void AddListener(string name, UnityAction action)
    public void AddListener<T>(string name, UnityAction<T> action)
    
    // 移除监听
    public void RemoveListener(string name, UnityAction action)
    public void RemoveListener<T>(string name, UnityAction<T> action)
    
    // 触发事件
    public void TriggerEvent(string name)
    public void TriggerEvent<T>(string name, T message)
}
```

### EventName (枚举)

**游戏核心事件列表**:

| 事件名 | 说明 |
|--------|------|
| GameStart | 游戏开始 |
| GameOver | 游戏结束 |
| WhenDeath | 棋子死亡 |
| WhenAttackTakeDamages | 造成伤害 |
| WhenBeAttack | 受到攻击 |
| WhenUseSkill | 使用技能 |
| WhenPlantChess | 放置棋子 |
| WhenChessEnterWar | 棋子进入战斗 |
| WhenLeaveLevel | 离开关卡 |
| SelectState | 选择状态 |
| DamageFlatAmount | 固定伤害 |
| DamagePersentAmout | 百分比伤害 |
| ... | ... |

## 使用示例

```csharp
// 订阅事件（通常在 Awake/OnEnable 中）
EventController.Instance.AddListener(EventName.GameStart.ToString(), OnGameStart);
EventController.Instance.AddListener<Chess>(EventName.WhenDeath.ToString(), OnChessDeath);

// 触发事件
EventController.Instance.TriggerEvent(EventName.GameStart.ToString());
EventController.Instance.TriggerEvent<Chess>(EventName.WhenDeath.ToString(), deadChess);

// 移除监听（重要！在 OnDestroy/OnDisable 中）
EventController.Instance.RemoveListener(EventName.GameStart.ToString(), OnGameStart);
```

## 注意事项

1. **内存泄漏风险**: 必须在对象销毁时移除事件监听
2. **线程安全**: 事件在 Unity 主线程触发
3. **性能考虑**: 避免在 Update 中频繁触发事件
4. **泛型支持**: 支持带参数的事件 `<T>`
