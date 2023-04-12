using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathSummon : MonoBehaviour
{
    public GameObject small;
    public Chess hold;
    private void Awake()
    {
        if(hold == null)hold=transform.parent.GetComponent<Chess>();
        EventController.Instance.AddListener(hold.instanceID + EventName.WhenDeath.ToString(),Summon);
    }
    public void Summon()
    {
        Debug.Log("summon");
        if (!GameManage.instance.ifGameStart) return;
        Chess c = Instantiate(small).GetComponent<Chess>();
        hold.standTile.ChessEnter(c);
        c.tag = hold.tag;
        //GameManage.instance.teams[c.tag].AddMember(c);
    }
}
