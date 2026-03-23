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
}
