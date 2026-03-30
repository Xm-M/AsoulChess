using UnityEngine;

/// <summary>
/// 蛇头碰撞吃豆：与 <see cref="CarArmor"/> / <see cref="BowlingArmor"/> 一样挂在棋子上，
/// 在 <see cref="OnTriggerEnter2D"/> 里交给 <see cref="SnakeGridController.TryEatFood"/>。
/// 不继承 <see cref="CarArmor"/>，避免对碰撞体施加伤害与音效逻辑。
/// </summary>
public class SnakeArmor : ArmorBase
{
    [Tooltip("空则运行时查找")]
    public SnakeGridController snakeDriver;

    Collider2D _col;

    [Min(0f)]
    [SerializeField]
    float eatCooldown = 0.08f;

    [Tooltip("吃掉关卡食物后为蛇头（本体）回复的生命值；实际量会乘蛇头 Property 的回复增益 healRate")]
    [Min(0f)]
    [SerializeField]
    float healBodyOnEatFood = 100f;

    public AudioPlayer player;

    float _nextEatTime;

    public override void InitArmor()
    {
        _col = GetComponent<Collider2D>();
        user.WhenEnterGame.AddListener(ResetArmor);
        if (snakeDriver == null)
            snakeDriver = FindObjectOfType<SnakeGridController>();
    }

    public override void ResetArmor(Chess chess)
    {
        _nextEatTime = 0f;
        if (_col != null)
            _col.enabled = true;
    }

    public override void GetDamage(DamageMessege dm)
    {
    }

    public override void BrokenArmor()
    {
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        TryEat(collision);
    }

    void TryEat(Collider2D collision)
    {
        if (user == null || user.IfDeath) return;
        if (LevelManage.instance == null || !LevelManage.instance.IfGameStart) return;
        if (Time.time < _nextEatTime) return;

        Chess other = collision.GetComponent<Chess>();
        if (other == null || other == user || other.IfDeath) return;

        if (snakeDriver == null)
            snakeDriver = FindObjectOfType<SnakeGridController>();

        if (snakeDriver != null && snakeDriver.TryEatFood(other, user))
        {
            HealSnakeBodyOnEatFood();
            _nextEatTime = Time.time + eatCooldown;
            return;
        }

        if (other.CompareTag(user.tag))
            return;

        if (user.propertyController == null || other.propertyController == null)
            return;

        int userSize = user.propertyController.GetSize();
        int targetSize = other.propertyController.GetSize();

        if (targetSize < userSize)
        {
            player.RandomPlay();
            if (!other.IfDeath)
                other.Death();
            EventController.Instance.TriggerEvent(EventName.SnakeEatZombie.ToString(), SnakeGameEventId.EatZombie);
            _nextEatTime = Time.time + eatCooldown;
            return;
        }

        EventController.Instance.TriggerEvent(EventName.SnakeHitWall.ToString(), SnakeGameEventId.StumbleLargerEnemy);
        LevelController_Snake level = snakeDriver != null ? snakeDriver.snakeLevel : FindObjectOfType<LevelController_Snake>();
        level?.NotifySnakeDefeat(user);
    }

    /// <summary>成功吃掉 <see cref="SnakeGridController.TryEatFood"/> 认定的食物后，为蛇头本体回血。</summary>
    void HealSnakeBodyOnEatFood()
    {
        player.RandomPlay();
        if (healBodyOnEatFood <= 0f) return;
        Chess body = snakeDriver != null ? snakeDriver.head : user;
        if (body == null || body.IfDeath || body.propertyController == null) return;
        body.propertyController.Heal(healBodyOnEatFood);
    }
}
