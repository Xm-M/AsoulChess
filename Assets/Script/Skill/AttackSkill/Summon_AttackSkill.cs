using System.Collections;
using System.Collections.Generic;
using UnityEngine;

 
public class Summon_AttackSkill : AttackSkill
{
    public Chess pre;
    public Vector2Int tileOffset;
    public float moveSpeed;
    public AnimationCurve moveCurve;//这个curve的结尾一定是0;
    public float maxHeight;
    public override void SkillEffect(Chess user)
    {
        Chess c = ChessFactory.instance.ChessCreate(pre, user.moveController.standTile, user.tag);
        c.StartCoroutine(SummonMove(c));
        base.SkillEffect(user);
    }
    IEnumerator SummonMove(Chess summons)
    {
        //yield return new WaitForSeconds(Interval);
        Vector2Int mapPos=tileOffset+summons.moveController.standTile.mapPos;
        Tile targetTile;
        if (MapManage.instance.IfInMapRange(mapPos.x, mapPos.y))
            targetTile = MapManage.instance.tiles[mapPos.x, mapPos.y];
        else
            targetTile = summons.moveController.standTile;
        Vector2 startPos = new Vector2(summons.transform.position.x, summons.transform.position.y+ moveCurve.Evaluate(0) * maxHeight);
        float maxLength = (targetTile.transform.position.x - startPos.x) + 0.1f;
        float x = 0;
        while (Mathf.Abs(x)<1f)
        {
            x = (summons.transform.position.x + Time.deltaTime * moveSpeed - startPos.x) / maxLength;
            float y = moveCurve.Evaluate(x) * maxHeight;
            summons.transform.position = new Vector3(summons.transform.position.x + Time.deltaTime * moveSpeed, startPos.y + y);
            yield return null;
        }
        summons.transform.position = targetTile.transform.position;
        summons.WhenChessEnterWar();

    }
    
} 
