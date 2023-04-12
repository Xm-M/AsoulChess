using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : Weapon
{

    public LineRenderer line;
    public Transform shootPos;
    public List<Color> colors;
    public List<Color> startColors;
    public float changeSpeed;
    RaycastHit2D[] raycastHit2D;
    int color = 0;
    bool ifColorChange=false;
    //protected override void Start()
    //{       
    //    base.Start();
    //    Physics2D.queriesStartInColliders = false;
    //    line = GetComponent<LineRenderer>();
    //}
    public override void Attack()
    {
        base.Attack();
        line.enabled = true;
        line.SetPosition(0, shootPos.position);
        line.SetPosition(1, master.target.transform.position);
    }
    //public override void Stand()
    //{
    //    base.Stand();
    //    line.enabled = false;
    //}
    public void ColorChange()
    {
        if(!ifColorChange)
        StartCoroutine(ChangeColor());
    }
    IEnumerator ChangeColor()
    {
        ifColorChange = true;
        Vector4 c = line.endColor - colors[color];
        //&& Mathf.Abs(line.endColor.r - colors[color].r) > 0.0002 &&
        //    Mathf.Abs(line.endColor.g - colors[color].g) > 0.0002 && Mathf.Abs(line.endColor.b - colors[color].b) > 0.0002
        while (c.magnitude>=0.003)
        {
            line.endColor = Vector4.MoveTowards(line.endColor,colors[color],changeSpeed*Time.deltaTime);
            line.startColor = Vector4.MoveTowards(line.startColor, startColors[color], changeSpeed * Time.deltaTime);
            c = line.endColor - colors[color];
            yield return null;
        }
        ifColorChange = false;
        if (color < colors.Count - 1) color++;
        else color = 0;
    }
    
    public void Shoot()
    {
        line.enabled = true;
        line.SetPosition(0, shootPos.position);
        raycastHit2D = Physics2D.RaycastAll(shootPos.position, master.target.transform.position - shootPos.position, (master.target.transform.position - shootPos.position).magnitude);
        foreach(var a in raycastHit2D)
        {
            if (a.collider.GetComponent<Chess>()&&!a.collider.CompareTag(master.tag))
            {
                TakeDamage(a.collider.GetComponent<Chess>());
                //ObjectPool.instance.Create(effect).transform.position=a.collider.transform.position;
            }
        }
    }
}
