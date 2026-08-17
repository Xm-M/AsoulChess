using System;
using AsoulChess.Game.Core.Audio;
using AsoulChess.Game.Core.Events;
using AsoulChess.Game.Core.Physics;
using AsoulChess.Game.Core.Pooling;
using AsoulChess.Game.Core.Scenes;
using AsoulChess.Game.Core.Timing;
using AsoulChess.Game.Core.UI;

namespace AsoulChess.Game.Core.Services
{
    /// <summary>
    /// Light service locator for L0 framework modules.
    /// </summary>
    public static class GameServices
    {
        public static IEventBus Events { get; private set; }
        public static ITimerService Timers { get; private set; }
        public static IGameplayTimerService GameTimers { get; private set; }
        public static IGameObjectPool Pool { get; private set; }
        public static IPhysics2DQueryBufferPool Physics2DBuffers { get; private set; }
        public static IAudioService Audio { get; private set; }
        public static IUIService UI { get; private set; }
        public static ISceneLoadService SceneLoader { get; private set; }

        public static void Register(
            IEventBus events = null,
            ITimerService timers = null,
            IGameplayTimerService gameTimers = null,
            IGameObjectPool pool = null,
            IPhysics2DQueryBufferPool physics2DBuffers = null,
            IAudioService audio = null,
            IUIService ui = null,
            ISceneLoadService sceneLoader = null)
        {
            if (events != null) Events = events;
            if (timers != null) Timers = timers;
            if (gameTimers != null) GameTimers = gameTimers;
            if (pool != null) Pool = pool;
            if (physics2DBuffers != null) Physics2DBuffers = physics2DBuffers;
            if (audio != null) Audio = audio;
            if (ui != null) UI = ui;
            if (sceneLoader != null) SceneLoader = sceneLoader;
        }

        public static void RegisterDefaults()
        {
            Events ??= new EventBus();
            Timers ??= new TimerService();
            GameTimers ??= new GameplayTimerService();
            Pool ??= new GameObjectPool();
            Physics2DBuffers ??= new Physics2DQueryBufferPool();
            Audio ??= new AudioService();
        }

        public static void Clear()
        {
            Events = null;
            Timers = null;
            GameTimers = null;
            Pool = null;
            Physics2DBuffers = null;
            Audio = null;
            UI = null;
            SceneLoader = null;
        }

        public static T Require<T>(T service, string name) where T : class
        {
            if (service == null)
                throw new InvalidOperationException($"{name} is not registered. Call GameServices.Register or RegisterDefaults first.");
            return service;
        }
    }
}
