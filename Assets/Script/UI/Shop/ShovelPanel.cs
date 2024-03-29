using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// ������� ���ڲ��� ����϶�Ҳ��Ҫ�������� ��Ϊ����һ�����ж���ֲ��
/// </summary>
public class ShovelPanel : MonoBehaviour
{
    public Image shovelImage;
    public Image shove;
    public AudioPlayer au;
    bool ifSelect;
    [LabelText("ȡ����Ч")]
    public string cancel;
    [LabelText("������Ч")]
    public string dig;
    private void Awake()
    {
        ifSelect = false;
    }
    public void SelectImage()
    {
        if (!ifSelect&& LevelManage.instance.IfGameStart)
        {
            EventController.Instance.TriggerEvent(EventName.WhenShovel.ToString());
            shovelImage.gameObject.SetActive(false);
            shove.gameObject.SetActive(true);
            shove.transform.position = Input.mousePosition;
            ifSelect = true;
        }
    }
    public void Update()
    {
        if (ifSelect)
        {
            shove.transform.position = Input.mousePosition;
            if (Input.GetMouseButtonDown(1))
            {
                Cancle();
                au.PlayAudio(cancel);
            }
            else if (Input.GetMouseButtonDown(0))
            {
                Vector2 rayPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(rayPos, Vector2.zero, 0, 1 << 7);
                if (hit.collider != null)
                {
                    Debug.Log(hit.collider.gameObject.name);
                    au.PlayAudio(dig);
                    hit.collider.GetComponent<Chess>().Death();
                }
                else
                {
                    au.PlayAudio(cancel);
                }
                Cancle();
            }
        }
    }
    public void Cancle()
    {
        shovelImage.gameObject.SetActive(true);
        shove.gameObject.SetActive(false);
        ifSelect = false;
    }
}
