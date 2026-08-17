using System;
using System.Collections.Generic;
using AsoulChess.Game.Entity.Buffs;

namespace AsoulChess.Game.Entity.Controllers
{
    /// <summary>
    /// Buff registry per entity (framework analogue of AVZ <c>BuffController</c>).
    /// Same-id add refreshes via <see cref="Buff.OnReset"/>; templates are cloned on first apply.
    /// </summary>
    public sealed class BuffController : IEntityController
    {
        readonly Dictionary<string, Buff> _active = new Dictionary<string, Buff>();
        readonly List<Buff> _enterCombatTemplates = new List<Buff>();

        GameEntity _entity;

        /// <summary>Game-side gate e.g. control immunity (AVZ: ZombieKing stand).</summary>
        public Func<Buff, bool> CanAddBuff { get; set; }

        public IReadOnlyCollection<Buff> ActiveBuffs => _active.Values;

        public void InitController(GameEntity entity) => _entity = entity;

        public void OnEnterCombat()
        {
            for (int i = 0; i < _enterCombatTemplates.Count; i++)
                Add(_enterCombatTemplates[i]);
        }

        public void OnLeaveCombat() => ClearAll();

        public void Tick(float deltaTime)
        {
            if (_active.Count == 0) return;

            _scratch.Clear();
            foreach (var kv in _active)
                _scratch.Add(kv.Value);

            for (int i = 0; i < _scratch.Count; i++)
            {
                var buff = _scratch[i];
                if (!_active.ContainsKey(buff.Id) || !ReferenceEquals(_active[buff.Id], buff))
                    continue;

                buff.Tick(deltaTime);
                if (buff.IsFinished)
                    RemoveInstance(buff);
            }
        }

        readonly List<Buff> _scratch = new List<Buff>();

        /// <summary>Templates applied on each <see cref="OnEnterCombat"/> (AVZ: <c>onEnterBuff</c>).</summary>
        public void SetEnterCombatBuffs(IEnumerable<Buff> templates)
        {
            _enterCombatTemplates.Clear();
            if (templates == null) return;
            foreach (var t in templates)
            {
                if (t != null)
                    _enterCombatTemplates.Add(t);
            }
        }

        public void AddEnterCombatBuff(Buff template)
        {
            if (template != null)
                _enterCombatTemplates.Add(template);
        }

        public bool Contains(string buffId) => !string.IsNullOrEmpty(buffId) && _active.ContainsKey(buffId);

        public bool TryGet(string buffId, out Buff buff) => _active.TryGetValue(buffId, out buff);

        /// <summary>Add or refresh buff from template (AVZ: <c>AddBuff</c>).</summary>
        public void Add(Buff template)
        {
            if (template == null || _entity == null)
                return;

            if (CanAddBuff != null && !CanAddBuff(template))
                return;

            string id = ResolveId(template);

            if (_active.TryGetValue(id, out var existing))
            {
                existing.OnReset(template);
                _entity.Events?.Trigger(EntityEvents.BuffAdded, _entity);
                return;
            }

            var instance = template.Clone();
            instance.Id = id;
            instance.Owner = this;
            _active[id] = instance;
            instance.OnApply(_entity);
            _entity.Events?.Trigger(EntityEvents.BuffAdded, _entity);
        }

        public void AddBuff(Buff template) => Add(template);

        public void TryFinish(string buffId)
        {
            if (TryGet(buffId, out var buff))
                RemoveInstance(buff);
        }

        public void ClearAll()
        {
            if (_active.Count == 0) return;

            _scratch.Clear();
            foreach (var kv in _active)
                _scratch.Add(kv.Value);

            for (int i = 0; i < _scratch.Count; i++)
                RemoveInstance(_scratch[i], triggerEvent: false);

            _active.Clear();
        }

        internal void NotifyFinished(Buff buff) => RemoveInstance(buff);

        void RemoveInstance(Buff buff, bool triggerEvent = true)
        {
            if (buff == null) return;

            if (!string.IsNullOrEmpty(buff.Id))
                _active.Remove(buff.Id);

            buff.Owner = null;
            buff.OnRemove(_entity);

            if (triggerEvent)
                _entity?.Events?.Trigger(EntityEvents.BuffRemoved, _entity);
        }

        static string ResolveId(Buff template)
        {
            if (!string.IsNullOrEmpty(template.Id))
                return template.Id;
            return template.GetType().Name;
        }
    }
}
