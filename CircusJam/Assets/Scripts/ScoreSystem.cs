using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public enum PokerHand
{
    HighCard,
    OnePair,
    TwoPair,
    ThreeOfAKind,
    Straight,
    Flush,
    FullHouse,
    FourOfAKind,
    StraightFlush,
    RoyalFlush,
    FiveOfAKind
}

public class ScoreSystem : MonoBehaviour
{
    private static readonly Dictionary<PokerHand, int> HandMultipliers = new Dictionary<PokerHand, int>
    {
        { PokerHand.HighCard,       1  },
        { PokerHand.OnePair,        2  },
        { PokerHand.TwoPair,        3  },
        { PokerHand.ThreeOfAKind,   4  },
        { PokerHand.Straight,       5  },
        { PokerHand.Flush,          6  },
        { PokerHand.FullHouse,      7  },
        { PokerHand.FourOfAKind,    8  },
        { PokerHand.StraightFlush,  9  },
        { PokerHand.RoyalFlush,     11 },
        { PokerHand.FiveOfAKind,    12 },
    };

    public static PokerHand GetPokerHand(CardData[] rowCards)
    {
        var cards = rowCards.Where(c => c != null).ToList();
        var ranks = cards.Select(c => c.RankValue).Where(v => v != 0).ToList();

        if (ranks.Count == 0)
            return PokerHand.HighCard;

        // Count occurrences of each value
        var counts = new Dictionary<int, int>();
        foreach (int v in ranks)
        {
            counts.TryGetValue(v, out int c);
            counts[v] = c + 1;
        }

        // Sort group sizes descending, e.g. [3,2] for a full house
        var groupSizes = counts.Values.OrderByDescending(c => c).ToList();

        if (groupSizes[0] == 5) return PokerHand.FiveOfAKind;
        if (groupSizes[0] == 4) return PokerHand.FourOfAKind;
        if (groupSizes[0] == 3 && groupSizes.Count > 1 && groupSizes[1] == 2) return PokerHand.FullHouse;

        bool isFiveCards = cards.Count == 5;
        bool isFlush = isFiveCards && cards.Select(c => c.Suit).Distinct().Count() == 1;

        // Straight: exactly 5 distinct consecutive ranks (including A-2-3-4-5)
        bool isStraight = false;
        List<int> sorted = null;
        if (isFiveCards && groupSizes[0] == 1)
        {
            sorted = ranks.OrderBy(v => v).ToList();
            isStraight = true;
            for (int i = 1; i < sorted.Count; i++)
            {
                if (sorted[i] != sorted[i - 1] + 1)
                {
                    isStraight = false;
                    break;
                }
            }

            if (!isStraight)
            {
                isStraight = sorted[0] == 2 && sorted[1] == 3 && sorted[2] == 4 && sorted[3] == 5 && sorted[4] == 14;
            }
        }

        if (isFlush && isStraight)
        {
            bool isRoyal = sorted != null && sorted[0] == 10 && sorted[1] == 11 && sorted[2] == 12 && sorted[3] == 13 && sorted[4] == 14;
            return isRoyal ? PokerHand.RoyalFlush : PokerHand.StraightFlush;
        }

        if (isFlush) return PokerHand.Flush;
        if (isStraight) return PokerHand.Straight;

        if (groupSizes[0] == 3) return PokerHand.ThreeOfAKind;
        if (groupSizes.Count >= 2 && groupSizes[0] == 2 && groupSizes[1] == 2) return PokerHand.TwoPair;
        if (groupSizes[0] == 2) return PokerHand.OnePair;

        return PokerHand.HighCard;
    }

    public static string GetHandName(PokerHand hand) => hand switch
    {
        PokerHand.HighCard      => "High Card",
        PokerHand.OnePair       => "One Pair",
        PokerHand.TwoPair       => "Two Pair",
        PokerHand.ThreeOfAKind  => "Three of a Kind",
        PokerHand.Straight      => "Straight",
        PokerHand.Flush         => "Flush",
        PokerHand.FullHouse     => "Full House",
        PokerHand.FourOfAKind   => "Four of a Kind",
        PokerHand.StraightFlush => "Straight Flush",
        PokerHand.RoyalFlush    => "Royal Flush",
        PokerHand.FiveOfAKind   => "Five of a Kind",
        _                       => "Unknown"
    };

    public static int CalculateRowScore(Board board, int row)
    {
        CardData[] cards = new CardData[5];
        for (int i = 0; i < 5; i++)
            cards[i] = board.GetCard(row, i);

        PokerHand hand = GetPokerHand(cards);
        int baseScore = GetContributingScore(cards, hand);
        return baseScore * HandMultipliers[hand];
    }

    private static int GetContributingScore(CardData[] rowCards, PokerHand hand)
    {
        var cards = rowCards.Where(c => c != null).ToList();
        if (cards.Count == 0) return 0;

        var counts = new Dictionary<int, int>();
        foreach (var card in cards)
        {
            counts.TryGetValue(card.RankValue, out int c);
            counts[card.RankValue] = c + 1;
        }

        switch (hand)
        {
            case PokerHand.HighCard:
                int maxRank = cards.Max(c => c.RankValue);
                return cards.Where(c => c.RankValue == maxRank).Max(c => c.PointValue);

            case PokerHand.OnePair:
                int pairRank = counts.First(kvp => kvp.Value == 2).Key;
                return cards.Where(c => c.RankValue == pairRank).Sum(c => c.PointValue);

            case PokerHand.TwoPair:
                var pairRanks = new HashSet<int>(counts.Where(kvp => kvp.Value == 2).Select(kvp => kvp.Key));
                return cards.Where(c => pairRanks.Contains(c.RankValue)).Sum(c => c.PointValue);

            case PokerHand.ThreeOfAKind:
                int threeRank = counts.First(kvp => kvp.Value == 3).Key;
                return cards.Where(c => c.RankValue == threeRank).Sum(c => c.PointValue);

            case PokerHand.FourOfAKind:
                int fourRank = counts.First(kvp => kvp.Value == 4).Key;
                return cards.Where(c => c.RankValue == fourRank).Sum(c => c.PointValue);

            // Straight, Flush, FullHouse, StraightFlush, RoyalFlush, FiveOfAKind: all cards contribute
            default:
                return cards.Sum(c => c.PointValue);
        }
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
