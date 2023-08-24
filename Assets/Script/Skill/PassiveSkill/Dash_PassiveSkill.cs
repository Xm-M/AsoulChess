using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
public class Dash_PassiveSkill : TimeSkill
{
    public float DashSpeed = 5;
    public float maxLength = 15f;
    public GameObject explode;
    bool dashOver;
    public override void SkillEffect(Chess user)
    {
        base.SkillEffect(user);
        dashOver = false;
        user.StartCoroutine(Dash(user));
    }
    IEnumerator Dash(Chess user)
    {
        Vector2 starPos=user.transform.position;
        Debug.Log("dash");
        while (Vector2.Distance(starPos,user.transform.position)<maxLength)
        {
            user.transform.position=Vector2.MoveTowards(user.transform.position, 
                user.transform.position+user.transform.right,Time.fixedDeltaTime*DashSpeed);
            yield return new WaitForFixedUpdate();
        }
        ObjectPool.instance.Create(explode).transform.position=user.transform.position;
        user.moveController.standTile= MapManage.instance.GetTile(user.transform.position);
        dashOver = true;
    }
    public override bool ifSkilOver(Chess chess)
    {
        return dashOver;
    }
}
