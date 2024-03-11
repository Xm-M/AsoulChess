using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

/// <summary>
/// 这个类就只负责生成，具体的初始化由具体的类来做
/// </summary>
public class ChessFactory : IManager
{
    Dictionary<string, Stack<Chess>> chessPool;
    public Scene chessScene;
    int instanceID;
    public Chess ChessCreate(Chess chess,string chessName) {
        if (chessPool.ContainsKey(chessName))
        {
            if (chessPool[chessName].Count > 0)
                return chessPool[chessName].Pop();
        }
        else
        {
            chessPool.Add(chessName, new Stack<Chess>());
        }
        GameObject c = GameObject.Instantiate(chess.gameObject);
        //SceneManager.MoveGameObjectToScene(c, chessScene);
        Chess newchess = c.GetComponent<Chess>();
        newchess.InitChess();
        newchess.instanceID = instanceID;
        instanceID++;
        return newchess;
    }
    public void RecycleChess(Chess c,string chessName)
    {
        if (chessPool.ContainsKey(chessName))
        {
            c.gameObject.SetActive(false);
            chessPool[chessName].Push(c);
        }
        else
        {
            Debug.LogWarning("Cant Recycle This Chess" + c.name);
        }
    }
    public void InitManage()
    {
        chessScene = SceneManager.CreateScene("ChessScene");
        chessPool=new Dictionary<string, Stack<Chess>>();
    }

    public void OnGameStart()
    {
         instanceID = 0;
    }
    public void OnGameOver()
    {
         foreach(var stack in chessPool.Values)
        {
            foreach(var chess in stack)
            {
                GameObject.Destroy(chess.gameObject);
            }
        }
    }
}
 