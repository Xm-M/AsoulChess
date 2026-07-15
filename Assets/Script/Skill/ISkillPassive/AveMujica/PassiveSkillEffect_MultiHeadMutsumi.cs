using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 多首的怪物被动：外部压力每满阈值清零并生成无实体分身头；
/// 致命伤有分身时吞一头并 Resume（对齐 Mortis）；压力紫砂（stress &gt; stressLimit）除外，直接真死。
/// 攻击由 <see cref="MultiHeadShootAttack"/> 负责。
/// 当前 <c>stressLimit</c> 相对入场 max / min(出头阈值) 落入两档分界时，
/// 通过 <see cref="AnimatorController.ChangeFloat(float)"/> 设置 Blend（0/1/2）切换三套动画；
/// 主动技终局后 Blend=3。
/// </summary>
public class PassiveSkillEffect_MultiHeadMutsumi : ISkillEffect
{
    [SerializeReference]
    public Buff stressBuff;

    [SerializeReference]
    public Buff ResumeBuff;

    [Tooltip("分身头 Prefab（Sprite+Animator，无 Collider/Chess）；可空则只计数不显示")]
    public GameObject headPrefab;

    [Min(1)]
    [Tooltip("软上限；硬上限由压力死亡阈值递减实现（约 20）")]
    public int maxClones = MultiHeadMutsumiKeys.DefaultMaxClones;

    [Min(1)]
    [Tooltip("攒满多少压力生成 1 个分身并清零")]
    public int stressPerClone = MultiHeadMutsumiKeys.DefaultStressPerClone;

    [Min(1)]
    [Tooltip("入场写入「压力」Buff.stressLimit；145 且每头 -5 → 20 头后为 45")]
    public int initialStressLimit = MultiHeadMutsumiKeys.DefaultInitialStressLimit;

    [Min(0)]
    [Tooltip("每成功增加一个分身，压力死亡阈值 -N")]
    public int stressLimitReducePerClone = MultiHeadMutsumiKeys.DefaultStressLimitReducePerClone;

    [Min(0)]
    [Tooltip("槽位数公式分母侧：n=(initialStressLimit-本值)/每次减 limit；默认 45（20 头后阈值）")]
    public int headSlotLimitFloor = MultiHeadMutsumiKeys.DefaultHeadSlotLimitFloor;

    [Tooltip("分身左右总范围（相对本体本地 X）；均分为 n 槽后随机填空槽")]
    public Vector2 headLocalXRange = new Vector2(-0.35f, 0.35f);

    [Tooltip("若武器仍是 ShootBullet，入场时替换为 MultiHeadShootAttack（跨行弹请在 Prefab 上配 crossRowBullet）")]
    public bool wrapShootBulletAttack = true;

    [Min(0.05f)]
    [Tooltip("分身动画与本体状态同步轮询间隔（秒）；仅在 idle↔attack 切换或本体重播 attack 时触发 Play")]
    public float headAnimSyncInterval = 0.08f;

    Chess _user;
    UnityAction _onStress;
    UnityAction<DamageMessege> _onGetDamage;
    UnityAction<Chess> _onRemove;
    bool _processingStress;
    IAttackFunction _savedAttack;
    int _lastAnimTier = -1;
    /// <summary>尚未占用的头槽下标 0..n-1。</summary>
    readonly List<int> _freeHeadSlots = new List<int>(24);
    /// <summary>与 CloneHeads 平行：每个头占用的槽下标。</summary>
    readonly List<int> _occupiedHeadSlots = new List<int>(24);
    Timer _headAnimSyncTimer;
    string _lastSyncedHeadAnim;
    float _lastMasterAttackNormTime = -1f;

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        if (user == null) return;
        _user = user;
        _lastAnimTier = -1;
        _lastSyncedHeadAnim = null;
        _lastMasterAttackNormTime = -1f;

        Buff buff = stressBuff != null ? stressBuff.Clone() : new Buff_StressBuff_Death();
        if (buff is Buff_StressBuff_Death stressDeath)
            stressDeath.stressLimit = initialStressLimit;
        user.buffController.AddBuff(buff);

        MultiHeadMutsumiKeys.SetCloneCount(user, 0);
        MultiHeadMutsumiKeys.GetOrCreateHeadList(user)?.Clear();
        RebuildFreeHeadSlots();

        TryWrapAttack(user);
        RefreshAnimTierByStressLimit();
        StartHeadAnimSync();

        _onStress = OnStressContextChanged;
        user.skillController.context.AddEvent(_onStress);

        _onGetDamage = OnGetDamage;
        user.propertyController.onGetDamage.AddListener(_onGetDamage);

        _onRemove = OnRemove;
        user.OnRemove.AddListener(_onRemove);
    }

    void TryWrapAttack(Chess user)
    {
        if (!wrapShootBulletAttack) return;
        var weapon = user.equipWeapon?.weapon as Weapon_Sample;
        if (weapon == null) return;
        if (weapon.attackFunction is MultiHeadShootAttack)
            return;

        GameObject bulletPrefab = null;
        if (weapon.attackFunction is ShootBullet sb)
            bulletPrefab = sb.bullet;

        _savedAttack = weapon.attackFunction;
        weapon.attackFunction = new MultiHeadShootAttack
        {
            bullet = bulletPrefab,
            crossRowBullet = null
        };
    }

    void OnStressContextChanged()
    {
        if (_processingStress || _user == null || _user.IfDeath)
            return;
        if (_user.skillController?.context == null)
            return;
        if (!_user.skillController.context.TryGet(MultiHeadMutsumiKeys.StressThreshold, out int stress))
            return;
        if (stress < stressPerClone)
            return;

        // 主动技终局：不再出头（压力仍可涨至紫砂）
        if (MultiHeadMutsumiKeys.IsFinaleLocked(_user))
            return;

        // 阈值已被分身压到低于当前压力（如 20 头后 limit=45、stress=50）→ 不吞压力、不加工，交给 Buff 紫砂
        if (TryGetStressDeathBuff(out Buff_StressBuff_Death deathBuff) && stress > deathBuff.stressLimit)
            return;

        _processingStress = true;
        try
        {
            _user.skillController.context.Set(MultiHeadMutsumiKeys.StressThreshold, 0);
            TryAddClone();
        }
        finally
        {
            _processingStress = false;
        }
    }

    void TryAddClone()
    {
        if (MultiHeadMutsumiKeys.IsFinaleLocked(_user))
            return;

        int count = MultiHeadMutsumiKeys.GetCloneCount(_user);
        int cap = Mathf.Max(1, maxClones);
        if (count >= cap)
            return;

        count++;
        MultiHeadMutsumiKeys.SetCloneCount(_user, count);
        SpawnHeadVisual();
        ReduceStressDeathLimit();
    }

    void ReduceStressDeathLimit()
    {
        if (MultiHeadMutsumiKeys.IsFinaleLocked(_user))
            return;
        if (stressLimitReducePerClone <= 0)
            return;
        if (!TryGetStressDeathBuff(out Buff_StressBuff_Death deathBuff))
            return;
        deathBuff.stressLimit = Mathf.Max(0, deathBuff.stressLimit - stressLimitReducePerClone);
        RefreshAnimTierByStressLimit();
    }

    /// <summary>
    /// max=<see cref="initialStressLimit"/>，min=<see cref="stressPerClone"/>；
    /// limit &gt; 高档线 → 0；低档线 &lt; limit ≤ 高 → 1；limit ≤ 低 → 2。
    /// 主动技终局后 Blend=3，本方法不再改写。
    /// </summary>
    void RefreshAnimTierByStressLimit()
    {
        if (_user?.animatorController == null)
            return;
        if (MultiHeadMutsumiKeys.IsFinaleLocked(_user))
            return;
        if (!TryGetStressDeathBuff(out Buff_StressBuff_Death deathBuff))
            return;

        float max = Mathf.Max(1, initialStressLimit);
        float min = Mathf.Clamp(stressPerClone, 0, (int)max);
        float span = max - min;
        float high = span * (2f / 3f) + min;
        float low = span * (1f / 3f) + min;

        float limit = deathBuff.stressLimit;
        int tier = 0;
        if (limit <= low)
            tier = 2;
        else if (limit <= high)
            tier = 1;

        if (tier == _lastAnimTier)
            return;
        _lastAnimTier = tier;
        _user.animatorController.ChangeFloat(tier);
    }

    bool TryGetStressDeathBuff(out Buff_StressBuff_Death deathBuff)
    {
        deathBuff = null;
        if (_user?.buffController?.buffDic == null)
            return false;
        if (!_user.buffController.buffDic.TryGetValue("压力", out Buff buff))
            return false;
        deathBuff = buff as Buff_StressBuff_Death;
        return deathBuff != null;
    }

    /// <summary>
    /// n = (initialStressLimit - headSlotLimitFloor) / stressLimitReducePerClone；
    /// 默认 (145-45)/5 = 20。
    /// </summary>
    int GetHeadSlotCount()
    {
        int reduce = Mathf.Max(1, stressLimitReducePerClone);
        int n = (initialStressLimit - headSlotLimitFloor) / reduce;
        return Mathf.Max(1, n);
    }

    void RebuildFreeHeadSlots()
    {
        _freeHeadSlots.Clear();
        _occupiedHeadSlots.Clear();
        int n = GetHeadSlotCount();
        for (int i = 0; i < n; i++)
            _freeHeadSlots.Add(i);
    }

    /// <summary>槽 i 在 [headLocalXRange.x, .y] 上均分后的中心本地 X；Y 与本体对齐（localY=0）。</summary>
    float SlotCenterLocalX(int slotIndex, int slotCount)
    {
        float minX = headLocalXRange.x;
        float maxX = headLocalXRange.y;
        if (slotCount <= 1)
            return (minX + maxX) * 0.5f;
        float t = (slotIndex + 0.5f) / slotCount;
        return Mathf.Lerp(minX, maxX, t);
    }

    int TakeRandomFreeSlot()
    {
        if (_freeHeadSlots.Count > 0)
        {
            int pick = Random.Range(0, _freeHeadSlots.Count);
            int slot = _freeHeadSlots[pick];
            _freeHeadSlots.RemoveAt(pick);
            return slot;
        }
        // 槽已满仍出头（软上限与公式不一致时）：随机叠到某一槽中心
        return Random.Range(0, GetHeadSlotCount());
    }

    void SpawnHeadVisual()
    {
        if (_user == null || headPrefab == null)
            return;

        List<Transform> heads = MultiHeadMutsumiKeys.GetOrCreateHeadList(_user);
        if (heads == null) return;

        int slotCount = GetHeadSlotCount();
        int slot = TakeRandomFreeSlot();
        float x = SlotCenterLocalX(slot, slotCount);

        GameObject go = Object.Instantiate(headPrefab, _user.transform);
        // Y 与本体相同：挂在本体下 localY=0
        go.transform.localPosition = new Vector3(x, 0f, 0f);
        go.transform.localRotation = Quaternion.identity;
        go.SetActive(true);
        heads.Add(go.transform);
        _occupiedHeadSlots.Add(slot);

        // 新生头对齐当前同步态（无则 idle）
        Animator headAnim = go.GetComponent<Animator>();
        if (headAnim != null)
            headAnim.Play(string.IsNullOrEmpty(_lastSyncedHeadAnim) ? "idle" : _lastSyncedHeadAnim, 0, 0f);
    }

    void StartHeadAnimSync()
    {
        StopHeadAnimSync();
        if (GameManage.instance?.timerManage == null)
            return;
        float interval = Mathf.Max(0.05f, headAnimSyncInterval);
        _headAnimSyncTimer = GameManage.instance.timerManage.AddTimer(SyncHeadAnimsFromMaster, interval, true);
        SyncHeadAnimsFromMaster();
    }

    void StopHeadAnimSync()
    {
        if (_headAnimSyncTimer != null)
        {
            _headAnimSyncTimer.Stop();
            _headAnimSyncTimer = null;
        }
    }

    /// <summary>
    /// 精度方案 2：本体进入/重播 attack 或回到非 attack 时，分身 Play 对应状态（不同步 normalizedTime）。
    /// </summary>
    void SyncHeadAnimsFromMaster()
    {
        if (_user == null || _user.IfDeath)
            return;
        if (MultiHeadMutsumiKeys.IsFinaleLocked(_user))
            return;

        Animator masterAnim = _user.animatorController?.animator;
        if (masterAnim == null)
            return;

        AnimatorStateInfo info = masterAnim.GetCurrentAnimatorStateInfo(0);
        bool masterAttacking = info.IsName("attack");
        string desired = masterAttacking ? "attack" : "idle";

        bool shouldPlay = desired != _lastSyncedHeadAnim;
        if (masterAttacking && desired == _lastSyncedHeadAnim)
        {
            // 本体 AttackController 周期 PlayAttack 会重置进度，分身跟着再 Play 一次
            float norm = info.normalizedTime % 1f;
            if (_lastMasterAttackNormTime >= 0f && norm + 0.02f < _lastMasterAttackNormTime)
                shouldPlay = true;
            _lastMasterAttackNormTime = norm;
        }
        else if (!masterAttacking)
        {
            _lastMasterAttackNormTime = -1f;
        }

        if (!shouldPlay)
            return;

        _lastSyncedHeadAnim = desired;
        PlayAllHeadAnims(desired);
    }

    void PlayAllHeadAnims(string stateName)
    {
        List<Transform> heads = MultiHeadMutsumiKeys.GetOrCreateHeadList(_user);
        if (heads == null || heads.Count == 0)
            return;
        for (int i = 0; i < heads.Count; i++)
        {
            Transform t = heads[i];
            if (t == null)
                continue;
            Animator anim = t.GetComponent<Animator>();
            if (anim == null || !anim.isActiveAndEnabled)
                continue;
            anim.Play(stateName, 0, 0f);
        }
    }

    void DestroyOneHeadVisual()
    {
        List<Transform> heads = MultiHeadMutsumiKeys.GetOrCreateHeadList(_user);
        if (heads == null || heads.Count == 0)
            return;

        int last = heads.Count - 1;
        Transform t = heads[last];
        heads.RemoveAt(last);

        if (last < _occupiedHeadSlots.Count)
        {
            int slot = _occupiedHeadSlots[last];
            _occupiedHeadSlots.RemoveAt(last);
            if (!_freeHeadSlots.Contains(slot))
                _freeHeadSlots.Add(slot);
        }

        if (t != null)
            Object.Destroy(t.gameObject);
    }

    void ClearAllHeads()
    {
        MultiHeadMutsumiKeys.ClearAllHeadVisuals(_user);
        RebuildFreeHeadSlots();
    }

    void OnGetDamage(DamageMessege dm)
    {
        Chess user = dm?.damageTo;
        if (user == null || user != _user)
            return;
        if (user.stateController?.currentState?.state?.stateName == StateName.ResumeState)
            return;
        if (user.propertyController.GetHp() > 0)
            return;

        // 压力紫砂（stress > stressLimit）：真死，不吞分身 Resume
        if (IsStressOverLimitSuicide(user))
            return;

        int clones = MultiHeadMutsumiKeys.GetCloneCount(user);
        if (clones <= 0)
            return;

        user.propertyController.ChangeHp(1);
        MultiHeadMutsumiKeys.SetCloneCount(user, clones - 1);
        DestroyOneHeadVisual();
        user.stateController.ChangeState(StateName.ResumeState);
        if (user.GetComponent<AudioPlayer>() is AudioPlayer ap)
            ap.PlaySubN(1);
        if (ResumeBuff != null)
            user.buffController.AddBuff(ResumeBuff.Clone());
    }

    static bool IsStressOverLimitSuicide(Chess user)
    {
        if (user?.buffController?.buffDic == null
            || !user.buffController.buffDic.TryGetValue("压力", out Buff buff)
            || buff is not Buff_StressBuff_Death deathBuff)
            return false;
        if (user.skillController?.context == null
            || !user.skillController.context.TryGet(MultiHeadMutsumiKeys.StressThreshold, out int stress))
            return false;
        return stress > deathBuff.stressLimit;
    }

    void OnRemove(Chess chess)
    {
        StopHeadAnimSync();
        MultiHeadMutsumiKeys.StopStressTickTimer(_user);
        MultiHeadMutsumiKeys.DestroyAllGhosts(_user);
        ClearAllHeads();

        if (_user != null)
        {
            if (_onStress != null)
                _user.skillController?.context?.RemoveEvent(_onStress);
            if (_onGetDamage != null)
                _user.propertyController?.onGetDamage?.RemoveListener(_onGetDamage);
            if (_onRemove != null)
                _user.OnRemove.RemoveListener(_onRemove);

            if (_savedAttack != null && _user.equipWeapon?.weapon is Weapon_Sample w)
                w.attackFunction = _savedAttack;
        }

        _user = null;
        _onStress = null;
        _onGetDamage = null;
        _onRemove = null;
        _savedAttack = null;
        _freeHeadSlots.Clear();
        _occupiedHeadSlots.Clear();
        _lastSyncedHeadAnim = null;
        _lastMasterAttackNormTime = -1f;
    }
}
