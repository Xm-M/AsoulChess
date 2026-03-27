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
            if (!other.IfDeath)
                other.Death();
            _nextEatTime = Time.time + eatCooldown;
            return;
        }

        LevelController_Snake level = snakeDriver != null ? snakeDriver.snakeLevel : FindObjectOfType<LevelController_Snake>();
        level?.NotifySnakeDefeat();
    }
}
