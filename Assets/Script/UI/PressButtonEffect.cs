using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PressButtonEffect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public Image targetImage;

    private Vector3 originalPosition;
    private Color originalColor;

    public Vector3 pressedOffset = new Vector3(0, -6f, 0);
    public Color pressedColor = new Color(0.7f, 0.65f, 0.75f, 1f);

    void Start()
    {
        originalPosition = transform.localPosition;

        if (targetImage == null)
            targetImage = GetComponent<Image>();

        originalColor = targetImage.color;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        transform.localPosition = originalPosition + pressedOffset;
        targetImage.color = pressedColor;
    }

    public void OnPointerUp(PointerEventData eventData) { ResetButton(); }

    public void OnPointerExit(PointerEventData eventData) { ResetButton(); }
    private void ResetButton()
    {
        transform.localPosition = originalPosition;
        targetImage.color = originalColor;
    }
}