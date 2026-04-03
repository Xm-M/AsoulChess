using UnityEngine;

namespace Unity.PrefabSystem.Examples
{
    /// <summary>
    /// 预制体系统示例代码
    /// 演示预制体的实例化、变体使用、嵌套预制体等核心功能
    /// </summary>
    public class PrefabExamples : MonoBehaviour
    {
        #region 字段

        [Header("预制体引用")]
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private GameObject effectPrefab;
        
        [Header("实例化位置")]
        [SerializeField] private Transform spawnPoint;
        
        #endregion

        #region 基础实例化示例

        /// <summary>
        /// 示例1：基础实例化
        /// 最简单的预制体实例化方式
        /// </summary>
        public void BasicInstantiate()
        {
            // 基础实例化
            GameObject enemy = Instantiate(enemyPrefab);
            
            // 指定位置和旋转实例化
            GameObject enemyAtPosition = Instantiate(
                enemyPrefab,
                spawnPoint.position,
                spawnPoint.rotation
            );
            
            // 指定父级实例化
            GameObject enemyWithParent = Instantiate(enemyPrefab, transform);
            
            // 完整参数实例化
            GameObject enemyFull = Instantiate(
                enemyPrefab,
                spawnPoint.position,
                spawnPoint.rotation,
                transform
            );
        }

        /// <summary>
        /// 示例2：泛型实例化（推荐）
        /// 使用泛型方法避免类型转换
        /// </summary>
        public void GenericInstantiate()
        {
            // ❌ 需要类型转换
            // Enemy enemy = (Enemy)Instantiate(enemyPrefab);
            
            // ✅ 泛型实例化（推荐）
            Enemy enemy = Instantiate(enemyPrefab).GetComponent<Enemy>();
            
            // ✅ 如果预制体根对象挂载了组件
            // Enemy enemy = Instantiate<Enemy>(enemyPrefab.GetComponent<Enemy>());
        }

        #endregion

        #region 实例化性能优化示例

        /// <summary>
        /// 示例3：批量实例化优化
        /// 演示如何高效批量实例化预制体
        /// </summary>
        public void BatchInstantiate()
        {
            int count = 100;
            GameObject[] enemies = new GameObject[count];
            
            // ❌ 低效方式：每次都查找组件
            for (int i = 0; i < count; i++)
            {
                GameObject obj = Instantiate(enemyPrefab);
                enemies[i] = obj;
                Enemy enemy = obj.GetComponent<Enemy>(); // 每次GetComponent
            }
            
            // ✅ 优化方式：预先获取组件引用
            Enemy[] enemyComponents = new Enemy[count];
            for (int i = 0; i < count; i++)
            {
                enemyComponents[i] = Instantiate(enemyPrefab).GetComponent<Enemy>();
            }
            
            // ✅ 更优方式：使用对象池（参见unity-object-pool Skill）
        }

        /// <summary>
        /// 示例4：延迟实例化
        /// 使用协程分帧实例化，避免卡顿
        /// </summary>
        public System.Collections.IEnumerator DelayedInstantiate(int count, float interval)
        {
            for (int i = 0; i < count; i++)
            {
                Instantiate(enemyPrefab);
                yield return new WaitForSeconds(interval);
            }
        }

        #endregion

        #region 预制体变体示例

        /// <summary>
        /// 示例5：预制体变体使用
        /// 基础预制体 → 变体预制体
        /// </summary>
        [Header("预制体变体")]
        [SerializeField] private GameObject baseEnemyPrefab;
        [SerializeField] private GameObject fastEnemyPrefab;  // 变体：速度快
        [SerializeField] private GameObject tankEnemyPrefab;  // 变体：血量高
        
        public void SpawnEnemyByType(EnemyType type)
        {
            GameObject prefabToSpawn = type switch
            {
                EnemyType.Fast => fastEnemyPrefab,
                EnemyType.Tank => tankEnemyPrefab,
                _ => baseEnemyPrefab
            };
            
            Instantiate(prefabToSpawn, spawnPoint.position, Quaternion.identity);
        }
        
        public enum EnemyType { Normal, Fast, Tank }

        #endregion

        #region 嵌套预制体示例

        /// <summary>
        /// 示例6：嵌套预制体
        /// 复杂对象的模块化组合
        /// </summary>
        [Header("嵌套预制体")]
        [SerializeField] private GameObject weaponPrefab;
        [SerializeField] private GameObject armorPrefab;
        [SerializeField] private GameObject accessoryPrefab;
        
        public void EquipWeapon(GameObject character)
        {
            Transform weaponSlot = character.transform.Find("WeaponSlot");
            if (weaponSlot != null)
            {
                // 实例化嵌套预制体
                GameObject weapon = Instantiate(weaponPrefab, weaponSlot);
                weapon.localPosition = Vector3.zero;
                weapon.localRotation = Quaternion.identity;
            }
        }

        #endregion

        #region 运行时预制体管理示例

        /// <summary>
        /// 示例7：运行时加载预制体
        /// 从Resources或AssetBundle加载
        /// </summary>
        public void LoadPrefabAtRuntime()
        {
            // ⚠️ Resources.Load（简单但性能一般）
            GameObject prefab = Resources.Load<GameObject>("Prefabs/Enemy");
            if (prefab != null)
            {
                Instantiate(prefab);
            }
            
            // ✅ 推荐：使用Addressables（更高效、内存友好）
            // Addressables.InstantiateAsync("Enemy");
        }

        /// <summary>
        /// 示例8：动态预制体容器
        /// 管理实例化对象的父级
        /// </summary>
        private Transform prefabContainer;
        
        private void Awake()
        {
            // 创建容器对象
            prefabContainer = new GameObject("SpawnedObjects").transform;
        }
        
        public GameObject SpawnWithContainer(GameObject prefab, Vector3 position)
        {
            GameObject instance = Instantiate(prefab, position, Quaternion.identity);
            instance.transform.SetParent(prefabContainer); // 统一管理
            return instance;
        }

        #endregion

        #region 预制体清理示例

        /// <summary>
        /// 示例9：清理实例化对象
        /// 场景切换或重置时清理
        /// </summary>
        public void ClearSpawnedObjects()
        {
            if (prefabContainer != null)
            {
                // 销毁容器下所有子对象
                foreach (Transform child in prefabContainer)
                {
                    Destroy(child.gameObject);
                }
                
                // 或者直接销毁容器
                // Destroy(prefabContainer.gameObject);
            }
        }

        #endregion

        #region 编辑器扩展示例

#if UNITY_EDITOR
        /// <summary>
        /// 示例10：编辑器预制体操作
        /// 仅在编辑器中运行
        /// </summary>
        [UnityEditor.MenuItem("Tools/Prefab/Create Variant Example")]
        public static void CreatePrefabVariantExample()
        {
            // 获取选中预制体
            GameObject selectedPrefab = UnityEditor.Selection.activeObject as GameObject;
            if (selectedPrefab == null) return;
            
            // 创建变体
            string path = $"Assets/Prefabs/{selectedPrefab.name}_Variant.prefab";
            GameObject variant = UnityEditor.PrefabUtility.SaveAsPrefabAsset(
                selectedPrefab,
                path
            );
            
            Debug.Log($"创建预制体变体: {path}");
        }
#endif

        #endregion
    }
}
