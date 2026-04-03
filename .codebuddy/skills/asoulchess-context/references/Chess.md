# Chess 模块

## 基本信息

**定位**: 游戏核心单位，采用组件化设计
**复杂度**: 高
**脚本数**: 12个
**核心类**: Chess, PropertyController, SkillController, StateController, BuffController, MoveController, AttackController, AnimatorController

## 核心类详解

### Chess

**类型**: MonoBehaviour
**职责**: 棋子主类，协调所有控制器

```csharp
public class Chess : MonoBehaviour
{
    public AttackController equipWeapon;      // 攻击管理
    public PropertyController propertyController;  // 属性管理
    public SkillController skillController;   // 技能管理
    public StateController stateController;   // 状态管理
    public BuffController buffController;     // Buff管理
    public MoveController moveController;     // 移动控制
    public AnimatorController animatorController;  // 动画控制
    
    public UnityEvent<Chess> WhenEnterGame;   // 进入游戏事件
    public UnityEvent<Chess> DeathEvent;      // 死亡事件
    public bool IfDeath { get; set; }
}
```

**生命周期方法**:
- `InitChess()` - 初始化所有 Controller（仅生成时调用）
- `WhenChessEnterWar()` - 进入战斗时调用
- `Death()` - 死亡处理
- `Update()` - 状态更新

### PropertyController

**类型**: 可序列化类
**职责**: 管理棋子属性，处理伤害计算

**核心属性** (Property 类):
- `attack` - 基础攻击
- `Hp/HpMax` - 生命值
- `crit/critDamage` - 暴击率/暴击伤害
- `dodgeRate` - 闪避率
- `AR` - 护甲
- `speed` - 移动速度
- `acceleRated` - 攻速/移速加成
- `attackRange` - 攻击距离

**伤害计算方法**:
```csharp
public void GetDamage(DamageMessege mes)  // 受到伤害
public void TakeDamage(DamageMessege mes) // 造成伤害
public void Heal(float heal)              // 治疗
```

### 其他 Controllers

| Controller | 职责 |
|-----------|------|
| SkillController | 技能释放和冷却管理 |
| StateController | 状态机管理（Idle, Attack, Dizzy等） |
| BuffController | Buff 添加/移除/更新 |
| MoveController | 移动逻辑和路径查找 |
| AttackController | 攻击逻辑和伤害触发 |
| AnimatorController | 动画控制和特效触发 |

## 扩展方式

### 1. 创建新棋子类型
```csharp
public class MyChess : Chess
{
    public override void Death()
    {
        // 自定义死亡逻辑
        base.Death();
    }
}
```

### 2. 自定义属性计算
```csharp
public class MyPropertyController : PropertyController
{
    public override void GetDamage(DamageMessege mes)
    {
        // 自定义伤害计算
        base.GetDamage(mes);
    }
}
```

## 依赖关系

**依赖的模块**:
- Event (事件触发)
- Buff (Buff效果)
- UI (伤害数字显示)
- Manage (工厂创建)

**被依赖的模块**:
- LevelSystem (棋子生成)
- Fetter (羁绊效果)
- Weapon (武器攻击)

## 使用示例

```csharp
// 初始化棋子
chess.InitChess();

// 进入战斗
chess.WhenChessEnterWar();

// 创建伤害信息
var damage = new DamageMessege(attacker, target, 100, DamageType.Physical);
target.propertyController.GetDamage(damage);

// 添加Buff
var buff = new ColdBuff { continueTime = 3f };
chess.buffController.AddBuff(buff);
```

## 注意事项

1. **Controller 初始化**: 必须在 InitChess() 中初始化
2. **事件清理**: Death() 时会自动清理 DeathEvent 和 OnRemove
3. **对象池**: 死亡时通过 ChessTeamManage 回收，不是直接销毁
