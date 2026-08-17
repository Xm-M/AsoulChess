using System.Collections.Generic;

namespace AsoulChess.Game.Entity.Skills
{
    /// <summary>Per-entity black board for skills (AVZ SkillContext analogue).</summary>
    public sealed class SkillContext
    {
        readonly Dictionary<string, object> _data = new Dictionary<string, object>();

        public void Set(string key, object value) => _data[key] = value;

        public bool TryGet<T>(string key, out T value)
        {
            if (_data.TryGetValue(key, out var raw) && raw is T typed)
            {
                value = typed;
                return true;
            }
            value = default;
            return false;
        }

        public void Remove(string key) => _data.Remove(key);
        public void Clear() => _data.Clear();
    }
}
