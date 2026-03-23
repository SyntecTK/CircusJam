using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class HandManager : MonoBehaviour, IDropHandler
{
    [SerializeField] private DeckManager deckManager;
    [SerializeField] private bool isPlayer = true;

    [Header("Bezier Curve Points")]
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform controlPoint;
    [SerializeField] private Transform endPoint;

    [Header("Card Settings")]
    [SerializeField] private RectTransform cardPrefab;
    [SerializeField] private int cardCount = 6;

    private List<RectTransform> cards = new List<RectTransform>();

    public bool IsPlayer => isPlayer;

    private void Start()
    {
        PopulateHand();
        ArrangeCards();
    }

    public void PopulateHand()
    {
        foreach (var c in cards)
        {
            if (c != null) Destroy(c.gameObject);
        }
        cards.Clear();

        for (int i = 0; i < cardCount; i++)
        {
            int drawnValue = deckManager.DrawCard(isPlayer);
            if (drawnValue == 0)
            {
                Debug.LogWarning("Keine Karten mehr im Deck zum Ziehen!");
                break;
            }

            RectTransform card = Instantiate(cardPrefab, transform);
            AssignCardValue(card, drawnValue, i);
            cards.Add(card);
        }
        ArrangeCards();
    }

    private void AssignCardValue(RectTransform card, int value, int index)
    {
        Card cardData = card.GetComponent<Card>();
        if (cardData == null)
        {
            card.name = $"Card_{index}";
            Debug.LogWarning("Spawned card is missing the Card component.");
            return;
        }

        cardData.value = value;
        card.name = $"Card_{index}_{value}";

        CardDrag cardDrag = card.GetComponent<CardDrag>();
        if (cardDrag != null)
        {
            cardDrag.SetOwnerHand(this);
        }

        TMP_Text cardText = card.GetComponentInChildren<TMP_Text>();
        if (cardText != null)
        {
            cardText.text = value.ToString();
        }
    }

    public void DiscardUnplayedCards()
    {
        for (int i = cards.Count - 1; i >= 0; i--)
        {
            RectTransform cardRect = cards[i];
            if (cardRect == null) continue;

            // Nur Karten discarden, die noch in der Hand sind (Kind dieses Transforms)
            if (cardRect.parent != transform)
            {
                continue; // Karte wurde aufs Board gespielt, nicht discarden
            }

            Card cardData = cardRect.GetComponent<Card>();
            if (cardData != null)
            {
                deckManager.AddToDiscard(cardData.value, isPlayer);
            }

            Destroy(cardRect.gameObject);
        }
        cards.Clear();
    }

    public void OnDrop(PointerEventData eventData)
    {
        CardDrag draggedCard = eventData.pointerDrag != null ? eventData.pointerDrag.GetComponent<CardDrag>() : null;
        if (draggedCard == null || draggedCard.OwnerHand != this)
        {
            return;
        }

        draggedCard.transform.SetParent(transform);
        draggedCard.transform.localRotation = Quaternion.identity;
        draggedCard.transform.localScale = Vector3.one;
        draggedCard.wasDropped = true;
        NotifyHandChanged();
    }

    public void NotifyHandChanged()
    {
        ArrangeCards();
    }

    private void ArrangeCards()
    {
        List<RectTransform> handCards = new List<RectTransform>();
        for (int i = 0; i < cards.Count; i++)
        {
            RectTransform card = cards[i];
            if (card != null && card.parent == transform)
            {
                handCards.Add(card);
            }
        }

        int count = handCards.Count;
        if (count == 0) return;

        float spacing = cardCount > 1 ? 1f / (cardCount - 1) : 0f;
        float tStart = 0.5f - (count - 1) * spacing / 2f;

        for (int i = 0; i < count; i++)
        {
            float t = count == 1 ? 0.5f : tStart + i * spacing;
            Vector2 pos = transform.InverseTransformPoint(GetBezierPoint(t));
            handCards[i].anchoredPosition = pos;

            Vector2 tangent = GetBezierTangent(t);
            float angle = Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg;
            handCards[i].localRotation = Quaternion.Euler(0, 0, angle);
        }
    }

    private Vector2 GetBezierPoint(float t)
    {
        Vector2 p0 = startPoint.position;
        Vector2 p1 = controlPoint.position;
        Vector2 p2 = endPoint.position;

        return Mathf.Pow(1 - t, 2) * p0 + 2 * (1 - t) * t * p1 + Mathf.Pow(t, 2) * p2;
    }

    private Vector2 GetBezierTangent(float t)
    {
        Vector2 p0 = startPoint.position;
        Vector2 p1 = controlPoint.position;
        Vector2 p2 = endPoint.position;

        return 2 * (1 - t) * (p1 - p0) +
               2 * t * (p2 - p1);
    }

    //---------------------- Editor Gizmos ----------------------
    private void OnDrawGizmosSelected()
    {
        if (startPoint == null || controlPoint == null || endPoint == null) return;

        Gizmos.color = Color.green;

        Vector3 prev = startPoint.position;

        for (int i = 1; i <= 20; i++)
        {
            float t = i / 20f;
            Vector3 point = GetBezierPoint(t);
            Gizmos.DrawLine(prev, point);
            prev = point;
        }
    }
}
