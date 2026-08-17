using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AsoulChess.Game.Core.Pooling
{
    /// <summary>
    /// GameObject pool with optional dedicated pool scene (AVZ ObjectPool behaviour).
    /// </summary>
    public sealed class GameObjectPool : IGameObjectPool
    {
        readonly Dictionary<string, Stack<GameObject>> _pools = new Dictionary<string, Stack<GameObject>>();
        Scene _poolScene;
        bool _poolSceneCreated;

        public bool ForceDestroyOnRelease { get; set; }

        public GameObject Get(GameObject prefab)
        {
            if (prefab == null)
            {
                Debug.LogWarning("[GameObjectPool] Get called with null prefab.");
                return null;
            }

            EnsurePoolScene();

            string key = prefab.name;
            if (_pools.TryGetValue(key, out var stack))
            {
                while (stack.Count > 0)
                {
                    var cached = stack.Pop();
                    if (cached == null) continue;
                    cached.SetActive(true);
                    return cached;
                }
            }
            else
            {
                _pools[key] = new Stack<GameObject>();
            }

            var created = Object.Instantiate(prefab);
            created.name = key;
            if (_poolSceneCreated && _poolScene.IsValid())
                SceneManager.MoveGameObjectToScene(created, _poolScene);
            return created;
        }

        public void Release(GameObject instance)
        {
            if (instance == null) return;

            if (ForceDestroyOnRelease)
            {
                Object.Destroy(instance);
                return;
            }

            string key = instance.name.Replace("(Clone)", string.Empty).Trim();
            if (!_pools.TryGetValue(key, out var stack))
            {
                stack = new Stack<GameObject>();
                _pools[key] = stack;
            }

            instance.SetActive(false);
            if (!stack.Contains(instance))
                stack.Push(instance);
        }

        public void Clear(bool destroyInactive = true)
        {
            foreach (var pair in _pools)
            {
                while (pair.Value.Count > 0)
                {
                    var go = pair.Value.Pop();
                    if (go == null) continue;
                    if (destroyInactive)
                        Object.Destroy(go);
                }
            }

            _pools.Clear();
        }

        void EnsurePoolScene()
        {
            if (_poolSceneCreated) return;
            _poolScene = SceneManager.CreateScene("AsoulChessObjectPool");
            _poolSceneCreated = true;
        }
    }
}
