using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 雪橇僵尸被动：入场时给予固定 <see cref="Buff_BaseValueBuff_AcceleRate"/>（rate=1，见 <see cref="SledAcceleRateBuffName"/>）；
/// <see cref="ArmorBase.OnArmorBroken"/> 时移除该加速；仅当本体仍存活时按行刷出步行雪橇尸。
/// 离开冰道破甲由 <see cref="Armor_Sled"/> 处理。
/// </summary>
[Serializable]
public class PassiveSkillEffect_SledZombie : ISkillEffect
{
    /// <summary>与入场添加的加速 Buff 同名，用于护甲销毁时 <see cref="BuffController.TryOverBuff"/>。</summary>
    public const string SledAcceleRateBuffName = "雪橇被动移速";

    [Tooltip("破损后额外生成的步行雪橇僵尸（本体算第 4 只，此处通常填 3）")]
    public PropertyCreator footZombieCreator;

    [Min(0)]
    public int extraFootZombieCount = 3;

    [Tooltip("可选。第 i 只额外僵尸在按规则选好 standTile 并 CreateChess 后，将位置重设为列表第 i 个 Transform 的 world position；未填或该项为空则仍用格子上的随机偏移")]
    public List<Transform> footZombieWorldPositions;

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        if (user == null) return;

        bool disbanded = false;
        Armor_Sled armor = null;

        UnityAction<ArmorBase> onArmorBroken = null;
        UnityAction<Chess> onDeath = null;
        UnityAction<Chess> onRemove = null;

        void CleanupSubscriptions()
        {
            if (armor != null && onArmorBroken != null)
                armor.OnArmorBroken.RemoveListener(onArmorBroken);
            if (user != null)
            {
                user.OnRemove.RemoveListener(onRemove);
                if (onDeath != null)
                    user.DeathEvent.RemoveListener(onDeath);
            }
        }

        /// <param name="spawnEvenIfDead">为 true 时表示从 <see cref="Chess.DeathEvent"/> 调用，此时 <see cref="Chess.IfDeath"/> 已为 true，仍需刷步行尸。</param>
        void DisbandSledAndSpawnIfAlive(bool spawnEvenIfDead = false)
        {
            if (disbanded || user == null) return;
            disbanded = true;
            CleanupSubscriptions();

            var speedStub = new Buff_BaseValueBuff_AcceleRate { buffName = SledAcceleRateBuffName };
            user.buffController.TryOverBuff(speedStub);
            Timer t;
            if (extraFootZombieCount > 0 && footZombieCreator != null && (spawnEvenIfDead || !user.IfDeath))
                t=GameManage.instance.timerManage.AddTimer( ()=>SpawnFootZombies(user, footZombieCreator, extraFootZombieCount, footZombieWorldPositions),Time.deltaTime);
        }

        onArmorBroken = _ => DisbandSledAndSpawnIfAlive(false);

        /// <summary>进 <see cref="DeathState"/> 时：若雪橇仍未破甲（例如本体被非甲路径击杀），补一次与破甲相同的召唤，避免亡语丢失。</summary>
        onDeath = _ =>
        {
            if (disbanded || user == null) return;
            if (armor != null && !armor.IsBroken)
                DisbandSledAndSpawnIfAlive(true);
        };

        onRemove = _ => { CleanupSubscriptions(); };

        UnityAction<Chess> onEnter = null;
        onEnter = _ =>
        {
            user.WhenEnterGame.RemoveListener(onEnter);

            armor = user.GetComponentInChildren<Armor_Sled>();
            user.buffController.AddBuff(new Buff_BaseValueBuff_AcceleRate
            {
                rate = 1f,
                buffName = SledAcceleRateBuffName
            });

            //user.UnSelectable();

            if (armor != null)
                armor.OnArmorBroken.AddListener(onArmorBroken);
            user.DeathEvent.AddListener(onDeath);
            user.OnRemove.AddListener(onRemove);
        };

        user.WhenEnterGame.AddListener(onEnter);
    }

    static void SpawnFootZombies(Chess leader, PropertyCreator footCreator, int count, List<Transform> worldPositionOverrides)
    {
        if (leader == null || footCreator == null || count <= 0) return;
        var pvz = MapManage.instance as MapManage_PVZ;
        if (pvz == null || leader.moveController?.standTile == null) return;
        if (leader.IfDeath||leader.propertyController.GetHpPerCent()<=0) return;    
        //List<Tile> preTiles = pvz.preTiles;
        //if (preTiles == null || preTiles.Count == 0) return;

        //Vector2Int row = leader.moveController.standTile.mapPos;
        //var rowTiles = new List<Tile>();
        //for (int i = 0; i < preTiles.Count; i++)
        //{
        //    Tile t = preTiles[i];
        //    if (t == null) continue;
        //    if ((t.tileType & footCreator.chessTileType) == 0) continue;
        //    if (t.mapPos.y == row.y)
        //        rowTiles.Add(t);
        //}
        //if (rowTiles.Count == 0)
        //{
        //    for (int i = 0; i < preTiles.Count; i++)
        //    {
        //        Tile t = preTiles[i];
        //        if (t != null && (t.tileType & footCreator.chessTileType) != 0)
        //            rowTiles.Add(t);
        //    }
        //}
        //if (rowTiles.Count == 0) return;

        for (int k = 0; k < count; k++)
        {
            Tile stand = leader.moveController.nextTile;
            Chess z = ChessTeamManage.Instance.CreateChess(footCreator, stand, "Enemy");
            if (worldPositionOverrides != null && k < worldPositionOverrides.Count && worldPositionOverrides[k] != null)
                z.transform.position = worldPositionOverrides[k].position;
            else
            {
                float dx = UnityEngine.Random.Range(0f, 3.75f);
                z.transform.position = stand.transform.position + Vector3.right * dx;
            }
        }
    }
}
