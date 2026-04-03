# GameManage 模块

## 基本信息

**定位**: 全局游戏管理器，负责所有子系统的初始化和协调
**复杂度**: 高
**脚本数**: 8个
**核心类**: GameManage, ChessManage, ChessTeamManage, ChessFactory, UIManage, TimerManage, AudioManage, SceneManage

## 核心类详解

### GameManage

**类型**: MonoBehaviour 单例
**职责**: 游戏主管理器，管理所有子系统生命周期

```csharp
public class GameManage : MonoBehaviour
{
    public static GameManage instance;
    public SceneManage sceneManage;
    public UIManage UIManage;
    public TimerManage timerManage;
    public AudioManage audioManage;
    public ChessFactory chessFactory;
    public ChessTeamManage chessTeamManage;
    public FetterController fetterManage;
    public List<PropertyCreator> allChess;
    public List<GameObject> PlayerChess;
    public UnityEvent WhenGameOver, WhenGameStart;
}
```

**关键方法**:
- `Awake()` - 初始化所有单例和子系统
- `QuitGame()` - 退出游戏

**生命周期**:
1. Awake: 创建所有管理器实例
2. Start: 初始化各管理器
3. Update: 更新 TimerManage

### ChessTeamManage

**类型**: 普通类
**职责**: 管理玩家和敌人的棋子队伍

```csharp
public class ChessTeamManage
{
    public static ChessTeamManage Instance;
    public ChessManage player;
    public EnemyManage enemy;
    
    public List<Chess> GetTeam(string tag)
    public List<Chess> GetEnemyTeam(string tag)
    public void RecycleChess(Chess chess)
    public void ChangeTeam(Chess chess)
}
```

### ChessManage / EnemyManage

**类型**: 可序列化类
**职责**: 管理一方棋子的创建和回收

```csharp
[Serializable]
public class ChessManage : IManager
{
    public string playerTag;
    public List<Chess> chesses;
    
    public virtual Chess CreateChess(PropertyCreator creator, Tile tile)
    public virtual void RecycleChess(Chess chess)
}
```

## 依赖关系

**依赖的模块**:
- Event (事件通知)
- Chess (棋子创建)
- UI (界面管理)
- LevelSystem (关卡切换)

**被依赖的模块**:
- 几乎所有模块都依赖 GameManage

## 使用示例

```csharp
// 获取管理器
var sceneManage = GameManage.instance.sceneManage;
var chessFactory = GameManage.instance.chessFactory;

// 创建棋子
Chess chess = GameManage.instance.chessFactory.ChessCreate(prefab, name);

// 切换队伍
ChessTeamManage.Instance.ChangeTeam(chess);
```

## 注意事项

1. **DontDestroyOnLoad**: GameManage 在场景切换时不销毁
2. **初始化顺序**: Awake → Start，注意依赖顺序
3. **单例风险**: 测试时可能需要 Mock
