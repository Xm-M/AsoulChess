using System;
using UnityEngine;

namespace AsoulChess.Game.Entity.Combat
{
    /// <summary>Runtime combat stats (framework analogue of AVZ <see cref="Property"/>).</summary>
    [Serializable]
    public class EntityStats
    {
        public float Attack;
        public float AttackRate = 1f;

        public float Crit;
        public float CritDamage = 1.3f;
        public float DodgeRate;

        public float ExtraDamage;
        public float Armor;
        [Range(0f, 1f)] public float ExtraDefence;

        public float Hp = 100f;
        public float HpMax = 100f;

        public float LifeStealing;
        public float HealRate = 1f;

        public float Speed;
        public float AccelerateRate = 1f;
        public float MoveAccelerateRate = 1f;

        public float DizzinessTime;
        public float Tenacity;

        public float AttackRange = 1f;
        public int Price = 50;
        public int Rarity = 4000;
        public int WaveLimit = 1;
        public float Cooldown = 7.5f;
        public int Size = 1;

        public EntityStats() { }

        public EntityStats(EntityStats template) => ResetFrom(template);

        public void ResetFrom(EntityStats template)
        {
            if (template == null) return;

            Attack = template.Attack;
            AttackRate = 1f;
            Crit = template.Crit;
            CritDamage = template.CritDamage;
            DodgeRate = template.DodgeRate;
            ExtraDamage = template.ExtraDamage;
            ExtraDefence = template.ExtraDefence;
            Armor = template.Armor;
            Hp = template.Hp;
            HpMax = template.HpMax;
            Tenacity = template.Tenacity;
            DizzinessTime = 0f;
            Speed = template.Speed;
            LifeStealing = template.LifeStealing;
            HealRate = template.HealRate;
            AccelerateRate = 1f;
            MoveAccelerateRate = 1f;
            AttackRange = template.AttackRange;
            Price = template.Price;
            Rarity = template.Rarity;
            WaveLimit = template.WaveLimit;
            Cooldown = template.Cooldown;
            Size = template.Size;
        }

        public EntityStats Clone() => new EntityStats(this);
    }
}
