using UnityEngine;
using System.Collections.Generic;

public class DeckManager : MonoBehaviour
{
    private List<CardIdentity> playerDrawDeck = new List<CardIdentity>();
    private List<CardIdentity> enemyDrawDeck = new List<CardIdentity>();
    private List<CardIdentity> playerDiscard = new List<CardIdentity>();
    private List<CardIdentity> enemyDiscard = new List<CardIdentity>();
    private bool isInitialized;

    public int PlayerDrawCount => playerDrawDeck.Count;
    public int EnemyDrawCount => enemyDrawDeck.Count;
    public int PlayerDiscardCount => playerDiscard.Count;
    public int EnemyDiscardCount => enemyDiscard.Count;

    private void Awake()
    {
        InitializeDecks();
    }

    private void InitializeDecks()
    {
        if (isInitialized)
        {
            return;
        }

        playerDrawDeck.Clear();
        enemyDrawDeck.Clear();
        playerDiscard.Clear();
        enemyDiscard.Clear();

        List<CardIdentity> fullDeck = CreateDeck();
        Shuffle(fullDeck);
        SplitDeck(fullDeck);
        isInitialized = true;
    }

    private List<CardIdentity> CreateDeck()
    {
        List<CardIdentity> deck = new List<CardIdentity>();
        for (int rank = 2; rank <= 14; rank++)
        {
            for (int suit = 0; suit < 4; suit++)
            {
                deck.Add(new CardIdentity(rank, (CardSuit)suit));
            }
        }
        return deck;
    }

    private void Shuffle(List<CardIdentity> deck)
    {
        for (int i = deck.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            (deck[i], deck[randomIndex]) = (deck[randomIndex], deck[i]);
        }
    }

    private void SplitDeck(List<CardIdentity> fullDeck)
    {
        int halfSize = fullDeck.Count / 2;
        playerDrawDeck = fullDeck.GetRange(0, halfSize);
        enemyDrawDeck = fullDeck.GetRange(halfSize, halfSize);
    }

    /// <summary>
    /// Zieht eine Karte. Gibt die Kartenidentitaet zurueck.
    /// Mischt automatisch den Ablagestapel ins Deck, wenn das Deck leer ist.
    /// </summary>
    public CardIdentity DrawCard(bool isPlayer)
    {
        InitializeDecks();

        List<CardIdentity> drawDeck = isPlayer ? playerDrawDeck : enemyDrawDeck;
        if (drawDeck.Count == 0)
        {
            ShuffleDiscardIntoDeck(isPlayer);
        }
        if (drawDeck.Count > 0)
        {
            CardIdentity value = drawDeck[0];
            drawDeck.RemoveAt(0);
            return value;
        }
        return default; // Keine Karten mehr
    }

    /// <summary>
    /// Fuegt eine Karte zum Ablagestapel hinzu.
    /// </summary>
    public void AddToDiscard(CardIdentity card, bool isPlayer)
    {
        if (!card.IsValid) return;

        if (isPlayer)
        {
            playerDiscard.Add(card);
        }
        else
        {
            enemyDiscard.Add(card);
        }
    }

    private void ShuffleDiscardIntoDeck(bool isPlayer)
    {
        List<CardIdentity> discard = isPlayer ? playerDiscard : enemyDiscard;
        List<CardIdentity> drawDeck = isPlayer ? playerDrawDeck : enemyDrawDeck;
        drawDeck.AddRange(discard);
        Shuffle(drawDeck);
        discard.Clear();
    }

    public List<CardIdentity> GetPlayerDrawDeckCards()
    {
        return new List<CardIdentity>(playerDrawDeck);
    }

    public List<CardIdentity> GetEnemyDrawDeckCards()
    {
        return new List<CardIdentity>(enemyDrawDeck);
    }

    public List<CardIdentity> GetPlayerDiscardCards()
    {
        return new List<CardIdentity>(playerDiscard);
    }

    public List<CardIdentity> GetEnemyDiscardCards()
    {
        return new List<CardIdentity>(enemyDiscard);
    }
}
