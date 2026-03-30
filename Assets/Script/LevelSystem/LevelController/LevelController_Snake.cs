using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 贪吃蛇关卡：用 <see cref="LevelData.MaxWave"/> / n / t / <see cref="LevelData.createZombieType"/> 驱动波次预算公式；
/// 食物池来自 <see cref="foodCreatorsOverride"/>（优先）或 <see cref="LevelData.zombieList"/>；
/// 出怪若使用 <see cref="snakeWaveZombieCreatorsOverride"/> 可与食物分离。
/// <para>
/// <b>按波节奏（<see cref="paceFoodByWave"/>=true，默认）</b>：每波开始时按本波「总价预算」像僵尸波一样一次性生成食物；
/// 本波内不再补货；进下一波条件对齐僵尸关：<see cref="WaveCanAdvance"/> 思路（清空且过 mintime，或 t&gt;maxtime 等）。
/// </para>
/// <para>
/// <b>旧版多食物补给（paceFoodByWave=false）</b>：<see cref="minFoodOnField"/>～<see cref="maxFoodOnField"/> + 定时补货；
/// <see cref="useWaveValueBudgetCap"/> 为 true 时用本波总价卡住补充。
/// </para>
/// <para>
/// <b>僵尸（可选）</b>：<see cref="spawnZombiesWithFoodWave"/> 为真时，每波 <see cref="BeginFoodWave"/> 与食物同步，
/// 动态 <see cref="WaveData.InitWave"/> + <see cref="WaveData.EnterWave"/>；僵尸列表用 <see cref="snakeWaveZombieCreatorsOverride"/>（优先），否则与 <see cref="LevelData.zombieList"/> 相同（常与食物共用，需分离时请填覆盖）。
/// </para>
/// </summary>
public class LevelController_Snake : LevelController
{
    [FoldoutGroup("蛇头"), LabelText("蛇头 PropertyCreator")]
    [Tooltip("非空则在 GameStart 时在地图上创建蛇头，并交给 SnakeGridController")]
    public PropertyCreator snakeHeadCreator;

    [FoldoutGroup("蛇头"), LabelText("蛇头队伍 tag")]
    public string snakeHeadTeamTag = "Player";

    [FoldoutGroup("蛇头"), LabelText("蛇头出生格(x,y)")]
    [Tooltip("x 或 y 为负数时自动找第一个空位")]
    public Vector2Int headSpawnGrid = new Vector2Int(-1, -1);

    [FoldoutGroup("蛇头"), LabelText("SnakeGridController")]
    [Tooltip("空则在场景内查找")]
    public SnakeGridController snakeDriver;

    [FoldoutGroup("蛇关卡"), LabelText("食物池覆盖")]
    [Tooltip("非空时优先使用；否则用 LevelData.zombieList")]
    public List<PropertyCreator> foodCreatorsOverride;

    [FoldoutGroup("蛇关卡"), LabelText("场上食物数量下限")]
    [MinValue(0)] public int minFoodOnField = 4;

    [FoldoutGroup("蛇关卡"), LabelText("场上食物数量上限")]
    [MinValue(1)] public int maxFoodOnField = 12;

    [FoldoutGroup("蛇关卡"), LabelText("按波次像僵尸一样刷食物")]
    [Tooltip("开：每波开始时按本波总价预算一次性生成，吃完或时间到进下一波；关：用 min/max + foodSpawnInterval 持续补货")]
    public bool paceFoodByWave = true;

    [FoldoutGroup("蛇关卡"), LabelText("用本波总价上限卡住补给")]
    [Tooltip("关：只按 min/max 数量 + 定时补货（现代多食物）。开：总价達本波预算后不再生成，直到被吃掉使总价下降")]
    public bool useWaveValueBudgetCap = false;

    [FoldoutGroup("蛇关卡"), LabelText("生成尝试间隔(秒)")]
    [Tooltip("每隔多久尝试生成 1 个食物（在数量与可选预算允许时）")]
    [MinValue(0.05f)] public float foodSpawnInterval = 0.35f;

    [FoldoutGroup("蛇关卡"), LabelText("随机格子排除最左列")]
    public bool excludeLeftmostColumn = true;

    [FoldoutGroup("蛇关卡"), LabelText("生成用队伍标签")]
    public string foodTeamTag = "Player";

    [FoldoutGroup("蛇关卡/难度"), LabelText("波次增加蛇移速")]
    [Tooltip("每进入新一波在 BeginFoodWave 时给蛇头叠一层 Buff_BaseValueBuff_AcceleRate（与 ChangeAcceleRate 一致，影响 GetMoveSpeed）")]
    public bool snakeSpeedScaleByWave = true;

    [FoldoutGroup("蛇关卡/难度"), LabelText("每波移速倍率增量")]
    [Tooltip("第 n 波总加成 = (n-1)×本值；acceleRated 基础为 1，故 0.05 表示从第 2 波起每波约 +5% 移速")]
    [Min(0f)]
    public float snakeAcceleRatePerWave = 0.04f;

    [FoldoutGroup("蛇关卡/难度"), LabelText("受挫眩晕时长(秒)")]
    [Tooltip("原立即 GameOver(false) 的场合改为对蛇头施加 DizznessBuff 眩晕，时长为此值")]
    [Min(0.05f)]
    public float snakeDefeatStunDuration = 2f;

    const string SnakeWaveMoveSpeedBuffName = "蛇关波次移速";
    const string SnakeDefeatStunBuffName = "蛇关受挫眩晕";

    [FoldoutGroup("蛇关卡/墓碑"), LabelText("墓碑 PropertyCreator")]
    [Tooltip("非空且间隔>0 时，按波次在空地上生成（规则对齐 Hammer：随机 x≥最小列、排除蛇身/食物/已有墓碑）")]
    public PropertyCreator tombstoneCreator;

    [FoldoutGroup("蛇关卡/墓碑"), LabelText("每隔几波生成一次")]
    [Tooltip("0=不生成；第 w 波满足 w % 本值==0 时在 BeginFoodWave 生成（如 3 表示第 3、6、9…波）")]
    [Min(0)]
    public int tombstoneEveryNWaves = 3;

    [FoldoutGroup("蛇关卡/墓碑"), LabelText("每次生成数量")]
    [Min(0)]
    public int tombstonesPerSpawn = 2;

    [FoldoutGroup("蛇关卡/墓碑"), LabelText("墓碑队伍 tag")]
    public string tombstoneTeamTag = "Enemy";

    [FoldoutGroup("蛇关卡/墓碑"), LabelText("最小列 x")]
    [Tooltip("与 Hammer 关一致：随机格从该列起，默认 2（避开最左列）")]
    [Min(0)]
    public int tombstoneMinGridX = 2;

    [FoldoutGroup("蛇关卡/僵尸"), LabelText("与食物同波出怪")]
    [Tooltip("开：每波 BeginFoodWave 时现算一波僵尸（与 WaveData.InitWave 总价、随机选种一致），不预先为 MaxWave 逐波 InitWave")]
    public bool spawnZombiesWithFoodWave = true;

    [FoldoutGroup("蛇关卡/僵尸"), LabelText("出怪僵尸列表（优先于 LevelData）")]
    [Tooltip("非空：仅用于 WaveData.InitWave 出怪，与食物池分离。食物请用「食物池覆盖」或 LevelData.zombieList；若食物与僵尸共用同一列表会混淆，应在此填真实僵尸列表。")]
    public List<PropertyCreator> snakeWaveZombieCreatorsOverride;

    [ShowInInspector, ReadOnly]
    [ShowIf("@UnityEngine.Application.isPlaying")]
    readonly List<Chess> _activeFood = new List<Chess>();

    readonly List<Chess> _tombstones = new List<Chess>();

    Timer _foodTimer;

    [ShowInInspector, ReadOnly]
    [ShowIf("@UnityEngine.Application.isPlaying")]
    int _currentWaveBudget;

    List<PropertyCreator> _budgetPool;
    List<float> _budgetFateList;

    public IReadOnlyList<Chess> ActiveFood => _activeFood;

    void OnValidate()
    {
        maxFoodOnField = Mathf.Max(1, maxFoodOnField);
        minFoodOnField = Mathf.Max(0, minFoodOnField);
        if (minFoodOnField > maxFoodOnField)
            minFoodOnField = maxFoodOnField;
    }

    IReadOnlyList<PropertyCreator> FoodPoolSource
    {
        get
        {
            if (foodCreatorsOverride != null && foodCreatorsOverride.Count > 0)
                return foodCreatorsOverride;
            return levelData != null ? levelData.zombieList : null;
        }
    }

    /// <summary>与 <see cref="WaveData.InitWave"/> 中僵尸总价算法一致。</summary>
    public static int ComputeWaveValueBudget(int wave1Based, LevelData data)
    {
        TryComputeWaveValueBudgetDetailed(wave1Based, data, out int finalBudget, out _, out _, out _);
        return finalBudget;
    }

    /// <summary>与 <see cref="ComputeWaveValueBudget"/> 相同，额外输出公式中间量便于 Debug。</summary>
    public static void TryComputeWaveValueBudgetDetailed(int wave1Based, LevelData data, out int finalBudget,
        out int coreValueBeforeX25, out int afterX25BeforeDifficulty, out float difficultyMult)
    {
        finalBudget = 0;
        coreValueBeforeX25 = 0;
        afterX25BeforeDifficulty = 0;
        difficultyMult = 1f;
        if (data == null) return;

        int maxZombieValue;
        if (wave1Based % 10 != 0)
        {
            if (data.createZombieType == CreateZombieType.一类有限制 ||
                data.createZombieType == CreateZombieType.一类无限制)
                maxZombieValue = ((int)((wave1Based - 1) / 3) + data.n) * data.t;
            else
                maxZombieValue = ((int)((wave1Based - 1) * 2 / 5) + data.n) * data.t;
        }
        else
        {
            int flagWaveIndex = wave1Based / 10;
            maxZombieValue = (data.t + 1) * 5 * Mathf.Max(1, flagWaveIndex);
        }

        coreValueBeforeX25 = maxZombieValue;
        afterX25BeforeDifficulty = maxZombieValue * 25;
        difficultyMult = DifficultyManager.GetZombieMultiplier();
        finalBudget = Mathf.RoundToInt(afterX25BeforeDifficulty * difficultyMult);
    }

    public override void CreateZombieWaves()
    {
        if (zombies == null) zombies = new List<Chess>();
        else zombies.Clear();
        if (waveDatas == null) waveDatas = new List<WaveData>();
        else waveDatas.Clear();

        if (levelData == null)
        {
            Debug.LogError("[LevelController_Snake] 没有关卡数据");
            t = 0;
            currentWave = -1;
            return;
        }

        // 不在此预跑全部波的 InitWave；僵尸在每波 BeginFoodWave 时动态 new WaveData + InitWave + EnterWave（与普通关总价/随机逻辑一致）

        t = 0;
        currentWave = -1;
    }

    public override void GameStart()
    {
        base.GameStart();
        StopFoodLoop();
        SpawnSnakeHeadIfConfigured();
        if (!paceFoodByWave && foodSpawnInterval > 0f && GameManage.instance != null && GameManage.instance.timerManage != null)
            _foodTimer = GameManage.instance.timerManage.AddTimer(OnFoodSpawnTick, foodSpawnInterval, true);
    }

    void SpawnSnakeHeadIfConfigured()
    {
        SnakeGridController driver = snakeDriver != null ? snakeDriver : FindObjectOfType<SnakeGridController>();
        if (driver == null)
        {
            if (snakeHeadCreator != null)
                Debug.LogWarning("[LevelController_Snake] 配置了蛇头 Creator 但场景内没有 SnakeGridController");
            return;
        }

        if (snakeHeadCreator == null)
        {
            if (driver.head != null)
                driver.AssignHeadFromLevel(driver.head);
            return;
        }

        if (driver.head != null)
        {
            Debug.LogWarning("[LevelController_Snake] 已存在蛇头，不会重复 Create；请去掉场景里的头或清空 snakeHeadCreator");
            driver.AssignHeadFromLevel(driver.head);
            return;
        }

        Tile tile = ResolveHeadSpawnTile();
        if (tile == null)
        {
            Debug.LogError("[LevelController_Snake] 无法为蛇头找到空 Tile");
            return;
        }

        Chess h = ChessTeamManage.Instance.CreateChess(snakeHeadCreator, tile, snakeHeadTeamTag);
        tile.PlantChess(h);
        driver.AssignHeadFromLevel(h);
    }

    Tile ResolveHeadSpawnTile()
    {
        var map = MapManage.instance;
        if (map == null || map.tiles == null) return null;

        if (headSpawnGrid.x >= 0 && headSpawnGrid.y >= 0 &&
            map.IfInMapRange(headSpawnGrid.x, headSpawnGrid.y))
        {
            Tile t = map.tiles[headSpawnGrid.x, headSpawnGrid.y];
            if (t != null && IsTileFreeForSnakeSpawn(t)) return t;
        }

        int xMin = excludeLeftmostColumn ? 1 : 0;
        for (int x = xMin; x < map.mapSize.x; x++)
        {
            for (int y = 0; y < map.mapSize.y; y++)
            {
                if (!map.IfInMapRange(x, y)) continue;
                Tile tile = map.tiles[x, y];
                if (tile != null && IsTileFreeForSnakeSpawn(tile))
                    return tile;
            }
        }

        return null;
    }

    static bool IsTileFreeForSnakeSpawn(Tile tile)
    {
        if (tile.stander != null) return false;
        if (tile.chessesIntile == null) return true;
        for (int i = 0; i < tile.chessesIntile.Count; i++)
        {
            if (tile.chessesIntile[i] != null && !tile.chessesIntile[i].IfDeath)
                return false;
        }
        return true;
    }

    void StopFoodLoop()
    {
        _foodTimer?.Stop();
        _foodTimer = null;
    }

    void OnFoodSpawnTick()
    {
        if (paceFoodByWave) return;
        if (LevelManage.instance == null || !LevelManage.instance.IfGameStart) return;
        if (currentWave < 0) return;
        TrySpawnFoodsIfNeeded();
    }

    protected override void Update()
    {
        if (levelData == null || !LevelManage.instance.IfGameStart) return;

        if (currentWave < levelData.MaxWave)
        {
            t += Time.deltaTime;
            if (currentWave == -1 && t > mintime)
            {
                SaveSystem.SaveCurrentLevel();
                EventController.Instance.TriggerEvent(EventName.FirstZombieComming.ToString());
                UIManage.GetView<TextPanel>()?.FirstZombieCom();
                mintime = Random.Range(0, 6);
                maxtime = mintime + 23;
                currentWave = 0;
                t = 0;
                UIManage.Show<ProgressBar>();
                UIManage.GetView<ProgressBar>()?.SetFlag(levelData.MaxWave / 10);
                UIManage.GetView<ProgressBar>()?.MoveBar(1, levelData.MaxWave);
                BeginFoodWave(1);
            }
            else if (currentWave >= 0)
            {
                bool canAdvance = paceFoodByWave ? SnakeWaveCanAdvance() : (t > maxtime);
                if (!canAdvance) { }
                else
                {
                    if (currentWave >= levelData.MaxWave - 1)
                    {
                        NotifySnakeVictory();
                        return;
                    }

                    if (currentWave < levelData.MaxWave - 1)
                        SaveSystem.SaveCurrentLevel();

                    t = -2f;
                    UIManage.GetView<ProgressBar>()?.MoveBar(currentWave + 1, levelData.MaxWave);
                    currentWave++;
                    if (currentWave % 10 == 9)
                    {
                        mintime = 4f;
                        maxtime = 50f;
                    }
                    else
                    {
                        mintime = 4f;
                        maxtime = Random.Range(0, 6) + 23f;
                    }

                    t = 0f;
                    BeginFoodWave(currentWave + 1);
                }
            }
        }
    }

    /// <summary>对齐 <see cref="LevelController.WaveCanAdvance"/>：用「场上食物清空」代替「僵尸死完」。</summary>
    bool SnakeWaveCanAdvance()
    {
        if (levelData == null || currentWave < 0) return false;
        _activeFood.RemoveAll(c => c == null || c.IfDeath);
        bool foodCleared = _activeFood.Count == 0;
        int wave1Based = currentWave + 1;

        if (wave1Based % 10 == 9)
            return foodCleared && t > mintime;

        return (foodCleared && t > mintime) || (t > maxtime);
    }

    void BeginFoodWave(int wave1Based)
    {
        TryComputeWaveValueBudgetDetailed(wave1Based, levelData, out _currentWaveBudget,
            out int coreBefore25, out int after25, out float diffMult);
        BuildBudgetPool(wave1Based, out _budgetPool, out _budgetFateList);
        if (_budgetPool == null || _budgetPool.Count == 0)
            Debug.LogWarning($"[LevelController_Snake] 第 {wave1Based} 波没有可用食物（检查 waveLimit 与列表）");
        _activeFood.RemoveAll(c => c == null);

        Debug.Log(
            $"[LevelController_Snake] 第{wave1Based}波 食物预算 " +
            $"core(×25前)={coreBefore25} ×25后={after25} ×难度={diffMult:F3} => 最终_currentWaveBudget={_currentWaveBudget} | " +
            $"LevelData n={levelData?.n} t={levelData?.t} createType={levelData?.createZombieType}");

        if (paceFoodByWave)
            SpawnWaveFoodFromBudget();
        else
            TrySpawnFoodBurstTowardMin();

        ApplySnakeWaveMoveSpeedBuff(wave1Based);
        TrySpawnTombstonesForWave(wave1Based);
        SpawnZombiesForFoodWave(wave1Based);
    }

    /// <summary>
    /// 与 <see cref="WaveData.InitWave"/> / <see cref="WaveData.EnterWave"/> 相同逻辑：本波总价、稀有度随机、生成位置（preTiles）与普通关卡一致。
    /// 使用运行时复制的 <see cref="LevelData"/>，其中 <see cref="LevelData.zombieList"/> 来自 <see cref="snakeWaveZombieCreatorsOverride"/>（优先），避免与「食物」共用同一列表。
    /// </summary>
    void SpawnZombiesForFoodWave(int wave1Based)
    {
        if (!spawnZombiesWithFoodWave || levelData == null || wave1Based < 1) return;
        LevelData data = BuildLevelDataForSnakeWaveSpawn();
        var wd = new WaveData();
        wd.InitWave(wave1Based, data);
        if (Application.isPlaying)
            Object.Destroy(data);
        wd.EnterWave();
    }

    /// <summary>
    /// 复制关卡 n/t/CreateZombieType/MaxWave，僵尸列表优先用 <see cref="snakeWaveZombieCreatorsOverride"/>。
    /// </summary>
    LevelData BuildLevelDataForSnakeWaveSpawn()
    {
        var d = ScriptableObject.CreateInstance<LevelData>();
        d.n = levelData.n;
        d.t = levelData.t;
        d.MaxWave = levelData.MaxWave;
        d.createZombieType = levelData.createZombieType;
        d.outcome = levelData.outcome;
        if (snakeWaveZombieCreatorsOverride != null && snakeWaveZombieCreatorsOverride.Count > 0)
            d.zombieList = new List<PropertyCreator>(snakeWaveZombieCreatorsOverride);
        else if (levelData.zombieList != null)
            d.zombieList = new List<PropertyCreator>(levelData.zombieList);
        else
            d.zombieList = new List<PropertyCreator>();
        return d;
    }

    /// <summary>
    /// 参考 <see cref="LevelController_HammerZombie.CreateStone"/>：在空地上生成墓碑 Chess，排除蛇身、场上食物与已有墓碑。
    /// </summary>
    void TrySpawnTombstonesForWave(int wave1Based)
    {
        if (tombstoneCreator == null || tombstoneEveryNWaves <= 0 || tombstonesPerSpawn <= 0)
            return;
        if (wave1Based < 1 || wave1Based % tombstoneEveryNWaves != 0)
            return;

        var positions = GetTombstoneRandomPositions(tombstonesPerSpawn);
        for (int i = 0; i < positions.Count; i++)
            SpawnTombstoneAt(positions[i]);
    }

    void SpawnTombstoneAt(Vector2Int grid)
    {
        var map = MapManage.instance;
        if (map == null || !map.IfInMapRange(grid.x, grid.y)) return;
        Tile tile = map.tiles[grid.x, grid.y];
        if (tile == null) return;

        Chess c = ChessTeamManage.Instance.CreateChess(tombstoneCreator, tile, tombstoneTeamTag);
        tile.PlantChess(c);
        _tombstones.Add(c);
        c.OnRemove.AddListener(OnTombstoneRemoved);
    }

    void OnTombstoneRemoved(Chess chess)
    {
        if (chess != null)
            _tombstones.Remove(chess);
    }

    /// <summary>与 Hammer 的 <see cref="LevelController_HammerZombie.GetRandomPositions"/> 类似，并排除蛇身/食物/墓碑。</summary>
    List<Vector2Int> GetTombstoneRandomPositions(int n)
    {
        var map = MapManage.instance;
        if (map == null || map.tiles == null || n <= 0)
            return new List<Vector2Int>();

        var snakeCells = new HashSet<Vector2Int>();
        SnakeGridController drv = snakeDriver != null ? snakeDriver : FindObjectOfType<SnakeGridController>();
        drv?.CopySnakeOccupiedCellsTo(snakeCells);

        var blocked = new HashSet<Vector2Int>();
        for (int i = 0; i < _activeFood.Count; i++)
        {
            Chess f = _activeFood[i];
            if (f == null || f.IfDeath) continue;
            Tile st = f.moveController?.standTile;
            if (st != null)
                blocked.Add(st.mapPos);
        }

        for (int i = 0; i < _tombstones.Count; i++)
        {
            Chess tb = _tombstones[i];
            if (tb == null || tb.IfDeath) continue;
            Tile st = tb.moveController?.standTile;
            if (st != null)
                blocked.Add(st.mapPos);
        }

        int xMin = Mathf.Clamp(tombstoneMinGridX, 0, Mathf.Max(0, map.mapSize.x - 1));
        // 不刷最右一列 x=mapSize-1；y 仍用满高度 0..mapSize.y-1
        int xMaxExclusive = Mathf.Max(0, map.mapSize.x - 1);
        var all = new List<Vector2Int>();
        for (int x = xMin; x < xMaxExclusive; x++)
        {
            for (int y = 0; y < map.mapSize.y; y++)
            {
                if (!map.IfInMapRange(x, y)) continue;
                var cell = new Vector2Int(x, y);
                if (snakeCells.Count > 0 && snakeCells.Contains(cell)) continue;
                if (blocked.Contains(cell)) continue;
                Tile tile = map.tiles[x, y];
                if (tile == null || !IsTileFreeForFood(tile)) continue;
                all.Add(cell);
            }
        }

        for (int i = 0; i < all.Count; i++)
        {
            int j = UnityEngine.Random.Range(i, all.Count);
            (all[i], all[j]) = (all[j], all[i]);
        }

        n = Mathf.Min(n, all.Count);
        return all.Count == 0 ? new List<Vector2Int>() : all.GetRange(0, n);
    }

    /// <summary>
    /// 按波次提高蛇头移速：对 <see cref="Buff_BaseValueBuff_AcceleRate"/> 使用固定 buffName，重复进入波次时 <see cref="BuffController.AddBuff"/> 会 BuffReset 升档。
    /// </summary>
    void ApplySnakeWaveMoveSpeedBuff(int wave1Based)
    {
        if (!snakeSpeedScaleByWave || snakeAcceleRatePerWave <= 0f || wave1Based < 1) return;

        SnakeGridController drv = snakeDriver != null ? snakeDriver : FindObjectOfType<SnakeGridController>();
        if (drv == null || drv.head == null || drv.head.IfDeath || drv.head.buffController == null) return;

        float rate = (wave1Based - 1) * snakeAcceleRatePerWave;
        if (rate <= 0f) return;

        var buff = new Buff_BaseValueBuff_AcceleRate { rate = rate };
        buff.buffName = SnakeWaveMoveSpeedBuffName;
        drv.head.buffController.AddBuff(buff);
    }

    /// <summary>与 WaveData.InitWave 相同思路：在剩余预算内反复随机类型并生成，直到预算用尽或放不下。</summary>
    void SpawnWaveFoodFromBudget()
    {
        _activeFood.RemoveAll(c => c == null || c.IfDeath);
        if (_budgetPool == null || _budgetFateList == null || _budgetPool.Count == 0) return;

        int remaining = _currentWaveBudget;
        int guard = 0;
        while (remaining > 0 && _activeFood.Count < maxFoodOnField && guard++ < 500)
        {
            PropertyCreator creator = PickCreatorForBudget(_budgetPool, _budgetFateList, remaining);
            if (creator == null) break;
            int price = creator.baseProperty.price;
            if (price <= 0) break;
            if (!TrySpawnFoodChess(creator)) break;
            remaining -= price;
        }

        int fieldSum = GetFieldFoodValueSum();
        Debug.Log(
            $"[LevelController_Snake] 本波已生成食物 场上总价={fieldSum} 个数={_activeFood.Count} 剩余未分配预算={remaining}");
    }

    void BuildBudgetPool(int wave1Based, out List<PropertyCreator> pool, out List<float> fateList)
    {
        pool = new List<PropertyCreator>();
        fateList = new List<float>();
        var src = FoodPoolSource;
        if (src == null || src.Count == 0) return;

        int raritySum = 0;
        if (levelData.createZombieType == CreateZombieType.一类有限制 ||
            levelData.createZombieType == CreateZombieType.二类有限制)
        {
            for (int i = 0; i < src.Count; i++)
            {
                var c = src[i];
                if (c == null) continue;
                if (c.baseProperty.waveLimit <= wave1Based)
                {
                    pool.Add(c);
                    raritySum += c.baseProperty.rarity;
                }
            }
        }
        else
        {
            for (int i = 0; i < src.Count; i++)
            {
                if (src[i] == null) continue;
                pool.Add(src[i]);
                raritySum += src[i].baseProperty.rarity;
            }
        }

        if (pool.Count == 0 || raritySum <= 0) return;

        fateList.Add((float)pool[0].baseProperty.rarity / raritySum);
        for (int i = 1; i < pool.Count; i++)
            fateList.Add(fateList[i - 1] + (float)pool[i].baseProperty.rarity / raritySum);
    }

    int GetFieldFoodValueSum()
    {
        int sum = 0;
        for (int i = 0; i < _activeFood.Count; i++)
        {
            Chess c = _activeFood[i];
            if (c == null || c.IfDeath) continue;
            var cr = c.propertyController?.creator;
            if (cr != null)
                sum += cr.baseProperty.price;
        }

        return sum;
    }

    /// <summary>与 InitWave 内按剩余预算挑选僵尸相同的逻辑。</summary>
    static PropertyCreator PickCreatorForBudget(List<PropertyCreator> pool, List<float> fateList, int remaining)
    {
        if (pool == null || pool.Count == 0) return null;
        if (remaining <= 0) return null;

        float fate = Random.Range(0f, 1f);
        int picked = -1;
        for (int i = 0; i < fateList.Count; i++)
        {
            if (fate < fateList[i])
            {
                picked = i;
                break;
            }
        }

        if (picked < 0) picked = pool.Count - 1;

        int price = pool[picked].baseProperty.price;
        if (price > remaining)
        {
            int cheapest = int.MaxValue;
            picked = -1;
            for (int i = 0; i < pool.Count; i++)
            {
                int p = pool[i].baseProperty.price;
                if (p <= remaining && p < cheapest)
                {
                    cheapest = p;
                    picked = i;
                }
            }
        }

        if (picked < 0 || pool[picked].baseProperty.price > remaining) return null;
        return pool[picked];
    }

    /// <summary>进波时尽快把数量补到下限（每帧多次尝试，有格子上限）。</summary>
    void TrySpawnFoodBurstTowardMin()
    {
        int guard = 0;
        while (_activeFood.Count < minFoodOnField && _activeFood.Count < maxFoodOnField && guard++ < 48)
        {
            if (!TrySpawnFoodsIfNeededInternal())
                break;
        }
    }

    void TrySpawnFoodsIfNeeded()
    {
        TrySpawnFoodsIfNeededInternal();
    }

    /// <returns>是否成功生成 1 个食物</returns>
    bool TrySpawnFoodsIfNeededInternal()
    {
        if (paceFoodByWave) return false;

        _activeFood.RemoveAll(c => c == null || c.IfDeath);
        if (_budgetPool == null || _budgetFateList == null || _budgetPool.Count == 0) return false;

        if (_activeFood.Count >= maxFoodOnField) return false;

        int sum = GetFieldFoodValueSum();
        if (useWaveValueBudgetCap && sum >= _currentWaveBudget && _activeFood.Count >= minFoodOnField)
            return false;

        int remaining = useWaveValueBudgetCap
            ? Mathf.Max(0, _currentWaveBudget - sum)
            : int.MaxValue / 4;

        PropertyCreator creator = PickCreatorForBudget(_budgetPool, _budgetFateList, remaining);
        if (creator == null) return false;

        return TrySpawnFoodChess(creator);
    }

    bool TrySpawnFoodChess(PropertyCreator creator)
    {
        var map = MapManage.instance;
        if (map == null || creator == null) return false;

        var positions = SampleRandomEmptyCells(1);
        if (positions.Count == 0) return false;

        Vector2Int p = positions[0];
        Tile tile = map.tiles[p.x, p.y];
        if (tile == null) return false;

        Chess food = ChessTeamManage.Instance.CreateChess(creator, tile, foodTeamTag);
        tile.PlantChess(food);
        _activeFood.Add(food);
        food.OnRemove.AddListener(OnFoodRemoved);
        EventController.Instance.TriggerEvent(EventName.SnakeFoodSpawned.ToString(), SnakeGameEventId.FoodSpawned);
        return true;
    }

    List<Vector2Int> SampleRandomEmptyCells(int n)
    {
        var all = new List<Vector2Int>();
        var map = MapManage.instance;
        if (map == null) return all;

        HashSet<Vector2Int> snakeCells = null;
        SnakeGridController drv = snakeDriver != null ? snakeDriver : FindObjectOfType<SnakeGridController>();
        if (drv != null)
        {
            snakeCells = new HashSet<Vector2Int>();
            drv.CopySnakeOccupiedCellsTo(snakeCells);
        }

        int xMin = excludeLeftmostColumn ? 1 : 0;
        // 不刷最右一列 x=mapSize-1；y 仍用满高度（含最上一行）0..mapSize.y-1
        int xMaxExclusive = Mathf.Max(0, map.mapSize.x - 1);
        for (int x = xMin; x < xMaxExclusive; x++)
        {
            for (int y = 0; y < map.mapSize.y; y++)
            {
                if (!map.IfInMapRange(x, y)) continue;
                var cell = new Vector2Int(x, y);
                if (snakeCells != null && snakeCells.Contains(cell)) continue;
                Tile tile = map.tiles[x, y];
                if (tile == null) continue;
                if (!IsTileFreeForFood(tile)) continue;
                all.Add(cell);
            }
        }

        for (int i = 0; i < all.Count; i++)
        {
            int j = Random.Range(i, all.Count);
            (all[i], all[j]) = (all[j], all[i]);
        }

        int take = Mathf.Min(n, all.Count);
        if (take <= 0) return new List<Vector2Int>();
        return all.GetRange(0, take);
    }

    static bool IsTileFreeForFood(Tile tile)
    {
        if (tile.stander != null) return false;
        if (tile.chessesIntile != null)
        {
            for (int i = 0; i < tile.chessesIntile.Count; i++)
            {
                if (tile.chessesIntile[i] != null && !tile.chessesIntile[i].IfDeath)
                    return false;
            }
        }

        return true;
    }

    /// <summary>吃掉食物后调用：摘掉引用并继续往本波预算补刷。</summary>
    public void NotifyFoodEaten(Chess food)
    {
        if (food != null)
        {
            food.OnRemove.RemoveListener(OnFoodRemoved);
            _activeFood.Remove(food);
        }

        if (!paceFoodByWave)
            TrySpawnFoodsIfNeeded();
    }

    void OnFoodRemoved(Chess c)
    {
        _activeFood.Remove(c);
    }

    public void NotifySnakeVictory()
    {
        if (LevelManage.instance != null && LevelManage.instance.IfGameStart)
            LevelManage.instance.GameOver(true);
    }

    /// <summary>
    /// 原立即失败；现改为对蛇头施加 <see cref="DizznessBuff"/>（与属性眩晕时间、<see cref="DizzinessState"/> 一致）。
    /// </summary>
    /// <param name="snake">蛇头；可空则从 <see cref="SnakeGridController.head"/> 解析</param>
    public void NotifySnakeDefeat(Chess snake = null)
    {
        if (LevelManage.instance == null || !LevelManage.instance.IfGameStart) return;

        if (snake == null)
        {
            SnakeGridController drv = snakeDriver != null ? snakeDriver : FindObjectOfType<SnakeGridController>();
            snake = drv != null ? drv.head : null;
        }

        if (snake == null || snake.IfDeath || snake.buffController == null) return;

        var buff = new DizznessBuff
        {
            buffName = SnakeDefeatStunBuffName,
            continueTime = Mathf.Max(0.05f, snakeDefeatStunDuration)
        };
        snake.buffController.AddBuff(buff);
    }

    public override void GameOver(bool win)
    {
        StopFoodLoop();
        SnakeGridController drv = snakeDriver != null ? snakeDriver : FindObjectOfType<SnakeGridController>();
        drv?.ClearSnakeForLevelEnd();
        ClearAllTombstones();
        ClearAllFood();
        _budgetPool = null;
        _budgetFateList = null;
        base.GameOver(win);
    }

    void ClearAllTombstones()
    {
        for (int i = 0; i < _tombstones.Count; i++)
        {
            Chess c = _tombstones[i];
            if (c == null) continue;
            c.OnRemove.RemoveListener(OnTombstoneRemoved);
            if (!c.IfDeath) c.Death();
        }

        _tombstones.Clear();
    }

    void ClearAllFood()
    {
        for (int i = 0; i < _activeFood.Count; i++)
        {
            Chess c = _activeFood[i];
            if (c == null) continue;
            c.OnRemove.RemoveListener(OnFoodRemoved);
            if (!c.IfDeath) c.Death();
        }

        _activeFood.Clear();
    }
}
