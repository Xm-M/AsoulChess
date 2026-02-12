using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
public class SkillEffect_ZZZLight: ISkillEffect
{
    Transform player;          // 角色
    [LabelText("角色头顶偏移")]
    public Vector2 headOffset = new Vector2(0, 1.5f); // 角色头顶偏移
    [LabelText("上升高度")]
    public float riseHeight = 1f;
    [LabelText("上升的时间")]
    public float riseDuration = 0.3f;
    [LabelText("汇聚的时间")]
    public float gatherDuration = 0.5f;
    [LabelText("汇聚时的角速度")]
    public float angularSpeedDeg = 10f;
    [LabelText("剑围绕的最小椭圆长 ")]
    public float minRadius = 0.7f;
    [LabelText("剑围绕的最小椭圆宽")]
    public float minRadiusLong = 0.8f;
    [LabelText("小圈转圈的时间")]
    public float orbitDuration = 0.7f;
    [LabelText("小圈散开的时间")]
    public float scatterDuration = 0.5f;
    [LabelText("下落的时间")]
    public float dropDuration = 0.4f;
    [LabelText("每圈的剑数量")]
    public int swordsPerRing = 6;
    [LabelText("最小的内圈半径")]
    public float baseRingRadius = 1.0f;
 
    [LabelText("下落的距离")]
    public float groundY = -2.0f; // 你草地的y值 

    public float innerRadiusX = 1.0f;
    public float innerRadiusY = 0.6f;

    public float midRadiusX = 1.7f;
    public float midRadiusY = 1.0f;

    public float outerRadiusX = 2.4f;
    public float outerRadiusY = 1.4f;

    [LabelText("波的幅度")]
    public float[] waveAmp ;   // 内→外 波越高
    [LabelText("每圈的波数")]
    public float[] waveFreq  ;        // 每圈多少个波
                                                                     // 也可以写死，用 ringIndex 算：amp = 0.15f + ring * 0.05f 等
    public List<SwordController> activeSwords;

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        user.skillController.context.TryGet<List<SwordController>>("sword",out activeSwords);
        player = user.transform;
        user.StartCoroutine(SkillRoutine(user));
    }

    IEnumerator SkillRoutine(Chess user)
    {
        if (activeSwords.Count == 0)
        {
            Debug.Log("场上没有剑");
            yield break;
        }

        // 初始化旋转中心
        foreach (var s in activeSwords)
            s.InitOrbit(player, headOffset);

        // 阶段1：全部先升起
        foreach (var s in activeSwords)
        {
            s.StopAllCoroutines();
            s.Rise(riseHeight, riseDuration);
        }
        yield return new WaitForSeconds(riseDuration);

        // 阶段2：汇聚到最小半径
        //float minRadiusX = 1.2f;  // 你自己调：内圈椭圆的长半轴
        //float minRadiusY = 0.6f;  // 短半轴，决定扁度
        float extraTurns = 1f;    // 螺旋时多转几圈

        int count = activeSwords.Count;
        if (count > 0)
        {
            float angleStep = Mathf.PI * 2f / count;
            float baseAngle = 0f; // 可以给个偏移

            for (int i = 0; i < count; i++)
            {
                float targetAngle = baseAngle + angleStep * i;
                var s = activeSwords[i];

                s.StopAllCoroutines();
                s.StartCoroutine(
                    s.MoveToMinEllipseToAngle(minRadius, minRadiusLong,
                                              gatherDuration,
                                              targetAngle,
                                              extraTurns)
                );
            }

            yield return new WaitForSeconds(gatherDuration);
        }

        // 阶段3：在小圈上旋转一会儿
        foreach (var s in activeSwords)
        {
            s.StopAllCoroutines();
            s.StartCoroutine(s.Orbit(360f, orbitDuration)); // 一秒转一圈
        }
        yield return new WaitForSeconds(orbitDuration);
        user.StartCoroutine(ScatterPhase());
        yield return new WaitForSeconds(scatterDuration*2);

        // 阶段5：下落 + 伤害
        foreach (var s in activeSwords)
        {
            s.StopAllCoroutines();
            s.StartCoroutine(s.DropAndHit(groundY, dropDuration));
        }
        yield return new WaitForSeconds(dropDuration);
        foreach(var chess in GameManage.instance.chessTeamManage.GetEnemyTeam(user.tag))
        {
            DamageMessege Dm = user.skillController.DM;
            Dm.damage = user.propertyController.GetAttack() * count;
            Dm.damageFrom = user;
            Dm.damageTo = chess;
            user.propertyController.TakeDamage(Dm);
        }
        // 技能结束，你可以清理列表或者让剑回到正常逻辑
        activeSwords.Clear();
    }
    IEnumerator ScatterPhase()
    {
        int total = activeSwords.Count;
        if (total <= 0) yield break;

        const int ringCount = 3;

        float[] radiusX = new float[ringCount]
        {
        innerRadiusX, midRadiusX, outerRadiusX
        };
        float[] radiusY = new float[ringCount]
        {
        innerRadiusY, midRadiusY, outerRadiusY
        };

        // 1）让每把剑都知道中心点
        foreach (var s in activeSwords)
            s.InitOrbit(player, headOffset);

        // 2）计算每圈周长
        float[] C = new float[ringCount];
        float sumC = 0f;
        for (int i = 0; i < ringCount; i++)
        {
            C[i] = EllipseCircumference(radiusX[i], radiusY[i]);
            sumC += C[i];
        }

        // 3）按周长比例得到理想数量
        float[] ideal = new float[ringCount];
        int[] counts = new int[ringCount];
        int assigned = 0;

        for (int i = 0; i < ringCount; i++)
        {
            ideal[i] = (sumC > 0f) ? (total * C[i] / sumC) : (total / (float)ringCount);
            counts[i] = Mathf.FloorToInt(ideal[i]);
            assigned += counts[i];
        }

        // 特殊情况：总剑数 < 3，就至少保证前几圈有 1 把
        if (total <= ringCount)
        {
            for (int i = 0; i < total; i++)
                counts[i] = 1;
            for (int i = total; i < ringCount; i++)
                counts[i] = 0;
            assigned = total;
        }

        // 4）把剩余的剑，根据小数部分大小，优先补给那些“更应该多一点”的圈
        int remaining = total - assigned;
        if (remaining > 0)
        {
            // 记录小数部分和索引
            List<(int index, float frac)> fracList = new List<(int, float)>();
            for (int i = 0; i < ringCount; i++)
            {
                float frac = ideal[i] - Mathf.Floor(ideal[i]);
                fracList.Add((i, frac));
            }
            // 按小数部分从大到小排序
            fracList.Sort((a, b) => b.frac.CompareTo(a.frac));

            int k = 0;
            while (remaining > 0 && k < fracList.Count)
            {
                counts[fracList[k].index]++;
                remaining--;
                k++;
                if (k >= fracList.Count) k = 0; // 防止极端情况
            }
        }

        // 5）按照 counts[i] 每圈均匀分布到椭圆上
        int swordIndex = 0;

        // 每圈的波形参数（可以在 Inspector 做成 public 数组/字段）
        

        for (int ring = 0; ring < ringCount; ring++)
        {
            int count = counts[ring];
            if (count <= 0) continue;

            float a = radiusX[ring];
            float b = radiusY[ring];

            float angleStep = Mathf.PI * 2f / count;

            // 每一圈再加一个相位偏移，避免三圈波峰对齐
            float baseAngleOffset = ring * 0.3f;
            float wavePhase = ring * 0.8f;  // 波的相位偏移

            for (int i = 0; i < count && swordIndex < total; i++, swordIndex++)
            {
                float angle = baseAngleOffset + angleStep * i;

                var sword = activeSwords[swordIndex];
                sword.StopAllCoroutines();
                sword.StartCoroutine(
                    sword.ScatterToEllipse(
                        a,
                        b,
                        angle,
                        Time.deltaTime*10,
                        waveAmp[ring],
                        waveFreq[ring],
                        wavePhase
                    )
                );
            }
        }

        yield return new WaitForSeconds(scatterDuration);
        
    }

    float EllipseCircumference(float a, float b)
    {
        // Ramanujan approximation
        float h = Mathf.Pow(a - b, 2f) / Mathf.Pow(a + b, 2f);
        return Mathf.PI * (a + b) * (1f + (3f * h) / (10f + Mathf.Sqrt(4f - 3f * h)));
    }
  

}

