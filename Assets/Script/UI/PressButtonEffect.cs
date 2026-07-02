using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PressButtonEffect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    private Vector3 originalPosition;

    public Vector3 pressedOffset = new Vector3(0, -6f, 0);

    void Start()
    {
        originalPosition = transform.localPosition;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        transform.localPosition = originalPosition + pressedOffset;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        ResetButton();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ResetButton();
    }

    private void ResetButton()
    {
        transform.localPosition = originalPosition;
    }
}