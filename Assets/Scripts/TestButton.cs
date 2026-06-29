using UnityEngine;
using UnityEngine.EventSystems;

public class TestButton : MonoBehaviour,
    IPointerDownHandler,
    IPointerUpHandler,
    IPointerClickHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("Down");
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log("Up");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Click Interface");
    }
}