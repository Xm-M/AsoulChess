using UnityEngine;

/// <summary>
/// 基础动画控制示例
/// 展示动画播放、暂停、速度控制、反向播放等基础功能
/// 适合Unity初学者学习
/// </summary>
public class ExampleAnimationBasic : MonoBehaviour
{
    [Header("组件引用")]
    [SerializeField] private Animator animator;
    
    [Header("动画控制参数")]
    [SerializeField] private string animationStateName = "Idle";
    [SerializeField] private float animationSpeed = 1.0f;
    [SerializeField] private bool playOnStart = true;
    
    // 状态哈希缓存（性能优化）
    private static readonly int IdleHash = Animator.StringToHash("Idle");
    private static readonly int WalkHash = Animator.StringToHash("Walk");
    
    // 当前播放状态
    private bool isPlaying = true;
    private float defaultSpeed;
    
    private void Awake()
    {
        // 自动获取Animator组件
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        
        // 缓存默认速度
        defaultSpeed = animator.speed;
    }
    
    private void Start()
    {
        // 开始时自动播放
        if (playOnStart)
        {
            PlayAnimation(IdleHash);
        }
    }
    
    private void Update()
    {
        // 测试输入控制
        HandleTestInput();
    }
    
    /// <summary>
    /// 处理测试输入（用于演示）
    /// </summary>
    private void HandleTestInput()
    {
        // 1键：播放Idle动画
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            PlayAnimation(IdleHash);
            Debug.Log("播放Idle动画");
        }
        
        // 2键：播放Walk动画
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            PlayAnimation(WalkHash);
            Debug.Log("播放Walk动画");
        }
        
        // 空格键：暂停/继续
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TogglePause();
        }
        
        // 上箭头：加速
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            SetAnimationSpeed(animationSpeed + 0.5f);
        }
        
        // 下箭头：减速
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            SetAnimationSpeed(animationSpeed - 0.5f);
        }
        
        // R键：反向播放
        if (Input.GetKeyDown(KeyCode.R))
        {
            ToggleReverse();
        }
    }
    
    #region 基础动画控制方法
    
    /// <summary>
    /// 播放指定动画状态
    /// </summary>
    /// <param name="stateHash">状态哈希值（使用StringToHash获取）</param>
    /// <param name="layer">动画层索引（默认为0，基础层）</param>
    public void PlayAnimation(int stateHash, int layer = 0)
    {
        if (animator == null)
        {
            Debug.LogWarning("Animator组件未设置！");
            return;
        }
        
        // 直接播放动画（无过渡）
        animator.Play(stateHash, layer);
        isPlaying = true;
    }
    
    /// <summary>
    /// 播放指定动画状态（字符串版本，不推荐频繁调用）
    /// </summary>
    /// <param name="stateName">状态名称</param>
    public void PlayAnimation(string stateName)
    {
        if (animator == null)
        {
            Debug.LogWarning("Animator组件未设置！");
            return;
        }
        
        // ⚠️ 警告：字符串版本性能较差，建议使用哈希版本
        animator.Play(stateName);
        isPlaying = true;
    }
    
    /// <summary>
    /// 平滑过渡到指定动画状态
    /// </summary>
    /// <param name="stateHash">目标状态哈希值</param>
    /// <param name="transitionDuration">过渡持续时间（归一化值，0-1）</param>
    public void CrossFadeToAnimation(int stateHash, float transitionDuration = 0.25f)
    {
        if (animator == null)
        {
            Debug.LogWarning("Animator组件未设置！");
            return;
        }
        
        // 平滑过渡到目标状态
        animator.CrossFade(stateHash, transitionDuration);
        isPlaying = true;
    }
    
    /// <summary>
    /// 暂停动画播放
    /// </summary>
    public void PauseAnimation()
    {
        if (animator != null)
        {
            animator.speed = 0f;
            isPlaying = false;
            Debug.Log("动画已暂停");
        }
    }
    
    /// <summary>
    /// 继续动画播放
    /// </summary>
    public void ResumeAnimation()
    {
        if (animator != null)
        {
            animator.speed = animationSpeed;
            isPlaying = true;
            Debug.Log("动画继续播放");
        }
    }
    
    /// <summary>
    /// 切换暂停/播放状态
    /// </summary>
    public void TogglePause()
    {
        if (isPlaying)
        {
            PauseAnimation();
        }
        else
        {
            ResumeAnimation();
        }
    }
    
    /// <summary>
    /// 设置动画播放速度
    /// </summary>
    /// <param name="speed">速度倍率（1.0为正常速度）</param>
    public void SetAnimationSpeed(float speed)
    {
        if (animator != null)
        {
            // 限制速度范围
            animationSpeed = Mathf.Clamp(speed, -5f, 5f);
            animator.speed = animationSpeed;
            
            Debug.Log($"动画速度设置为: {animationSpeed}x");
        }
    }
    
    /// <summary>
    /// 重置动画速度为默认值
    /// </summary>
    public void ResetAnimationSpeed()
    {
        SetAnimationSpeed(defaultSpeed);
    }
    
    /// <summary>
    /// 切换正向/反向播放
    /// </summary>
    public void ToggleReverse()
    {
        if (animationSpeed > 0)
        {
            SetAnimationSpeed(-Mathf.Abs(animationSpeed));
            Debug.Log("反向播放动画");
        }
        else
        {
            SetAnimationSpeed(Mathf.Abs(animationSpeed));
            Debug.Log("正向播放动画");
        }
    }
    
    /// <summary>
    /// 从指定时间点播放动画
    /// </summary>
    /// <param name="stateHash">状态哈希值</param>
    /// <param name="normalizedTime">归一化时间（0-1，0为开头，1为结尾）</param>
    public void PlayAtTime(int stateHash, float normalizedTime)
    {
        if (animator != null)
        {
            // 从指定时间点开始播放
            animator.Play(stateHash, 0, normalizedTime);
            isPlaying = true;
        }
    }
    
    /// <summary>
    /// 获取当前动画播放进度
    /// </summary>
    /// <returns>归一化时间（0-1）</returns>
    public float GetAnimationProgress()
    {
        if (animator != null)
        {
            // ⚠️ 注意：避免在Update中频繁调用此方法
            var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            return stateInfo.normalizedTime;
        }
        return 0f;
    }
    
    /// <summary>
    /// 判断当前动画是否播放完成
    /// </summary>
    /// <returns>是否播放完成</returns>
    public bool IsAnimationComplete()
    {
        if (animator != null)
        {
            var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            
            // normalizedTime >= 1.0 表示动画播放完成
            // 注意：对于循环动画，normalizedTime会持续增加
            return stateInfo.normalizedTime >= 1.0f && !stateInfo.loop;
        }
        return false;
    }
    
    /// <summary>
    /// 判断动画是否正在播放
    /// </summary>
    /// <returns>是否正在播放</returns>
    public bool IsPlaying()
    {
        return isPlaying && animator != null && animator.speed > 0f;
    }
    
    #endregion
    
    #region 编辑器调试
    
    #if UNITY_EDITOR
    private void OnGUI()
    {
        // 显示调试信息
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label($"动画状态: {(isPlaying ? "播放中" : "已暂停")}");
        GUILayout.Label($"播放速度: {animationSpeed:F1}x");
        GUILayout.Label($"当前进度: {GetAnimationProgress():F2}");
        GUILayout.Label("\n控制键:");
        GUILayout.Label("1 - 播放Idle动画");
        GUILayout.Label("2 - 播放Walk动画");
        GUILayout.Label("空格 - 暂停/继续");
        GUILayout.Label("↑/↓ - 调整速度");
        GUILayout.Label("R - 反向播放");
        GUILayout.EndArea();
    }
    #endif
    
    #endregion
}