using System.Collections.Generic;
using UnityEngine;

namespace AsoulChess.Game.Core.Physics
{
    /// <summary>
    /// NonAlloc Physics2D query buffer pool (from AVZ CheckObjectPoolManage).
    /// </summary>
    public sealed class Physics2DQueryBufferPool : IPhysics2DQueryBufferPool
    {
        sealed class ArrayPool<T>
        {
            readonly Stack<T[]> _pool;
            readonly int _size;

            public ArrayPool(int size, int initialCapacity)
            {
                _size = size;
                _pool = new Stack<T[]>(initialCapacity);
                for (int i = 0; i < initialCapacity; i++)
                    _pool.Push(new T[size]);
            }

            public T[] Rent() => _pool.Count > 0 ? _pool.Pop() : new T[_size];

            public void Return(T[] array)
            {
                if (array != null && array.Length == _size)
                    _pool.Push(array);
            }
        }

        readonly Dictionary<int, ArrayPool<RaycastHit2D>> _hitPools = new Dictionary<int, ArrayPool<RaycastHit2D>>();
        readonly Dictionary<int, ArrayPool<Collider2D>> _colPools = new Dictionary<int, ArrayPool<Collider2D>>();

        static int GetInitialCapacity(int size)
        {
            if (size == 1000) return 3;
            if (size >= 500 && size <= 5000) return 2;
            return 1;
        }

        public RaycastHit2D[] RentHitBuffer(int size)
        {
            if (!_hitPools.TryGetValue(size, out var pool))
            {
                pool = new ArrayPool<RaycastHit2D>(size, GetInitialCapacity(size));
                _hitPools[size] = pool;
            }

            return pool.Rent();
        }

        public Collider2D[] RentColliderBuffer(int size)
        {
            if (!_colPools.TryGetValue(size, out var pool))
            {
                pool = new ArrayPool<Collider2D>(size, GetInitialCapacity(size));
                _colPools[size] = pool;
            }

            return pool.Rent();
        }

        public void ReturnHitBuffer(int size, RaycastHit2D[] buffer)
        {
            if (_hitPools.TryGetValue(size, out var pool))
                pool.Return(buffer);
        }

        public void ReturnColliderBuffer(int size, Collider2D[] buffer)
        {
            if (_colPools.TryGetValue(size, out var pool))
                pool.Return(buffer);
        }

        public void ClearAll()
        {
            _hitPools.Clear();
            _colPools.Clear();
        }
    }
}
