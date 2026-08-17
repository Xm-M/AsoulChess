using System;
using AsoulChess.Game.Entity.Combat;
using AsoulChess.Game.Entity.State;
using UnityEngine;

namespace AsoulChess.Game.Entity.Controllers
{
    /// <summary>
    /// HP / damage / heal pipeline (framework analogue of AVZ PropertyController core math).
    /// UI, Animator, Buff, State hooks are C# events — game layer subscribes.
    /// </summary>
    public sealed class PropertyController : IEntityController
    {
        EntityStats _template;
        EntityStats _data;
        GameEntity _entity;

        public EntityStats Data => _data;

        /// <summary>Fired before mitigation; subscribers may modify <see cref="DamageInfo.Amount"/>.</summary>
        public event Action<DamageInfo> BeforeReceiveDamage;

        public event Action<DamageInfo> ReceivedDamage;
        public event Action<DamageInfo> DamageDealt;
        public event Action<DamageInfo> HealCast;

        /// <summary>HP ratio changed (heal/damage); AVZ uses for VisualTier anim.</summary>
        public event Action<DamageInfo> HpRatioChanged;

        public event Action<float> AccelerateRateChanged;

        public PropertyController(EntityStats template)
        {
            _template = template ?? new EntityStats();
            _data = _template.Clone();
        }

        public PropertyController(float maxHp, float attack)
            : this(new EntityStats { Hp = maxHp, HpMax = maxHp, Attack = attack })
        {
        }

        public void SetTemplate(EntityStats template)
        {
            _template = template ?? new EntityStats();
            if (_data == null)
                _data = _template.Clone();
        }

        public void InitController(GameEntity entity) => _entity = entity;

        public void OnEnterCombat()
        {
            _data ??= _template.Clone();
            _data.ResetFrom(_template);
        }

        public void OnLeaveCombat()
        {
            BeforeReceiveDamage = null;
            ReceivedDamage = null;
            DamageDealt = null;
            HealCast = null;
            HpRatioChanged = null;
            AccelerateRateChanged = null;
        }

        public void Tick(float deltaTime) { }

        public bool IsAlive => _data != null && _data.Hp > 0f;

        public void ReceiveDamage(DamageInfo info)
        {
            if (_entity == null || info == null || !IsAlive)
                return;

            BeforeReceiveDamage?.Invoke(info);

            if (!info.SuppressAttachedEffect && info.AttachedBuff != null)
                _entity.Buffs?.Add(info.AttachedBuff);

            if (info.Type == DamageType.Heal)
            {
                Heal(info.Amount);
                HpRatioChanged?.Invoke(info);
                return;
            }

            if (info.Type == DamageType.Miss)
                info.Amount = 0f;
            else if (info.Type != DamageType.Real)
                info.Amount *= 1f - _data.Armor / (_data.Armor + 100f);

            info.Amount *= 1f - _data.ExtraDefence;

            if (UnityEngine.Random.value < GetDodge())
            {
                info.Amount = 0f;
                info.Type = DamageType.Miss;
            }

            if (info.Amount > 0f)
            {
                _data.Hp -= info.Amount;
                ReceivedDamage?.Invoke(info);
                HpRatioChanged?.Invoke(info);
                _entity.Events?.Trigger(EntityEvents.Damaged, _entity);

                if ((info.Element & ElementType.Grind) != 0 && info.Source != null)
                {
                    var sourceProperty = info.Source.Property;
                    if (sourceProperty != null && sourceProperty.GetSize() > GetSize())
                    {
                        _entity.Die();
                        return;
                    }
                }

                if (_data.Hp <= 0f)
                    _entity.Die();
            }
        }

        public void DealDamage(DamageInfo info)
        {
            if (info?.Target == null || !info.Target.IsAlive)
                return;

            if (info.Type != DamageType.Heal)
            {
                if (UnityEngine.Random.value < GetCrit())
                {
                    info.Amount *= GetCritDamage();
                    info.IsCritical = true;
                }
                else
                {
                    info.IsCritical = false;
                }

                info.Amount *= 1f + GetExtraDamage();

                info.Target.Property?.ReceiveDamage(info);
                DamageDealt?.Invoke(info);

                float lifeSteal = info.Amount * _data.LifeStealing;
                if (lifeSteal > 0f)
                    Heal(lifeSteal);
            }
            else
            {
                HealCast?.Invoke(info);
                info.Target.Property?.ReceiveDamage(info);
            }
        }

        /// <summary>Shortcut for demos: real damage without full message setup.</summary>
        public void ApplyDamage(float amount, GameEntity source = null)
        {
            DealDamage(new DamageInfo(source, _entity, amount, DamageType.Real));
        }

        public void Heal(float amount)
        {
            if (!IsAlive || amount <= 0f) return;
            amount *= _data.HealRate;

            if (_data.HpMax > _data.Hp + amount)
                _data.Hp += amount;
            else
                _data.Hp = _data.HpMax;

            _entity.Events?.Trigger(EntityEvents.Healed, _entity);
        }

        public void ChangeHp(float value) => _data.Hp = value;

        public void ChangeHpMax(float delta)
        {
            _data.HpMax += delta;
            if (delta > 0f) _data.Hp += delta;
        }

        public void ChangeAttack(float rateDelta)
        {
            _data.AttackRate += rateDelta;
            _data.Attack = _template.Attack * _data.AttackRate;
        }

        public void ChangeCrit(float delta) =>
            _data.Crit = delta < 0f ? Mathf.Max(0f, _data.Crit + delta) : _data.Crit + delta;

        public void ChangeDodgeRate(float delta) => _data.DodgeRate += delta;
        public void ChangeCritDamage(float delta) => _data.CritDamage += delta;
        public void ChangeArmor(float delta) => _data.Armor += delta;
        public void ChangeExtraDamage(float delta) => _data.ExtraDamage += delta;
        public void ChangeExtraDefence(float delta) => _data.ExtraDefence += delta;
        public void ChangeLifeStealing(float delta) => _data.LifeStealing = Mathf.Max(0f, _data.LifeStealing + delta);
        public void ChangeHealRate(float delta) => _data.HealRate = Mathf.Max(0f, _data.HealRate + delta);

        public void ChangeAccelerateRate(float delta)
        {
            _data.AccelerateRate += delta;
            AccelerateRateChanged?.Invoke(_data.AccelerateRate);
        }

        public void ChangeMoveAccelerateRate(float delta) => _data.MoveAccelerateRate += delta;

        public void ChangeDizzinessTime(float value)
        {
            if (_data == null) return;

            if (CurrentStateIsDizzy())
                _data.DizzinessTime = Mathf.Max(_data.DizzinessTime, value);
            else
            {
                _data.DizzinessTime = value;
                if (GetDizzinessTime() > 0f)
                    _entity.State?.ChangeState(EntityStateId.Dizzy);
            }
        }

        bool CurrentStateIsDizzy() =>
            _entity?.State != null && _entity.State.Current == EntityStateId.Dizzy;

        public float GetDizzinessTime() => _data.DizzinessTime * (1f - _data.Tenacity);

        public float GetTenacity() => _data.Tenacity;
        public void SetTenacity(float value) => _data.Tenacity = Mathf.Clamp01(value);
        public void ResetDizzinessTime() => _data.DizzinessTime = 0f;

        public void ChangeAttackRange(float delta) => _data.AttackRange += delta;
        public void SetAttackRange(float value) => _data.AttackRange = value;

        public float GetMoveSpeed() =>
            _data.Speed * Mathf.Max(0f, _data.AccelerateRate) * Mathf.Max(0f, _data.MoveAccelerateRate);

        public float GetAttack() => Mathf.Max(_data.Attack, 0f);
        public float GetExtraDamage() => _data.ExtraDamage;
        public float GetDodge() => _data.DodgeRate;
        public float GetExtraDefence() => _data.ExtraDefence;
        public float GetCooldownScaled(float baseCooldown) => baseCooldown / GetAccelerate();
        public float GetArmor() => _data.Armor;
        public float GetHp() => _data.Hp;
        public float GetMaxHp() => _data.HpMax;
        public float GetCrit() => _data.Crit;
        public float GetCritDamage()
        {
            float crit = GetCrit();
            float critDamage = _data.CritDamage;
            if (crit > 1f) critDamage += (crit - 1f) / 2f;
            return critDamage;
        }

        public float GetAccelerate() => _data.AccelerateRate;
        public float GetAttackRange() => _data.AttackRange;
        public int GetPrice() => _data.Price;
        public int GetRarity() => _data.Rarity;
        public void ChangePrice(float multiplier) => _data.Price = Mathf.Max(0, (int)(_data.Price * multiplier));
        public void ChangePrice(int delta) => _data.Price = Mathf.Max(0, _data.Price + delta);
        public int GetSize() => _data.Size;
        public void ChangeSize(int delta) => _data.Size = Mathf.Max(1, _data.Size + delta);
        public float GetHpPercent() => _data.HpMax > 0f ? _data.Hp / _data.HpMax : 0f;
        public float GetHealRate() => _data.HealRate;
        public float GetLifeStealing() => _data.LifeStealing;

        // Legacy names used by existing entity package / samples
        public float MaxHp => GetMaxHp();
        public float Hp => GetHp();
        public float Attack => GetAttack();
        public float AttackBonus { get; set; }
        public float CurrentAttack => Mathf.Max(0f, Attack + AttackBonus);
    }
}
