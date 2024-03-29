
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
public class PrePlantImage : MonoBehaviour
{
    public static PrePlantImage instance; 
    public Image image;
    private void Awake()
    {
        instance = this;
        gameObject.SetActive(false);
    }
    public void TryToPlant(PropertyCreator creator,UnityAction CancelPlant,UnityAction Plant)
    {
        transform.position = Input.mousePosition;
        gameObject.SetActive(true);
        image.sprite = creator.chessSprite;
        MapManage.instance.AwakeTile();
        StartCoroutine(Plants(creator, CancelPlant, Plant));
    }
    IEnumerator Plants(PropertyCreator creator, UnityAction CancelPlant, UnityAction Plant)
    {
        while (true)
        {
            transform.position = Input.mousePosition;
            if (Input.GetMouseButtonDown(1))
            {
                CancelPlant?.Invoke();
                break;
            }
            else if (Input.GetMouseButtonDown(0))
            {
                Vector2 rayPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(rayPos, Vector2.zero, 0, 1 << 9);
                if (hit.collider != null)
                {
                    Tile t = hit.collider.GetComponent<Tile>();
                    if (creator.IfCanPlant(hit.collider.GetComponent<Tile>()))
                    {
                        Chess c = ChessTeamManage.Instance.CreateChess(creator, t, "Player");
                        t.PlantChess(c);
                        Plant?.Invoke();
                        break;
                    }
                }
            }
            yield return null;
        }
        gameObject.SetActive(false);
        MapManage.instance.SleepTile();
    }
}
