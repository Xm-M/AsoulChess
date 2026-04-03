using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Unity.CoroutineSystem.Examples
{
    /// <summary>
    /// 协程系统示例代码
    /// 演示协程的核心用法和最佳实践
    /// </summary>
    public class CoroutineExamples : MonoBehaviour
    {
        #region 示例1：基础协程

        /// <summary>
        /// 示例1：基础协程
        /// 协程的基本结构和启动方式
        /// </summary>
        public void BasicCoroutine()
        {
            // 启动协程
            StartCoroutine(SimpleCoroutine());
            
            // 带参数的协程
            StartCoroutine(CoroutineWithParam(5f));
        }

        private IEnumerator SimpleCoroutine()
        {
            Debug.Log("协程开始");
            yield return new WaitForSeconds(2f);
            Debug.Log("2秒后执行");
        }

        private IEnumerator CoroutineWithParam(float delay)
        {
            yield return new WaitForSeconds(delay);
            Debug.Log($"延迟{delay}秒后执行");
        }

        #endregion

        #region 示例2：WaitForSeconds缓存

        /// <summary>
        /// 示例2：WaitForSeconds缓存
        /// 避免每次yield都创建新对象，减少GC
        /// </summary>
        
        // ❌ 错误方式：每次都创建新对象
        private IEnumerator BadWaitCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(1f); // 每次产生GC
            }
        }

        // ✅ 正确方式：缓存WaitForSeconds
        private WaitForSeconds waitOneSecond = new WaitForSeconds(1f);
        private WaitForSeconds waitHalfSecond = new WaitForSeconds(0.5f);

        private IEnumerator GoodWaitCoroutine()
        {
            while (true)
            {
                yield return waitOneSecond; // 无GC
                Debug.Log("每秒执行一次");
            }
        }

        #endregion

        #region 示例3：协程停止

        /// <summary>
        /// 示例3：协程停止
        /// 正确的停止协程方式
        /// </summary>
        private Coroutine runningCoroutine;
        private IEnumerator currentRoutine;

        public void StartAndStopCoroutine()
        {
            // ✅ 方式1：使用Coroutine引用（推荐）
            runningCoroutine = StartCoroutine(LongRunningCoroutine());
            
            // 停止
            if (runningCoroutine != null)
            {
                StopCoroutine(runningCoroutine);
                runningCoroutine = null;
            }

            // ✅ 方式2：使用IEnumerator引用
            currentRoutine = LongRunningCoroutine();
            StartCoroutine(currentRoutine);
            StopCoroutine(currentRoutine);

            // ❌ 不推荐：使用字符串
            // StartCoroutine("LongRunningCoroutine");
            // StopCoroutine("LongRunningCoroutine");

            // 停止所有协程
            StopAllCoroutines();
        }

        private IEnumerator LongRunningCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(1f);
                Debug.Log("运行中...");
            }
        }

        #endregion

        #region 示例4：分帧执行

        /// <summary>
        /// 示例4：分帧执行
        /// 避免单帧处理大量数据导致卡顿
        /// </summary>
        public void ProcessInBatches()
        {
            StartCoroutine(ProcessItemsInBatches(1000, 50));
        }

        private IEnumerator ProcessItemsInBatches(int totalItems, int itemsPerFrame)
        {
            for (int i = 0; i < totalItems; i++)
            {
                // 处理单个物品
                ProcessItem(i);

                // 每处理N个物品暂停一帧
                if (i % itemsPerFrame == 0)
                {
                    yield return null;
                }
            }

            Debug.Log("所有物品处理完成");
        }

        private void ProcessItem(int index)
        {
            // 处理逻辑
        }

        #endregion

        #region 示例5：协程嵌套

        /// <summary>
        /// 示例5：协程嵌套
        /// 一个协程等待另一个协程完成
        /// </summary>
        public void NestedCoroutine()
        {
            StartCoroutine(ParentCoroutine());
        }

        private IEnumerator ParentCoroutine()
        {
            Debug.Log("父协程开始");
            
            // 等待子协程完成
            yield return StartCoroutine(ChildCoroutine());
            
            Debug.Log("子协程完成后继续");
        }

        private IEnumerator ChildCoroutine()
        {
            Debug.Log("子协程开始");
            yield return new WaitForSeconds(2f);
            Debug.Log("子协程结束");
        }

        #endregion

        #region 示例6：条件等待

        /// <summary>
        /// 示例6：条件等待
        /// WaitUntil和WaitWhile的使用
        /// </summary>
        private bool isReady = false;
        private bool isPaused = false;

        public void ConditionalWait()
        {
            StartCoroutine(WaitForReady());
            StartCoroutine(WaitWhilePaused());
        }

        private IEnumerator WaitForReady()
        {
            // 等待条件为true
            yield return new WaitUntil(() => isReady);
            Debug.Log("准备就绪！");
        }

        private IEnumerator WaitWhilePaused()
        {
            // 等待条件为false
            yield return new WaitWhile(() => isPaused);
            Debug.Log("继续执行！");
        }

        // 外部触发条件
        public void SetReady() => isReady = true;
        public void SetPaused(bool paused) => isPaused = paused;

        #endregion

        #region 示例7：自定义YieldInstruction

        /// <summary>
        /// 示例7：自定义等待条件
        /// 继承CustomYieldInstruction创建自定义等待
        /// </summary>
        public class WaitForAnimation : CustomYieldInstruction
        {
            private Animator animator;
            private string stateName;

            public WaitForAnimation(Animator animator, string stateName)
            {
                this.animator = animator;
                this.stateName = stateName;
            }

            public override bool keepWaiting
            {
                get
                {
                    // 动画正在播放时继续等待
                    return animator.GetCurrentAnimatorStateInfo(0).IsName(stateName);
                }
            }
        }

        public void CustomWait()
        {
            Animator animator = GetComponent<Animator>();
            StartCoroutine(WaitForAnimationCoroutine(animator, "Attack"));
        }

        private IEnumerator WaitForAnimationCoroutine(Animator animator, string stateName)
        {
            animator.SetTrigger("Attack");
            yield return new WaitForAnimation(animator, stateName);
            Debug.Log("动画播放完成");
        }

        #endregion

        #region 示例8：协程队列

        /// <summary>
        /// 示例8：协程队列
        /// 顺序执行多个协程
        /// </summary>
        public class CoroutineQueue
        {
            private Queue<IEnumerator> queue = new Queue<IEnumerator>();
            private MonoBehaviour owner;
            private bool isRunning;

            public CoroutineQueue(MonoBehaviour owner)
            {
                this.owner = owner;
            }

            public void Enqueue(IEnumerator coroutine)
            {
                queue.Enqueue(coroutine);
                if (!isRunning)
                {
                    owner.StartCoroutine(ProcessQueue());
                }
            }

            private IEnumerator ProcessQueue()
            {
                isRunning = true;
                while (queue.Count > 0)
                {
                    yield return owner.StartCoroutine(queue.Dequeue());
                }
                isRunning = false;
            }
        }

        private CoroutineQueue coroutineQueue;

        private void Awake()
        {
            coroutineQueue = new CoroutineQueue(this);
        }

        public void QueueExample()
        {
            coroutineQueue.Enqueue(Task1());
            coroutineQueue.Enqueue(Task2());
            coroutineQueue.Enqueue(Task3());
        }

        private IEnumerator Task1()
        {
            Debug.Log("任务1开始");
            yield return new WaitForSeconds(1f);
            Debug.Log("任务1完成");
        }

        private IEnumerator Task2()
        {
            Debug.Log("任务2开始");
            yield return new WaitForSeconds(1f);
            Debug.Log("任务2完成");
        }

        private IEnumerator Task3()
        {
            Debug.Log("任务3开始");
            yield return new WaitForSeconds(1f);
            Debug.Log("任务3完成");
        }

        #endregion

        #region 示例9：超时检测

        /// <summary>
        /// 示例9：超时检测
        /// 协程执行超时处理
        /// </summary>
        public IEnumerator WithTimeout(IEnumerator coroutine, float timeout)
        {
            float startTime = Time.time;

            while (coroutine.MoveNext())
            {
                if (Time.time - startTime > timeout)
                {
                    Debug.LogWarning("协程执行超时！");
                    yield break;
                }
                yield return coroutine.Current;
            }
        }

        public void TimeoutExample()
        {
            StartCoroutine(WithTimeout(LongTask(), 5f));
        }

        private IEnumerator LongTask()
        {
            for (int i = 0; i < 100; i++)
            {
                yield return new WaitForSeconds(0.1f);
            }
        }

        #endregion

        #region 示例10：协程生命周期管理

        /// <summary>
        /// 示例10：协程生命周期管理
        /// 确保协程正确停止
        /// </summary>
        private Coroutine gameCoroutine;

        private void OnEnable()
        {
            gameCoroutine = StartCoroutine(GameLoop());
        }

        private void OnDisable()
        {
            // 对象禁用时停止协程
            if (gameCoroutine != null)
            {
                StopCoroutine(gameCoroutine);
                gameCoroutine = null;
            }
        }

        private void OnDestroy()
        {
            // 对象销毁时停止所有协程
            StopAllCoroutines();
        }

        private IEnumerator GameLoop()
        {
            while (true)
            {
                // 游戏循环逻辑
                yield return null;
            }
        }

        #endregion
    }
}
