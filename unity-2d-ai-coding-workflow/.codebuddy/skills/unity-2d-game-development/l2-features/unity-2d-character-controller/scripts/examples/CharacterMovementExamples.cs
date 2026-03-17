using UnityEngine;
using UnityEngine.InputSystem;

namespace Unity.CharacterMovement.Examples
{
    #region 示例1：平台跳跃控制器（完整版）

    /// <summary>
    /// 示例1：完整的平台跳跃控制器
    /// 包含移动、跳跃、二段跳、冲刺等核心功能
    /// </summary>
    public class PlatformerController : MonoBehaviour
    {
        [Header("移动参数")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float acceleration = 10f;
        [SerializeField] private float deceleration = 10f;

        [Header("跳跃参数")]
        [SerializeField] private float jumpForce = 12f;
        [SerializeField] private float gravityScale = 2.5f;
        [SerializeField] private float fallGravityMultiplier = 2f;
        [SerializeField] private int maxJumpCount = 2;

        [Header("冲刺参数")]
        [SerializeField] private float dashSpeed = 15f;
        [SerializeField] private float dashDuration = 0.2f;
        [SerializeField] private float dashCooldown = 0.5f;

        [Header("地面检测")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundCheckRadius = 0.2f;
        [SerializeField] private LayerMask groundLayer;

        [Header("输入")]
        [SerializeField] private InputAction moveAction;
        [SerializeField] private InputAction jumpAction;
        [SerializeField] private InputAction dashAction;

        // 组件缓存
        private Rigidbody2D rb;
        private Animator animator;

        // 状态变量
        private bool isGrounded;
        private bool isFacingRight = true;
        private int jumpCount;
        private bool isDashing;
        private float dashTimer;
        private float dashCooldownTimer;
        private float currentVelocity;

        private void Awake()
        {
            // 缓存组件
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();

            // 配置刚体
            rb.gravityScale = gravityScale;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        private void OnEnable()
        {
            moveAction.Enable();
            jumpAction.Enable();
            dashAction.Enable();

            jumpAction.performed += OnJump;
            jumpAction.canceled += OnJumpReleased;
            dashAction.performed += OnDash;
        }

        private void OnDisable()
        {
            moveAction.Disable();
            jumpAction.Disable();
            dashAction.Disable();

            jumpAction.performed -= OnJump;
            jumpAction.canceled -= OnJumpReleased;
            dashAction.performed -= OnDash;
        }

        private void FixedUpdate()
        {
            CheckGround();
            
            if (isDashing)
            {
                UpdateDash();
            }
            else
            {
                HandleMovement();
                ApplyGravity();
            }

            UpdateDashCooldown();
            UpdateAnimator();
        }

        private void CheckGround()
        {
            bool wasGrounded = isGrounded;
            isGrounded = Physics2D.OverlapCircle(
                groundCheck.position,
                groundCheckRadius,
                groundLayer
            ) != null;

            // 重置跳跃计数
            if (isGrounded && !wasGrounded)
            {
                jumpCount = 0;
            }
        }

        private void HandleMovement()
        {
            float moveInput = moveAction.ReadValue<Vector2>().x;
            float targetVelocity = moveInput * moveSpeed;

            // 平滑加速/减速
            if (Mathf.Abs(moveInput) > 0.1f)
            {
                currentVelocity = Mathf.MoveTowards(
                    currentVelocity,
                    targetVelocity,
                    acceleration * Time.fixedDeltaTime
                );
            }
            else
            {
                currentVelocity = Mathf.MoveTowards(
                    currentVelocity,
                    0f,
                    deceleration * Time.fixedDeltaTime
                );
            }

            rb.velocity = new Vector2(currentVelocity, rb.velocity.y);

            // 翻转角色
            if (moveInput > 0.1f && !isFacingRight) Flip();
            if (moveInput < -0.1f && isFacingRight) Flip();
        }

        private void ApplyGravity()
        {
            // 下落时增加重力
            if (rb.velocity.y < 0)
            {
                rb.gravityScale = gravityScale * fallGravityMultiplier;
            }
            else
            {
                rb.gravityScale = gravityScale;
            }
        }

        private void OnJump(InputAction.CallbackContext context)
        {
            if (jumpCount < maxJumpCount)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                jumpCount++;
            }
        }

        private void OnJumpReleased(InputAction.CallbackContext context)
        {
            // 松开跳跃键时降低跳跃高度
            if (rb.velocity.y > 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
            }
        }

        private void OnDash(InputAction.CallbackContext context)
        {
            if (dashCooldownTimer <= 0 && !isDashing)
            {
                isDashing = true;
                dashTimer = dashDuration;
                dashCooldownTimer = dashCooldown;

                // 冲刺方向
                float dashDirection = isFacingRight ? 1f : -1f;
                rb.velocity = new Vector2(dashDirection * dashSpeed, 0);
                rb.gravityScale = 0; // 冲刺时不受重力影响
            }
        }

        private void UpdateDash()
        {
            dashTimer -= Time.fixedDeltaTime;
            if (dashTimer <= 0)
            {
                isDashing = false;
                rb.gravityScale = gravityScale;
            }
        }

        private void UpdateDashCooldown()
        {
            if (dashCooldownTimer > 0)
            {
                dashCooldownTimer -= Time.fixedDeltaTime;
            }
        }

        private void Flip()
        {
            isFacingRight = !isFacingRight;
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }

        private void UpdateAnimator()
        {
            if (animator != null)
            {
                animator.SetFloat("Speed", Mathf.Abs(currentVelocity));
                animator.SetBool("IsGrounded", isGrounded);
                animator.SetBool("IsDashing", isDashing);
            }
        }

        // 调试可视化
        private void OnDrawGizmosSelected()
        {
            if (groundCheck != null)
            {
                Gizmos.color = isGrounded ? Color.green : Color.red;
                Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
            }
        }
    }

    #endregion

    #region 示例2：俯视移动控制器

    /// <summary>
    /// 示例2：俯视移动控制器
    /// 四方向移动，无重力影响
    /// </summary>
    public class TopDownController : MonoBehaviour
    {
        [Header("移动参数")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float sprintMultiplier = 1.5f;

        [Header("输入")]
        [SerializeField] private InputAction moveAction;
        [SerializeField] private InputAction sprintAction;

        private Rigidbody2D rb;
        private Animator animator;
        private Vector2 moveInput;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();

            // 俯视移动不需要重力
            rb.gravityScale = 0;
        }

        private void OnEnable()
        {
            moveAction.Enable();
            sprintAction.Enable();
        }

        private void OnDisable()
        {
            moveAction.Disable();
            sprintAction.Disable();
        }

        private void FixedUpdate()
        {
            HandleMovement();
            UpdateAnimator();
        }

        private void HandleMovement()
        {
            moveInput = moveAction.ReadValue<Vector2>();
            
            // 是否冲刺
            bool isSprinting = sprintAction.IsPressed();
            float currentSpeed = isSprinting ? moveSpeed * sprintMultiplier : moveSpeed;

            // 标准化对角线移动速度
            Vector2 velocity = moveInput.normalized * currentSpeed;
            rb.velocity = velocity;

            // 翻转角色朝向
            if (moveInput.x != 0)
            {
                Vector3 scale = transform.localScale;
                scale.x = moveInput.x > 0 ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
                transform.localScale = scale;
            }
        }

        private void UpdateAnimator()
        {
            if (animator != null)
            {
                animator.SetFloat("MoveX", moveInput.x);
                animator.SetFloat("MoveY", moveInput.y);
                animator.SetFloat("Speed", moveInput.magnitude);
            }
        }
    }

    #endregion

    #region 示例3：横版格斗控制器

    /// <summary>
    /// 示例3：横版格斗控制器
    /// 基于状态机的角色控制
    /// </summary>
    public class FighterController : MonoBehaviour
    {
        public enum State
        {
            Idle,
            Move,
            Attack,
            Dash,
            Hurt,
            Dead
        }

        [Header("移动参数")]
        [SerializeField] private float moveSpeed = 4f;
        [SerializeField] private float dashSpeed = 12f;
        [SerializeField] private float dashDuration = 0.15f;

        [Header("攻击参数")]
        [SerializeField] private float attackDamage = 10f;
        [SerializeField] private float attackRange = 1f;
        [SerializeField] private LayerMask enemyLayer;

        [Header("输入")]
        [SerializeField] private InputAction moveAction;
        [SerializeField] private InputAction attackAction;
        [SerializeField] private InputAction dashAction;

        private State currentState = State.Idle;
        private Rigidbody2D rb;
        private Animator animator;
        private bool isFacingRight = true;
        private float stateTimer;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            rb.gravityScale = 2f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        private void OnEnable()
        {
            moveAction.Enable();
            attackAction.Enable();
            dashAction.Enable();

            attackAction.performed += OnAttack;
            dashAction.performed += OnDash;
        }

        private void OnDisable()
        {
            moveAction.Disable();
            attackAction.Disable();
            dashAction.Disable();

            attackAction.performed -= OnAttack;
            dashAction.performed -= OnDash;
        }

        private void FixedUpdate()
        {
            switch (currentState)
            {
                case State.Idle:
                    UpdateIdle();
                    break;
                case State.Move:
                    UpdateMove();
                    break;
                case State.Attack:
                    UpdateAttack();
                    break;
                case State.Dash:
                    UpdateDash();
                    break;
                case State.Hurt:
                    UpdateHurt();
                    break;
            }

            UpdateAnimator();
        }

        private void UpdateIdle()
        {
            float moveInput = moveAction.ReadValue<Vector2>().x;
            if (Mathf.Abs(moveInput) > 0.1f)
            {
                ChangeState(State.Move);
            }
        }

        private void UpdateMove()
        {
            float moveInput = moveAction.ReadValue<Vector2>().x;
            rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);

            // 翻转
            if (moveInput > 0.1f && !isFacingRight) Flip();
            if (moveInput < -0.1f && isFacingRight) Flip();

            // 停止移动
            if (Mathf.Abs(moveInput) < 0.1f)
            {
                rb.velocity = new Vector2(0, rb.velocity.y);
                ChangeState(State.Idle);
            }
        }

        private void UpdateAttack()
        {
            stateTimer -= Time.fixedDeltaTime;
            if (stateTimer <= 0)
            {
                ChangeState(State.Idle);
            }
        }

        private void UpdateDash()
        {
            stateTimer -= Time.fixedDeltaTime;
            if (stateTimer <= 0)
            {
                rb.gravityScale = 2f;
                ChangeState(State.Idle);
            }
        }

        private void UpdateHurt()
        {
            stateTimer -= Time.fixedDeltaTime;
            if (stateTimer <= 0)
            {
                ChangeState(State.Idle);
            }
        }

        private void OnAttack(InputAction.CallbackContext context)
        {
            if (currentState == State.Idle || currentState == State.Move)
            {
                ChangeState(State.Attack);
                stateTimer = 0.3f; // 攻击持续时间
                rb.velocity = Vector2.zero;

                // 检测攻击命中
                Collider2D[] hits = Physics2D.OverlapCircleAll(
                    transform.position,
                    attackRange,
                    enemyLayer
                );
                foreach (var hit in hits)
                {
                    // 应用伤害
                    Debug.Log($"命中: {hit.name}");
                }
            }
        }

        private void OnDash(InputAction.CallbackContext context)
        {
            if (currentState == State.Idle || currentState == State.Move)
            {
                ChangeState(State.Dash);
                stateTimer = dashDuration;

                float dashDirection = isFacingRight ? 1f : -1f;
                rb.velocity = new Vector2(dashDirection * dashSpeed, 0);
                rb.gravityScale = 0;
            }
        }

        private void ChangeState(State newState)
        {
            currentState = newState;
        }

        private void Flip()
        {
            isFacingRight = !isFacingRight;
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }

        private void UpdateAnimator()
        {
            if (animator != null)
            {
                animator.SetInteger("State", (int)currentState);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }

    #endregion

    #region 示例4：高级地面检测

    /// <summary>
    /// 示例4：高级地面检测系统
    /// 多点检测 + 坡道检测
    /// </summary>
    public class AdvancedGroundDetection : MonoBehaviour
    {
        [Header("检测点")]
        [SerializeField] private Transform[] groundCheckPoints;
        [SerializeField] private float checkRadius = 0.1f;
        [SerializeField] private LayerMask groundLayer;

        [Header("坡道设置")]
        [SerializeField] private float maxSlopeAngle = 45f;

        private bool isGrounded;
        private Vector2 groundNormal;
        private float slopeAngle;

        public bool IsGrounded => isGrounded;
        public Vector2 GroundNormal => groundNormal;
        public float SlopeAngle => slopeAngle;

        public void CheckGround()
        {
            isGrounded = false;
            groundNormal = Vector2.up;
            slopeAngle = 0f;

            foreach (var point in groundCheckPoints)
            {
                Collider2D hit = Physics2D.OverlapCircle(
                    point.position,
                    checkRadius,
                    groundLayer
                );

                if (hit != null)
                {
                    isGrounded = true;

                    // 获取地面法线
                    RaycastHit2D rayHit = Physics2D.Raycast(
                        point.position,
                        Vector2.down,
                        checkRadius + 0.1f,
                        groundLayer
                    );

                    if (rayHit.collider != null)
                    {
                        groundNormal = rayHit.normal;
                        slopeAngle = Vector2.Angle(groundNormal, Vector2.up);
                    }

                    break;
                }
            }
        }

        public Vector2 GetSlopeMoveDirection(Vector2 input)
        {
            if (isGrounded && slopeAngle < maxSlopeAngle)
            {
                // 沿坡道移动
                return Vector2.Perpendicular(groundNormal) * -input.x;
            }
            return input;
        }

        private void OnDrawGizmosSelected()
        {
            if (groundCheckPoints != null)
            {
                Gizmos.color = isGrounded ? Color.green : Color.red;
                foreach (var point in groundCheckPoints)
                {
                    if (point != null)
                    {
                        Gizmos.DrawWireSphere(point.position, checkRadius);
                    }
                }
            }
        }
    }

    #endregion

    #region 示例5：跳跃缓冲与土狼时间

    /// <summary>
    /// 示例5：跳跃优化系统
    /// 跳跃缓冲 + 土狼时间
    /// </summary>
    public class JumpOptimizer : MonoBehaviour
    {
        [Header("跳跃参数")]
        [SerializeField] private float jumpForce = 12f;
        [SerializeField] private float coyoteTime = 0.1f;    // 土狼时间
        [SerializeField] private float jumpBufferTime = 0.1f; // 跳跃缓冲

        private Rigidbody2D rb;
        private bool isGrounded;
        private float coyoteTimer;
        private float jumpBufferTimer;
        private bool jumpInput;

        public void OnJumpInput()
        {
            jumpInput = true;
            jumpBufferTimer = jumpBufferTime;
        }

        public void UpdateGroundState(bool grounded)
        {
            isGrounded = grounded;

            if (grounded)
            {
                coyoteTimer = coyoteTime;
            }
        }

        public void Update()
        {
            // 更新计时器
            if (!isGrounded && coyoteTimer > 0)
            {
                coyoteTimer -= Time.deltaTime;
            }

            if (jumpBufferTimer > 0)
            {
                jumpBufferTimer -= Time.deltaTime;
            }

            // 尝试跳跃
            if (jumpInput && jumpBufferTimer > 0)
            {
                if (isGrounded || coyoteTimer > 0)
                {
                    Jump();
                    coyoteTimer = 0;
                    jumpBufferTimer = 0;
                }
            }

            jumpInput = false;
        }

        private void Jump()
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }
    }

    #endregion
}
