using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class SwordController : MonoBehaviour
{
    //[LabelText("旋转角度")]
    public float orbitAngle;   // 当前绕角色旋转的角度（弧度）
    public float orbitRadius;  // 当前半径
    public Vector2 orbitOffset; // 角色头顶的偏移
    Transform center;          // 旋转中心（角色）
    Animator animator;
    public SpriteRenderer spriteR ;
    public void InitOrbit(Transform center, Vector2 offset)
    {
        this.center = center;
        orbitOffset = offset;
        animator = GetComponent<Animator>();
        //spriteR = GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// 升起的函数
    /// </summary>
    /// <param name="height"> 升起的高度 </param>
    /// <param name="duration"> 升起的总时间 </param>
    /// <returns></returns>
    public void Rise(float height, float duration)
    {
        
        animator.Play("sword_up"); 
    }

    // 从当前世界位置 -> 过渡到绕角色旋转（收拢到最小半径）
    /// <summary>
    /// 移动到角色头顶 并围绕着旋转
    /// </summary>
    /// <param name="minRadius"> 最小围绕半径 </param>
    /// <param name="duration"> 靠拢时间 </param>
    /// <returns></returns>
    //public IEnumerator MoveToMinOrbit(float minRadius, float duration, float angularSpeedDeg)
    //{
    //    Vector2 centerPos = (Vector2)center.position + orbitOffset;

    //    // 先算当前相对位置的半径和角度
    //    Vector2 fromCenter = (Vector2)transform.position - centerPos;
    //    float startRadius = fromCenter.magnitude;
    //    orbitAngle = Mathf.Atan2(fromCenter.y, fromCenter.x); // 作为初始角度
    //    orbitRadius = startRadius;

    //    float t = 0f;
    //    float speed = angularSpeedDeg * Mathf.Deg2Rad; // 转为弧度速度

    //    while (t < 1f)
    //    {
    //        t += Time.deltaTime / duration;

    //        // 半径越来越小
    //        orbitRadius = Mathf.Lerp(startRadius, minRadius, t);
    //        // 角度一直增加，让它绕圈
    //        orbitAngle += speed * Time.deltaTime;

    //        Vector2 pos = centerPos
    //            + new Vector2(Mathf.Cos(orbitAngle), Mathf.Sin(orbitAngle)) * orbitRadius;
    //        transform.position = pos;

    //        yield return null;
    //    }
    //}
   public IEnumerator MoveToMinEllipseToAngle(
    float minRadiusX,      // 椭圆长半轴
    float minRadiusY,      // 椭圆短半轴
    float duration,
    float targetAngleRad,  // 目标角度（弧度），最后要停在这里
    float extraTurns = 1f  // 额外多转几圈（0 = 不多转）
)
{
    Vector2 centerPos = (Vector2)center.position + orbitOffset;

    // 当前世界位置相对中心
    Vector2 fromCenter = (Vector2)transform.position - centerPos;
    float startAngle = Mathf.Atan2(fromCenter.y, fromCenter.x);
    float len = fromCenter.magnitude;

    // —— 计算“当前所在大椭圆”的半径（和最终小椭圆保持同样的扁度）——
    // 设最终椭圆扁度 ratio = b / a
    float ratio = minRadiusY / minRadiusX;

    float dirX = Mathf.Cos(startAngle);
    float dirY = Mathf.Sin(startAngle);

    // 找到一个 s，使得 (dirX*s, dirY*s*ratio) 的长度 ≈ 当前半径 len
    float denom = Mathf.Sqrt(dirX * dirX + dirY * dirY * ratio * ratio);
    float s0 = (denom > 0f) ? (len / denom) : len;

    float startRadiusX = s0;        // 当前“大椭圆”的长半轴
    float startRadiusY = s0 * ratio;// 当前“大椭圆”的短半轴

    // 最终想转到的角度：目标角度 + N 圈
    float finalAngle = targetAngleRad + extraTurns * Mathf.PI * 2f;

    float t = 0f;
    while (t < 1f)
    {
        t += Time.deltaTime / duration;
        float k = Mathf.Clamp01(t);

        // 椭圆半径由大 → 小
        float rX = Mathf.Lerp(startRadiusX, minRadiusX, k);
        float rY = Mathf.Lerp(startRadiusY, minRadiusY, k);
        // 角度由当前角度 → 目标角度 + 多转几圈
        float a  = Mathf.Lerp(startAngle, finalAngle, k);

        Vector2 local = new Vector2(Mathf.Cos(a) * rX,
                                    Mathf.Sin(a) * rY);

        transform.position = centerPos + local;

        // 如果还要在后面 Orbit 阶段用到，可以顺便存一下
        orbitRadius = (rX + rY) * 0.5f; // 随便取个均值
        orbitAngle  = a;

        yield return null;
    }
}


    // 在当前半径上绕角色旋转一段时间
    /// <summary>
    /// 在当前半径上绕角色旋转一段时间
    /// </summary>
    /// <param name="angularSpeedDeg"> 旋转角速度 </param>
    /// <param name="duration"></param>
    /// <returns></returns>
    public IEnumerator Orbit(float angularSpeedDeg, float duration)
    {
        float time = 0f;
        Vector2 centerPos = (Vector2)center.position + orbitOffset;
        float speed = angularSpeedDeg * Mathf.Deg2Rad;

        while (time < duration)
        {
            time += Time.deltaTime;
            orbitAngle += speed * Time.deltaTime;
            Vector2 pos = centerPos + new Vector2(Mathf.Cos(orbitAngle), Mathf.Sin(orbitAngle)) * orbitRadius;
            transform.position = pos;
            yield return null;
        }
    }

    // 散开到指定圈（目标半径 + 角度）
    public IEnumerator ScatterToRing(float targetRadius, float targetAngle, float duration)
    {
        float startRadius = orbitRadius;
        float startAngle = orbitAngle;

        float t = 0f;
        Vector2 centerPos = (Vector2)center.position + orbitOffset;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            orbitRadius = Mathf.Lerp(startRadius, targetRadius, t);
            orbitAngle = Mathf.Lerp(startAngle, targetAngle, t);
            Vector2 pos = centerPos + new Vector2(Mathf.Cos(orbitAngle), Mathf.Sin(orbitAngle)) * orbitRadius;
            transform.position = pos;
            yield return null;
        }
    }

    // 下落
    public IEnumerator DropAndHit( float groundY, float duration)
    {
        //Vector3 start = transform.position;
        //Vector3 end = new Vector3(start.x, start.y+ groundY, start.z);
        //float t = 0f;
        animator.Play("sword_drop");
        yield return new WaitForSeconds(1);
        
        animator.Play("sword_death");
    }
    public void Recycle()
    {
        if(gameObject.activeSelf)
            ObjectPool.instance.Recycle(gameObject);
    }
    public IEnumerator ScatterToEllipse(
      float radiusX,
      float radiusY,
      float angleRad,
      float duration,
      float waveAmplitude,   // 波峰高度
      float waveFrequency,   // 每圈多少个波
      float wavePhase       // 整体相位偏移，用来错开每一圈
    )
    {
        Vector2 c = (Vector2)center.position + orbitOffset;

        Vector2 start = transform.position;

        // —— 计算最终目标点（带波形） ——
        float wave = Mathf.Sin(angleRad * waveFrequency + wavePhase) * waveAmplitude;

        // 把波形加在“本地 y”上，相当于椭圆上下一点点起伏
        Vector2 targetLocal = new Vector2(
            Mathf.Cos(angleRad) * radiusX,
            Mathf.Sin(angleRad) * radiusY + wave
        );

        Vector2 target = c + targetLocal;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            transform.position = Vector2.Lerp(start, target, t);

            // 如果想让剑尖朝外/朝里，这里可以顺便更新方向
            // Vector2 dir = ((Vector2)transform.position - c).normalized;
            // transform.up = dir;   // 朝外
            // transform.up = -dir;  // 朝内

            yield return null;
        }
    }
    public void SetSort(int n)
    {
        spriteR.sortingOrder = n;
    }
}

