---
name: unity-coroutine-system
description: Unity协程系统，提供分帧执行、延时操作、异步流程控制等功能。是Unity中处理异步逻辑的核心机制。
---

# Unity Coroutine System 协程系统

## 概述

Unity协程系统，提供分帧执行、延时操作、异步流程控制等功能。协程是Unity中处理异步逻辑的核心机制，适合延时、分帧加载、流程控制等场景。

## API白名单 ⚠️ 强制遵守

### ✅ 推荐使用的API

| API | 用途 | 性能等级 |
|-----|------|---------|
| `StartCoroutine(IEnumerator)` | 启动协程 | ⭐⭐⭐ 高性能 |
| `StopCoroutine(IEnumerator/Coroutine)` | 停止协程 | ⭐⭐⭐ 高性能 |
| 缓存的 `WaitForSeconds` | 延时等待 | ⭐⭐⭐ 高性能 |
| `WaitForSecondsRealtime` | 不受时间缩放影响的延时 | ⭐⭐⭐ 高性能 |
| `WaitUntil` | 条件等待（true时继续） | ⭐⭐⭐ 高性能 |
| `WaitWhile` | 条件等待（false时继续） | ⭐⭐⭐ 高性能 |
| `yield return null` | 等待下一帧 | ⭐⭐⭐ 高性能 |
| `CustomYieldInstruction` | 自定义等待条件 | ⭐⭐ 中等性能 |

### ⚠️ 性能警告API

| API | 性能问题 | 替代方案 |
|-----|----------|----------|
| `StartCoroutine(string)` | 性能差，无编译时检查 | 使用IEnumerator版本 |
| `new WaitForSeconds()` 在循环中 | 每次分配21字节GC | 预缓存对象 |
| `StopAllCoroutines()` | 无法精确控制 | 针对性停止 |

### ❌ 禁止/警告的API

| API | 警告原因 | 替代方案 |
|-----|----------|----------|
| 禁用脚本(`enable=false`) | 不会停止协程 | 显式调用StopCoroutine |

---

## 功能边界 ⚠️ 强制说明

### 本Skill涵盖范围

- ✅ 协程启动与停止
- ✅ YieldInstruction派生类
- ✅ CustomYieldInstruction自定义
- ✅ 协程生命周期管理
- ✅ 协程与async/await对比

### 不在本Skill范围内

- ❌ async/await完整教程 → 不涉及，使用协程
- ❌ 多线程编程 → 不涉及
- ❌ Job System → 不涉及

### 性能限制

| 指标 | 建议值 | 说明 |
|------|--------|------|
| 同时运行协程数 | ≤ 100 | 避免大量协程 |
| WaitForSeconds缓存 | 必须 | 避免GC |
| 嵌套协程深度 | ≤ 5 | 避免调用栈过深 |

---

## 渐进式学习路径

### 阶段1：基础使用

```csharp
// 缓存WaitForSeconds
private WaitForSeconds waitOneSecond = new WaitForSeconds(1f);

IEnumerator DelayedAction()
{
    yield return waitOneSecond;
    Debug.Log("1秒后执行");
}
```

### 阶段2：常用功能

```csharp
// 条件等待
IEnumerator WaitForCondition()
{
    yield return new WaitUntil(() => playerHealth <= 0);
    Debug.Log("玩家死亡");
}

// 分帧处理
IEnumerator ProcessItems(List<int> items)
{
    foreach (int item in items)
    {
        ProcessItem(item);
        yield return null; // 每帧处理一个
    }
}
```

### 阶段3：正确停止

```csharp
private IEnumerator currentCoroutine;

void StartTask()
{
    currentCoroutine = MyCoroutine();
    StartCoroutine(currentCoroutine);
}

void StopTask()
{
    if (currentCoroutine != null)
    {
        StopCoroutine(currentCoroutine);
    }
}
```

---

## References

- [API参考文档](references/api-reference.md)
