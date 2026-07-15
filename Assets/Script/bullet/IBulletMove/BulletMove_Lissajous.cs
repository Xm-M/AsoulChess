using UnityEngine;

/// <summary>
/// 李萨如轨迹：x = A·sin(a·t+φ)，y = B·sin(b·t)，中心锚定在射手所在格。
/// A/B/a/b 从 <see cref="OkuwakiCurveKeys"/> 读取；ω ∝ 射手攻速。
/// </summary>
public class BulletMove_Lissajous : IBulletMove
{
    public float omegaScale = 1f;
    public float phase;

    float _t;

    public void InitMove(Bullet bullet)
    {
        _t = 0f;
    }

    public void MoveBullet(Bullet bullet)
    {
        var shooter = bullet?.shooter;
        if (shooter == null || shooter.IfDeath)
            return;

        var ctx = shooter.skillController?.context;
        int ampRow = 1;
        int ampCol = 1;
        int ratioA = 1;
        int ratioB = 1;
        ctx?.TryGet(OkuwakiCurveKeys.AmplitudeRow, out ampRow);
        ctx?.TryGet(OkuwakiCurveKeys.AmplitudeCol, out ampCol);
        ctx?.TryGet(OkuwakiCurveKeys.RatioA, out ratioA);
        ctx?.TryGet(OkuwakiCurveKeys.RatioB, out ratioB);

        var map = MapManage.instance;
        float tileX = map != null ? map.tileSize.x : 1f;
        float tileY = map != null ? map.tileSize.y : 1f;

        float omega = omegaScale * Mathf.Max(0.01f, shooter.propertyController.GetAccelerate());
        _t += Time.deltaTime * omega;

        Vector2 center = OkuwakiGridHelper.GetAnchorWorldPos(shooter);
        float x = ampRow * tileX * Mathf.Sin(ratioA * _t + phase);
        float y = ampCol * tileY * Mathf.Sin(ratioB * _t);
        bullet.transform.position = center + new Vector2(x, y);
    }
}
