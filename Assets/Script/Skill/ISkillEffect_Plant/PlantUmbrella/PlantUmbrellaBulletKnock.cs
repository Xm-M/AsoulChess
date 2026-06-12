using UnityEngine;

/// <summary>
/// 莴苣保护伞：优先将敌方子弹弹飞出屏幕，失败则销毁。
/// </summary>
public static class PlantUmbrellaBulletKnock
{
    public const int BulletLayerMask = 1 << 6;

    public static bool TryKnockOffScreen(Bullet bullet, Vector2 knockDirection, float knockSpeed, float offScreenPadding)
    {
        if (bullet == null || bullet.shooter == null || bullet.shooter.IfDeath)
            return false;
        if (!bullet.shooter.CompareTag("Enemy"))
            return false;

        Vector2 dir = knockDirection.sqrMagnitude > 1e-4f ? knockDirection.normalized : Vector2.up;
        bullet.transform.right = dir;
        bullet.DisableHitDamage();

        if (bullet.bulletMove is IBulletMoveRight rightMove)
        {
            rightMove.speed = knockSpeed;
            return true;
        }

        if (bullet.bulletMove is IBulletMove_LineMove lineMove)
        {
            lineMove.ApplyKnockOff(dir, knockSpeed);
            return true;
        }

        return false;
    }

    public static void DestroyOrRecycle(Bullet bullet)
    {
        if (bullet == null) return;
        bullet.DisableHitDamage();
        bullet.RecycleBullet();
    }

    public static bool IsOffScreen(Vector2 worldPos, float padding)
    {
        var map = MapManage.instance;
        if (map?.tiles == null || map.mapSize.x <= 0 || map.mapSize.y <= 0)
            return worldPos.y > 15f || worldPos.y < -5f;

        var origin = map.tiles[0, 0].transform.position;
        var topRight = map.tiles[map.mapSize.x - 1, map.mapSize.y - 1].transform.position;
        float minX = origin.x - map.tileSize.x - padding;
        float maxX = topRight.x + map.tileSize.x + padding;
        float minY = origin.y - map.tileSize.y - padding;
        float maxY = topRight.y + map.tileSize.y * 2f + padding;

        return worldPos.x < minX || worldPos.x > maxX || worldPos.y < minY || worldPos.y > maxY;
    }
}
