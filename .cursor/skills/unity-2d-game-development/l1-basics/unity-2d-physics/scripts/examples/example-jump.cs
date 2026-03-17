using UnityEngine;

/// <summary>
/// 2D角色跳跃示例
/// 演示AddForce和射线检测的使用
///
/// 使用方法：
/// 1. 创建角色对象（如Capsule）
/// 2. 添加Rigidbody2D组件
///    - Body Type: Dynamic
///    - Constraints: Freeze Rotation Z（勾选）
/// 3. 添加CapsuleCollider2D组件
/// 4. 创建地面对象（如Square）
///    - 设置Layer为"Ground"
///    - 添加BoxCollider2D
/// 5. 将此脚本挂载到角色上
/// 6. 在Inspector中设置Ground Layer为"Ground"层
/// 7. 运行游戏，使用Space键跳跃
/// </summary>
public class JumpExample2D : MonoBehaviour
{
    [Header("移动参数")]
    [Tooltip("移动速度")]
    [SerializeField] private float _moveSpeed = 5f;

    [Header("跳跃参数")]
    [Tooltip("跳跃力度")]
    [SerializeField] private float _jumpForce = 10f;

    [Header("检测参数")]
    [Tooltip("地面层级")]
    [SerializeField] private LayerMask _groundLayer;

    [Tooltip("地面检测距离")]
    [SerializeField] private float _groundCheckDistance = 0.1f;

    // 组件引用
    private Rigidbody2D _rb;
    private Collider2D _collider;

    // 状态
    private bool _isGrounded;

    void Awake()
    {
        // ✅ 缓存组件引用
        _rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();

        // ✅ 冻结旋转
        _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    void Update()
    {
        // 移动输入
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        Move(horizontalInput);

        // 跳跃输入
        if (Input.GetButtonDown("Jump") && _isGrounded)
        {
            Jump();
        }
    }

    void FixedUpdate()
    {
        // ✅ 在FixedUpdate中检测地面（物理检测应在FixedUpdate）
        _isGrounded = CheckGround();
    }

    /// <summary>
    /// 移动角色
    /// </summary>
    void Move(float horizontalInput)
    {
        // ✅ 直接设置velocity控制移动
        _rb.velocity = new Vector2(horizontalInput * _moveSpeed, _rb.velocity.y);
    }

    /// <summary>
    /// 跳跃
    /// </summary>
    void Jump()
    {
        // ✅ 使用AddForce施加跳跃力
        // ForceMode2D.Impulse：瞬间力，不受质量影响
        _rb.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);
    }

    /// <summary>
    /// 检测地面
    /// </summary>
    /// <returns>是否在地面上</returns>
    bool CheckGround()
    {
        // ✅ 使用Raycast检测地面
        // 参数：起点、方向、距离、层级过滤
        RaycastHit2D hit = Physics2D.Raycast(
            _collider.bounds.center,                      // 起点：碰撞器中心
            Vector2.down,                                  // 方向：向下
            _collider.bounds.extents.y + _groundCheckDistance, // 距离：碰撞器半高 + 检测距离
            _groundLayer                                   // 层级：只检测Ground层
        );

        // 返回是否检测到地面
        return hit.collider != null;
    }

    // ✅ 可视化调试（Scene视图中可见）
    void OnDrawGizmos()
    {
        if (_collider == null) return;

        // 绘制地面检测射线
        // 绿色：在地面上
        // 红色：不在地面上
        Gizmos.color = _isGrounded ? Color.green : Color.red;
        Gizmos.DrawRay(
            _collider.bounds.center,
            Vector2.down * (_collider.bounds.extents.y + _groundCheckDistance)
        );
    }
}
