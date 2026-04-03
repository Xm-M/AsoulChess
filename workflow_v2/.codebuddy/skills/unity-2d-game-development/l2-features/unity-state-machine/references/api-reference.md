# State Machine API参考

## Animator 类

### 参数设置方法

```csharp
// 设置Float参数
public void SetFloat(string name, float value);
public void SetFloat(int id, float value);  // 使用Hash

// 设置Integer参数
public void SetInteger(string name, int value);
public void SetInteger(int id, int value);

// 设置Bool参数
public void SetBool(string name, bool value);
public void SetBool(int id, bool value);

// 触发Trigger
public void SetTrigger(string name);
public void SetTrigger(int id);
public void ResetTrigger(string name);
public void ResetTrigger(int id);
```

### 状态查询方法

```csharp
// 获取当前状态信息
public AnimatorStateInfo GetCurrentAnimatorStateInfo(int layerIndex);

// 获取下一状态信息
public AnimatorStateInfo GetNextAnimatorStateInfo(int layerIndex);

// 检查是否在指定状态
bool isInState = animator.GetCurrentAnimatorStateInfo(0).IsName("Attack");

// 获取参数值
float speed = animator.GetFloat("Speed");
bool isGrounded = animator.GetBool("IsGrounded");
```

---

## AnimatorStateInfo 结构体

### 属性

| 属性 | 类型 | 说明 |
|------|------|------|
| `length` | float | 状态时长（秒） |
| `normalizedTime` | float | 归一化时间（0-1） |
| `speed` | float | 播放速度 |
| `speedMultiplier` | float | 速度乘数 |
| `loop` | bool | 是否循环 |

---

## StateMachineBehaviour 类

### 回调方法

```csharp
public class CustomStateBehaviour : StateMachineBehaviour
{
    // 进入状态
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex);
    
    // 每帧更新
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex);
    
    // 退出状态
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex);
    
    // 进入子状态机
    public override void OnStateMachineEnter(Animator animator, int stateMachinePathHash);
    
    // 退出子状态机
    public override void OnStateMachineExit(Animator animator, int stateMachinePathHash);
}
```

---

## 性能优化

### 参数Hash缓存

```csharp
// 缓存参数Hash
private static readonly int SpeedHash = Animator.StringToHash("Speed");
private static readonly int JumpHash = Animator.StringToHash("Jump");

// 使用Hash设置参数
animator.SetFloat(SpeedHash, speed);
animator.SetTrigger(JumpHash);
```
