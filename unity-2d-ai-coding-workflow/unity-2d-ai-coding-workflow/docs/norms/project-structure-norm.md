# 项目目录结构规范

## 规范目的

建立统一的Unity项目目录结构，确保项目组织清晰、资源易于管理、团队协作顺畅。

## 强制目录结构

### 完整结构

```
ProjectName/
├── Assets/
│   ├── _Project/              # ⚠️ 所有项目资源放在此目录
│   │   ├── Scripts/           # 代码文件
│   │   │   ├── Core/          # 核心系统
│   │   │   │   ├── Managers/  # 管理器类
│   │   │   │   ├── Events/    # 事件系统
│   │   │   │   ├── Pool/      # 对象池
│   │   │   │   └── Utilities/ # 工具类
│   │   │   ├── Game/          # 游戏逻辑
│   │   │   │   ├── Character/ # 角色系统
│   │   │   │   │   ├── Player/
│   │   │   │   │   └── Enemy/
│   │   │   │   ├── UI/        # UI系统
│   │   │   │   │   ├── Panels/
│   │   │   │   │   └── Components/
│   │   │   │   ├── Item/      # 物品系统
│   │   │   │   ├── AI/        # AI系统
│   │   │   │   └── Level/     # 关卡系统
│   │   │   └── Editor/        # 编辑器扩展
│   │   ├── Art/               # 美术资源
│   │   │   ├── Sprites/       # Sprite资源
│   │   │   │   ├── Character/
│   │   │   │   ├── UI/
│   │   │   │   └── Environment/
│   │   │   ├── Animations/    # 动画文件
│   │   │   │   ├── Character/
│   │   │   │   └── UI/
│   │   │   ├── Materials/     # 材质
│   │   │   └── UI/            # UI美术
│   │   ├── Prefabs/           # 预制件
│   │   │   ├── Character/
│   │   │   ├── UI/
│   │   │   └── Environment/
│   │   ├── Scenes/            # 场景文件
│   │   │   ├── Levels/
│   │   │   ├── MainMenu.unity
│   │   │   └── Bootstrap.unity
│   │   ├── ScriptableObjects/ # 数据文件
│   │   │   ├── Character/
│   │   │   ├── Item/
│   │   │   └── Level/
│   │   └── Resources/         # 动态加载（慎用）
│   │       └── Prefabs/
│   ├── Plugins/               # 第三方插件
│   │   └── [PluginName]/
│   └── StreamingAssets/       # 流媒体资源
├── Packages/                   # Package Manager包
├── ProjectSettings/            # 项目设置
└── docs/                       # 项目文档
    ├── architecture/           # 架构文档
    │   ├── decisions/         # 架构决策记录
    │   └── analysis/          # 影响面分析报告
    ├── prompts/               # Prompt模板
    │   ├── basic/
    │   ├── intermediate/
    │   └── advanced/
    ├── skills/                # Skills手册
    ├── best-practices/        # 最佳实践
    └── case-studies/          # 实战案例
```

## 命名规范

### 文件命名

**脚本文件**：
- 格式：PascalCase
- 示例：`PlayerController.cs`, `GameManager.cs`, `ItemData.cs`

**场景文件**：
- 格式：PascalCase
- 示例：`MainMenu.unity`, `Level_01.unity`, `Tutorial.unity`

**Prefab文件**：
- 格式：PascalCase
- 示例：`Player.prefab`, `Enemy_Goblin.prefab`, `UI_Panel_Inventory.prefab`

**Sprite文件**：
- 格式：描述性命名，下划线分隔
- 示例：`Player_Idle_01.png`, `UI_Button_Play.png`, `Env_Tile_Grass_01.png`

**ScriptableObject文件**：
- 格式：PascalCase
- 示例：`PlayerData.asset`, `Weapon_Sword.asset`

### 代码命名

**类名**：
- 格式：PascalCase
- 示例：`PlayerController`, `GameManager`, `ItemData`

**方法名**：
- 格式：PascalCase
- 示例：`GetPlayerPosition()`, `TakeDamage()`, `UpdateScore()`

**公共变量**：
- 格式：PascalCase
- 示例：`public int MaxHealth;`, `public float MoveSpeed;`

**私有变量**：
- 格式：_camelCase
- 示例：`private int _currentHealth;`, `private float _moveSpeed;`

**常量**：
- 格式：UPPER_CASE
- 示例：`public const int MAX_PLAYERS = 4;`, `private const float MOVE_SPEED = 5f;`

**参数**：
- 格式：camelCase
- 示例：`void TakeDamage(int damageAmount)`, `void Move(Vector2 direction)`

### 枚举命名

**格式**：PascalCase

**示例**：
```csharp
public enum PlayerState
{
    Idle,
    Walk,
    Run,
    Jump,
    Attack
}

public enum ItemType
{
    Consumable,
    Equipment,
    Material,
    Quest
}
```

## Assembly Definition要求

### 必须创建Asmdef

**Core.asmdef**:
- 路径：`Assets/_Project/Scripts/Core/`
- 包含：管理器、事件系统、对象池、工具类
- 依赖：无项目代码依赖

**Game.asmdef**:
- 路径：`Assets/_Project/Scripts/Game/`
- 包含：游戏逻辑代码
- 依赖：Core

**Editor.asmdef**:
- 路径：`Assets/_Project/Scripts/Editor/`
- 包含：编辑器扩展代码
- 依赖：Core, Game

### 依赖关系

```
Editor → Game → Core
```

**规则**：
- Core不依赖任何项目代码
- Game依赖Core
- Editor依赖Core和Game

## 目录使用指南

### Scripts目录

**按功能模块划分**：
- `Core/`：核心系统，不依赖游戏逻辑
- `Game/`：游戏逻辑，按系统划分
- `Editor/`：编辑器扩展工具

**类文件组织**：
- 每个类一个文件
- 文件名与类名一致
- 相关类放在同一子目录

### Art目录

**按资源类型划分**：
- `Sprites/`：2D图片资源
- `Animations/`：动画控制器和动画片段
- `Materials/`：材质球
- `UI/`：UI专用美术资源

**资源组织**：
- 按功能模块分类
- 同类资源集中管理
- 便于打包和优化

### Prefabs目录

**按用途划分**：
- `Character/`：角色预制件
- `UI/`：UI预制件
- `Environment/`：环境预制件

**命名规范**：
- 使用描述性名称
- 包含类型前缀
- 便于搜索和管理

### Scenes目录

**按用途划分**：
- `Levels/`：游戏关卡
- 根目录：系统场景（主菜单、启动场景等）

**场景命名**：
- `Bootstrap.unity`：启动场景
- `MainMenu.unity`：主菜单
- `Level_XX.unity`：关卡场景

## 禁止事项

### ❌ 禁止的操作

**目录结构**：
- ❌ 在Assets根目录直接创建文件
- ❌ 在_Plugin目录外创建插件文件
- ❌ 随意创建新目录，不遵循结构规范

**命名**：
- ❌ 使用中文命名
- ❌ 文件名包含空格
- ❌ 使用特殊字符（除下划线和连字符）
- ❌ 脚本与Prefab同名冲突

**资源管理**：
- ❌ 过度使用Resources目录
- ❌ 资源散乱放置
- ❌ 重复的资源文件

## 最佳实践

### 资源导入设置

**Sprite导入**：
- Texture Type: Sprite (2D and UI)
- Pixels Per Unit: 根据项目设定（通常100）
- Filter Mode: Point（像素风）/ Bilinear（平滑）
- Compression: 根据平台选择

**音频导入**：
- Load Type: Decompress on Load（小文件）/ Streaming（大文件）
- Compression Format: Vorbis（背景音乐）/ ADPCM（音效）
- Quality: 根据需要调整

### 版本控制

**忽略文件**：
- Library目录
- Temp目录
- Obj目录
- Build目录

**跟踪文件**：
- Assets目录
- Packages目录
- ProjectSettings目录
- docs目录

## 相关规范

- 架构设计与影响面分析规范
- 代码审查规范
- 版本控制与协作规范

## 更新历史

- 2026-03-16: 初始版本发布
