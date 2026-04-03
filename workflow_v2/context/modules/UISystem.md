# UI系统 (UI System)

## 概述

基于 View-Manager 架构的UI系统，支持面板管理、对象池复用、事件驱动更新。

## 核心架构

### UIManage (静态类)
**文件**: `Assets/Script/Manage/UIManage.cs`

全局UI管理器，使用静态方法管理所有View。

```csharp
public class UIManage
{
    // 显示/隐藏面板
    public static void Show<T>() where T : View
    public static void Close<T>() where T : View
    public static void Close(string name)
    
    // 获取面板实例
    public static T GetView<T>() where T : View
    
    // 检查面板是否显示
    public static bool IfViewShow<T>() where T : View
}
```

**关键注意点**:
- UIManage 是**静态类**，没有 `instance` 字段
- 使用 `UIManage.GetView<T>()` 而不是 `UIManage.instance.GetView<T>()`

### View (基类)
**文件**: `Assets/Script/UI/View/View.cs`

所有UI面板的基类。

```csharp
public abstract class View : MonoBehaviour
{
    public virtual void Init() { }      // 初始化
    public virtual void Show() { }      // 显示
    public virtual void Hide() { }      // 隐藏
}
```

生命周期:
1. `Init()` - 面板创建时调用，注册事件监听
2. `Show()` - 面板显示时调用
3. `Hide()` - 面板隐藏时调用，清理资源

## 主要UI面板

### PlantsShop (选卡栏)
**文件**: `Assets/Script/UI/Shop/PlantsShop.cs`

游戏主选卡界面，包含上方主卡和下方选择区。

```csharp
public class PlantsShop : View
{
    public List<ShopSelectIcon> currentSelectIcons;  // 选中的卡牌（下方）
    public List<ShopIcon> currentShopIcons;          // 场上主卡（上方）
    public List<ShopSelectIcon> allSelectIcons;      // 所有可选卡牌
    
    public void GameStart() { }                      // 游戏开始
    public void AddShopIcon(ShopIcon shopIcon) { }   // 添加主卡
    public void RemoveShopIcon(ShopIcon shopicon) { } // 移除主卡
}
```

**存档相关**:
- `currentShopIcons` 的CD状态需要存档
- 通过反射访问 `t` 字段获取当前CD计时
- 读档后需要恢复每张主卡的CD

### ShopIcon (主卡)
**文件**: `Assets/Script/UI/Shop/ShopIcon.cs`

上方栏的卡片，有CD冷却机制。

```csharp
public class ShopIcon : MonoBehaviour
{
    public PropertyCreator good;    // 棋子配置
    public float coldDown;          // 总CD时间
    private float t;                // 当前CD计时（私有字段）
    
    public void InitShopIcon(ShopSelectIcon s) { }
    public bool IfColdDown() { }    // 检查CD是否结束
    public void ColdDown() { }      // 开始CD
}
```

**关键字段**:
- `coldDown` - 总CD时间（public）
- `t` - 当前CD计时（private，需要反射访问）
- CD计算: `remainingCD = coldDown - t`

### ItemPanel (道具栏/小卡容器)
**文件**: `Assets/Script/UI/View/ItemPanel.cs`

存放羁绊生成的小卡(Item_PlantCard)。

```csharp
public class ItemPanel : View
{
    Dictionary<Type, Stack<UIItem>> itemPool;  // 对象池
    
    public T Create<T>() where T : UIItem      // 创建/获取小卡
    public void Recycle<T>(UIItem item)        // 回收小卡到对象池
}
```

**对象池机制**:
- 小卡使用后被 `Recycle()` 回收到对象池（隐藏，非销毁）
- 再次使用时从对象池取出复用
- 读档时需要清理对象池，避免重复小卡

### Item_PlantCard (小卡/一次性卡)
**文件**: `Assets/Script/UI/UIItem/Item_PlantCard.cs`

羁绊系统生成的一次性使用卡片。

```csharp
public class Item_PlantCard : UIItem
{
    public PropertyCreator creator;     // 棋子配置
    private bool plantOver;             // 是否已使用（私有字段）
    
    public void OnPointerClick(PointerEventData eventData) { }
    public override void Recycle() { }  // 使用后回收
}
```

**使用流程**:
1. 点击小卡 → 进入种植预览模式
2. 点击格子 → 放置棋子
3. 调用 `Recycle()` → 小卡隐藏并回收到对象池

**存档相关**:
- 需要记录哪些小卡已使用 (`plantOver = true`)
- 读档时需要销毁已使用的小卡（包括对象池中的）

### SunLightPanel (阳光面板)
**文件**: `Assets/Script/UI/Shop/SunLightPanel.cs`

显示和管理阳光值。

```csharp
public class SunLightPanel : MonoBehaviour
{
    public static SunLightPanel instance;
    public int sunLight { get; private set; }
    
    public void ChangeSunLight(int num) { }  // 改变阳光值
    public void SetSunLight(int num) { }     // 设置阳光值（读档用）
}
```

**存档相关**:
- 存档时记录 `sunLight`
- 读档时使用 `SetSunLight()` 恢复（不触发事件）

## 对象池系统

### 工作原理

```csharp
// 创建小卡
public T Create<T>() where T : UIItem
{
    Type t = typeof(T);
    
    // 1. 检查对象池是否有可用对象
    if (itemPool.ContainsKey(t) && itemPool[t].Count > 0)
    {
        T target = itemPool[t].Pop() as T;
        target.gameObject.SetActive(true);  // 激活
        return target;
    }
    
    // 2. 没有则创建新的
    GameObject item = Instantiate(pre, transform);
    return item.GetComponent<T>();
}

// 回收小卡
public void Recycle<T>(UIItem item) where T : UIItem
{
    item.gameObject.SetActive(false);  // 隐藏
    item.transform.SetParent(transform);
    itemPool[typeof(T)].Push(item);    // 放入对象池
}
```

### 存档影响

**问题**: 小卡使用后被隐藏而非销毁，读档时羁绊系统会重新生成小卡，导致重复。

**解决**:
```csharp
// 读档前清理所有小卡
private static void ClearAllPlantCards()
{
    // 1. 销毁所有活跃小卡
    Item_PlantCard[] plantCards = itemPanel.GetComponentsInChildren<Item_PlantCard>(true);
    foreach (var card in plantCards) Destroy(card.gameObject);
    
    // 2. 清理对象池缓存
    TryClearPlantCardPool(itemPanel);
}
```

## 事件系统

UI面板通过 EventController 订阅游戏事件:

```csharp
public override void Init()
{
    // 订阅事件
    EventController.Instance.AddListener(EventName.WhenLeaveLevel.ToString(), Hide);
    EventController.Instance.AddListener(EventName.GameStart.ToString(), OnGameStart);
}

private void OnDestroy()
{
    // 取消订阅（避免内存泄漏）
    EventController.Instance.RemoveListener(EventName.WhenLeaveLevel.ToString(), Hide);
}
```

常用事件:
- `WhenLeaveLevel` - 离开关卡时清理UI
- `GameStart` - 游戏开始时初始化UI
- `WhenPlantChess` - 放置棋子时（羁绊系统用）

## 存档数据流

```
存档时:
PlantsShop.currentShopIcons → 遍历获取每张卡的CD → ShopIconSaveData
ItemPanel.GetComponentsInChildren<Item_PlantCard> → 获取小卡状态 → PlantCardSaveData
SunLightPanel.instance.sunLight → ShopSaveData.sunLight

读档时:
ShopSaveData.shopIconData → 设置ShopIcon.t字段 → 恢复CD
ShopSaveData.plantCards → 销毁已使用的小卡
ShopSaveData.sunLight → SunLightPanel.SetSunLight()
```

## 常见问题

| 问题 | 原因 | 解决方案 |
|------|------|---------|
| UIManage.instance不存在 | UIManage是静态类 | 使用 `UIManage.GetView<T>()` |
| 小卡无限使用 | 回收到对象池后重复生成 | 读档时清理对象池 |
| CD不恢复 | t字段是私有的 | 使用反射访问 |
| 阳光值不恢复 | 使用了ChangeSunLight | 使用SetSunLight避免触发事件 |

## 依赖关系

```
PlantsShop
├── ShopIcon (主卡)
│   ├── PropertyCreator (配置)
│   └── CD计时 (t字段)
└── ShopSelectIcon (选卡)

ItemPanel
└── Item_PlantCard (小卡)
    ├── PropertyCreator (配置)
    └── plantOver (使用状态)

SunLightPanel
└── sunLight (当前阳光值)
```
