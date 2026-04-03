using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 示例：使用InputActionAsset配置
/// 演示如何通过Asset配置多个输入动作
/// </summary>
public class ExampleInputActions : MonoBehaviour
{
    [Header("Input Actions Asset")]
    [SerializeField] private InputActionAsset inputActions;
    
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction fireAction;
    
    void Awake()
    {
        // 从Asset中获取Actions
        var gameplayMap = inputActions.FindActionMap("Gameplay");
        
        moveAction = gameplayMap.FindAction("Move");
        jumpAction = gameplayMap.FindAction("Jump");
        fireAction = gameplayMap.FindAction("Fire");
    }
    
    void OnEnable()
    {
        // 启用所有Actions
        moveAction.Enable();
        jumpAction.Enable();
        fireAction.Enable();
        
        // 订阅事件
        moveAction.performed += OnMove;
        jumpAction.performed += OnJump;
        fireAction.performed += OnFire;
    }
    
    void OnDisable()
    {
        moveAction.Disable();
        jumpAction.Disable();
        fireAction.Disable();
        
        moveAction.performed -= OnMove;
        jumpAction.performed -= OnJump;
        fireAction.performed -= OnFire;
    }
    
    void OnMove(InputAction.CallbackContext context)
    {
        Vector2 move = context.ReadValue<Vector2>();
        Debug.Log($"移动: {move}");
    }
    
    void OnJump(InputAction.CallbackContext context)
    {
        Debug.Log("跳跃！");
    }
    
    void OnFire(InputAction.CallbackContext context)
    {
        Debug.Log("射击！");
    }
}
