using System.Collections.Generic;
using UnityEngine;

public class GameOverMap : MonoBehaviour
{
    Collider2D _col;
    ContactFilter2D _filter;
    readonly Collider2D[] _results = new Collider2D[8];

    void Awake()
    {
        _col = GetComponent<Collider2D>();
        if (_col != null)
        {
            _filter = new ContactFilter2D();
            _filter.useLayerMask = true;
            _filter.useTriggers = true;
            // 同时检测 Enemy 和 Unselectable（气球僵尸等），避免 layer 不碰撞导致漏检
            int enemyLayer = LayerMask.NameToLayer("Enemy");
            int unselectableLayer = LayerMask.NameToLayer("Unselectable");
            _filter.layerMask = (1 << enemyLayer) | (unselectableLayer >= 0 ? (1 << unselectableLayer) : 0);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy") && LevelManage.instance.currentLevel.levelMode != LevelMode.TestMode)
        {
            LevelManage.instance.GameOver(false);
        }
        else if (collision.gameObject.GetComponent<Chess>() != null)
        {
            collision.gameObject.GetComponent<Chess>().Death();
        }
    }

    void Update()
    {
        if (LevelManage.instance?.currentLevel?.levelMode == LevelMode.TestMode) return;
        if (_col == null || !_col.enabled) return;

        int n = _col.OverlapCollider(_filter, _results);
        for (int i = 0; i < n; i++)
        {
            if (_results[i].CompareTag("Enemy"))
            {
                LevelManage.instance.GameOver(false);
                return;
            }
        }
    }
}
