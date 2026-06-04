using UnityEngine;

/// <summary>钉耙：秒杀踩到的第一只敌方棋子后离场。</summary>
public class TileEffect_Rake : TileEffect
{
    bool consumed;

    public override void EnterTile(Tile tile)
    {
        if (tile == null)
        {
            ObjectPool.instance?.Recycle(gameObject);
            return;
        }
        if (!tile.AddObjectToTile<TileEffect_Rake>(this))
            ObjectPool.instance?.Recycle(gameObject);
    }

    public override void LeaveTile(Tile tile)
    {
        if (tile != null)
            tile.RemoveObjectFromTile<TileEffect_Rake>();
        ObjectPool.instance?.Recycle(gameObject);
    }

    public override void ResetTileEffect()
    {
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (consumed) return;
        TryConsume(collision);
    }

    protected override void OnTriggerStay2D(Collider2D collision)
    {
        if (consumed) return;
        TryConsume(collision);
    }

    void TryConsume(Collider2D collision)
    {
        Chess chess = collision != null ? collision.GetComponent<Chess>() : null;
        if (chess == null || chess.IfDeath || !chess.CompareTag("Enemy")) return;

        consumed = true;
        float maxHp = chess.propertyController != null ? chess.propertyController.GetMaxHp() : 1f;
        var dm = new DamageMessege(null, chess, maxHp * 100f, DamageType.Real);
        chess.propertyController?.GetDamage(dm);

        Tile tile = chess.moveController != null ? chess.moveController.standTile : null;
        LeaveTile(tile);
    }
}
