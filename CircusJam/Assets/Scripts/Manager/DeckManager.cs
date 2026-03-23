using UnityEngine;
using System.Collections.Generic;

public class DeckManager : MonoBehaviour
{
    private List<int> playerDrawDeck = new List<int>();
    private List<int> enemyDrawDeck = new List<int>();
    private List<int> playerDiscard = new List<int>();
    private List<int> enemyDiscard = new List<int>();
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

        List<int> fullDeck = CreateDeck();
        Shuffle(fullDeck);
        SplitDeck(fullDeck);
        isInitialized = true;
    }

    private List<int> CreateDeck()
    {
        List<int> deck = new List<int>();
        for (int value = 1; value <= 13; value++)
        {
            for (int i = 0; i < 4; i++)
            {
                deck.Add(value);
            }
        }
        return deck;
    }

    private void Shuffle(List<int> deck)
    {
        for (int i = deck.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            (deck[i], deck[randomIndex]) = (deck[randomIndex], deck[i]);
        }
    }

    private void SplitDeck(List<int> fullDeck)
    {
        int halfSize = fullDeck.Count / 2;
        playerDrawDeck = fullDeck.GetRange(0, halfSize);
        enemyDrawDeck = fullDeck.GetRange(halfSize, halfSize);
    }

    /// <summary>
    /// Zieht eine Karte. Gibt den Kartenwert zurueck, oder 0 wenn keine Karten mehr vorhanden.
    /// Mischt automatisch den Ablagestapel ins Deck, wenn das Deck leer ist.
    /// </summary>
    public int DrawCard(bool isPlayer)
    {
        InitializeDecks();

        List<int> drawDeck = isPlayer ? playerDrawDeck : enemyDrawDeck;
        if (drawDeck.Count == 0)
        {
            ShuffleDiscardIntoDeck(isPlayer);
        }
        if (drawDeck.Count > 0)
        {
            int value = drawDeck[0];
            drawDeck.RemoveAt(0);
            return value;
        }
        return 0; // Keine Karten mehr
    }

    /// <summary>
    /// Fuegt einen Kartenwert zum Ablagestapel hinzu.
    /// </summary>
    public void AddToDiscard(int cardValue, bool isPlayer)
    {
        if (cardValue <= 0) return;

        if (isPlayer)
        {
            playerDiscard.Add(cardValue);
        }
        else
        {
            enemyDiscard.Add(cardValue);
        }
    }

    private void ShuffleDiscardIntoDeck(bool isPlayer)
    {
        List<int> discard = isPlayer ? playerDiscard : enemyDiscard;
        List<int> drawDeck = isPlayer ? playerDrawDeck : enemyDrawDeck;
        drawDeck.AddRange(discard);
        Shuffle(drawDeck);
        discard.Clear();
    }

    public List<int> GetPlayerDrawDeckValues()
    {
        return new List<int>(playerDrawDeck);
    }

    public List<int> GetEnemyDrawDeckValues()
    {
        return new List<int>(enemyDrawDeck);
    }

    public List<int> GetPlayerDiscardValues()
    {
        return new List<int>(playerDiscard);
    }

    public List<int> GetEnemyDiscardValues()
    {
        return new List<int>(enemyDiscard);
    }
}
