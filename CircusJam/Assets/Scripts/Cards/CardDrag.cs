using UnityEngine;
using UnityEngine.EventSystems;

public class CardDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Transform originalParent;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private BoardSlot sourceSlot;

    public bool wasDropped = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        sourceSlot = originalParent != null ? originalParent.GetComponent<BoardSlot>() : null;
        wasDropped = false;

        Card cardData = GetComponent<Card>();
        if (sourceSlot != null && cardData != null)
        {
            sourceSlot.RemoveCardFromSlot(cardData);
        }

        transform.SetParent(canvas.transform);

        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if(!wasDropped)
        {
            transform.SetParent(originalParent);
            transform.localPosition = Vector3.zero;

            Card cardData = GetComponent<Card>();
            if (sourceSlot != null && cardData != null)
            {
                sourceSlot.RestoreCardToSlot(cardData);
            }
        }
        canvasGroup.blocksRaycasts = true;
    }
}
