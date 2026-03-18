using UnityEngine;

/// <summary>
/// 难度系统：0=简单 1=普通 2=困难 3=噩梦
/// </summary>
public static class DifficultyManager
{
    /// <summary>Test 模式下使用的难度（不写存档），非 Test 模式忽略</summary>
    public static int TestModeDifficulty { get; set; } = 1;

    static int GetLevel()
    {
        if (GameManage.instance != null && GameManage.instance.mode == GameMode.Test)
            return Mathf.Clamp(TestModeDifficulty, 0, 3);
        return PlayerSaveContext.CurrentData?.difficultyLevel ?? 1;
    }

    /// <summary>僵尸价值倍率：简单0.75 普通1 困难2 噩梦4</summary>
    public static float GetZombieMultiplier()
    {
        int lv = GetLevel();
        return lv switch { 0 => 0.75f, 1 => 1f, 2 => 2f, 3 => 4f, _ => 1f };
    }

    /// <summary>小推车数量：-1=全部 困难3台 噩梦1台</summary>
    public static int GetCarCount()
    {
        int lv = GetLevel();
        return lv switch { 0 => -1, 1 => -1, 2 => 3, 3 => 1, _ => -1 };
    }
}
