using UnityEngine;

/// <summary>
/// 商店面板上下文：车钥匙点击后打开商店时，返回按钮进入下一关；其他时候返回首页
/// </summary>
public static class CoinShopContext
{
    /// <summary>本次关闭商店后是否进入下一关（车钥匙点击后为 true）</summary>
    public static bool ReturnToNextLevelOnClose { get; set; }

    /// <summary>车钥匙关卡配置的下一关，ReturnToNextLevelOnClose 时使用</summary>
    public static LevelData NextLevelToReturnTo { get; set; }

    public static void SetReturnToNextLevel(LevelData nextLevel)
    {
        ReturnToNextLevelOnClose = true;
        NextLevelToReturnTo = nextLevel;
    }

    public static void ClearReturnToNextLevel()
    {
        ReturnToNextLevelOnClose = false;
        NextLevelToReturnTo = null;
    }
}
