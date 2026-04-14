# 项目概览（AVZ）

## 项目定位

Unity **2D** 类 **PVZ** 策略 / 自走棋向玩法（AsoulChess / AVZ）。核心循环围绕**格子地图**、**植物/僵尸棋子（Chess）**、**技能与 Buff**、**关卡控制器**展开。

## 统计（约 2026-03，以 `Assets/Script` 为准）

| 项目 | 数量 |
|------|------|
| C# 脚本 | **约 331** 个（`Assets/Script/**/*.cs`） |
| 场景 | 见 `Assets/Scenes/` |
| 预制体 | 见 `Assets/Prefab/` 等 |

## 目录结构（核心）

```
Assets/
├── Script/                 # 游戏逻辑（单数 Script）
│   ├── Manage/           # GameManage、ChessTeamManage、LevelManage…
│   ├── Chess/            # Chess、各 Controller、PropertyCreator、AnimatorController*
│   ├── Skill/            # ISkill、SkillBase、ISkillEffect*、被动等
│   ├── State/            # StateController、State、Transition
│   ├── Buff/
│   ├── LevelSystem/
│   ├── Map/              # Tile、MapManage、门、冰格等
│   ├── UI/
│   ├── Event/
│   ├── Weapon/           # 攻击、索敌、护甲
│   ├── bullet/、Effect/、Fetter/、SaveSystem/、Dialogue/、Snake/ …
├── Prefab/
├── Scenes/
├── SO/                   # ScriptableObject 配置
├── Resources/
└── …
```

## 核心架构（简图）

```
GameManage (singleton)
    ├── LevelManage → LevelController → 局内规则 / 刷怪 / 胜利条件
    ├── ChessTeamManage / ChessManage / EnemyManage → Chess 生命周期
    ├── UIManage、TimerManage、ObjectPool、WeatherManage …
    └── EventController（全局事件）

Chess
    ├── PropertyController / SkillController / StateController
    ├── BuffController / MoveController / AttackController
    └── AnimatorController（或子类，如 AnimatorController_Zombieking）
```

## 数据流（典型）

- **受伤**: `DamageMessege` → `PropertyController` / 护甲 → 表现与死亡判定  
- **技能**: 状态进入 `SkillState` → `AnimatorController.PlaySkill()` → 动画事件 `SkillController.UseSkill()` → `ISkill` / `ISkillEffect`  
- **关卡**: `LevelManage` 选择关卡数据 → `LevelController` 驱动阶段与棋子创建  

## AI 工作流（强制项摘要）

开发前：需求卡片 → `@architecture-design-analyzer` → `@impact-scope-analyzer` → 审批后再编码。  
开发后：`@unity-code-review`、编译与功能验证。  
详见：`unity-2d-ai-coding-workflow/.../ai-coding-workflow-guide.md` 与 `.cursor/rules/unity-2d-game-development-workflow.mdc`。

## 编辑器与第三方

- **Odin Inspector**（`Sirenix.OdinInspector`）用于 Inspector 组织与序列化增强  
- 部分 UI 使用 **Pixel** 等第三方包（以 `Packages/manifest.json` 为准）
