using UnityEngine;
using AsoulChess.Game.Core.Physics;
using AsoulChess.Game.Core.Services;

public class CheckObjectPoolManage
{
    static IPhysics2DQueryBufferPool Buffers
    {
        get
        {
            if (GameServices.Physics2DBuffers == null)
                GameServices.RegisterDefaults();
            return GameServices.Physics2DBuffers;
        }
    }

    public CheckObjectPoolManage() { }

    public void InitManage() { }

    public static RaycastHit2D[] GetHitArray(int size) => Buffers.RentHitBuffer(size);

    public static Collider2D[] GetColArray(int size) => Buffers.RentColliderBuffer(size);

    public static void ReleaseArray(int size, RaycastHit2D[] array) => Buffers.ReturnHitBuffer(size, array);

    public static void ReleaseColArray(int size, Collider2D[] array) => Buffers.ReturnColliderBuffer(size, array);

    public static void WhenStadgeClear() => Buffers.ClearAll();

    public static void ClearAll() => Buffers.ClearAll();
}

public interface IManager
{
    void InitManage();
    void OnGameStart();
    void OnGameOver();
}
