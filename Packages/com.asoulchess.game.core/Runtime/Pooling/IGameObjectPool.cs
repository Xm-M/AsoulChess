using UnityEngine;

namespace AsoulChess.Game.Core.Pooling
{
    public interface IGameObjectPool
    {
        bool ForceDestroyOnRelease { get; set; }
        GameObject Get(GameObject prefab);
        void Release(GameObject instance);
        void Clear(bool destroyInactive = true);
    }
}
