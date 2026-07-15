using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 章鱼噼消耗品：按宿主压力消灭僵尸（霸凌者优先，Boss 免疫），并清除宿主压力 / 霸凌目标 Buff。
/// </summary>
public class SkillEffect_Takopi : ISkillEffect
{
    public const string StressBuffName = "压力";
    public const string BullyTargetBuffName = "霸凌目标";
    public const int StressPerKill = 5;

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        if (user?.moveController?.standTile == null)
            return;

        Chess host = user.moveController.standTile.stander;
        if (host == null || host.IfDeath)
            return;

        int stress = ReadStress(host);
        int killBudget = Mathf.Max(1, stress / StressPerKill);
        EliminateZombies(user, killBudget);
        ClearHostBuffs(host);
        ClearZombieBullyContextFor(host);
    }

    static int ReadStress(Chess host)
    {
        if (host?.skillController?.context == null)
            return 0;
        return host.skillController.context.TryGet<int>("stress", out int stress)
            ? Mathf.Max(0, stress)
            : 0;
    }

    static void EliminateZombies(Chess user, int killBudget)
    {
        var teamManage = GameManage.instance?.chessTeamManage;
        if (teamManage == null || killBudget <= 0)
            return;

        var enemies = teamManage.GetEnemyTeam(user.tag);
        if (enemies == null || enemies.Count == 0)
            return;

        var eligible = new List<Chess>();
        var bullies = new List<Chess>();
        for (int i = 0; i < enemies.Count; i++)
        {
            Chess enemy = enemies[i];
            if (!ZombieEliminationRules.IsEligibleEliminationTarget(enemy))
                continue;
            eligible.Add(enemy);
            if (IsBully(enemy))
                bullies.Add(enemy);
        }

        int remaining = Mathf.Min(killBudget, eligible.Count);
        for (int k = 0; k < remaining; k++)
        {
            Chess target = PickTarget(bullies, eligible);
            if (target == null)
                break;

            eligible.Remove(target);
            bullies.Remove(target);
            if (!target.IfDeath)
                target.Death();
        }
    }

    static Chess PickTarget(List<Chess> bullies, List<Chess> eligible)
    {
        if (bullies.Count > 0)
            return bullies[Random.Range(0, bullies.Count)];
        if (eligible.Count == 0)
            return null;
        return eligible[Random.Range(0, eligible.Count)];
    }

    static bool IsBully(Chess chess) =>
        chess?.propertyController?.creator != null
        && chess.propertyController.creator.chessName == PassiveSkill_Clinger.BullyChessName;

    static void ClearHostBuffs(Chess host)
    {
        if (host?.buffController == null)
            return;

        TryOverBuffByName(host, StressBuffName);
        TryOverBuffByName(host, BullyTargetBuffName);
        host.skillController?.context?.Set<int>("stress", 0);
    }

    static void TryOverBuffByName(Chess host, string buffName)
    {
        if (host.buffController.buffDic != null
            && host.buffController.buffDic.TryGetValue(buffName, out Buff buff))
        {
            host.buffController.TryOverBuff(buff);
        }
    }

    static void ClearZombieBullyContextFor(Chess host)
    {
        var teamManage = GameManage.instance?.chessTeamManage;
        if (teamManage == null || host == null)
            return;

        var enemies = teamManage.GetEnemyTeam(host.tag);
        if (enemies == null)
            return;

        for (int i = 0; i < enemies.Count; i++)
        {
            Chess zombie = enemies[i];
            if (zombie?.skillController?.context == null)
                continue;
            if (zombie.skillController.context.TryGet<Chess>("霸凌目标", out Chess marked)
                && marked == host)
            {
                zombie.skillController.context.Remove("霸凌目标");
            }
        }
    }
}
