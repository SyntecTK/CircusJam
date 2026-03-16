using UnityEngine;

public class Board : MonoBehaviour
{
    public Card[,] grid = new Card[3,3];

    public void PlaceCard(int row, int column, Card card)
    {
        grid[row, column] = card;
    }

    public Card GetCard(int row, int column)
    {
        return grid[row, column];
    }

    public void RemoveCard(int row, int column)
    {
        grid[row, column] = null;
    }
}
