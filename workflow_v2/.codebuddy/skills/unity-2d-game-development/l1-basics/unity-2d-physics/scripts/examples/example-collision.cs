using UnityEngine;

/// <summary>
/// 2D碰撞检测示例
/// 演示OnCollision和OnTrigger事件
///
/// 使用方法：
/// 1. 创建角色对象
///    - 添加Rigidbody2D（Dynamic）
///    - 添加Collider2D（不勾选Is Trigger）
/// 2. 创建墙壁对象
///    - 添加Collider2D（不勾选Is Trigger）
/// 3. 创建金币对象
///    - 添加Collider2D（勾选Is Trigger）
/// 4. 将此脚本挂载到角色上
/// 5. 运行游戏，观察碰撞和触发事件
/// </summary>
public class CollisionExample2D : MonoBehaviour
{
    [Header("音效")]
    [Tooltip("碰撞音效")]
    [SerializeField] private AudioClip _collisionSound;

    [Tooltip("收集音效")]
    [SerializeField] private AudioClip _collectSound;

    private AudioSource _audioSource;

    void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    /// <summary>
    /// 碰撞开始（非Trigger碰撞器）
    /// 当角色碰撞墙壁时触发
    /// </summary>
    /// <param name="collision">碰撞信息</param>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"碰撞到：{collision.gameObject.name}");

        // 根据碰撞对象的不同标签执行不同逻辑
        if (collision.gameObject.CompareTag("Wall"))
        {
            // 碰撞墙壁
            Debug.Log("撞墙了！");

            // 播放碰撞音效
            if (_collisionSound != null && _audioSource != null)
            {
                _audioSource.PlayOneShot(_collisionSound);
            }

            // 获取碰撞点信息
            ContactPoint2D contact = collision.contacts[0];
            Debug.Log($"碰撞点：{contact.point}");
            Debug.Log($"碰撞法线：{contact.normal}");
            Debug.Log($"相对速度：{collision.relativeVelocity}");
        }
        else if (collision.gameObject.CompareTag("Enemy"))
        {
            // 碰撞敌人
            Debug.Log("被敌人撞到了！");

            // 计算碰撞伤害（基于相对速度）
            float damage = collision.relativeVelocity.magnitude;
            TakeDamage(damage);
        }
    }

    /// <summary>
    /// 碰撞持续（非Trigger碰撞器）
    /// 当角色持续接触墙壁时触发（每帧）
    /// </summary>
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("MovingPlatform"))
        {
            // 持续接触移动平台
            // 可以在这里实现跟随平台移动的逻辑
            Debug.Log("在移动平台上");
        }
    }

    /// <summary>
    /// 碰撞结束（非Trigger碰撞器）
    /// 当角色离开墙壁时触发
    /// </summary>
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            // 离开地面
            Debug.Log("离开地面");
        }
    }

    /// <summary>
    /// 触发器开始（Trigger碰撞器）
    /// 当角色进入金币区域时触发
    /// </summary>
    /// <param name="other">进入的碰撞器</param>
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"进入触发器：{other.gameObject.name}");

        if (other.gameObject.CompareTag("Coin"))
        {
            // 收集金币
            Debug.Log("收集金币！+10分");

            // 播放收集音效
            if (_collectSound != null && _audioSource != null)
            {
                _audioSource.PlayOneShot(_collectSound);
            }

            // 销毁金币
            Destroy(other.gameObject);
        }
        else if (other.gameObject.CompareTag("Spike"))
        {
            // 触碰陷阱
            Debug.Log("触碰到陷阱！");
            TakeDamage(20f);
        }
        else if (other.gameObject.CompareTag("CheckPoint"))
        {
            // 触发存档点
            Debug.Log("到达存档点");
            SaveCheckpoint(other.transform.position);
        }
    }

    /// <summary>
    /// 触发器持续（Trigger碰撞器）
    /// 当角色持续在区域内时触发（每帧）
    /// </summary>
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Water"))
        {
            // 在水中
            Debug.Log("在水中，游泳速度降低");
        }
        else if (other.gameObject.CompareTag("HealingZone"))
        {
            // 在治疗区域
            Debug.Log("在治疗区域，持续回血");
            Heal(1f * Time.deltaTime);
        }
    }

    /// <summary>
    /// 触发器结束（Trigger碰撞器）
    /// 当角色离开区域时触发
    /// </summary>
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Water"))
        {
            // 离开水域
            Debug.Log("离开水域");
        }
    }

    /// <summary>
    /// 受到伤害
    /// </summary>
    void TakeDamage(float damage)
    {
        Debug.Log($"受到伤害：{damage}");
        // 实现伤害逻辑
    }

    /// <summary>
    /// 治疗
    /// </summary>
    void Heal(float amount)
    {
        Debug.Log($"治疗：{amount}");
        // 实现治疗逻辑
    }

    /// <summary>
    /// 保存存档点
    /// </summary>
    void SaveCheckpoint(Vector3 position)
    {
        Debug.Log($"保存存档点：{position}");
        // 实现存档逻辑
    }
}
