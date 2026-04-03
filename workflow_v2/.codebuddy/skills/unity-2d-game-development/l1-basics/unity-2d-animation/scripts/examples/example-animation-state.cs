using UnityEngine;

/// <summary>
/// 动画状态机切换示例
/// 展示使用枚举管理动画状态，实现角色idle/walk/run/jump四种状态切换
/// 包含性能优化：StringToHash缓存、减少状态查询频率
/// </summary>
public class ExampleAnimationState : MonoBehaviour
{
    [Header("组件引用")]
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody2D rb;
    
    [Header("移动参数")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runSpeed = 6f;
    [SerializeField] private float jumpForce = 5f;
    
    [Header("地面检测")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.1f;
    [SerializeField] private LayerMask groundLayer;
    
    // 动画状态枚举
    private enum AnimationState
    {
        Idle,
        Walk,
        Run,
        Jump
    }
    
    // 当前动画状态
    private AnimationState currentState = AnimationState.Idle;
    
    // 参数哈希缓存（性能优化：避免每次调用都计算字符串哈希）
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int JumpTriggerHash = Animator.StringToHash("Jump");
    
    // 状态哈希缓存
    private static readonly int IdleHash = Animator.StringToHash("Idle");
    private static readonly int WalkHash = Animator.StringToHash("Walk");
    private static readonly int RunHash = Animator.StringToHash("Run");
    private static readonly int JumpHash = Animator.StringToHash("Jump");
    
    // 移动输入
    private float moveInput;
    private bool isGrounded;
    
    // 性能优化：状态查询标记
    private bool needCheckJumpComplete = false;
    
    private void Awake()
    {
        // 自动获取组件
        if (animator == null) animator = GetComponent<Animator>();
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (groundCheck == null) groundCheck = transform;
    }
    
    private void Update()
    {
        // 获取输入
        HandleInput();
        
        // 地面检测
        CheckGround();
        
        // 更新动画状态
        UpdateAnimationState();
        
        // 检查跳跃动画是否完成
        if (needCheckJumpComplete)
        {
            CheckJumpComplete();
        }
    }
    
    private void FixedUpdate()
    {
        // 物理移动
        HandleMovement();
    }
    
    /// <summary>
    /// 处理输入
    /// </summary>
    private void HandleInput()
    {
        // 水平移动输入
        moveInput = Input.GetAxis("Horizontal");
        
        // 跳跃输入
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Jump();
        }
        
        // 按住Shift加速跑
        bool isRunning = Input.GetKey(KeyCode.LeftShift) && Mathf.Abs(moveInput) > 0.1f;
        if (isRunning)
        {
            currentState = AnimationState.Run;
        }
    }
    
    /// <summary>
    /// 地面检测
    /// </summary>
    private void CheckGround()
    {
        // 使用圆形射线检测地面
        bool wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        
        // 更新Animator参数
        animator.SetBool(IsGroundedHash, isGrounded);
        
        // 落地事件
        if (!wasGrounded && isGrounded)
        {
            OnLanded();
        }
    }
    
    /// <summary>
    /// 处理移动
    /// </summary>
    private void HandleMovement()
    {
        // 根据当前状态选择速度
        float currentSpeed = (currentState == AnimationState.Run) ? runSpeed : walkSpeed;
        
        // 应用移动
        rb.velocity = new Vector2(moveInput * currentSpeed, rb.velocity.y);
        
        // 翻转角色朝向
        if (moveInput != 0)
        {
            transform.localScale = new Vector3(Mathf.Sign(moveInput), 1, 1);
        }
    }
    
    /// <summary>
    /// 更新动画状态
    /// </summary>
    private void UpdateAnimationState()
    {
        // 跳跃状态优先级最高
        if (currentState == AnimationState.Jump)
        {
            return;
        }
        
        // 根据移动输入和地面状态切换动画
        if (!isGrounded)
        {
            // 在空中：跳跃状态
            SwitchState(AnimationState.Jump);
        }
        else if (Mathf.Abs(moveInput) < 0.1f)
        {
            // 静止：Idle状态
            SwitchState(AnimationState.Idle);
        }
        else if (currentState == AnimationState.Run)
        {
            // 移动中且按住Shift：Run状态
            SwitchState(AnimationState.Run);
        }
        else
        {
            // 移动中：Walk状态
            SwitchState(AnimationState.Walk);
        }
        
        // 更新速度参数（用于Blend Tree或状态过渡）
        float speed = Mathf.Abs(moveInput) * ((currentState == AnimationState.Run) ? 1.5f : 1f);
        animator.SetFloat(SpeedHash, speed);
    }
    
    /// <summary>
    /// 切换动画状态
    /// </summary>
    /// <param name="newState">目标状态</param>
    private void SwitchState(AnimationState newState)
    {
        // 如果状态相同，不执行切换
        if (currentState == newState)
        {
            return;
        }
        
        // 切换状态
        AnimationState oldState = currentState;
        currentState = newState;
        
        // 根据新状态播放对应动画
        switch (newState)
        {
            case AnimationState.Idle:
                animator.CrossFade(IdleHash, 0.1f);
                break;
                
            case AnimationState.Walk:
                animator.CrossFade(WalkHash, 0.1f);
                break;
                
            case AnimationState.Run:
                animator.CrossFade(RunHash, 0.1f);
                break;
                
            case AnimationState.Jump:
                animator.CrossFade(JumpHash, 0.05f);
                break;
        }
        
        Debug.Log($"动画状态切换: {oldState} → {newState}");
    }
    
    /// <summary>
    /// 跳跃
    /// </summary>
    private void Jump()
    {
        // 应用跳跃力
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        
        // 触发跳跃动画
        animator.SetTrigger(JumpTriggerHash);
        
        // 切换到跳跃状态
        SwitchState(AnimationState.Jump);
        
        // 标记需要检查跳跃完成
        needCheckJumpComplete = true;
        
        Debug.Log("角色跳跃");
    }
    
    /// <summary>
    /// 检查跳跃动画是否完成
    /// 性能优化：只在需要时检查，避免每帧查询
    /// </summary>
    private void CheckJumpComplete()
    {
        // 如果已经落地，重置标记
        if (isGrounded)
        {
            needCheckJumpComplete = false;
            return;
        }
        
        // ⚠️ 注意：避免在Update中频繁调用GetCurrentAnimatorStateInfo
        // 只在必要时（needCheckJumpComplete为true）才查询状态
        var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        
        // 如果正在过渡中，跳过检查
        if (animator.IsInTransition(0))
        {
            return;
        }
        
        // 检查是否还在Jump状态
        if (stateInfo.shortNameHash != JumpHash)
        {
            needCheckJumpComplete = false;
        }
    }
    
    /// <summary>
    /// 落地事件
    /// </summary>
    private void OnLanded()
    {
        Debug.Log("角色落地");
        needCheckJumpComplete = false;
    }
    
    #region 公共方法（供外部调用）
    
    /// <summary>
    /// 强制切换到指定动画状态
    /// </summary>
    /// <param name="state">目标状态</param>
    public void ForceState(AnimationState state)
    {
        currentState = state;
        
        switch (state)
        {
            case AnimationState.Idle:
                animator.Play(IdleHash);
                break;
            case AnimationState.Walk:
                animator.Play(WalkHash);
                break;
            case AnimationState.Run:
                animator.Play(RunHash);
                break;
            case AnimationState.Jump:
                animator.Play(JumpHash);
                break;
        }
    }
    
    /// <summary>
    /// 获取当前动画状态
    /// </summary>
    /// <returns>当前状态</returns>
    public AnimationState GetCurrentState()
    {
        return currentState;
    }
    
    /// <summary>
    /// 判断是否在地面
    /// </summary>
    /// <returns>是否在地面</returns>
    public bool IsGrounded()
    {
        return isGrounded;
    }
    
    #endregion
    
    #region 编辑器可视化
    
    #if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            // 绘制地面检测范围
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
    
    private void OnGUI()
    {
        // 显示调试信息
        GUILayout.BeginArea(new Rect(10, 10, 250, 150));
        GUILayout.Label($"当前状态: {currentState}");
        GUILayout.Label($"移动输入: {moveInput:F2}");
        GUILayout.Label($"是否在地面: {isGrounded}");
        GUILayout.Label($"速度: {rb.velocity}");
        GUILayout.Label("\n控制键:");
        GUILayout.Label("A/D或←/→: 移动");
        GUILayout.Label("空格: 跳跃");
        GUILayout.Label("Shift+移动: 跑步");
        GUILayout.EndArea();
    }
    #endif
    
    #endregion
}