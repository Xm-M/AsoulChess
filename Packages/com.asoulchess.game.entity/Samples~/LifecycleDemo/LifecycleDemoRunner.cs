using AsoulChess.Game.Core.Events;
using AsoulChess.Game.Core.Services;
using AsoulChess.Game.Core.Timing;
using AsoulChess.Game.Entity;
using UnityEngine;

namespace AsoulChess.Game.Entity.Samples
{
    /// <summary>
    /// P1-0 sample: Space enter combat (if left), D die, L leave combat.
    /// </summary>
    public sealed class LifecycleDemoRunner : MonoBehaviour
    {
        GameEntity _entity;

        void Awake()
        {
            var events = new EventBus();
            var timers = new TimerService();
            GameServices.Register(events, timers, null);
            gameObject.AddComponent<TimerServiceBehaviour>().Bind(timers);

            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "LifecycleUnit";
            _entity = go.AddComponent<GameEntity>();
            _entity.RegisterController(new LoggingLifecycleController());
            _entity.Init(events, timers);
            _entity.Died += e => Debug.Log($"[P1-0] Died event: {e.name}");

            _entity.EnterCombat();
            Debug.Log("[P1-0 LifecycleDemo] EnterCombat done. Keys: L=LeaveCombat, D=Die, E=EnterCombat");
        }

        void OnDestroy() => GameServices.Clear();

        void Update()
        {
            if (_entity == null) return;
            if (Input.GetKeyDown(KeyCode.L)) _entity.LeaveCombat();
            if (Input.GetKeyDown(KeyCode.E)) _entity.EnterCombat();
            if (Input.GetKeyDown(KeyCode.D)) _entity.Die();
        }
    }
}
