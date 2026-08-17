using System;
using System.Collections.Generic;
using AsoulChess.Game.Core.Events;
using AsoulChess.Game.Core.Services;
using AsoulChess.Game.Core.Timing;
using AsoulChess.Game.Entity.Controllers;
using AsoulChess.Game.Entity.State;
using UnityEngine;

namespace AsoulChess.Game.Entity
{
    /// <summary>
    /// Combat entity root (framework analogue of AVZ <c>Chess</c> — not a drop-in replacement).
    /// P1-0: lifecycle shell + controller registry only; register controllers before <see cref="Init"/>.
    /// </summary>
    public class GameEntity : MonoBehaviour
    {
        readonly List<IEntityController> _controllers = new List<IEntityController>();

        bool _initialized;
        bool _isDead;

        public IEventBus Events { get; private set; }
        public ITimerService Timers { get; private set; }
        public IEntityRecycleHandler RecycleHandler { get; set; }
        public ISimulationGate SimulationGate { get; set; }

        public PropertyController Property { get; private set; }
        public StateController State { get; private set; }
        public SkillController Skills { get; private set; }
        public BuffController Buffs { get; private set; }

        public bool IsAlive => !_isDead && (Property == null || Property.IsAlive);
        public bool InCombat { get; private set; }

        public event Action<GameEntity> Died;

        /// <summary>Register before <see cref="Init"/>; safe to call after Init (late controllers get Init immediately).</summary>
        public void RegisterController(IEntityController controller)
        {
            if (controller == null || _controllers.Contains(controller))
                return;

            _controllers.Add(controller);
            AssignTypedReference(controller);

            if (_initialized)
                controller.InitController(this);
        }

        public T GetController<T>() where T : class, IEntityController
        {
            for (int i = 0; i < _controllers.Count; i++)
            {
                if (_controllers[i] is T typed)
                    return typed;
            }

            return null;
        }

        /// <summary>Wire services; calls <see cref="IEntityController.InitController"/> on all registered controllers.</summary>
        public void Init(IEventBus events = null, ITimerService timers = null)
        {
            Events = events ?? GameServices.Events ?? new EventBus();
            Timers = timers ?? GameServices.Timers ?? new TimerService();
            _initialized = true;
            _isDead = false;

            for (int i = 0; i < _controllers.Count; i++)
                _controllers[i].InitController(this);
        }

        public void EnterCombat()
        {
            if (InCombat || _isDead)
                return;

            InCombat = true;
            for (int i = 0; i < _controllers.Count; i++)
                _controllers[i].OnEnterCombat();

            Events?.Trigger(EntityEvents.EnterCombat, this);
        }

        public void LeaveCombat()
        {
            if (!InCombat)
                return;

            InCombat = false;
            for (int i = 0; i < _controllers.Count; i++)
                _controllers[i].OnLeaveCombat();
        }

        public void Die()
        {
            if (_isDead)
                return;

            _isDead = true;
            Buffs?.ClearAll();

            for (int i = 0; i < _controllers.Count; i++)
                _controllers[i].OnLeaveCombat();

            InCombat = false;
            State?.SetState(EntityStateId.Dead);

            StopAllCoroutines();
            Events?.Trigger(EntityEvents.Death, this);
            Died?.Invoke(this);
            RecycleHandler?.Recycle(this);
        }

        void Update()
        {
            if (!InCombat || !IsAlive)
                return;

            float dt = Time.deltaTime;
            for (int i = 0; i < _controllers.Count; i++)
                _controllers[i].Tick(dt);
        }

        void OnDestroy()
        {
            Buffs?.ClearAll();
            LeaveCombat();
        }

        void AssignTypedReference(IEntityController controller)
        {
            switch (controller)
            {
                case PropertyController property:
                    Property = property;
                    break;
                case StateController state:
                    State = state;
                    break;
                case SkillController skills:
                    Skills = skills;
                    break;
                case BuffController buffs:
                    Buffs = buffs;
                    break;
            }
        }
    }
}
