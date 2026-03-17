# Coroutine System 基础概念

## 什么是协程？

协程（Coroutine）是Unity中一种轻量级的异步编程机制，允许在多帧内分步执行代码，而不阻塞主线程。协程本质上是迭代器（Iterator），通过yield语句暂停执行并在下一帧或指定条件后继续。

---

## 协程核心概念

### 1. 迭代器模式

协程基于C#的IEnumerator接口实现：

```csharp
public interface IEnumerator
{
    object Current { get; }  // 当前yield值
    bool MoveNext();          // 移动到下一个yield
    void Reset();             // 重置迭代器
}
```

### 2. yield语句

yield语句控制协程的暂停点：

| yield语句 | 执行时机 | 用途 |
|-----------|---------|------|
| `yield return null` | 下一帧 | 等待一帧 |
| `yield return new WaitForEndOfFrame()` | 帧渲染后 | 等待渲染完成 |
| `yield return new WaitForFixedUpdate()` | FixedUpdate后 | 等待物理帧 |
| `yield return new WaitForSeconds(t)` | 指定秒后 | 等待时间 |
| `yield return new WaitForSecondsRealtime(t)` | 真实时间秒后 | 不受时间缩放影响 |
| `yield return StartCoroutine()` | 子协程完成后 | 协程嵌套 |
| `yield return new WaitUntil(Func)` | 条件为true时 | 等待条件 |
| `yield return new WaitWhile(Func)` | 条件为false时 | 等待条件 |
| `yield break` | 立即结束 | 终止协程 |

### 3. 协程生命周期

```
协程执行流程：
1. StartCoroutine() 调用
2. 执行到第一个yield
3. 暂停，返回控制权
4. 等待yield条件满足
5. 继续执行到下一个yield
6. 重复3-5直到协程结束
```

---

## 协程 vs 其他异步方案

### 协程 vs Update

| 特性 | 协程 | Update |
|------|------|--------|
| 代码结构 | 线性、清晰 | 分散、复杂 |
| 计时管理 | 自动 | 需手动累加 |
| 状态管理 | 自动保存 | 需手动维护 |
| 可读性 | 高 | 低 |
| 性能 | 较优 | 每帧调用 |

**示例对比**：

```csharp
// Update实现延迟执行
private float timer = 0f;
private bool isWaiting = false;

void Update()
{
    if (isWaiting)
    {
        timer += Time.deltaTime;
        if (timer >= 2f)
        {
            Debug.Log("2秒后执行");
            isWaiting = false;
            timer = 0f;
        }
    }
}

// 协程实现延迟执行
IEnumerator DelayedAction()
{
    yield return new WaitForSeconds(2f);
    Debug.Log("2秒后执行");
}
```

### 协程 vs async/await

| 特性 | 协程 | async/await |
|------|------|-------------|
| Unity版本 | 所有版本 | Unity 2017+ |
| 主线程执行 | 是 | 需配置 |
| 异常处理 | 需try-catch | 原生支持 |
| 返回值 | 无（可out） | Task<T> |
| 取消机制 | StopCoroutine | CancellationToken |

**何时选择协程**：
- 需要访问Unity API（必须在主线程）
- 简单的时序控制
- 兼容旧版本Unity

**何时选择async/await**：
- 复杂异步逻辑
- 需要返回值
- 需要取消机制

---

## 协程执行时机

### Unity执行顺序

```
一帧内的执行顺序：
1. Initialization
   - Awake()
   - OnEnable()
   - Start()
   └── 协程在此开始执行

2. Physics
   - FixedUpdate()
   - 协程 yield return new WaitForFixedUpdate()

3. Input events
   - 各种输入事件

4. Game logic
   - Update()
   - 协程 yield return null 在此之后执行

5. Rendering preparation
   - 协程 yield return new WaitForEndOfFrame()

6. Rendering
   - 渲染场景

7. Gizmos/GUI
   - OnDrawGizmos()
   - OnGUI()
```

### yield时机详解

```
yield return null:
  → 等待Update之后，下一帧继续

yield return WaitForEndOfFrame:
  → 等待所有渲染完成后继续
  → 可用于截图、读取渲染纹理

yield return WaitForFixedUpdate:
  → 等待下一个FixedUpdate之后继续
  → 用于物理相关操作

yield return WaitForSeconds(2f):
  → 等待游戏时间2秒后继续
  → 受Time.timeScale影响

yield return WaitForSecondsRealtime(2f):
  → 等待真实时间2秒后继续
  → 不受Time.timeScale影响
```

---

## 协程性能特性

### 内存开销

```
协程对象开销：
- IEnumerator对象：约40-60字节
- 装箱值类型：yield值会被装箱
- 闭包：捕获变量增加GC压力

优化建议：
- 避免频繁创建WaitForSeconds
- 缓存WaitForSeconds对象
- 使用对象池管理协程
```

### GC压力

```csharp
// ❌ 每次调用都会产生GC
IEnumerator BadCoroutine()
{
    while (true)
    {
        yield return new WaitForSeconds(1f); // 每次装箱
    }
}

// ✅ 缓存WaitForSeconds减少GC
private WaitForSeconds waitOneSecond = new WaitForSeconds(1f);

IEnumerator GoodCoroutine()
{
    while (true)
    {
        yield return waitOneSecond; // 无GC
    }
}
```

### 协程数量建议

```
单场景协程数量建议：
- 活跃协程：< 100个
- 如需更多：考虑对象池或Update

协程替代方案：
- 计时器：使用Timer类
- 状态机：使用FSM
- 大量协程：使用Job System
```

---

## 协程常见模式

### 1. 延迟执行

```csharp
IEnumerator DelayedExecute(float delay, System.Action action)
{
    yield return new WaitForSeconds(delay);
    action?.Invoke();
}

// 调用
StartCoroutine(DelayedExecute(2f, () => {
    Debug.Log("2秒后执行");
}));
```

### 2. 分帧执行

```csharp
IEnumerator ProcessInFrames(int itemsPerFrame, System.Action<int> processItem, int totalItems)
{
    for (int i = 0; i < totalItems; i++)
    {
        processItem(i);
        
        if (i % itemsPerFrame == 0)
        {
            yield return null; // 每处理N个物品暂停一帧
        }
    }
}
```

### 3. 超时检测

```csharp
 IEnumerator WithTimeout(float timeout, IEnumerator coroutine)
{
    float startTime = Time.time;
    
    while (coroutine.MoveNext())
    {
        if (Time.time - startTime > timeout)
        {
            Debug.LogWarning("协程超时");
            yield break;
        }
        yield return coroutine.Current;
    }
}
```

### 4. 协程队列

```csharp
public class CoroutineQueue
{
    private Queue<IEnumerator> queue = new Queue<IEnumerator>();
    private bool isRunning = false;
    private MonoBehaviour owner;

    public void Enqueue(IEnumerator coroutine)
    {
        queue.Enqueue(coroutine);
        if (!isRunning)
        {
            owner.StartCoroutine(ProcessQueue());
        }
    }

    IEnumerator ProcessQueue()
    {
        isRunning = true;
        while (queue.Count > 0)
        {
            yield return owner.StartCoroutine(queue.Dequeue());
        }
        isRunning = false;
    }
}
```

---

## 协程生命周期管理

### 协程与GameObject

```
GameObject激活状态影响：
- GameObject.SetActive(false)：协程继续运行
- MonoBehaviour.enabled = false：协程继续运行
- GameObject销毁：协程自动停止

结论：协程与MonoBehaviour绑定，不与激活状态绑定
```

### 协程与场景加载

```
场景加载对协程的影响：
- LoadSceneMode.Single：协程停止（对象被销毁）
- LoadSceneMode.Additive：协程继续运行
- DontDestroyOnLoad：协程跨场景运行
```

### 协程停止方式

```csharp
// 方式1：StopCoroutine
private Coroutine myCoroutine;

void Start()
{
    myCoroutine = StartCoroutine(MyRoutine());
}

void StopMyCoroutine()
{
    if (myCoroutine != null)
    {
        StopCoroutine(myCoroutine);
    }
}

// 方式2：StopAllCoroutines
void StopAll()
{
    StopAllCoroutines(); // 停止该MonoBehaviour所有协程
}

// 方式3：yield break
IEnumerator ConditionalRoutine()
{
    if (shouldStop)
    {
        yield break; // 立即结束
    }
    yield return null;
}
```

---

## 常见问题

### Q: 协程不停止？

```
原因：
1. StopCoroutine参数错误
2. 协程在不同的MonoBehaviour上
3. 使用字符串启动协程

解决：
✅ 使用Coroutine引用停止
✅ 确保停止正确的协程
❌ 避免使用字符串启动
```

### Q: WaitForSeconds时间不准？

```
原因：
- yield检查在帧更新时进行
- 帧率不稳定导致时间误差
- Time.timeScale影响

解决：
- 使用WaitForSecondsRealtime（不受时间缩放影响）
- 增加时间容差判断
- 对于精确计时使用Time.time
```

### Q: 协程内存泄漏？

```
原因：
- 协程持有对象引用
- 无限循环协程未停止
- 闭包捕获大型对象

解决：
- 在OnDisable/OnDestroy中停止协程
- 避免无限协程
- 使用弱引用或传递必要参数
```

---

## 扩展学习

### 官方文档
- [Unity Coroutines](https://docs.unity3d.com/Manual/Coroutines.html)
- [MonoBehaviour](https://docs.unity3d.com/ScriptReference/MonoBehaviour.html)

### 进阶主题
- CustomYieldInstruction自定义等待
- 协程与async/await混合使用
- 协程性能优化技巧
- 协程框架设计
