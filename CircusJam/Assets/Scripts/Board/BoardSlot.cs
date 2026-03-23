using UnityEngine;
using UnityEngine.EventSystems;

public class BoardSlot : MonoBehaviour, IDropHandler
{
    [SerializeField] private int row;
    [SerializeField] private int column;
    [SerializeField] private bool isPlayerSlot;
    [SerializeField] private Board playerBoard;
    [SerializeField] private Board enemyBoard;

    private Board cachedParentBoard;

    private void Awake()
    {
        cachedParentBoard = GetComponentInParent<Board>();
    }

    public int Row => row;
    public int Column => column;
    public bool IsPlayerSlot => isPlayerSlot;

    public void RemoveCardFromSlot(CardData card)
    {
        Board targetBoard = ResolveBoard();
        if (targetBoard == null || card == null)
        {
            return;
        }

        if (targetBoard.GetCard(row, column) == card)
        {
            targetBoard.RemoveCard(row, column);
            EventManager.CardRemoved(row, isPlayerSlot);
        }
    }

    public void RestoreCardToSlot(CardData card)
    {
        Board targetBoard = ResolveBoard();
        if (targetBoard == null || card == null)
        {
            return;
        }

        targetBoard.PlaceCard(row, column, card);
        EventManager.CardDropped(row, isPlayerSlot);
    }

    private Board ResolveBoard()
    {
        Board targetBoard = isPlayerSlot ? playerBoard : enemyBoard;
        if (targetBoard == null)
        {
            targetBoard = cachedParentBoard;
        }

        return targetBoard;
    }

    public void OnDrop(PointerEventData eventData)
    {
        CardDrag card = eventData.pointerDrag != null ? eventData.pointerDrag.GetComponent<CardDrag>() : null;
        if (card == null)
        {
            return;
        }

        if (card.OwnerHand != null && card.OwnerHand.IsPlayer != isPlayerSlot)
        {
            Debug.Log("Karten koennen nur auf das eigene Board gelegt werden.");
            return;
        }

        // Prüfe Kartenlimit pro Zug
        if (isPlayerSlot && GameManager.Instance != null && !GameManager.Instance.CanPlayCard)
        {
            Debug.Log("Kartenlimit erreicht! Maximal " + GameManager.Instance.MaxCardsPerTurn + " Karten pro Zug.");
            return;
        }

        Board targetBoard = ResolveBoard();
        CardData droppedCard = card.GetComponent<CardData>();
        if (targetBoard == null || droppedCard == null)
        {
            return;
        }

        CardData existingCard = targetBoard.GetCard(row, column);
        if (existingCard != null && existingCard != droppedCard)
        {
            Debug.Log("Slot is already occupied. Drop rejected.");
            return;
        }

        card.transform.SetParent(transform);
        card.transform.localPosition = Vector3.zero;
        card.transform.localRotation = Quaternion.identity;
        card.transform.localScale = Vector3.one;
        card.wasDropped = true;

        if (card.OwnerHand != null)
        {
            card.OwnerHand.NotifyHandChanged();
        }

        targetBoard.PlaceCard(row, column, droppedCard);
        EventManager.CardDropped(row, isPlayerSlot);
    }
}
