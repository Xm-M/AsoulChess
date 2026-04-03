# Coroutine System API参考

## MonoBehaviour 协程方法

### 启动协程

```csharp
// 推荐方式：使用IEnumerator
public Coroutine StartCoroutine(IEnumerator routine);

// 不推荐：使用方法名（性能差）
public Coroutine StartCoroutine(string methodName);
public Coroutine StartCoroutine(string methodName, object value);
```

### 停止协程

```csharp
// 推荐方式
public void StopCoroutine(IEnumerator routine);
public void StopCoroutine(Coroutine routine);

// 不推荐：停止所有同名协程
public void StopCoroutine(string methodName);

// 停止所有协程
public void StopAllCoroutines();
```

---

## YieldInstruction 派生类

### WaitForSeconds

```csharp
// 创建（每次创建产生GC，建议缓存）
WaitForSeconds wait = new WaitForSeconds(1f);

// 缓存示例
private WaitForSeconds waitOneSecond = new WaitForSeconds(1f);

IEnumerator MyCoroutine()
{
    yield return waitOneSecond;  // 无GC
}
```

### WaitForSecondsRealtime

```csharp
// 不受Time.timeScale影响
WaitForSecondsRealtime wait = new WaitForSecondsRealtime(1f);
```

### WaitForFixedUpdate

```csharp
// 等待下一次FixedUpdate
yield return new WaitForFixedUpdate();
```

### WaitForEndOfFrame

```csharp
// 等待帧渲染结束
yield return new WaitForEndOfFrame();
```

### WaitUntil / WaitWhile

```csharp
// 等待条件为true
yield return new WaitUntil(() => playerHealth <= 0);

// 等待条件为false
yield return new WaitWhile(() => IsGamePaused);
```

---

## CustomYieldInstruction

### 自定义等待条件

```csharp
public class WaitForMouseDown : CustomYieldInstruction
{
    public override bool keepWaiting
    {
        get { return !Input.GetMouseButtonDown(0); }
    }
}

// 使用
IEnumerator MyCoroutine()
{
    yield return new WaitForMouseDown();
    Debug.Log("鼠标左键被点击");
}
```

---

## 性能对比

| 操作 | GC分配 | 说明 |
|------|--------|------|
| `yield return null` | 9字节 | 基准开销 |
| `new WaitForSeconds()` | 21字节 | 每次创建 |
| 缓存的WaitForSeconds | 9字节 | 首次创建后无额外开销 |
| `StartCoroutine(string)` | 较高 | 反射开销 |
