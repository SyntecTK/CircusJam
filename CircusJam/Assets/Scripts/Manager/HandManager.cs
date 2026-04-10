using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HandManager : MonoBehaviour, IDropHandler
{
    [SerializeField] private DeckManager deckManager;
    [SerializeField] private CardSpriteDatabase cardSpriteDatabase;
    [SerializeField] private bool isPlayer = true;

    [Header("Bezier Curve Points")]
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform controlPoint;
    [SerializeField] private Transform endPoint;

    [Header("Card Settings")]
    [SerializeField] private RectTransform cardPrefab;
    [SerializeField] private int cardCount = 6;

    [Header("Card Animation")]
    [SerializeField] private float cardAnimDuration = 0.3f;
    [SerializeField] private float cardAnimDelay = 0.1f;

    private List<RectTransform> cards = new List<RectTransform>();

    public bool IsPlayer => isPlayer;

    private void Start()
    {
        ArrangeCards();
    }

    public void ConfigureOwner(bool ownerIsPlayer)
    {
        isPlayer = ownerIsPlayer;
    }

    public void PopulateHand()
    {
        StopAllCoroutines();

        foreach (var c in cards)
        {
            if (c != null) Destroy(c.gameObject);
        }
        cards.Clear();

        for (int i = 0; i < cardCount; i++)
        {
            CardIdentity drawnCard = deckManager.DrawCard(isPlayer);
            if (!drawnCard.IsValid)
            {
                Debug.LogWarning("Keine Karten mehr im Deck zum Ziehen!");
                break;
            }

            RectTransform card = Instantiate(cardPrefab, transform);
            AssignCardValue(card, drawnCard, i);
            cards.Add(card);
        }

        StartCoroutine(AnimateHandIn());
    }

    private void AssignCardValue(RectTransform card, CardIdentity drawnCard, int index)
    {
        CardData cardData = card.GetComponent<CardData>();
        if (cardData == null)
        {
            card.name = $"Card_{index}";
            Debug.LogWarning("Spawned card is missing the Card component.");
            return;
        }

        cardData.SetCard(drawnCard);
        card.name = $"Card_{index}_{CardData.GetRankLabel(drawnCard.rank)}_{CardData.GetSuitLabel(drawnCard.suit)}";

        CardDrag cardDrag = card.GetComponent<CardDrag>();
        if (cardDrag != null)
        {
            cardDrag.SetOwnerHand(this);
        }

        Image cardImage = card.GetComponentInChildren<Image>();
        Sprite sprite = cardSpriteDatabase != null ? cardSpriteDatabase.GetSprite(drawnCard.rank, drawnCard.suit) : null;
        if (cardImage != null && sprite != null)
        {
            cardImage.sprite = sprite;
        }

        TMP_Text cardText = card.GetComponentInChildren<TMP_Text>();
        if (cardText != null)
        {
            cardText.text = sprite != null
                ? string.Empty
                : $"{CardData.GetRankLabel(drawnCard.rank)}{CardData.GetSuitLabel(drawnCard.suit)}";
        }
    }

    public void DiscardUnplayedCards()
    {
        for (int i = cards.Count - 1; i >= 0; i--)
        {
            RectTransform cardRect = cards[i];
            if (cardRect == null) continue;

            if (cardRect.parent != transform)
            {
                continue;
            }

            CardData cardData = cardRect.GetComponent<CardData>();
            if (cardData != null)
            {
                deckManager.AddToDiscard(cardData.Identity, isPlayer);
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

    // ---------------------- Animation ----------------------

    private IEnumerator AnimateHandIn()
    {
        List<RectTransform> handCards = new List<RectTransform>();
        for (int i = 0; i < cards.Count; i++)
        {
            RectTransform card = cards[i];
            if (card != null && card.parent == transform)
                handCards.Add(card);
        }

        int count = handCards.Count;
        if (count == 0) yield break;

        float[] targetTs = new float[count];

        float spacing = cardCount > 1 ? 1f / (cardCount - 1) : 0f;
        float tStart = 0.5f - (count - 1) * spacing / 2f;

        // Hide all cards and place them at t=0
        for (int i = 0; i < count; i++)
        {
            float t = count == 1 ? 0.5f : tStart + i * spacing;
            targetTs[i] = t;

            CanvasGroup cg = handCards[i].GetComponent<CanvasGroup>();
            if (cg == null) cg = handCards[i].gameObject.AddComponent<CanvasGroup>();
            cg.alpha = 0f;

            Vector2 hidePos = transform.InverseTransformPoint(GetBezierPoint(0f));
            handCards[i].anchoredPosition = hidePos;

            Vector2 hideTangent = GetBezierTangent(0f);
            float hideAngle = Mathf.Atan2(hideTangent.y, hideTangent.x) * Mathf.Rad2Deg;
            handCards[i].localRotation = Quaternion.Euler(0, 0, hideAngle);
        }

        // Animate each card one by one from t=0 to its target
        Coroutine lastCoroutine = null;
        for (int i = 0; i < count; i++)
        {
            lastCoroutine = StartCoroutine(AnimateCardAlongBezier(
                handCards[i],
                0f,
                targetTs[i],
                cardAnimDuration
            ));

            if (i < count - 1)
                yield return new WaitForSeconds(cardAnimDelay);
        }

        if (lastCoroutine != null)
            yield return lastCoroutine;
    }

    private IEnumerator AnimateCardAlongBezier(RectTransform card, float startT, float targetT, float duration)
    {
        // Make card visible at animation start
        CanvasGroup cg = card.GetComponent<CanvasGroup>();
        if (cg != null) cg.alpha = 1f;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float tNorm = Mathf.Clamp01(elapsed / duration);
            float eased = 1f - (1f - tNorm) * (1f - tNorm);
            float currentT = Mathf.Lerp(startT, targetT, eased);

            Vector2 point = transform.InverseTransformPoint(GetBezierPoint(currentT));
            card.anchoredPosition = point;

            Vector2 tangent = GetBezierTangent(currentT);
            float angle = Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg;
            card.localRotation = Quaternion.Euler(0, 0, angle);

            yield return null;
        }

        Vector2 finalPos = transform.InverseTransformPoint(GetBezierPoint(targetT));
        card.anchoredPosition = finalPos;

        Vector2 finalTangent = GetBezierTangent(targetT);
        float finalAngle = Mathf.Atan2(finalTangent.y, finalTangent.x) * Mathf.Rad2Deg;
        card.localRotation = Quaternion.Euler(0, 0, finalAngle);
    }

    // ---------------------- Instant Layout ----------------------

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

    // ---------------------- Bezier Math ----------------------

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
