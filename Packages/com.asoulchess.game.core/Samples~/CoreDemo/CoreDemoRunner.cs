using AsoulChess.Game.Core.Events;
using AsoulChess.Game.Core.Pooling;
using AsoulChess.Game.Core.Services;
using AsoulChess.Game.Core.Timing;
using UnityEngine;

namespace AsoulChess.Game.Core.Samples
{
    /// <summary>
    /// Minimal demo: Space → event → 0.5s delay → spawn from pool → 1s → release.
    /// Attach to any GameObject in an empty scene. Assign a Prefab (or leave null to use a runtime Cube).
    /// </summary>
    public sealed class CoreDemoRunner : MonoBehaviour
    {
        public const string PingEvent = "core.demo.ping";

        [SerializeField] GameObject prefab;
        [SerializeField] KeyCode triggerKey = KeyCode.Space;

        EventBus _events;
        TimerService _timers;
        GameObjectPool _pool;
        TimerServiceBehaviour _timerHost;
        GameObject _runtimePrefab;

        void Awake()
        {
            _events = new EventBus();
            _timers = new TimerService();
            _pool = new GameObjectPool(transform);

            GameServices.Register(_events, _timers, _pool);

            _timerHost = gameObject.AddComponent<TimerServiceBehaviour>();
            _timerHost.Bind(_timers);

            if (prefab == null)
            {
                _runtimePrefab = GameObject.CreatePrimitive(PrimitiveType.Cube);
                _runtimePrefab.name = "CoreDemoCube";
                _runtimePrefab.SetActive(false);
                prefab = _runtimePrefab;
            }

            _events.AddListener(PingEvent, OnPing);
        }

        void OnDestroy()
        {
            _events?.RemoveListener(PingEvent, OnPing);
            _pool?.Clear();
            if (_runtimePrefab != null)
                Destroy(_runtimePrefab);
            GameServices.Clear();
        }

        void Update()
        {
            if (Input.GetKeyDown(triggerKey))
                _events.Trigger(PingEvent);
        }

        void OnPing()
        {
            Debug.Log("[CoreDemo] ping received → schedule spawn");
            _timers.Delay(() =>
            {
                var go = _pool.Get(prefab);
                go.transform.position = Vector3.zero;
                Debug.Log("[CoreDemo] pooled object spawned → schedule release");
                _timers.Delay(() =>
                {
                    _pool.Release(go);
                    Debug.Log("[CoreDemo] released back to pool");
                }, 1f);
            }, 0.5f);
        }
    }
}
