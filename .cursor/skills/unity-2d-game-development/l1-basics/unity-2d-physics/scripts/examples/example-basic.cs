using UnityEngine;

/// <summary>
/// 最基础的2D物理移动示例
/// 演示Rigidbody2D的基础使用
///
/// 使用方法：
/// 1. 创建一个2D对象（如Sprite Square）
/// 2. 添加Rigidbody2D组件（Body Type: Dynamic）
/// 3. 添加BoxCollider2D组件
/// 4. 将此脚本挂载到对象上
/// 5. 运行游戏，使用A/D或←/→键移动
/// </summary>
public class BasicMovement2D : MonoBehaviour
{
    [Header("移动参数")]
    [Tooltip("移动速度")]
    [SerializeField] private float _moveSpeed = 5f;

    // 组件引用（缓存，避免频繁GetComponent）
    private Rigidbody2D _rb;

    void Awake()
    {
        // ✅ 在Awake中获取组件引用
        _rb = GetComponent<Rigidbody2D>();

        // ✅ 冻结旋转（角色不需要旋转）
        _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    void Update()
    {
        // 获取水平输入（A/D 或 ←/→）
        // 返回值：-1（左）、0（不动）、1（右）
        float horizontalInput = Input.GetAxisRaw("Horizontal");

        // 移动角色
        Move(horizontalInput);
    }

    /// <summary>
    /// 移动角色
    /// </summary>
    /// <param name="horizontalInput">水平输入：-1到1</param>
    void Move(float horizontalInput)
    {
        // ✅ 直接设置velocity控制移动
        // 保留Y轴速度，只修改X轴速度
        _rb.velocity = new Vector2(horizontalInput * _moveSpeed, _rb.velocity.y);
    }

    // 可选：可视化调试
    void OnDrawGizmos()
    {
        // 绘制移动方向
        if (_rb != null)
        {
            Gizmos.color = Color.blue;
            Vector3 direction = new Vector3(_rb.velocity.x, 0, 0).normalized;
            Gizmos.DrawRay(transform.position, direction * 2f);
        }
    }
}
