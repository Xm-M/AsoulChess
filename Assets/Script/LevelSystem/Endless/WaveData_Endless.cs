using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 无尽模式波次数据：从 segmentPool 预生成，使用有效稀有度加权。
/// InitWave 的 wave 为<strong>全局波号</strong>（跨轮累计），用于预算与 waveLimit；不是本轮内 1..N。
/// </summary>
public class WaveData_Endless : WaveData
{
    /// <summary>约等于原版每波最多 500 只普僵 × 25 进制价值。</summary>
    public const int MaxZombieValuePerWave = 12500;

    readonly EndlessRunState runState;

    public WaveData_Endless(EndlessRunState runState)
    {
        this.runState = runState;
    }

    public override void InitWave(int wave, LevelData data)
    {
        // wave = 全局波号（例：每轮 10 波、第 3 轮第 1 波 → 21）
        if (runState == null || runState.segmentPool == null || runState.segmentPool.Count == 0)
        {
            base.InitWave(wave, data);
            return;
        }

        int raritySum = 0;
        this.wave = wave;
        outcome = data.outcome;
        if (zombieList == null)
        {
            zombieList = new List<ZombieInWaveData>();
            fateList = new List<float>();
        }
        else
        {
            zombieList.Clear();
            fateList.Clear();
        }

        var pool = runState.segmentPool;
        if (data.createZombieType == CreateZombieType.一类有限制 || data.createZombieType == CreateZombieType.二类有限制)
        {
            for (int i = 0; i < pool.Count; i++)
            {
                if (pool[i].PassesWavePoolFilter(wave))
                {
                    var zombieInWaveData = new ZombieInWaveData
                    {
                        zombieCreate = pool[i],
                        zombieNum = 0
                    };
                    zombieList.Add(zombieInWaveData);
                    raritySum += runState.GetEffectiveRarity(pool[i]);
                }
            }
        }
        else
        {
            for (int i = 0; i < pool.Count; i++)
            {
                var plantType = pool[i].plantType;
                if ((plantType & PlantType.LimitType) != 0 && wave % pool[i].baseProperty.waveLimit != 0)
                    continue;
                var zombieInWaveData = new ZombieInWaveData
                {
                    zombieCreate = pool[i],
                    zombieNum = 0
                };
                zombieList.Add(zombieInWaveData);
                raritySum += runState.GetEffectiveRarity(pool[i]);
            }
        }

        if (zombieList.Count == 0 || raritySum <= 0)
        {
            base.InitWave(wave, data);
            return;
        }

        fateList.Add((float)runState.GetEffectiveRarity(zombieList[0].zombieCreate) / raritySum);
        for (int i = 1; i < zombieList.Count; i++)
        {
            fateList.Add(fateList[i - 1] + (float)runState.GetEffectiveRarity(zombieList[i].zombieCreate) / raritySum);
        }

        maxZombieValue = 0;
        if (wave % 10 != 0)
        {
            if (data.createZombieType == CreateZombieType.一类有限制 || data.createZombieType == CreateZombieType.一类无限制)
                maxZombieValue = ((int)((wave - 1) / 3) + data.n) * data.t;
            else
                maxZombieValue = ((int)((wave - 1) * 2 / 5) + data.n) * data.t;
        }
        else
        {
            int flagWaveIndex = wave / 10;
            maxZombieValue = (data.t + 1) * 5 * Mathf.Max(1, flagWaveIndex);
        }
        maxZombieValue *= 25;
        maxZombieValue = Mathf.RoundToInt(maxZombieValue * DifficultyManager.GetZombieMultiplier());
        if (maxZombieValue > MaxZombieValuePerWave)
            maxZombieValue = MaxZombieValuePerWave;

        int num = 0;
        int targetZombieValue = maxZombieValue;
        bool unlimited = data.createZombieType == CreateZombieType.一类无限制 ||
                         data.createZombieType == CreateZombieType.二类无限制;
        hpmax = 0;

        if (unlimited)
        {
            int spent = 0;
            while (spent < targetZombieValue)
            {
                int pickedIndex = PickByRarity(fateList);
                if (pickedIndex < 0) break;
                int price = zombieList[pickedIndex].zombieCreate.baseProperty.price;
                if (price <= 0) break;
                zombieList[pickedIndex].zombieNum += 1;
                runState.RecordRarityUse(zombieList[pickedIndex].zombieCreate);
                spent += price;
                hpmax += zombieList[pickedIndex].zombieCreate.baseProperty.HpMax;
                num++;
                if (num > 500) break;
            }
        }
        else
        {
            while (maxZombieValue > 0)
            {
                int pickedIndex = PickByRarity(fateList);
                if (pickedIndex < 0) break;
                int price = zombieList[pickedIndex].zombieCreate.baseProperty.price;
                if (price > maxZombieValue)
                {
                    int cheapestPrice = int.MaxValue;
                    pickedIndex = -1;
                    for (int i = 0; i < zombieList.Count; i++)
                    {
                        int p = zombieList[i].zombieCreate.baseProperty.price;
                        if (p <= maxZombieValue && p < cheapestPrice)
                        {
                            cheapestPrice = p;
                            pickedIndex = i;
                        }
                    }
                }
                if (pickedIndex < 0 || zombieList[pickedIndex].zombieCreate.baseProperty.price > maxZombieValue)
                    break;
                zombieList[pickedIndex].zombieNum += 1;
                runState.RecordRarityUse(zombieList[pickedIndex].zombieCreate);
                maxZombieValue -= zombieList[pickedIndex].zombieCreate.baseProperty.price;
                hpmax += zombieList[pickedIndex].zombieCreate.baseProperty.HpMax;
                num++;
                if (num > 500) break;
            }
        }

        if (wave % 10 == 9 || wave % 10 == 0)
            enterPecent = 0;
        else
            enterPecent = UnityEngine.Random.Range(0.5f, 0.65f);

        if (wave % 10 == 0)
        {
            var enemyChess = Resources.LoadAll<PropertyCreator>("ChessData/Enemy");
            foreach (var z in enemyChess)
            {
                if (z.chessName == "旗帜僵尸")
                {
                    zombieList.Add(new ZombieInWaveData
                    {
                        zombieCreate = z,
                        zombieNum = 1
                    });
                    break;
                }
            }
        }
    }

    static int PickByRarity(List<float> fateList)
    {
        float fate = UnityEngine.Random.Range(0, 1f);
        for (int i = 0; i < fateList.Count; i++)
        {
            if (fate < fateList[i])
                return i;
        }
        return -1;
    }

    public override bool CheckZombieHp()
    {
        if (!createOver || waveZombies == null) return false;
        float hpcurrent = 0;
        int aliveCount = 0;
        foreach (var z in waveZombies)
        {
            if (z == null || z.IfDeath)
                continue;
            float hp = z.propertyController.GetHp();
            if (hp <= 0f)
                continue;
            hpcurrent += hp;
            aliveCount++;
        }
        if (aliveCount == 0)
            return true;
        if (wave == LevelManage.instance.currentLevel.MaxWave)
            return false;
        if (enterPecent <= 0f || hpmax <= 1e-5f)
            return false;
        float ratio = hpcurrent / hpmax;
        if (ratio >= 0f && ratio < enterPecent)
            return true;
        return false;
    }
}
