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

    public void RemoveCardFromSlot(Card card)
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

    public void RestoreCardToSlot(Card card)
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

        Board targetBoard = ResolveBoard();
        Card droppedCard = card.GetComponent<Card>();
        if (targetBoard == null || droppedCard == null)
        {
            return;
        }

        Card existingCard = targetBoard.GetCard(row, column);
        if (existingCard != null && existingCard != droppedCard)
        {
            Debug.Log("Slot is already occupied. Drop rejected.");
            return;
        }

        card.transform.SetParent(transform);
        card.transform.localPosition = Vector3.zero;
        card.wasDropped = true;

        targetBoard.PlaceCard(row, column, droppedCard);
        Debug.Log($"BoardSlot.OnDrop: row={row}, column={column}, isPlayerSlot={isPlayerSlot}, targetBoard={(targetBoard != null ? targetBoard.name : "null")}");
        EventManager.CardDropped(row, isPlayerSlot);
    }
}
