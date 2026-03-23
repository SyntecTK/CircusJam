[System.Serializable]
public enum CardSuit
{
    Herz,
    Karo,
    Kreuz,
    Pik
}

[System.Serializable]
public struct CardIdentity
{
    public int rank;
    public CardSuit suit;

    public CardIdentity(int rank, CardSuit suit)
    {
        this.rank = rank;
        this.suit = suit;
    }

    public bool IsValid => rank >= 2 && rank <= 14;
}
