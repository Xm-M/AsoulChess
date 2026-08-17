using UnityEngine;

namespace AsoulChess.Game.Core.Physics
{
    public interface IPhysics2DQueryBufferPool
    {
        RaycastHit2D[] RentHitBuffer(int size);
        Collider2D[] RentColliderBuffer(int size);
        void ReturnHitBuffer(int size, RaycastHit2D[] buffer);
        void ReturnColliderBuffer(int size, Collider2D[] buffer);
        void ClearAll();
    }
}
