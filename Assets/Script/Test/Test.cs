using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class Test : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PropertyCreator c =
                GameManage.instance.allChess[Random.Range(0, GameManage.instance.allChess.Count)];
            Item_PlantCard card= UIManage.GetView<ItemPanel>().Create<Item_PlantCard>() as Item_PlantCard;
            card.InitCard(c);
        }
    }
}
