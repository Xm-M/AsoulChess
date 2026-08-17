using AsoulChess.Game.Board;
using AsoulChess.Game.Core.Events;
using AsoulChess.Game.Core.Services;
using AsoulChess.Game.Core.Timing;
using AsoulChess.Game.Entity;
using AsoulChess.Game.Entity.Board;
using UnityEngine;

namespace AsoulChess.Game.Entity.Samples
{
    /// <summary>
    /// Grid combat demo: two units occupy cells, step closer, attack until one dies.
    /// Requires Core + Entity + Optional/Board + Board packages.
    /// </summary>
    public sealed class CombatGridDemoRunner : MonoBehaviour
    {
        [SerializeField] int boardWidth = 6;
        [SerializeField] int boardHeight = 3;
        [SerializeField] float cellSize = 1.2f;
        [SerializeField] float thinkInterval = 0.6f;

        EventBus _events;
        TimerService _timers;
        GridBoard _board;
        GameEntity _a;
        GameEntity _b;
        float _think;

        void Awake()
        {
            _events = new EventBus();
            _timers = new TimerService();
            GameServices.Register(_events, _timers, null);
            gameObject.AddComponent<TimerServiceBehaviour>().Bind(_timers);

            _board = new GridBoard(boardWidth, boardHeight, cellSize, origin: Vector3.zero);
            BuildTileVisuals();

            _a = CreateUnit("Red", 80f, 12f, 0, 1, Color.red);
            _b = CreateUnit("Blue", 80f, 10f, boardWidth - 1, 1, Color.blue);

            _a.Died += e => Debug.Log($"[CombatGrid] {e.name} died — demo over");
            _b.Died += e => Debug.Log($"[CombatGrid] {e.name} died — demo over");

            _a.EnterCombat();
            _b.EnterCombat();

            Debug.Log("[CombatGrid] Running. Units will step and attack automatically.");
        }

        void OnDestroy() => GameServices.Clear();

        void Update()
        {
            if (_a == null || _b == null) return;
            if (!_a.IsAlive || !_b.IsAlive) return;

            _think += Time.deltaTime;
            if (_think < thinkInterval) return;
            _think = 0f;

            TickUnit(_a, _b);
            TickUnit(_b, _a);
        }

        void TickUnit(GameEntity self, GameEntity enemy)
        {
            if (!self.IsAlive || !enemy.IsAlive) return;

            var attack = self.GetController<AttackController>();
            var move = self.GetController<MoveController>();
            var enemyMove = enemy.GetController<MoveController>();
            if (attack == null || move == null) return;

            if (attack.IsInRange(enemy))
            {
                if (attack.TryAttack(enemy))
                    Debug.Log($"[CombatGrid] {self.name} hits {enemy.name} → HP {enemy.Property.Hp:0}/{enemy.Property.MaxHp}");
                return;
            }

            if (enemyMove != null && enemyMove.IsOnBoard && move.TryStepToward(enemyMove.X, enemyMove.Y))
                Debug.Log($"[CombatGrid] {self.name} steps to ({move.X},{move.Y})");
        }

        GameEntity CreateUnit(string name, float hp, float atk, int x, int y, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.localScale = Vector3.one * (cellSize * 0.7f);
            var renderer = go.GetComponent<Renderer>();
            if (renderer != null)
                renderer.material.color = color;

            var entity = go.AddComponent<GameEntity>();
            var move = EntityBoardBootstrap.RegisterBoardCombat(entity, _board, hp, atk, 1, 0.85f);
            entity.Init(_events, _timers);
            move.TryPlace(x, y);
            return entity;
        }

        void BuildTileVisuals()
        {
            var root = new GameObject("BoardVisual");
            for (int y = 0; y < _board.Height; y++)
            for (int x = 0; x < _board.Width; x++)
            {
                var tile = _board.GetTile(x, y);
                var quad = GameObject.CreatePrimitive(PrimitiveType.Cube);
                quad.name = $"Tile_{x}_{y}";
                quad.transform.SetParent(root.transform, false);
                quad.transform.position = tile.WorldPosition + Vector3.down * 0.55f;
                quad.transform.localScale = new Vector3(cellSize * 0.95f, 0.1f, cellSize * 0.95f);
                var r = quad.GetComponent<Renderer>();
                if (r != null)
                    r.material.color = (x + y) % 2 == 0 ? new Color(0.35f, 0.35f, 0.4f) : new Color(0.45f, 0.45f, 0.5f);
            }
        }
    }
}
