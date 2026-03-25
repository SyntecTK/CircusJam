using System;

public static class EventManager
{
    public static event Action<int, bool> OnCardDropped;  // row, isPlayerSlot
    public static event Action<int, bool> OnCardRemoved;  // row, isPlayerSlot
    public static event Action OnTurnEnded;
    public static event Action OnPassTurn;
    public static event Action<string, int, int> OnGameOver; // winner, p1Points, p2Points

    public static void CardDropped(int row, bool isPlayerSlot)
    {
        OnCardDropped?.Invoke(row, isPlayerSlot);
    }

    public static void CardRemoved(int row, bool isPlayerSlot)
    {
        OnCardRemoved?.Invoke(row, isPlayerSlot);
    }

    public static void PassTurn()
    {
        OnPassTurn?.Invoke();
    }

    public static void TurnEnded()
    {
        OnTurnEnded?.Invoke();
    }

    public static void GameOver(string winner, int p1Points, int p2Points)
    {
        OnGameOver?.Invoke(winner, p1Points, p2Points);
    }
}
