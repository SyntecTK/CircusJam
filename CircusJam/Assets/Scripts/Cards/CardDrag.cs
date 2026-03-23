using UnityEngine;
using UnityEngine.EventSystems;

public class CardDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Transform originalParent;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private BoardSlot sourceSlot;
    private HandManager ownerHand;

    public bool wasDropped = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public HandManager OwnerHand => ownerHand;

    public void SetOwnerHand(HandManager handManager)
    {
        ownerHand = handManager;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        sourceSlot = originalParent != null ? originalParent.GetComponent<BoardSlot>() : null;
        wasDropped = false;

        CardData cardData = GetComponent<CardData>();
        if (sourceSlot != null && cardData != null)
        {
            sourceSlot.RemoveCardFromSlot(cardData);

            if (sourceSlot.IsPlayerSlot && GameManager.Instance != null)
            {
                GameManager.Instance.UndoPlayedCard();
            }
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
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;

            CardData cardData = GetComponent<CardData>();
            if (sourceSlot != null && cardData != null)
            {
                sourceSlot.RestoreCardToSlot(cardData);
            }

            if (ownerHand != null)
            {
                ownerHand.NotifyHandChanged();
            }
        }
        canvasGroup.blocksRaycasts = true;
    }
}
