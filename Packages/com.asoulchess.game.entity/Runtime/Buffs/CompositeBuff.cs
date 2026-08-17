using System;

namespace AsoulChess.Game.Entity.Buffs
{
    /// <summary>
    /// Applies child buff templates then finishes (AVZ <c>MultyBuff</c>).
    /// The composite itself is not kept on the target.
    /// </summary>
    public sealed class CompositeBuff : Buff
    {
        readonly Buff[] _children;

        public CompositeBuff(string id, params Buff[] children)
        {
            Id = id;
            _children = children ?? Array.Empty<Buff>();
        }

        CompositeBuff() => _children = Array.Empty<Buff>();

        public override void OnApply(GameEntity target)
        {
            BindTarget(target);
            if (target.Buffs == null)
            {
                MarkFinished();
                return;
            }

            for (int i = 0; i < _children.Length; i++)
            {
                var child = _children[i];
                if (child != null)
                    target.Buffs.Add(child);
            }

            MarkFinished();
        }

        public override void OnRemove(GameEntity target) { }

        public override Buff Clone()
        {
            var clones = new Buff[_children.Length];
            for (int i = 0; i < _children.Length; i++)
                clones[i] = _children[i]?.Clone();
            return new CompositeBuff(Id, clones);
        }
    }
}
