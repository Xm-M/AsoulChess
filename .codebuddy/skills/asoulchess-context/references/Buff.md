# Buff 模块

## 基本信息

**定位**: Buff/Debuff 效果系统，支持各种状态效果
**复杂度**: 中
**脚本数**: 5个
**核心类**: Buff, TimeBuff, MultyBuff, BuffController, Buff_Instrument

## 核心类详解

### Buff (抽象基类)

**类型**: 抽象类
**职责**: Buff 基类，定义 Buff 生命周期

```csharp
[Serializable]
public abstract class Buff
{
    public string buffName;
    public Chess target;
    
    public virtual void BuffEffect(Chess target)  // 生效时
    public virtual void BuffOver()                // 结束时
    public virtual void BuffReset(Buff resetBuff) // 重置时（叠加）
    public virtual Buff Clone()                   // 克隆
}
```

### TimeBuff (定时Buff)

**类型**: 继承 Buff
**职责**: 带持续时间的 Buff，自动计时结束

```csharp
public class TimeBuff : Buff
{
    public float continueTime;
    private Timer timer;
    
    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        timer = GameManage.instance.timerManage.AddTimer(BuffOver, continueTime, false);
    }
}
```

### MultyBuff (复合Buff)

**类型**: 继承 Buff
**职责**: 同时施加多个 Buff 效果

```csharp
public class MultyBuff : Buff
{
    public List<Buff> buffs;
    
    public override void BuffEffect(Chess target)
    {
        foreach(var buff in buffs)
            target.buffController.AddBuff(buff);
        BuffOver(); // 立即结束自身
    }
}
```

### BuffController

**类型**: Component
**职责**: 管理棋子的所有 Buff

```csharp
public class BuffController : Controller
{
    List<Buff> buffs;
    
    public void AddBuff(Buff buff)
    public void RemoveBuff(Buff buff)
    public void InitController(Chess chess)
    public void WhenControllerEnterWar()
    public void WhenControllerLeaveWar()
}
```

## 内置Buff类型

| Buff类 | 效果 |
|--------|------|
| ColdBuff | 减速 + 变蓝 |
| DizzyBuff | 眩晕 |
| BaseValueBuff | 基础数值修改 |

## 自定义Buff示例

```csharp
// 创建新Buff
public class MyBuff : TimeBuff
{
    public float damageBoost = 0.2f;
    
    public MyBuff()
    {
        buffName = "攻击力提升";
    }
    
    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        target.propertyController.ChangeExtraDamage(damageBoost);
    }
    
    public override void BuffOver()
    {
        target.propertyController.ChangeExtraDamage(-damageBoost);
        base.BuffOver();
    }
}

// 使用
var buff = new MyBuff { continueTime = 5f };
chess.buffController.AddBuff(buff);
```

## 依赖关系

**依赖的模块**:
- Chess (作用目标)
- Manage (TimerManage 计时)

**被依赖的模块**:
- Weapon (攻击附带的 Buff)
- Skill (技能附带的 Buff)
