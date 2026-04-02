using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 雪橇僵尸被动：在 <see cref="Chess.WhenEnterGame"/> 后叠加速、不可选中；
/// 监听 <see cref="MoveController.OnReachTile"/>：落地格无冰则 <see cref="Armor_Sled.BrokenArmor"/>（与被打爆同一条破损链）；
/// 监听 <see cref="ArmorBase.OnArmorBroken"/>：结束加速、恢复可选中、按行刷出步行雪橇尸。
/// 全部逻辑在 <see cref="SkillEffect"/> 内用委托闭包完成，无需额外挂载脚本。
/// </summary>
[Serializable]
public class PassiveSkillEffect_SledZombie : ISkillEffect
{
    [Tooltip("雪橇阶段额外移速（ChangeAcceleRate）；Buff 名用于结束时移除")]
    public Buff_BaseValueBuff_AcceleRate sledSpeedBuff;

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
        string speedBuffName = null;
        Armor_Sled armor = null;

        UnityAction<Chess, Tile> onReach = null;
        UnityAction<ArmorBase> onArmorBroken = null;
        UnityAction<Chess> onRemove = null;

        void CleanupSubscriptions()
        {
            if (user.moveController != null && onReach != null)
                user.moveController.OnReachTile.RemoveListener(onReach);
            if (armor != null && onArmorBroken != null)
                armor.OnArmorBroken.RemoveListener(onArmorBroken);
            if (user != null)
                user.OnRemove.RemoveListener(onRemove);
        }

        onArmorBroken = _ =>
        {
            if (disbanded || user == null || user.IfDeath) return;
            disbanded = true;
            CleanupSubscriptions();

            if (!string.IsNullOrEmpty(speedBuffName))
            {
                var stub = new Buff_BaseValueBuff_AcceleRate { buffName = speedBuffName };
                user.buffController.TryOverBuff(stub);
            }

            user.ResumeSelectable();

            if (extraFootZombieCount > 0 && footZombieCreator != null)
                SpawnFootZombies(user, footZombieCreator, extraFootZombieCount, footZombieWorldPositions);
        };

        onReach = (c, newTile) =>
        {
            if (disbanded || user == null || c != user || newTile == null) return;
            var snow = Effect_Snow.GetInstanceOrNull();
            if (snow != null && snow.HasIceAt(newTile.mapPos)) return;
            if (armor != null && !armor.IsBroken)
                armor.BrokenArmor();
        };

        onRemove = _ => { CleanupSubscriptions(); };

        UnityAction<Chess> onEnter = null;
        onEnter = _ =>
        {
            user.WhenEnterGame.RemoveListener(onEnter);

            armor = user.GetComponentInChildren<Armor_Sled>();
            if (sledSpeedBuff != null)
            {
                var b = (Buff_BaseValueBuff_AcceleRate)sledSpeedBuff.Clone();
                if (string.IsNullOrEmpty(b.buffName))
                    b.buffName = "雪橇移速";
                speedBuffName = b.buffName;
                user.buffController.AddBuff(b);
            }

            user.UnSelectable();

            if (armor != null)
                armor.OnArmorBroken.AddListener(onArmorBroken);
            if (user.moveController != null)
                user.moveController.OnReachTile.AddListener(onReach);
            user.OnRemove.AddListener(onRemove);
        };

        user.WhenEnterGame.AddListener(onEnter);
    }

    static void SpawnFootZombies(Chess leader, PropertyCreator footCreator, int count, List<Transform> worldPositionOverrides)
    {
        if (leader == null || footCreator == null || count <= 0) return;
        var pvz = MapManage.instance as MapManage_PVZ;
        if (pvz == null || leader.moveController?.standTile == null) return;

        List<Tile> preTiles = pvz.preTiles;
        if (preTiles == null || preTiles.Count == 0) return;

        Vector2Int row = leader.moveController.standTile.mapPos;
        var rowTiles = new List<Tile>();
        for (int i = 0; i < preTiles.Count; i++)
        {
            Tile t = preTiles[i];
            if (t == null) continue;
            if ((t.tileType & footCreator.chessTileType) == 0) continue;
            if (t.mapPos.y == row.y)
                rowTiles.Add(t);
        }
        if (rowTiles.Count == 0)
        {
            for (int i = 0; i < preTiles.Count; i++)
            {
                Tile t = preTiles[i];
                if (t != null && (t.tileType & footCreator.chessTileType) != 0)
                    rowTiles.Add(t);
            }
        }
        if (rowTiles.Count == 0) return;

        for (int k = 0; k < count; k++)
        {
            Tile stand = rowTiles[k % rowTiles.Count];
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
