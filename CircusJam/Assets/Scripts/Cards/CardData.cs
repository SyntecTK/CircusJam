using UnityEngine;

public class CardData : MonoBehaviour
{
    // Poker rank: 2..14 (11=J, 12=Q, 13=K, 14=A)
    public int value;
    public CardSuit suit;

    public CardData(int value)
    {
        this.value = value;
        this.suit = CardSuit.Herz;
    }

    public int RankValue => value;
    public CardSuit Suit => suit;
    public int PointValue => GetPointValueFromRank(value);

    public CardIdentity Identity => new CardIdentity(value, suit);

    public void SetCard(CardIdentity card)
    {
        value = card.rank;
        suit = card.suit;
    }

    public static int GetPointValueFromRank(int rank)
    {
        if (rank >= 2 && rank <= 9) return rank;
        if (rank >= 10 && rank <= 14) return 10;
        return 0;
    }

    public static string GetRankLabel(int rank)
    {
        return rank switch
        {
            11 => "B",
            12 => "D",
            13 => "K",
            14 => "A",
            _ => rank.ToString()
        };
    }

    public static string GetSuitLabel(CardSuit suit)
    {
        return suit switch
        {
            CardSuit.Herz => "H",
            CardSuit.Karo => "Ka",
            CardSuit.Kreuz => "Kr",
            CardSuit.Pik => "P",
            _ => "?"
        };
    }
}
