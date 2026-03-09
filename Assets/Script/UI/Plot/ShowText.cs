using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
public class ShowText : MonoBehaviour
{
    public float speed;
    public UnityEvent onTextOver;
    string str;
    Text tex;
    float i = 0;   //调整这个可以调整出现的速度
    int index = 0;
    string str1 = "";
    bool ison = true;

    // Start is called before the first frame update
    void Start()
    {
        tex = GetComponent<Text>();
        str = tex.text;
        tex.text = "";
         i = speed;
    }

    // Update is called once per frame
    void Update()
    {
        if (ison)
        {
            i -= Time.deltaTime;
            if (i <= 0)
            {
                if (index >= str.Length)
                {
                    ison = false;
                    onTextOver?.Invoke();
                    return;
                }
                str1 = str1 + str[index].ToString();
                tex.text = str1;
                index += 1;
                i = speed;
            }
        }

    }
}
