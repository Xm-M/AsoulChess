# UI 模块

## 基本信息

**定位**: 用户界面管理，基于 Pixel UI 框架
**复杂度**: 中
**脚本数**: 4个
**核心类**: UIManage, UIRoot, View (PixelUI)

## 核心类详解

### UIManage

**类型**: 普通类
**职责**: 管理所有 UI 视图的显示和隐藏

```csharp
public class UIManage 
{
    public static Dictionary<string, View> viewsDic;
    
    public UIManage()
    
    // 获取视图
    public static T GetView<T>() where T : View
    
    // 显示视图
    public static void Show<T>() where T : View
    public static void Show(string name)
    
    // 隐藏视图
    public static void Close<T>() where T : View
    public static void Close(string name)
}
```

### UIRoot

**类型**: MonoBehaviour
**职责**: UI 根节点，初始化所有视图

```csharp
public class UIRoot : MonoBehaviour
{
    public static Dictionary<string, View> m_views;
    
    void Awake()
    {
        // 收集所有子 View 组件
        // 存入 m_views 字典
    }
}
```

## UI 系统架构

基于 Pixel UI 框架的视图系统：

```
UIRoot (Scene 中)
├── StartUI (View)
├── GameOverUI (View)
├── DamagePanel (View)
├── TestScenePanel (View)
└── ...
```

## 使用示例

```csharp
// 显示开始界面
UIManage.Show<StartUI>();

// 隐藏开始界面
UIManage.Close<StartUI>();

// 获取视图实例
var damagePanel = UIManage.GetView<DamagePanel>();
damagePanel.ShowDamageMes(damageMessage);

// 通过名称显示
UIManage.Show("StartUI");
```

## 依赖关系

**依赖的模块**:
- Pixel UI (第三方框架)
- Chess (显示棋子信息)

**被依赖的模块**:
- GameManage (初始化)
- LevelSystem (游戏开始/结束界面)
