using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 示例：多玩家输入处理
/// 演示如何使用PlayerInputManager处理本地多人游戏
/// </summary>
public class ExampleInputMulti : MonoBehaviour
{
    [Header("玩家预制体")]
    [SerializeField] private GameObject playerPrefab;
    
    [Header("PlayerInputManager")]
    [SerializeField] private PlayerInputManager inputManager;
    
    void Start()
    {
        // 监听玩家加入和离开
        inputManager.onPlayerJoined += OnPlayerJoined;
        inputManager.onPlayerLeft += OnPlayerLeft;
        
        // 设置最大玩家数
        inputManager.maxPlayerCount = 4;
    }
    
    void OnPlayerJoined(PlayerInput playerInput)
    {
        Debug.Log($"玩家 {playerInput.playerIndex} 加入游戏");
        
        // 获取玩家控制器
        var player = playerInput.GetComponent<PlayerController>();
        if (player != null)
        {
            player.playerId = playerInput.playerIndex;
            player.controlScheme = playerInput.currentControlScheme;
        }
        
        // 设置出生点
        Vector3 spawnPos = GetSpawnPosition(playerInput.playerIndex);
        playerInput.transform.position = spawnPos;
    }
    
    void OnPlayerLeft(PlayerInput playerInput)
    {
        Debug.Log($"玩家 {playerInput.playerIndex} 离开游戏");
    }
    
    Vector3 GetSpawnPosition(int playerIndex)
    {
        // 根据玩家索引返回出生点
        Vector3[] spawnPoints = new Vector3[]
        {
            new Vector3(-5, 0, 0),
            new Vector3(5, 0, 0),
            new Vector3(-5, 3, 0),
            new Vector3(5, 3, 0)
        };
        
        return spawnPoints[playerIndex];
    }
}

/// <summary>
/// 玩家控制器示例
/// </summary>
public class PlayerController : MonoBehaviour
{
    public int playerId;
    public string controlScheme;
    
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Rigidbody2D rb;
    
    private Vector2 moveInput;
    
    void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
    
    void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Debug.Log($"玩家{playerId}跳跃");
        }
    }
    
    void FixedUpdate()
    {
        rb.velocity = moveInput * moveSpeed;
    }
}
