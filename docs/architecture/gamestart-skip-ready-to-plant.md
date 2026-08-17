# 架构 / 影响面: SkipReadyToPlant

**日期**: 2026-07-27 · L1

## 设计

```csharp
// 静态/会话标志（插件置位，TextPanel 消费）
public static class ReadyToPlantBanner
{
    public static bool SkipOnce;
}

public class GameStartPlugin_SkipReadyToPlant : ILevelPlugin
{
    public void StadgeEffect(...) => ReadyToPlantBanner.SkipOnce = true;
    public void OverPlugin(...) { } // 可选清标志
}

// TextPanel.GameStart:
if (ReadyToPlantBanner.SkipOnce) {
    ReadyToPlantBanner.SkipOnce = false;
    return;
}
// 原：PlayAudio + animator.Play
```

不改 `LevelController` 调用结构（仍 Show + GameStart）；Endless 自动覆盖。

离场 `OverPlugin` / `WhenLeaveLevel` 清残留标志，避免脏状态。

## 影响文件
| 文件 | 改动 |
|------|------|
| 新建 `GameStartPlugin_SkipReadyToPlant.cs` | 插件 |
| `TextPanel.cs` | GameStart 开头判断 |
| 可选小静态类同文件或独立 | 标志 |

## 风险：低（未挂插件零行为差）
