# 架构决策记录

## 设计模式应用

| 模式 | 应用场景 | 实现文件 | 评估 |
|------|---------|---------|------|
| 单例模式 | GameManage, LevelManage, EventController, ObjectPool | 各管理器类 | 合适，但注意测试难度 |
| 组件模式 | Chess 的多个 Controller | Chess.cs | 合适，高扩展性 |
| 观察者模式 | EventController 事件系统 | EventController.cs | 合适，解耦模块 |
| 对象池模式 | 棋子、特效、子弹复用 | ObjectPool.cs | 合适，性能优化 |
| 状态机模式 | 棋子状态管理 | StateController.cs | 合适 |
| 工厂模式 | 棋子创建 | ChessFactory.cs | 合适 |

## 架构风格

**当前风格**: 组件化 + 单例管理器 + 事件驱动

**架构特点**:
1. **中心化**: GameManage 作为中央管理器
2. **组件化**: Chess 通过多个 Controller 组合功能
3. **事件驱动**: EventController 解耦模块通信
4. **数据驱动**: ScriptableObject 配置数据

**优点**:
- 高内聚低耦合（通过事件）
- 易于扩展（新增 Controller）
- 配置灵活（SO 数据配置）

**局限性**:
- 单例过多，测试困难
- 事件链难以追踪
- 强依赖 GameManage

## 关键技术选型

| 技术领域 | 选型方案 | 说明 |
|---------|---------|------|
| 输入系统 | Unity 旧版 Input | Input.GetKeyUp |
| 渲染管线 | Built-in | 2D 游戏 |
| 物理系统 | 2D Physics | Rigidbody2D, Collider2D |
| UI系统 | uGUI + Pixel UI | 第三方 Pixel UI 框架 |
| 数据持久化 | 待确定 | 当前未看到存档系统 |
| 动画系统 | Animator | 状态机驱动 |
| 编辑器扩展 | Odin Inspector | Sirenix.OdinInspector |

## 编码规范

**命名约定**:
- 类名: PascalCase (GameManage, ChessManage)
- 方法名: PascalCase (InitManage, CreateChess)
- 私有字段: camelCase
- 公共字段: PascalCase
- 事件: PascalCase (WhenGameOver)

**代码组织**:
- 使用 Region 分隔功能区域
- Controller 类统一继承 Controller 基类
- 使用 SerializeReference 支持多态序列化

## 性能考虑

1. **对象池**: ObjectPool 减少 Instantiate/Destroy
2. **事件清理**: Death 时清理 UnityEvent
3. **Timer 复用**: TimerManage 管理计时器
4. **对象复用**: Chess 死亡时回收到对象池

## 扩展建议

1. **减少单例**: 考虑使用依赖注入
2. **存档系统**: 需要补充数据持久化
3. **资源管理**: 考虑 Addressables 替代 Resources
4. **输入系统**: 升级至 Input System 包
