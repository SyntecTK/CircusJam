using UnityEngine;
using System;

public static class EventManager
{
    public static event System.Action<int, bool> OnCardDropped;  // row, isPlayerSlot
    public static event System.Action<int, bool> OnCardRemoved;  // row, isPlayerSlot

    public static void CardDropped(int row, bool isPlayerSlot)
    {
        OnCardDropped?.Invoke(row, isPlayerSlot);
    }

    public static void CardRemoved(int row, bool isPlayerSlot)
    {
        OnCardRemoved?.Invoke(row, isPlayerSlot);
    }
}
