/// <summary>
/// 贪吃蛇相关 <see cref="EventController"/> 的 string 载荷：与监听器里 switch 一致即可。
/// </summary>
public static class SnakeGameEventId
{
    /// <summary>越界（原「撞墙」）。</summary>
    public const string StumbleOutOfBounds = "SnakeStumble_OutOfBounds";

    /// <summary>咬到自己。</summary>
    public const string StumbleSelfBite = "SnakeStumble_SelfBite";

    /// <summary>撞到体型更大或相等的敌方（触发眩晕）。</summary>
    public const string StumbleLargerEnemy = "SnakeStumble_LargerEnemy";

    /// <summary>仅用于 <see cref="GameStartPlugin_SnakeEvent"/> 里 tipBindings 的 eventId；<see cref="EventName.SnakeEatFood"/> 载荷见 <see cref="SnakeEatFoodPayload"/>。</summary>
    public const string EatFood = "SnakeEatFood";

    public const string EatZombie = "SnakeEatZombie";

    public const string FoodSpawned = "SnakeFoodSpawned";
}
