using UnityEngine;
using System.Collections.Generic;

public class ScoreSystem : MonoBehaviour
{
    public static int CalculateRowScore(Board board, int row)
    {
        Card[] cards = new Card[5];
        for (int i = 0; i < 5; i++)
        {
            cards[i] = board.GetCard(row, i);
        }

        int[] values = new int[5];
        for (int i = 0; i < 5; i++)
        {
            values[i] = cards[i]?.value ?? 0;
        }

        var valueCounts = new Dictionary<int, int>();
        foreach (int val in values)
        {
            if (val != 0)
            {
                if (valueCounts.ContainsKey(val))
                    valueCounts[val]++;
                else
                    valueCounts[val] = 1;
            }
        }

        int score = 0;
        foreach (var pair in valueCounts)
        {
            int val = pair.Key;
            int count = pair.Value;
            if (count >= 2)
            {
                score += val * count * count;
            }
            else
            {
                score += val;
            }
        }

        return score;
    }

    public static int CalculateTotalScore(Board board)
    {
        int total = 0;
        for (int row = 0; row < 3; row++)
        {
            total += CalculateRowScore(board, row);
        }
        return total;
    }
}
