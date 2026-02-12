using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 效果是随机选择一个本行敌方单位(如果本行没有地方单位就直接跳到最后一列) 算了 先把跳到最后一列写出来
/// 先把技能做了 
/// </summary>
public class ISkillEffect_Zombie_BullyMan : ISkillEffect
{
    //public float JumpSpeed = 10f;
    public Buff_BaseValueBuff_UnSelectable unSelectableBuff;
    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        Debug.Log("使用技能");
        Tile targetTile = MapManage.instance.tiles[0, user.moveController.standTile.mapPos.y];
        Chess target = null;
        user.skillController.context.TryGet<Chess>("霸凌目标", out target);
        if (target != null) targetTile = target.moveController.standTile;
        float maxHeight = config.baseDamage[0];
        float movespeed = config.baseDamage[1];
        user.StartCoroutine(Jump(user,user.transform, user.transform.position, targetTile.transform.position, maxHeight, movespeed));
        if (unSelectableBuff == null) unSelectableBuff = new Buff_BaseValueBuff_UnSelectable();
        unSelectableBuff.continueTime = (Mathf.Abs(targetTile.transform.position.x-user.transform.position.x) / movespeed);
        //Debug.Log(unSelectableBuff.continueTime);
        user.buffController.AddBuff(unSelectableBuff);
        user.moveController.standTile  = targetTile;
    }
    public IEnumerator Jump(Chess chess,Transform transform,Vector2 startPos, Vector2 endPos, float maxHeight, float moveSpeed)
    {
        // 初始化位置
        transform.position = startPos;

        float dx = Mathf.Abs(endPos.x - startPos.x);
        float dy = Mathf.Abs(endPos.y - startPos.y);

        // 水平距离几乎为0时，避免除0；退化成按总距离估算时间
        float distForTime = dx > 0.0001f ? dx : Vector2.Distance(startPos, endPos);

        // moveSpeed 非法则直接瞬移到终点
        if (moveSpeed <= 0.0001f || distForTime <= 0.0001f)
        {
            transform.position = endPos;
            yield break;
        }

        float duration = distForTime / moveSpeed; // 跳跃总时长
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / duration); // 0~1

            // x 线性插值（整体水平匀速）
            float x = Mathf.Lerp(startPos.x, endPos.x, t);

            // y = 起终点线性插值 + 抛物线额外高度
            // 4*t*(1-t) 在 t=0/1 为0，在 t=0.5 为1
            float baseY = Mathf.Lerp(startPos.y, endPos.y, t);
            float y = baseY + (4f * maxHeight * t * (1f - t));

            transform.position = new Vector2(x, y);
            yield return null;
        }
        // 结束时强制落到终点，避免浮点误差
        transform.position = endPos;
        chess.animatorController.animator.Play("land");
    }
}
