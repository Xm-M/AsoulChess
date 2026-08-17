using System;
using AsoulChess.Game.Entity.Animation;
using AsoulChess.Game.Entity.State;

namespace AsoulChess.Game.Entity.Controllers
{
    /// <summary>
    /// Entity FSM (framework analogue of AVZ <c>StateController</c> + <c>StateGraph</c>).
    /// Transitions and custom states are game-defined; default graph covers combat basics.
    /// </summary>
    public sealed class StateController : IEntityController
    {
        EntityStateGraph _template = EntityStateGraph.CreateCombatDefault();
        EntityStateGraph _runtime;

        StateNode _current;
        StateNode _previous;

        IEntityState _anyState;
        IStateTransition _anyTransition;

        GameEntity _entity;

        public EntityStateId Current { get; private set; } = EntityStateId.Idle;
        public EntityStateId PreviousId { get; private set; } = EntityStateId.Idle;

        public IAnimBridge Anim { get; set; } = NullAnimBridge.Instance;

        /// <summary>Graph template cloned per entity on init (AVZ: <c>StateGraph</c> asset).</summary>
        public EntityStateGraph GraphTemplate
        {
            get => _template;
            set => _template = value ?? EntityStateGraph.CreateCombatDefault();
        }

        public event Action<EntityStateId, EntityStateId> StateChanged;

        public void InitController(GameEntity entity)
        {
            _entity = entity;
            _runtime = _template.CloneRuntime();
        }

        public void OnEnterCombat()
        {
            _anyState = null;
            _anyTransition = null;
            ChangeState(_runtime.EnterCombatState);
        }

        public void OnLeaveCombat()
        {
            _anyState = null;
            _anyTransition = null;
        }

        public void Tick(float deltaTime) => TickState(deltaTime);

        public void TickState(float deltaTime)
        {
            if (_entity == null || _runtime == null)
                return;

            bool simRunning = _entity.SimulationGate?.IsSimulationRunning ?? true;
            if (!simRunning)
            {
                if (Current != EntityStateId.Prepare)
                    ChangeState(EntityStateId.Prepare);
                return;
            }

            if (_anyState != null)
            {
                _anyState.Execute(_entity, this, deltaTime);
                if (_anyTransition != null && _anyTransition.Evaluate(_entity))
                    RevertToPreviousState();
                return;
            }

            if (_current?.State == null)
                return;

            _current.State.Execute(_entity, this, deltaTime);

            for (int i = 0; i < _current.Transitions.Count; i++)
            {
                var edge = _current.Transitions[i];
                if (edge?.Transition != null && edge.Transition.Evaluate(_entity))
                {
                    ChangeState(edge.TargetId);
                    return;
                }
            }
        }

        public void SetState(EntityStateId state) => ChangeState(state);

        public void ChangeState(EntityStateId stateId)
        {
            if (Current == EntityStateId.Dead && stateId != EntityStateId.Dead)
                return;

            if (!_runtime.TryGetNode(stateId, out var node) || node == null)
                return;

            _current?.State?.Exit(_entity, this);

            _previous = _current;
            PreviousId = Current;

            _current = node;
            Current = stateId;

            _current.State.Enter(_entity, this);
            StateChanged?.Invoke(PreviousId, Current);
        }

        public void RevertToPreviousState()
        {
            if (_previous == null)
                return;

            _anyState = null;
            _anyTransition = null;

            _current?.State?.Exit(_entity, this);

            var prevId = PreviousId;
            _current = _previous;
            Current = _current.State.Id;

            _current.State.Enter(_entity, this);
            StateChanged?.Invoke(prevId, Current);
        }

        /// <summary>Overlay state until transition succeeds (AVZ: <c>ChangeAnyState</c>).</summary>
        public void EnterOverlayState(IEntityState overlay, IStateTransition exitWhen)
        {
            if (overlay == null) return;

            _anyState = overlay.Clone();
            _anyTransition = exitWhen?.Clone();

            _current?.State?.Exit(_entity, this);
            _previous = _current;
            PreviousId = Current;

            _anyState.Enter(_entity, this);
        }

        public void AddTransition(EntityStateId from, IStateTransition transition, EntityStateId to)
        {
            if (transition == null || _runtime == null) return;
            if (!_runtime.TryGetNode(from, out var node) || node == null) return;

            node.Transitions.Add(new StateTransitionEdge
            {
                Transition = transition,
                TargetId = to
            });
        }
    }
}
