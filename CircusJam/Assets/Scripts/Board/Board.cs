using UnityEngine;

public class Board : MonoBehaviour
{
    public CardData[,] grid = new CardData[3, 5];

    public void PlaceCard(int row, int column, CardData card)
    {
        grid[row, column] = card;
    }

    public CardData GetCard(int row, int column)
    {
        return grid[row, column];
    }

    public void RemoveCard(int row, int column)
    {
        grid[row, column] = null;
    }

    public bool IsFull()
    {
        for (int row = 0; row < grid.GetLength(0); row++)
            for (int col = 0; col < grid.GetLength(1); col++)
                if (grid[row, col] == null)
                    return false;
        return true;

    }
}
