# LevelSystem 模块

## 基本信息

**定位**: 关卡管理系统，控制游戏流程和关卡切换
**复杂度**: 高
**脚本数**: 6个
**核心类**: LevelManage, LevelController, LevelData, ModeManage

## 核心类详解

### LevelManage

**类型**: MonoBehaviour 单例
**职责**: 全局关卡管理器，控制关卡流程

```csharp
public class LevelManage : MonoBehaviour
{
    public static LevelManage instance;
    public LevelController currentController;
    public LevelData currentLevel;
    public bool IfGameStart { get; set; }
    
    public void ChangeLevel(LevelData levelData)  // 切换关卡
    public void RestartLevel()                    // 重新开始
    public void ReturnMenu()                      // 返回主菜单
    public void GameStart()                       // 游戏开始
    public void GameOver(bool win)                // 游戏结束
    public void GamePause()                       // 暂停
    public void GameContinue()                    // 继续
}
```

**关键状态**:
- `IfGameStart` - 游戏是否进行中
- `currentController` - 当前关卡控制器
- `currentLevel` - 当前关卡数据

**存档相关**:
- 存档时会保存 `IfGameStart`、`currentController` 的状态字段
- 存档字段包括: currentWave, t(计时器), mintime, maxtime
- 读档时会恢复这些字段，确保关卡继续从正确状态运行

### LevelController

**类型**: MonoBehaviour
**职责**: 单个关卡的逻辑控制（抽象基类）

```csharp
public class LevelController : MonoBehaviour
{
    public LevelData levelData;
    public virtual void GameOver(bool win) { }
    public virtual void OverPlugin() { }
}
```

**扩展方式**: 继承 LevelController 实现具体关卡逻辑

### LevelData

**类型**: ScriptableObject
**职责**: 关卡配置数据

```csharp
[CreateAssetMenu(fileName = "LevelData", menuName = "Level/LevelData")]
public class LevelData : ScriptableObject
{
    public string sceneName;
    public string levelName;
    // 其他关卡配置...
}
```

## 关卡流程

```
1. ChangeLevel(levelData)
   ├── LeaveState() - 清理当前关卡
   ├── LoadScene(sceneName) - 加载场景
   └── 新关卡初始化

2. PrepareLevel()
   └── 触发 SelectState 事件

3. GameStart()
   ├── IfGameStart = true
   └── 触发 GameStart 事件

4. GameOver(win)
   ├── IfGameStart = false
   ├── currentController.GameOver(win)
   └── 触发 GameOver 事件
```

## 依赖关系

**依赖的模块**:
- Manage (场景管理、棋子管理)
- Event (事件通知)

**被依赖的模块**:
- GameManage (关卡切换回调)
- UI (游戏开始/结束界面)

## 使用示例

```csharp
// 切换关卡
LevelManage.instance.ChangeLevel(nextLevelData);

// 开始游戏
LevelManage.instance.GameStart();

// 游戏结束
LevelManage.instance.GameOver(true); // true = 胜利

// 暂停/继续
LevelManage.instance.GamePause();
LevelManage.instance.GameContinue();
```

## 注意事项

1. **单例模式**: LevelManage.instance 全局访问
2. **事件驱动**: 关卡状态变化通过 EventController 通知其他模块
3. **数据驱动**: 关卡配置使用 ScriptableObject
4. **场景管理**: 关卡切换时自动清理当前关卡状态
