---
name: unity-singleton-manager
description: 用于快速生成Unity游戏全局管理器，如GameManager、AudioManager、LevelManager、SaveManager等，支持泛型单例基类和持久化功能。
---

# Unity单例管理器生成器

## 目的

快速生成Unity游戏全局管理器，提供线程安全的单例实现和预设功能模板，提高游戏架构开发效率。

## 使用场景

适用于以下情况：
- 创建游戏全局管理器（GameManager、AudioManager等）
- 需要单例模式保证唯一实例
- 需要跨场景持久化的管理器
- 需要统一的管理器架构

## 工作流程

### 步骤1：确认管理器类型

**操作**：询问用户管理器类型：
- **game** - 游戏管理器（游戏状态、暂停/恢复、场景切换）
- **level** - 关卡管理器（关卡加载、进度保存、解锁逻辑）
- **audio** - 音频管理器（BGM/SFX播放、音量控制）
- **save** - 存档管理器（数据序列化、存档槽位、自动存档）
- **pool** - 对象池管理器（对象创建/回收、容量管理）
- **custom** - 自定义管理器

**确认点**：等待用户选择管理器类型。

### 步骤2：确认持久化需求

**操作**：询问是否需要存档/读档功能：
```
是否需要持久化功能？
- 数据序列化（JSON）
- 存档/读档方法
- 自动存档
```

**确认点**：等待用户选择。

### 步骤3：确认自定义事件

**操作**：让用户提供自定义事件列表（如需要）：
```
[
  {
    "name": "OnCustomEvent",
    "type": "Action/Action<T>/Action<T1, T2>",
    "tooltip": "事件说明"
  }
]
```

**确认点**：等待用户提供事件列表（可选）。

### 步骤4：生成管理器类

**操作**：生成完整的管理器类，包含：
- ✅ 泛型单例基类（首次使用生成）
- ✅ 预设功能方法
- ✅ 自定义事件定义
- ✅ 持久化逻辑（如启用）
- ✅ 线程安全实现

## 预设功能说明

### game（游戏管理器）
- 游戏状态管理（菜单/游戏中/暂停/结束）
- 游戏暂停/恢复
- 时间缩放控制
- 场景切换管理
- 事件：OnGameStart, OnGamePause, OnGameResume, OnGameOver

### level（关卡管理器）
- 关卡加载/卸载
- 关卡进度保存
- 关卡解锁逻辑
- 关卡数据管理
- 事件：OnLevelStart, OnLevelComplete, OnLevelFailed, OnLevelLoaded

### audio（音频管理器）
- BGM播放控制
- SFX播放控制
- 音量管理
- 音频分组
- 事件：OnBGMChanged, OnVolumeChanged, OnSFXPlayed

### save（存档管理器）
- 数据序列化
- 存档槽位管理
- 自动存档
- 数据加密
- 事件：OnSaveComplete, OnLoadComplete, OnSaveFailed

### pool（对象池管理器）
- 对象池创建
- 对象获取/回收
- 池容量管理
- 自动扩容
- 事件：OnPoolCreated, OnObjectSpawned, OnObjectRecycled

## 代码规范

生成的代码遵循：
- 使用`Lazy<T>`实现线程安全延迟初始化
- `protected`构造函数防止外部实例化
- 实现`IDisposable`接口清理资源
- 使用`JsonUtility`进行序列化
- 使用`#region`划分代码区域
- 完整XML注释

## 验收标准

- [ ] 单例唯一性保证
- [ ] 线程安全
- [ ] DontDestroyOnLoad正常（如需要）
- [ ] 预设功能完整
- [ ] 事件触发正确
- [ ] 持久化功能正常（如启用）
- [ ] 无内存泄漏
- [ ] 包含完整注释

## 使用示例

### 示例1：游戏管理器

```
用户：我需要生成GameManager

AI：好的！请选择管理器类型：
1. game - 游戏管理器
2. level - 关卡管理器
3. audio - 音频管理器
4. save - 存档管理器
5. pool - 对象池管理器
6. custom - 自定义管理器

[用户选择：game]

AI：收到！游戏管理器。

是否需要持久化功能？

[用户选择：是]

AI：是否需要自定义事件？如需要请提供事件列表。

[用户确认：需要OnScoreChanged和OnLifeChanged事件]

AI：正在生成GameManager.cs...
[生成完整代码]
```

### 示例2：音频管理器

```
用户：我需要音频管理器

AI：请选择管理器类型...

[用户选择：audio]

AI：是否需要持久化功能？

[用户选择：是，保存音量设置]

AI：正在生成AudioManager.cs...
[生成完整代码]
```

## 注意事项

- 单例模式适合全局唯一的管理器
- 不要滥用单例，仅在真正需要全局访问时使用
- 单例对象会在场景切换时保留（DontDestroyOnLoad）
- 持久化数据保存在Application.persistentDataPath
