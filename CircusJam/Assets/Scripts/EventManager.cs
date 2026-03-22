using System;

public static class EventManager
{
    public static event Action<int, bool> OnCardDropped;  // row, isPlayerSlot
    public static event Action<int, bool> OnCardRemoved;  // row, isPlayerSlot
    public static event Action OnTurnEnded;

    public static void CardDropped(int row, bool isPlayerSlot)
    {
        OnCardDropped?.Invoke(row, isPlayerSlot);
    }

    public static void CardRemoved(int row, bool isPlayerSlot)
    {
        OnCardRemoved?.Invoke(row, isPlayerSlot);
    }

    public static void TurnEnded()
    {
        OnTurnEnded?.Invoke();
    }
}
