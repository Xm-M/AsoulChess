using System;
using System.Collections.Generic;

namespace AsoulChess.Game.Entity.State
{
    [Serializable]
    public sealed class StateTransitionEdge
    {
        public IStateTransition Transition;
        public EntityStateId TargetId;

        public StateTransitionEdge Clone() =>
            new StateTransitionEdge { Transition = Transition?.Clone(), TargetId = TargetId };
    }

    public sealed class StateNode
    {
        public IEntityState State;
        public List<StateTransitionEdge> Transitions = new List<StateTransitionEdge>();

        public StateNode Clone()
        {
            var clone = new StateNode { State = State?.Clone() };
            for (int i = 0; i < Transitions.Count; i++)
                clone.Transitions.Add(Transitions[i].Clone());
            return clone;
        }
    }

    /// <summary>Runtime state graph template (AVZ <c>StateGraph</c> analogue without ScriptableObject).</summary>
    public sealed class EntityStateGraph
    {
        readonly Dictionary<EntityStateId, StateNode> _nodes = new Dictionary<EntityStateId, StateNode>();

        public EntityStateId EnterCombatState { get; set; } = EntityStateId.Idle;

        public IReadOnlyDictionary<EntityStateId, StateNode> Nodes => _nodes;

        public void Register(IEntityState state, IEnumerable<StateTransitionEdge> transitions = null)
        {
            if (state == null) return;

            var node = new StateNode { State = state };
            if (transitions != null)
            {
                foreach (var edge in transitions)
                {
                    if (edge != null)
                        node.Transitions.Add(edge);
                }
            }

            _nodes[state.Id] = node;
        }

        public bool TryGetNode(EntityStateId id, out StateNode node) => _nodes.TryGetValue(id, out node);

        public EntityStateGraph CloneRuntime()
        {
            var copy = new EntityStateGraph { EnterCombatState = EnterCombatState };
            foreach (var kv in _nodes)
                copy._nodes[kv.Key] = kv.Value.Clone();
            return copy;
        }

        public static EntityStateGraph CreateCombatDefault()
        {
            var graph = new EntityStateGraph();
            graph.Register(new IdleEntityState());
            graph.Register(new AttackEntityState());
            graph.Register(new SkillEntityState());
            graph.Register(new DeathEntityState());
            graph.Register(new DizzyEntityState());
            graph.Register(new PrepareEntityState());
            graph.Register(new MoveEntityState());
            return graph;
        }
    }
}
