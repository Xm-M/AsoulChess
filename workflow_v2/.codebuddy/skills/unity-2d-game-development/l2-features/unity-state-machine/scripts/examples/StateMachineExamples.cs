using UnityEngine;
using System.Collections.Generic;

namespace Unity.StateMachine.Examples
{
    #region 示例1：枚举状态机

    /// <summary>
    /// 示例1：使用枚举的简单状态机
    /// 最基础的状态机实现方式
    /// </summary>
    public class EnumStateMachine : MonoBehaviour
    {
        public enum State
        {
            Idle,
            Patrol,
            Chase,
            Attack,
            Dead
        }

        private State currentState;
        private float stateTimer;

        private void Start()
        {
            ChangeState(State.Idle);
        }

        private void Update()
        {
            switch (currentState)
            {
                case State.Idle:
                    UpdateIdle();
                    break;
                case State.Patrol:
                    UpdatePatrol();
                    break;
                case State.Chase:
                    UpdateChase();
                    break;
                case State.Attack:
                    UpdateAttack();
                    break;
                case State.Dead:
                    UpdateDead();
                    break;
            }
        }

        private void ChangeState(State newState)
        {
            // 退出当前状态
            ExitState(currentState);
            
            // 切换状态
            currentState = newState;
            stateTimer = 0f;
            
            // 进入新状态
            EnterState(newState);
        }

        private void EnterState(State state)
        {
            Debug.Log($"进入状态: {state}");
            
            switch (state)
            {
                case State.Idle:
                    // 播放待机动画
                    break;
                case State.Patrol:
                    // 开始巡逻
                    break;
                case State.Attack:
                    // 攻击逻辑
                    break;
            }
        }

        private void ExitState(State state)
        {
            // 清理状态资源
        }

        private void UpdateIdle()
        {
            stateTimer += Time.deltaTime;
            if (stateTimer > 2f)
            {
                ChangeState(State.Patrol);
            }
        }

        private void UpdatePatrol() { /* 巡逻逻辑 */ }
        private void UpdateChase() { /* 追击逻辑 */ }
        private void UpdateAttack() { /* 攻击逻辑 */ }
        private void UpdateDead() { /* 死亡逻辑 */ }
    }

    #endregion

    #region 示例2：面向对象状态机

    /// <summary>
    /// 状态接口
    /// </summary>
    public interface IState
    {
        void Enter();
        void Execute();
        void Exit();
    }

    /// <summary>
    /// 状态机管理器
    /// </summary>
    public class StateMachine
    {
        private IState currentState;

        public void ChangeState(IState newState)
        {
            currentState?.Exit();
            currentState = newState;
            currentState.Enter();
        }

        public void Update()
        {
            currentState?.Execute();
        }

        public IState GetCurrentState() => currentState;
    }

    /// <summary>
    /// 示例2：使用状态模式的面向对象状态机
    /// 更灵活、可扩展的状态机设计
    /// </summary>
    public class ObjectOrientedStateMachine : MonoBehaviour
    {
        private StateMachine stateMachine;
        private Dictionary<string, IState> states;

        private void Awake()
        {
            stateMachine = new StateMachine();
            states = new Dictionary<string, IState>
            {
                { "Idle", new IdleState(this) },
                { "Patrol", new PatrolState(this) },
                { "Chase", new ChaseState(this) }
            };
        }

        private void Start()
        {
            stateMachine.ChangeState(states["Idle"]);
        }

        private void Update()
        {
            stateMachine.Update();
        }

        public void ChangeState(string stateName)
        {
            if (states.TryGetValue(stateName, out IState state))
            {
                stateMachine.ChangeState(state);
            }
        }
    }

    // 具体状态实现
    public class IdleState : IState
    {
        private ObjectOrientedStateMachine owner;
        private float timer;

        public IdleState(ObjectOrientedStateMachine owner)
        {
            this.owner = owner;
        }

        public void Enter()
        {
            Debug.Log("进入Idle状态");
            timer = 0f;
        }

        public void Execute()
        {
            timer += Time.deltaTime;
            if (timer > 2f)
            {
                owner.ChangeState("Patrol");
            }
        }

        public void Exit()
        {
            Debug.Log("退出Idle状态");
        }
    }

    public class PatrolState : IState
    {
        private ObjectOrientedStateMachine owner;

        public PatrolState(ObjectOrientedStateMachine owner)
        {
            this.owner = owner;
        }

        public void Enter() { Debug.Log("进入Patrol状态"); }
        public void Execute() { /* 巡逻逻辑 */ }
        public void Exit() { Debug.Log("退出Patrol状态"); }
    }

    public class ChaseState : IState
    {
        private ObjectOrientedStateMachine owner;

        public ChaseState(ObjectOrientedStateMachine owner)
        {
            this.owner = owner;
        }

        public void Enter() { Debug.Log("进入Chase状态"); }
        public void Execute() { /* 追击逻辑 */ }
        public void Exit() { Debug.Log("退出Chase状态"); }
    }

    #endregion

    #region 示例3：Animator状态机集成

    /// <summary>
    /// 示例3：与Animator集成的状态机
    /// 使用Animator的动画状态机
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class AnimatorStateMachine : MonoBehaviour
    {
        private Animator animator;
        
        // 使用Hash优化性能
        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int IsJumpingHash = Animator.StringToHash("IsJumping");
        private static readonly int AttackTriggerHash = Animator.StringToHash("AttackTrigger");
        private static readonly int DieTriggerHash = Animator.StringToHash("DieTrigger");

        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        public void SetSpeed(float speed)
        {
            // ✅ 使用Hash（推荐）
            animator.SetFloat(SpeedHash, speed);
            
            // ❌ 使用字符串（性能差）
            // animator.SetFloat("Speed", speed);
        }

        public void Jump()
        {
            animator.SetBool(IsJumpingHash, true);
        }

        public void Attack()
        {
            animator.SetTrigger(AttackTriggerHash);
        }

        public void Die()
        {
            animator.SetTrigger(DieTriggerHash);
        }

        // 检查当前动画状态
        public bool IsInState(string stateName)
        {
            return animator.GetCurrentAnimatorStateInfo(0).IsName(stateName);
        }

        // 获取动画进度
        public float GetAnimationProgress()
        {
            return animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
        }
    }

    #endregion

    #region 示例4：游戏流程状态机

    /// <summary>
    /// 示例4：游戏流程状态机（单例模式）
    /// 管理游戏全局状态
    /// </summary>
    public class GameStateMachine : MonoBehaviour
    {
        public static GameStateMachine Instance { get; private set; }

        public enum GameState
        {
            MainMenu,
            Loading,
            Playing,
            Paused,
            GameOver
        }

        private GameState currentState;

        public GameState CurrentState => currentState;

        public event System.Action<GameState> OnStateChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void ChangeGameState(GameState newState)
        {
            if (currentState == newState) return;

            GameState oldState = currentState;
            currentState = newState;

            HandleStateChange(oldState, newState);
            OnStateChanged?.Invoke(newState);
        }

        private void HandleStateChange(GameState oldState, GameState newState)
        {
            switch (newState)
            {
                case GameState.MainMenu:
                    Time.timeScale = 1f;
                    // 显示主菜单
                    break;
                case GameState.Loading:
                    // 显示加载界面
                    break;
                case GameState.Playing:
                    Time.timeScale = 1f;
                    // 隐藏所有UI
                    break;
                case GameState.Paused:
                    Time.timeScale = 0f;
                    // 显示暂停菜单
                    break;
                case GameState.GameOver:
                    // 显示游戏结束界面
                    break;
            }

            Debug.Log($"游戏状态变更: {oldState} → {newState}");
        }
    }

    #endregion

    #region 示例5：泛型状态机

    /// <summary>
    /// 泛型状态机
    /// 可用于任何枚举类型的状态
    /// </summary>
    public class GenericStateMachine<T> where T : System.Enum
    {
        private T currentState;
        private Dictionary<T, System.Action> enterActions;
        private Dictionary<T, System.Action> executeActions;
        private Dictionary<T, System.Action> exitActions;

        public GenericStateMachine()
        {
            enterActions = new Dictionary<T, System.Action>();
            executeActions = new Dictionary<T, System.Action>();
            exitActions = new Dictionary<T, System.Action>();
        }

        public void SetState(T state,
            System.Action onEnter = null,
            System.Action onExecute = null,
            System.Action onExit = null)
        {
            if (onEnter != null) enterActions[state] = onEnter;
            if (onExecute != null) executeActions[state] = onExecute;
            if (onExit != null) exitActions[state] = onExit;
        }

        public void ChangeState(T newState)
        {
            // 退出当前状态
            if (exitActions.TryGetValue(currentState, out var exitAction))
            {
                exitAction?.Invoke();
            }

            // 切换状态
            T previousState = currentState;
            currentState = newState;

            // 进入新状态
            if (enterActions.TryGetValue(currentState, out var enterAction))
            {
                enterAction?.Invoke();
            }
        }

        public void Update()
        {
            if (executeActions.TryGetValue(currentState, out var executeAction))
            {
                executeAction?.Invoke();
            }
        }

        public T GetCurrentState() => currentState;
    }

    // 使用示例
    public class GenericStateMachineExample : MonoBehaviour
    {
        public enum PlayerState
        {
            Idle,
            Walk,
            Run,
            Jump
        }

        private GenericStateMachine<PlayerState> stateMachine;

        private void Start()
        {
            stateMachine = new GenericStateMachine<PlayerState>();
            
            stateMachine.SetState(PlayerState.Idle,
                onEnter: () => Debug.Log("进入Idle"),
                onExecute: () => { /* Idle逻辑 */ }
            );
            
            stateMachine.ChangeState(PlayerState.Idle);
        }

        private void Update()
        {
            stateMachine.Update();
        }
    }

    #endregion
}
