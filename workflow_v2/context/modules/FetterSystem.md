# 羁绊系统 (Fetter System)

## 概述

羁绊系统是游戏的核心机制之一，当队伍中有特定标签的棋子组合时，触发特殊效果。例如"结束乐队"羁绊：放置特定植物时生成一次性小卡。

## 核心类

### Fetter (羁绊基类)
**文件**: `Assets/Script/Fetter/Fetter.cs`

所有羁绊的基类。

```csharp
public abstract class Fetter : SerializedScriptableObject
{
    public FetterData fetterData;
    
    public abstract void FetterEffect(int num);    // 羁绊触发
    public abstract void ResetFetter();            // 羁绊重置
    public virtual bool FetterLight(int num) { }   // 检查羁绊是否点亮
}
```

### FetterController (羁绊控制器)
**文件**: `Assets/Script/Fetter/FetterController.cs`

管理所有羁绊的检测和触发。

```csharp
public class FetterController
{
    Dictionary<string, int> fetterNumDic;   // 各羁绊数量统计
    Dictionary<string, Fetter> fetterDic;   // 羁绊配置
    List<Fetter> lightFetter;               // 已点亮的羁绊
    
    public void InitController() { }
    public void CheckFetter() { }           // 检测羁绊
}
```

**检测流程**:
1. 遍历 `PlantsShop.currentSelectIcons` 统计各羁绊标签数量
2. 检查是否满足羁绊条件
3. 触发 `FetterEffect()`

## 具体羁绊实现

### 结束乐队羁绊 (示例)
**文件**: `Assets/Script/Fetter/Fetter.cs` (内嵌类)

放置特定植物时生成小卡。

```csharp
public class Fetter_JieShuYueDui : Fetter
{
    [LabelText("卡牌列表")]
    public List<PropertyCreator> cards;     // 可生成的小卡配置
    [LabelText("间隔时间")]
    public float interval = 15f;
    [LabelText("下落速度")]
    public float fallSpeed = 2f;
    
    public override void FetterEffect(int num)
    {
        // 注册放置棋子事件监听
        EventController.Instance.AddListener<Chess>(
            EventName.WhenPlantChess.ToString(), 
            OnPlantChess
        );
        
        // 减少CD时间
        foreach(var icon in UIManage.GetView<PlantsShop>().currentShopIcons)
        {
            if (icon.good.plantTags.Contains("结束乐队"))
            {
                icon.coldDown = icon.coldDown * 0.6f;
            }
        }
    }
    
    public void OnPlantChess(Chess chess)
    {
        // 检查是否是羁绊相关棋子
        if (!cards.Contains(chess.propertyController.creator) && 
            chess.propertyController.creator.plantTags.Contains("结束乐队"))
        {
            // 生成对应的小卡
            Item_PlantCard card = UIManage.GetView<ItemPanel>().Create<Item_PlantCard>();
            card.InitCard(chess.transform.position, creator);
        }
    }
    
    public override void ResetFetter()
    {
        // 取消事件监听
        EventController.Instance.RemoveListener<Chess>(
            EventName.WhenPlantChess.ToString(), 
            OnPlantChess
        );
    }
}
```

## 小卡(Item_PlantCard)机制

### 生成流程

```
1. 玩家放置"结束乐队"棋子
2. 触发 WhenPlantChess 事件
3. Fetter.OnPlantChess() 被调用
4. 创建 Item_PlantCard
5. 小卡从屏幕上方掉落
```

### 使用流程

```
1. 点击小卡 → goodImage.color = 透明
2. 进入 PrePlantImage 预览模式
3. 点击格子 → 放置棋子
4. 调用 Recycle() → 小卡隐藏并回收到对象池
```

### 对象池机制

```csharp
// ItemPanel 对象池
Dictionary<Type, Stack<UIItem>> itemPool;

// 使用小卡
Item_PlantCard card = UIManage.GetView<ItemPanel>().Create<Item_PlantCard>();

// 回收小卡（隐藏而非销毁）
itemPanel.Recycle<Item_PlantCard>(card);
```

## 存档相关影响

### 问题1: 读档后小卡重复

**原因**: 
1. 读档时重新创建植物会触发 `WhenPlantChess` 事件
2. 羁绊系统重新生成小卡
3. 已有的小卡 + 新生成的小卡 = 重复

**解决**:
```csharp
// 读档前清理所有小卡
private static void ClearAllPlantCards()
{
    // 销毁所有活跃小卡
    Item_PlantCard[] plantCards = itemPanel.GetComponentsInChildren<Item_PlantCard>(true);
    foreach (var card in plantCards) Destroy(card.gameObject);
    
    // 清理对象池
    TryClearPlantCardPool(itemPanel);
}
```

### 问题2: 小卡无限使用

**原因**: 
- 小卡使用后被回收到对象池（隐藏）
- 读档时如果不清除对象池，可能错误地重新激活

**解决**: 
- 同上，清理对象池缓存

### 问题3: 已使用小卡恢复

**原因**: 
- 小卡使用后只是隐藏，没有标记为已使用
- 读档后羁绊重新生成，或对象池复用

**解决**:
```csharp
// 存档时记录小卡状态
foreach (var card in plantCards)
{
    bool isUsed = TryGetFieldValue<bool>(card, "plantOver");
    data.plantCards.Add(new PlantCardSaveData
    {
        cardId = card.creator?.chessName,
        isUsed = isUsed
    });
}

// 读档时销毁已使用的小卡
foreach (var savedCard in data.plantCards)
{
    if (savedCard.isUsed)
    {
        // 找到并销毁对应小卡
        foreach (var currentCard in currentCards)
        {
            if (currentCard.creator?.chessName == savedCard.cardId)
            {
                Destroy(currentCard.gameObject);
            }
        }
    }
}
```

## 事件系统

羁绊系统依赖以下事件:

### WhenPlantChess
放置棋子时触发。

```csharp
// 订阅
EventController.Instance.AddListener<Chess>(
    EventName.WhenPlantChess.ToString(), 
    OnPlantChess
);

// 触发 (在 ChessManage.cs 中)
EventController.Instance.TriggerEvent<Chess>(
    EventName.WhenPlantChess.ToString(), 
    chess
);
```

### GameOver / WhenLeaveLevel
游戏结束或离开关卡时清理。

```csharp
public override void ResetFetter()
{
    EventController.Instance.RemoveListener<Chess>(
        EventName.WhenPlantChess.ToString(), 
        OnPlantChess
    );
}
```

## 依赖关系

```
Fetter
├── FetterData (配置)
└── EventController (事件监听)

FetterController
├── PlantsShop.currentSelectIcons (统计羁绊)
├── FetterPanel (显示羁绊)
└── List<Fetter> (已点亮羁绊)

Fetter_JieShuYueDui (示例)
├── PropertyCreator cards (可生成的小卡)
├── ItemPanel (小卡容器)
│   └── Item_PlantCard (小卡)
└── EventName.WhenPlantChess (事件)
```

## 常见问题

| 问题 | 原因 | 解决方案 |
|------|------|---------|
| 读档后小卡重复 | WhenPlantChess事件重新触发 | 读档前清理所有小卡 |
| 小卡无限使用 | 对象池复用 | 清理对象池缓存 |
| 已使用小卡恢复 | 没有正确标记isUsed | 存档时记录plantOver字段 |
| 羁绊不触发 | 事件监听未注册 | 检查FetterEffect是否被调用 |

## 最佳实践

1. **事件注册/注销**: 在 `FetterEffect()` 注册，在 `ResetFetter()` 注销
2. **对象池清理**: 读档时必须清理对象池，避免重复
3. **状态标记**: 小卡使用后必须标记 `plantOver = true`
4. **配置查找**: 使用 `chessName` 作为唯一标识
