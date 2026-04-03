using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 示例：基础输入处理
/// 演示如何使用新Input System处理移动和跳跃
/// </summary>
public class ExampleInputBasic : MonoBehaviour
{
    [Header("移动设置")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;
    
    [Header("引用")]
    [SerializeField] private Rigidbody2D rb;
    
    // Input Actions
    private PlayerControls controls;
    private Vector2 moveInput;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        controls = new PlayerControls();
    }
    
    void OnEnable()
    {
        // 启用Action Map
        controls.Gameplay.Enable();
        
        // 订阅事件
        controls.Gameplay.Move.performed += OnMove;
        controls.Gameplay.Move.canceled += OnMoveCanceled;
        controls.Gameplay.Jump.performed += OnJump;
    }
    
    void OnDisable()
    {
        // 取消订阅
        controls.Gameplay.Move.performed -= OnMove;
        controls.Gameplay.Move.canceled -= OnMoveCanceled;
        controls.Gameplay.Jump.performed -= OnJump;
        
        // 禁用Action Map
        controls.Gameplay.Disable();
    }
    
    void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
    
    void OnMoveCanceled(InputAction.CallbackContext context)
    {
        moveInput = Vector2.zero;
    }
    
    void OnJump(InputAction.CallbackContext context)
    {
        if (IsGrounded())
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }
    }
    
    void FixedUpdate()
    {
        // 应用移动
        Vector2 velocity = new Vector2(moveInput.x * moveSpeed, rb.velocity.y);
        rb.velocity = velocity;
    }
    
    bool IsGrounded()
    {
        // 简单的地面检测
        return Physics2D.Raycast(transform.position, Vector2.down, 0.1f);
    }
}

// 注意：需要先生成PlayerControls类
// 在Input Actions Asset中勾选"Generate C# Class"
