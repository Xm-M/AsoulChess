using UnityEngine;

/// <summary>
/// 动画参数控制示例
/// 展示Bool、Float、Int、Trigger四种动画参数的使用方法
/// 包含StringToHash性能优化模式
/// </summary>
public class ExampleAnimationParams : MonoBehaviour
{
    [Header("组件引用")]
    [SerializeField] private Animator animator;
    
    [Header("Bool参数示例")]
    [SerializeField] private bool isGrounded = true;
    [SerializeField] private bool isMoving = false;
    [SerializeField] private bool isAttacking = false;
    
    [Header("Float参数示例")]
    [SerializeField] private float speed = 0f;
    [SerializeField] private float directionX = 0f;
    [SerializeField] private float directionY = 0f;
    
    [Header("Int参数示例")]
    [SerializeField] private int comboCount = 0;
    [SerializeField] private int weaponType = 0; // 0=剑, 1=弓, 2=法杖
    
    #region 参数哈希缓存（性能优化核心）
    
    // Bool参数哈希
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
    private static readonly int IsAttackingHash = Animator.StringToHash("IsAttacking");
    
    // Float参数哈希
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int DirectionXHash = Animator.StringToHash("DirectionX");
    private static readonly int DirectionYHash = Animator.StringToHash("DirectionY");
    
    // Int参数哈希
    private static readonly int ComboCountHash = Animator.StringToHash("ComboCount");
    private static readonly int WeaponTypeHash = Animator.StringToHash("WeaponType");
    
    // Trigger参数哈希
    private static readonly int JumpTriggerHash = Animator.StringToHash("Jump");
    private static readonly int AttackTriggerHash = Animator.StringToHash("Attack");
    private static readonly int HurtTriggerHash = Animator.StringToHash("Hurt");
    private static readonly int DieTriggerHash = Animator.StringToHash("Die");
    
    #endregion
    
    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }
    
    private void Update()
    {
        // 测试输入
        HandleTestInput();
        
        // 同步参数到Animator
        UpdateAnimatorParameters();
    }
    
    /// <summary>
    /// 处理测试输入
    /// </summary>
    private void HandleTestInput()
    {
        // Bool参数测试
        if (Input.GetKeyDown(KeyCode.G))
        {
            ToggleGrounded();
        }
        
        if (Input.GetKeyDown(KeyCode.M))
        {
            ToggleMoving();
        }
        
        // Float参数测试
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        directionX = h;
        directionY = v;
        speed = new Vector2(h, v).magnitude;
        
        // Trigger参数测试
        if (Input.GetKeyDown(KeyCode.J))
        {
            TriggerJump();
        }
        
        if (Input.GetKeyDown(KeyCode.K))
        {
            TriggerAttack();
        }
        
        if (Input.GetKeyDown(KeyCode.H))
        {
            TriggerHurt();
        }
        
        // Int参数测试
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SetWeaponType(0); // 剑
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SetWeaponType(1); // 弓
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SetWeaponType(2); // 法杖
        }
        
        // Combo测试
        if (Input.GetKeyDown(KeyCode.C))
        {
            IncrementCombo();
        }
        
        if (Input.GetKeyDown(KeyCode.V))
        {
            ResetCombo();
        }
    }
    
    /// <summary>
    /// 同步参数到Animator
    /// </summary>
    private void UpdateAnimatorParameters()
    {
        // Bool参数
        animator.SetBool(IsGroundedHash, isGrounded);
        animator.SetBool(IsMovingHash, isMoving);
        animator.SetBool(IsAttackingHash, isAttacking);
        
        // Float参数
        animator.SetFloat(SpeedHash, speed);
        animator.SetFloat(DirectionXHash, directionX);
        animator.SetFloat(DirectionYHash, directionY);
        
        // Int参数
        animator.SetInteger(ComboCountHash, comboCount);
        animator.SetInteger(WeaponTypeHash, weaponType);
    }
    
    #region Bool参数控制
    
    /// <summary>
    /// 切换地面状态
    /// </summary>
    public void ToggleGrounded()
    {
        isGrounded = !isGrounded;
        Debug.Log($"地面状态: {isGrounded}");
    }
    
    /// <summary>
    /// 设置地面状态
    /// </summary>
    /// <param name="grounded">是否在地面</param>
    public void SetGrounded(bool grounded)
    {
        isGrounded = grounded;
    }
    
    /// <summary>
    /// 切换移动状态
    /// </summary>
    public void ToggleMoving()
    {
        isMoving = !isMoving;
        Debug.Log($"移动状态: {isMoving}");
    }
    
    /// <summary>
    /// 设置移动状态
    /// </summary>
    /// <param name="moving">是否移动</param>
    public void SetMoving(bool moving)
    {
        isMoving = moving;
    }
    
    /// <summary>
    /// 设置攻击状态
    /// </summary>
    /// <param name="attacking">是否攻击</param>
    public void SetAttacking(bool attacking)
    {
        isAttacking = attacking;
    }
    
    #endregion
    
    #region Float参数控制
    
    /// <summary>
    /// 设置移动速度
    /// </summary>
    /// <param name="moveSpeed">速度值</param>
    public void SetSpeed(float moveSpeed)
    {
        speed = Mathf.Clamp01(moveSpeed);
    }
    
    /// <summary>
    /// 设置移动方向
    /// </summary>
    /// <param name="x">X方向</param>
    /// <param name="y">Y方向</param>
    public void SetDirection(float x, float y)
    {
        directionX = Mathf.Clamp(x, -1f, 1f);
        directionY = Mathf.Clamp(y, -1f, 1f);
    }
    
    /// <summary>
    /// 设置移动方向（Vector2版本）
    /// </summary>
    /// <param name="direction">方向向量</param>
    public void SetDirection(Vector2 direction)
    {
        SetDirection(direction.x, direction.y);
    }
    
    #endregion
    
    #region Int参数控制
    
    /// <summary>
    /// 设置武器类型
    /// </summary>
    /// <param name="type">武器类型（0=剑, 1=弓, 2=法杖）</param>
    public void SetWeaponType(int type)
    {
        weaponType = Mathf.Clamp(type, 0, 2);
        
        string weaponName = "";
        switch (weaponType)
        {
            case 0: weaponName = "剑"; break;
            case 1: weaponName = "弓"; break;
            case 2: weaponName = "法杖"; break;
        }
        
        Debug.Log($"切换武器: {weaponName}");
    }
    
    /// <summary>
    /// 增加连击数
    /// </summary>
    public void IncrementCombo()
    {
        comboCount++;
        Debug.Log($"连击数: {comboCount}");
        
        // 连击数上限
        if (comboCount > 10)
        {
            comboCount = 1;
        }
    }
    
    /// <summary>
    /// 重置连击数
    /// </summary>
    public void ResetCombo()
    {
        comboCount = 0;
        Debug.Log("连击数已重置");
    }
    
    /// <summary>
    /// 设置连击数
    /// </summary>
    /// <param name="count">连击数</param>
    public void SetCombo(int count)
    {
        comboCount = Mathf.Max(0, count);
    }
    
    #endregion
    
    #region Trigger参数控制
    
    /// <summary>
    /// 触发跳跃动画
    /// </summary>
    public void TriggerJump()
    {
        animator.SetTrigger(JumpTriggerHash);
        Debug.Log("触发跳跃动画");
    }
    
    /// <summary>
    /// 触发攻击动画
    /// </summary>
    public void TriggerAttack()
    {
        animator.SetTrigger(AttackTriggerHash);
        Debug.Log("触发攻击动画");
    }
    
    /// <summary>
    /// 触发受伤动画
    /// </summary>
    public void TriggerHurt()
    {
        animator.SetTrigger(HurtTriggerHash);
        Debug.Log("触发受伤动画");
    }
    
    /// <summary>
    /// 触发死亡动画
    /// </summary>
    public void TriggerDie()
    {
        animator.SetTrigger(DieTriggerHash);
        Debug.Log("触发死亡动画");
    }
    
    /// <summary>
    /// 重置跳跃触发器
    /// </summary>
    public void ResetJumpTrigger()
    {
        animator.ResetTrigger(JumpTriggerHash);
    }
    
    /// <summary>
    /// 重置攻击触发器
    /// </summary>
    public void ResetAttackTrigger()
    {
        animator.ResetTrigger(AttackTriggerHash);
    }
    
    #endregion
    
    #region 组合参数控制
    
    /// <summary>
    /// 执行跳跃（设置Bool + Trigger）
    /// </summary>
    public void PerformJump()
    {
        SetGrounded(false);
        TriggerJump();
    }
    
    /// <summary>
    /// 落地处理
    /// </summary>
    public void OnLand()
    {
        SetGrounded(true);
        ResetCombo();
    }
    
    /// <summary>
    /// 执行攻击（设置Bool + Trigger + Combo）
    /// </summary>
    public void PerformAttack()
    {
        SetAttacking(true);
        IncrementCombo();
        TriggerAttack();
        
        // 攻击结束后重置（实际项目中应通过动画事件调用）
        Invoke("EndAttack", 0.5f);
    }
    
    /// <summary>
    /// 结束攻击
    /// </summary>
    private void EndAttack()
    {
        SetAttacking(false);
    }
    
    #endregion
    
    #region 参数查询方法
    
    /// <summary>
    /// 获取当前速度参数值
    /// </summary>
    /// <returns>速度值</returns>
    public float GetSpeed()
    {
        return animator.GetFloat(SpeedHash);
    }
    
    /// <summary>
    /// 获取当前地面状态
    /// </summary>
    /// <returns>是否在地面</returns>
    public bool GetIsGrounded()
    {
        return animator.GetBool(IsGroundedHash);
    }
    
    /// <summary>
    /// 获取当前连击数
    /// </summary>
    /// <returns>连击数</returns>
    public int GetComboCount()
    {
        return animator.GetInteger(ComboCountHash);
    }
    
    #endregion
    
    #region 编辑器调试
    
    #if UNITY_EDITOR
    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 350, 350));
        GUILayout.Label("=== 动画参数控制示例 ===");
        GUILayout.Label("\n【Bool参数】");
        GUILayout.Label($"地面状态 (G): {isGrounded}");
        GUILayout.Label($"移动状态 (M): {isMoving}");
        GUILayout.Label($"攻击状态: {isAttacking}");
        
        GUILayout.Label("\n【Float参数】");
        GUILayout.Label($"速度: {speed:F2}");
        GUILayout.Label($"方向X: {directionX:F2}");
        GUILayout.Label($"方向Y: {directionY:F2}");
        
        GUILayout.Label("\n【Int参数】");
        GUILayout.Label($"连击数 (C/V): {comboCount}");
        GUILayout.Label($"武器类型 (1/2/3): {weaponType}");
        
        GUILayout.Label("\n【Trigger参数】");
        GUILayout.Label("J - 跳跃");
        GUILayout.Label("K - 攻击");
        GUILayout.Label("H - 受伤");
        
        GUILayout.Label("\n【移动控制】");
        GUILayout.Label("WASD/方向键 - 移动");
        GUILayout.EndArea();
    }
    #endif
    
    #endregion
}