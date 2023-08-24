using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Funaimu_Up : MonoBehaviour
{
    // Start is called before the first frame update
    public Vector3 up;
    public float time=2;
    Vector2 startPos;
    public void Up()
    {
        startPos = transform.position;
        transform.position += up;
        StartCoroutine(Down());
    }
    IEnumerator Down()
    {
        float moveSpeed=Vector2.Distance(transform.position, startPos)/time;
        while (Vector2.Distance(transform.position, startPos) > 0.1f) {
            transform.position = Vector2.MoveTowards(transform.position,startPos,moveSpeed*Time.deltaTime);
            yield return null;
        }

    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
