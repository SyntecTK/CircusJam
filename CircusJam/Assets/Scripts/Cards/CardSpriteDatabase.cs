using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CardSpriteEntry
{
    public int rank;
    public CardSuit suit;
    public Sprite sprite;
}

[CreateAssetMenu(fileName = "CardSpriteDatabase", menuName = "Cards/Card Sprite Database")]
public class CardSpriteDatabase : ScriptableObject
{
    [SerializeField] private List<CardSpriteEntry> entries = new List<CardSpriteEntry>();

    public Sprite GetSprite(int rank, CardSuit suit)
    {
        for (int i = 0; i < entries.Count; i++)
        {
            CardSpriteEntry entry = entries[i];
            if (entry == null)
            {
                continue;
            }

            if (entry.rank == rank && entry.suit == suit)
            {
                return entry.sprite;
            }
        }

        return null;
    }
}
