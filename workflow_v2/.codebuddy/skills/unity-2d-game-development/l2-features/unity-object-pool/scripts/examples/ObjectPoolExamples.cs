using UnityEngine;
using UnityEngine.Pool;
using System.Collections.Generic;

namespace Unity.ObjectPool.Examples
{
    #region 示例1：Unity官方ObjectPool使用

    /// <summary>
    /// 示例1：Unity官方ObjectPool基础使用
    /// Unity 2021+推荐使用
    /// </summary>
    public class UnityObjectPoolExample : MonoBehaviour
    {
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private int initialPoolSize = 10;
        [SerializeField] private int maxPoolSize = 50;

        private ObjectPool<GameObject> bulletPool;

        private void Awake()
        {
            // 创建对象池
            bulletPool = new ObjectPool<GameObject>(
                createFunc: CreateBullet,
                actionOnGet: OnGetBullet,
                actionOnRelease: OnReleaseBullet,
                actionOnDestroy: OnDestroyBullet,
                collectionCheck: true,
                defaultCapacity: initialPoolSize,
                maxSize: maxPoolSize
            );
        }

        private GameObject CreateBullet()
        {
            GameObject bullet = Instantiate(bulletPrefab);
            bullet.name = "Bullet_Pooled";
            return bullet;
        }

        private void OnGetBullet(GameObject bullet)
        {
            bullet.SetActive(true);
            bullet.transform.position = transform.position;
        }

        private void OnReleaseBullet(GameObject bullet)
        {
            bullet.SetActive(false);
            bullet.transform.SetParent(transform);
        }

        private void OnDestroyBullet(GameObject bullet)
        {
            Destroy(bullet);
        }

        public void FireBullet()
        {
            GameObject bullet = bulletPool.Get();
            // 子弹发射逻辑...
        }

        public void ReturnBullet(GameObject bullet)
        {
            bulletPool.Release(bullet);
        }

        private void OnDestroy()
        {
            bulletPool.Clear();
        }
    }

    #endregion

    #region 示例2：IPoolable接口模式

    /// <summary>
    /// IPoolable接口
    /// 定义池化对象的行为
    /// </summary>
    public interface IPoolable
    {
        void OnGetFromPool();
        void OnReturnToPool();
    }

    /// <summary>
    /// 示例2：实现IPoolable的对象池
    /// 对象自己管理重置逻辑
    /// </summary>
    public class PoolableBullet : MonoBehaviour, IPoolable
    {
        private Rigidbody rb;
        private TrailRenderer trail;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            trail = GetComponent<TrailRenderer>();
        }

        public void OnGetFromPool()
        {
            gameObject.SetActive(true);
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            trail?.Clear();
        }

        public void OnReturnToPool()
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            trail?.Clear();
            gameObject.SetActive(false);
        }

        private void OnCollisionEnter(Collision collision)
        {
            // 碰撞后返回池中
            PoolManager.Instance?.ReturnObject("Bullet", gameObject);
        }
    }

    #endregion

    #region 示例3：通用对象池管理器

    /// <summary>
    /// 示例3：多类型对象池管理器
    /// 管理多种预制体的对象池
    /// </summary>
    public class PoolManager : MonoBehaviour
    {
        public static PoolManager Instance { get; private set; }

        [System.Serializable]
        public class PoolConfig
        {
            public string key;
            public GameObject prefab;
            public int initialSize = 10;
            public int maxSize = 50;
        }

        [SerializeField] private List<PoolConfig> poolConfigs;
        
        private Dictionary<string, ObjectPool<GameObject>> pools;
        private Dictionary<string, Transform> poolContainers;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            InitializePools();
        }

        private void InitializePools()
        {
            pools = new Dictionary<string, ObjectPool<GameObject>>();
            poolContainers = new Dictionary<string, Transform>();

            foreach (var config in poolConfigs)
            {
                CreatePool(config);
            }
        }

        private void CreatePool(PoolConfig config)
        {
            // 创建容器
            Transform container = new GameObject($"Pool_{config.key}").transform;
            container.SetParent(transform);
            poolContainers[config.key] = container;

            // 创建对象池
            var pool = new ObjectPool<GameObject>(
                createFunc: () =>
                {
                    GameObject obj = Instantiate(config.prefab, container);
                    obj.name = $"{config.key}_Pooled";
                    return obj;
                },
                actionOnGet: (obj) =>
                {
                    obj.SetActive(true);
                    var poolable = obj.GetComponent<IPoolable>();
                    poolable?.OnGetFromPool();
                },
                actionOnRelease: (obj) =>
                {
                    var poolable = obj.GetComponent<IPoolable>();
                    poolable?.OnReturnToPool();
                    obj.SetActive(false);
                    obj.transform.SetParent(container);
                },
                actionOnDestroy: (obj) => Destroy(obj),
                collectionCheck: true,
                defaultCapacity: config.initialSize,
                maxSize: config.maxSize
            );

            pools[config.key] = pool;

            // 预热
            PrewarmPool(config.key, config.initialSize);
        }

        public GameObject GetObject(string key)
        {
            if (pools.TryGetValue(key, out var pool))
            {
                return pool.Get();
            }
            Debug.LogWarning($"Pool not found: {key}");
            return null;
        }

        public void ReturnObject(string key, GameObject obj)
        {
            if (pools.TryGetValue(key, out var pool))
            {
                pool.Release(obj);
            }
        }

        public void PrewarmPool(string key, int count)
        {
            if (pools.TryGetValue(key, out var pool))
            {
                var tempObjects = new List<GameObject>();
                for (int i = 0; i < count; i++)
                {
                    tempObjects.Add(pool.Get());
                }
                foreach (var obj in tempObjects)
                {
                    pool.Release(obj);
                }
            }
        }

        public void ClearPool(string key)
        {
            if (pools.TryGetValue(key, out var pool))
            {
                pool.Clear();
            }
        }

        public void ClearAllPools()
        {
            foreach (var pool in pools.Values)
            {
                pool.Clear();
            }
        }

        public int GetPoolCount(string key)
        {
            if (pools.TryGetValue(key, out var pool))
            {
                return pool.CountAll;
            }
            return 0;
        }
    }

    #endregion

    #region 示例4：自定义简单对象池

    /// <summary>
    /// 示例4：简单的自定义对象池
    /// 适用于不需要Unity官方ObjectPool的场景
    /// </summary>
    public class SimplePool<T> where T : class, new()
    {
        private Stack<T> pool;
        private int maxSize;
        private System.Action<T> resetAction;

        public SimplePool(int initialSize, int maxSize, System.Action<T> resetAction = null)
        {
            this.maxSize = maxSize;
            this.resetAction = resetAction;
            pool = new Stack<T>(initialSize);

            for (int i = 0; i < initialSize; i++)
            {
                pool.Push(new T());
            }
        }

        public T Get()
        {
            return pool.Count > 0 ? pool.Pop() : new T();
        }

        public void Release(T item)
        {
            if (pool.Count < maxSize)
            {
                resetAction?.Invoke(item);
                pool.Push(item);
            }
        }

        public void Clear()
        {
            pool.Clear();
        }

        public int Count => pool.Count;
    }

    // 使用示例
    public class SimplePoolExample : MonoBehaviour
    {
        private SimplePool<BulletData> bulletDataPool;

        private void Start()
        {
            bulletDataPool = new SimplePool<BulletData>(20, 100, data =>
            {
                // 重置数据
                data.damage = 0;
                data.speed = 0;
            });
        }

        public BulletData GetBulletData()
        {
            return bulletDataPool.Get();
        }

        public void ReturnBulletData(BulletData data)
        {
            bulletDataPool.Release(data);
        }
    }

    public class BulletData
    {
        public float damage;
        public float speed;
        public Vector3 direction;
    }

    #endregion

    #region 示例5：子弹发射系统

    /// <summary>
    /// 示例5：完整的子弹发射系统
    /// 演示对象池在实际项目中的应用
    /// </summary>
    public class BulletSpawner : MonoBehaviour
    {
        [Header("配置")]
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private int initialPoolSize = 20;
        [SerializeField] private int maxPoolSize = 100;
        [SerializeField] private float bulletSpeed = 30f;
        [SerializeField] private float bulletLifetime = 3f;

        private ObjectPool<GameObject> bulletPool;
        private Transform bulletContainer;

        private void Awake()
        {
            bulletContainer = new GameObject("Bullets").transform;
            
            bulletPool = new ObjectPool<GameObject>(
                createFunc: () =>
                {
                    GameObject bullet = Instantiate(bulletPrefab, bulletContainer);
                    bullet.AddComponent<BulletLifetime>().pool = bulletPool;
                    return bullet;
                },
                actionOnGet: (bullet) =>
                {
                    bullet.SetActive(true);
                    bullet.transform.position = transform.position;
                    bullet.transform.rotation = transform.rotation;
                },
                actionOnRelease: (bullet) =>
                {
                    bullet.SetActive(false);
                    bullet.transform.SetParent(bulletContainer);
                },
                actionOnDestroy: (bullet) => Destroy(bullet),
                collectionCheck: true,
                defaultCapacity: initialPoolSize,
                maxSize: maxPoolSize
            );

            // 预热
            Prewarm(initialPoolSize);
        }

        public void Fire()
        {
            GameObject bullet = bulletPool.Get();
            
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = transform.forward * bulletSpeed;
            }

            var lifetime = bullet.GetComponent<BulletLifetime>();
            if (lifetime != null)
            {
                lifetime.StartLifetime(bulletLifetime);
            }
        }

        private void Prewarm(int count)
        {
            var temp = new List<GameObject>();
            for (int i = 0; i < count; i++)
            {
                temp.Add(bulletPool.Get());
            }
            foreach (var item in temp)
            {
                bulletPool.Release(item);
            }
        }
    }

    /// <summary>
    /// 子弹生命周期管理
    /// </summary>
    public class BulletLifetime : MonoBehaviour
    {
        public ObjectPool<GameObject> pool;
        private float lifetime;
        private float timer;

        public void StartLifetime(float time)
        {
            lifetime = time;
            timer = 0f;
            enabled = true;
        }

        private void Update()
        {
            timer += Time.deltaTime;
            if (timer >= lifetime)
            {
                pool?.Release(gameObject);
                enabled = false;
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            pool?.Release(gameObject);
        }
    }

    #endregion

    #region 示例6：使用using语法

    /// <summary>
    /// 示例6：使用using语法自动释放
    /// Unity ObjectPool支持PooledObject<T>
    /// </summary>
    public class UsingSyntaxExample : MonoBehaviour
    {
        private ObjectPool<GameObject> pool;

        public void UseWithUsing()
        {
            // 使用using语法，作用域结束后自动释放
            using (var pooledObject = pool.Get(out GameObject obj))
            {
                obj.transform.position = transform.position;
                // 使用obj...
            } // 自动调用Release
        }
    }

    #endregion
}
