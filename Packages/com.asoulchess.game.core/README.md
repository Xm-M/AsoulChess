# AsoulChess Game Core (`com.asoulchess.game.core`)

L0 gameplay foundation for Unity projects (AVZ Phase 0):

| Module | Types | Purpose |
|--------|--------|---------|
| Events | `IEventBus`, `EventBus` | String-keyed pub/sub (no game `EventName` enum) |
| Timing | `ITimerService`, `TimerService`, `IGameplayTimerService`, `GameplayTimerService`, `GameTimer` | Simple delay timers + AVZ-style gameplay timers |
| Pooling | `IGameObjectPool`, `GameObjectPool` | GameObject get/release + optional pool scene |
| Physics | `IPhysics2DQueryBufferPool`, `Physics2DQueryBufferPool` | RaycastHit2D / Collider2D NonAlloc buffers |
| Audio | `IAudioService`, `AudioService` | Player registry + `AcquireUniqueLimited` dedup |
| UI | `IUIService`, `UIService`, `UIView` | View shell (Init/Show/Hide) |
| Scenes | `ISceneLoadService`, `SceneLoadService`, `ISceneTransitionView` | Async unload + additive load |
| Services | `GameServices` | Light locator |

**Not included:** chess, levels, skills, save, `EventName`, weather/fetter/prop, `GameManage` content fields.

## AVZ integration (0.2.0)

AVZ `EventController`, `TimerManage`, `ObjectPool`, `CheckObjectPoolManage`, `AudioManage` (limit dedup), `UIManage` shell, and `SceneManage` core delegate to `GameServices`. Access via **`GameManage.instance`** fields.

AVZ-specific: `EventName`, Save init in `UIManage.InitializeGameFlow`, Animator/ClockDemo transitions on `SceneManage`, full `AudioPlayer` Unique variants.

## Install

```
Packages/com.asoulchess.game.core/
```

Unity discovers embedded packages automatically.

## Quick start

```csharp
using AsoulChess.Game.Core.Services;
using UnityEngine;

public class Boot : MonoBehaviour
{
    void Awake()
    {
        GameServices.RegisterDefaults();
        GameServices.Events.AddListener("app.ready", () => Debug.Log("ready"));
        GameServices.Events.Trigger("app.ready");
    }
}
```

## Version

`0.2.0` — L0 modules + AVZ adapter wiring.
