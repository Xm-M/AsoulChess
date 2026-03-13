using Language.Lua;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ISkillReady_RandomExplode : ISkillReady
{
    float elapsed;
    float explosionTime;
    public Vector2 t1, t2;
    
 
    public bool IfSkillReady(Chess user, SkillConfig config, List<Chess> targets)
    {
        if (user.IfSelectable) return false;
        
        elapsed += Time.deltaTime;
        //Debug.Log(elapsed + " " + explosionTime);
        return elapsed >= explosionTime;
    }

    public void InitSkillReady(Chess user, SkillConfig config, List<Chess> targets)
    {
        bool isEarly = Random.Range(0f, 1f) < 0.05f; // 5% 早爆丑
        explosionTime = isEarly
            ? Random.Range(t1.x, t1.y)
            : Random.Range(t2.x, t2.y);
    }
}
