using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class DamagePanel : View
{
    [FoldoutGroup("Color")]
    public Color Physics;
    [FoldoutGroup("Color")]
    public Color Magic;
    [FoldoutGroup("Color")]
    public Color Real;
    [FoldoutGroup("Heal")]
    public Color Heal;
    public GameObject damageText;
    public GameObject healPrefab;
    public bool showDamage;
    public override void Init()
    {
        
    }
    public void ShowDamageMes(DamageMessege dm)
    {
        //Debug.Log("damhe");
        if (!showDamage||dm.damage<1) return;
        //Debug.Log("չʾ");
        GameObject text= ObjectPool.instance.Create(damageText);
        text.transform.SetParent(transform);
        text.transform.position=Camera.main.WorldToScreenPoint(dm.damageTo.transform.position+Vector3.up*0.75f);
        if (!dm.ifCrit)
        {
            text.transform.localScale = new Vector3(1f, 1f, 1f);
            text.GetComponentInChildren<Text>().text = ((int)dm.damage).ToString();
        }
        else
        {
            text.transform.localScale = new Vector3(1.25f, 1.25f, 1.25f);
            text.GetComponentInChildren<Text>().text = ((int)dm.damage).ToString()+ "!!";
        }
        Color newColor;
        switch (dm.damageType)
        {
            case DamageType.Physical:newColor = Physics; break;
            case DamageType.Magic:newColor = Magic; break;
            case DamageType.Real:newColor = Real;  break;
            default:newColor = Color.white; break;
        }
        text.GetComponentInChildren<Text>().color = newColor;
    
    }
    public void ShowHeal(float heal, Chess target)
    {
        if (!showDamage) return;
        if (target != null)
        {
            GameObject text = ObjectPool.instance.Create(damageText);
            text.transform.SetParent(transform);
            text.transform.localScale = new Vector3(1f, 1f, 1f);

            text.transform.position = target.transform.position + Vector3.up * 0.75f;
            Text t = text.GetComponentInChildren<Text>(); ;
            t.text = ((int)heal).ToString();
            t.color = Color.green;
            if (healPrefab != null)
            {
                GameObject healeffect = ObjectPool.instance.Create(healPrefab);
                healeffect.transform.SetParent(target.transform);
                healeffect.transform.localPosition = Vector3.zero;
            }
        }
    }
}
