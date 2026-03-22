using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HandManager : MonoBehaviour
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

    private void ArrangeCards()
    {
        int count = cards.Count;
        if (count == 0) return;

        for (int i = 0; i < count; i++)
        {
            float t = count == 1 ? 0.5f : (float)i / (count - 1);
            Vector2 pos = transform.InverseTransformPoint(GetBezierPoint(t));
            cards[i].anchoredPosition = pos;

            Vector2 tangent = GetBezierTangent(t);
            float angle = Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg;
            cards[i].localRotation = Quaternion.Euler(0, 0, angle);
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
