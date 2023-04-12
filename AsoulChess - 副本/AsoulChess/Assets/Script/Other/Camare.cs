using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camare : MonoBehaviour
{
    public Transform command;
    public float speed=6;
    private void Update()
    {
        transform.position = new Vector3(command.position.x, command.position.y, transform.position.z);
    }
    public void MoveTo(Vector3 target){
        StartCoroutine(Moving(target));
    }
    IEnumerator Moving(Vector3 target){
        while(Vector2.Distance(transform.position,target)>1f){
            Vector3 move=Vector2.MoveTowards(transform.position,target,speed*Time.deltaTime);
            transform.position=new Vector3(move.x,move.y,transform.position.z);
            yield return null;
        }
    }
}
