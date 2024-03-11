using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementReactionManage : MonoBehaviour
{
    public static ElementReactionManage instance;
    public List<ElementReaction> allReactions;
    Dictionary<int, ElementReaction> reactionDic;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            reactionDic = new Dictionary<int, ElementReaction>();
            for(int i = 0; i < allReactions.Count; i++)
            {
                reactionDic.Add(allReactions[i].reactionMark, allReactions[i]);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public bool ElementReaction(ElementType ElementIn,ElementType ElementInHold,Chess target)
    {
        int reaction = (int)ElementIn*10 + (int)ElementInHold;
        if (reactionDic.ContainsKey(reaction))
        {
            reactionDic[reaction].Reaction(target);
            return true;
        }
        else
        {
            return false;
        }
    }
}
//public class 
