using AsoulChess.Game.Entity;
using UnityEngine;

namespace AsoulChess.Game.Entity.Samples
{
    /// <summary>Logs lifecycle calls for P1-0 acceptance.</summary>
    public sealed class LoggingLifecycleController : IEntityController
    {
        readonly string _label;
        float _tickAccum;

        public LoggingLifecycleController(string label = "Lifecycle")
        {
            _label = label;
        }

        public void InitController(GameEntity entity) =>
            Debug.Log($"[P1-0 {_label}] InitController on {entity.name}");

        public void OnEnterCombat() =>
            Debug.Log($"[P1-0 {_label}] OnEnterCombat");

        public void OnLeaveCombat() =>
            Debug.Log($"[P1-0 {_label}] OnLeaveCombat");

        public void Tick(float deltaTime)
        {
            _tickAccum += deltaTime;
            if (_tickAccum < 1f) return;
            _tickAccum = 0f;
            Debug.Log($"[P1-0 {_label}] Tick");
        }
    }
}
