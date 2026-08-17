using AsoulChess.Game.Core.Events;
using AsoulChess.Game.Core.Services;
using AsoulChess.Game.Core.Timing;
using AsoulChess.Game.Entity;
using AsoulChess.Game.Entity.Buffs;
using AsoulChess.Game.Entity.Skills;
using UnityEngine;

namespace AsoulChess.Game.Entity.Samples
{
    /// <summary>
    /// Space: apply ATK buff → cast damage skill on dummy → log HP / death.
    /// </summary>
    public sealed class EntityDemoRunner : MonoBehaviour
    {
        [SerializeField] KeyCode triggerKey = KeyCode.Space;

        EventBus _events;
        TimerService _timers;
        GameEntity _attacker;
        GameEntity _dummy;

        void Awake()
        {
            _events = new EventBus();
            _timers = new TimerService();
            GameServices.Register(_events, _timers, null);

            gameObject.AddComponent<TimerServiceBehaviour>().Bind(_timers);

            _attacker = CreateEntity("Attacker", new Vector3(-1.5f, 0f, 0f), 100f, 15f);
            _dummy = CreateEntity("Dummy", new Vector3(1.5f, 0f, 0f), 40f, 0f);

            _attacker.Skills.AddSkill(new InstantDamageSkill("demo.strike", 1f));
            _dummy.Died += e => Debug.Log($"[EntityDemo] {e.name} died");

            _attacker.EnterCombat();
            _dummy.EnterCombat();

            Debug.Log("[EntityDemo] Ready. Press Space: buff attacker → strike dummy.");
        }

        void OnDestroy() => GameServices.Clear();

        void Update()
        {
            if (!Input.GetKeyDown(triggerKey)) return;
            if (_attacker == null || !_attacker.IsAlive || _dummy == null || !_dummy.IsAlive)
            {
                Debug.Log("[EntityDemo] Combat over. Restart scene to retry.");
                return;
            }

            _attacker.Buffs.Add(new TimedAttackBuff("demo.atk_up", 10f, 3f));
            Debug.Log($"[EntityDemo] Buff applied. Attacker ATK={_attacker.Property.CurrentAttack}");

            bool used = _attacker.Skills.TryUse(0, _dummy);
            Debug.Log(used
                ? $"[EntityDemo] Strike hit. Dummy HP={_dummy.Property.Hp}/{_dummy.Property.MaxHp}"
                : "[EntityDemo] Skill not ready");
        }

        GameEntity CreateEntity(string name, Vector3 pos, float hp, float atk)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.position = pos;
            var entity = go.AddComponent<GameEntity>();
            EntityCombatBootstrap.RegisterCoreControllers(entity, hp, atk);
            entity.Init(_events, _timers);
            return entity;
        }
    }
}
