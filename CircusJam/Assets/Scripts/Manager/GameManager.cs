using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private DeckManager deckManager;
    [SerializeField] private Board playerBoard;
    [SerializeField] private Board enemyBoard;
    [SerializeField] private HandManager playerHandManager;
    [SerializeField] private HandManager enemyHandManager;
    [SerializeField] private int maxCardsPerTurn = 4;

    private readonly int[] playerRowScores = new int[3];
    private readonly int[] enemyRowScores = new int[3];
    private readonly int[,] playerBoardState = new int[3, 5];
    private readonly int[,] enemyBoardState = new int[3, 5];

    private bool isPlayerTurn = true;
    private bool passedTurn = false;
    public bool PassedTurn => passedTurn;
    private int cardsPlayedThisTurn;

    public int[] PlayerRowScores => (int[])playerRowScores.Clone();
    public int[] EnemyRowScores => (int[])enemyRowScores.Clone();
    public DeckManager DeckManager => deckManager;
    public int CardsPlayedThisTurn => cardsPlayedThisTurn;
    public int MaxCardsPerTurn => maxCardsPerTurn;
    public bool CanPlayCard => cardsPlayedThisTurn < maxCardsPerTurn;
    public bool IsPlayerTurn => isPlayerTurn;

    private bool gameIsActive = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (playerHandManager != null)
        {
            playerHandManager.ConfigureOwner(true);
        }

        if (enemyHandManager != null)
        {
            enemyHandManager.ConfigureOwner(false);
        }

        if (isPlayerTurn && playerHandManager != null)
        {
            playerHandManager.PopulateHand();
        }
        else if (!isPlayerTurn && enemyHandManager != null)
        {
            enemyHandManager.PopulateHand();
        }

        cardsPlayedThisTurn = 0;
        RefreshState();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && gameIsActive)
        {
            PassTurn();
        }
    }

    private void OnEnable()
    {
        EventManager.OnCardDropped += HandleCardDropped;
        EventManager.OnCardRemoved += HandleCardRemoved;
    }

    private void OnDisable()
    {
        EventManager.OnCardDropped -= HandleCardDropped;
        EventManager.OnCardRemoved -= HandleCardRemoved;
    }

    public void PassTurn()
    {
        if(!passedTurn)
        {
            if (isPlayerTurn && playerHandManager != null)
            {
                playerHandManager.DiscardUnplayedCards();
            }
            else if (!isPlayerTurn && enemyHandManager != null)
            {
                enemyHandManager.DiscardUnplayedCards();
            }
            EventManager.PassTurn();
            passedTurn = true;
        }
        else
        {
            Debug.Log("Ended Turn");
            EndTurn();
            passedTurn = false;
        }
        
    }

    public void EndTurn()
    {
        LockBoardCards(playerBoard);
        LockBoardCards(enemyBoard);

        cardsPlayedThisTurn = 0;
        isPlayerTurn = !isPlayerTurn;

        if (isPlayerTurn && playerHandManager != null)
        {
            playerHandManager.PopulateHand();
        }
        else if (!isPlayerTurn && enemyHandManager != null)
        {
            enemyHandManager.PopulateHand();
        }

        RefreshState();
        EventManager.TurnEnded();
        Debug.Log($"Zug vorbei. {(isPlayerTurn ? "Spieler" : "Gegner")} ist jetzt dran.");
    }

    private static void LockBoardCards(Board board)
    {
        if (board == null || board.grid == null)
        {
            return;
        }

        int rows = board.grid.GetLength(0);
        int columns = board.grid.GetLength(1);

        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                CardData card = board.grid[row, column];
                if (card == null)
                {
                    continue;
                }

                CardDrag drag = card.GetComponent<CardDrag>();
                if (drag != null)
                {
                    drag.enabled = false;
                }
            }
        }
    }

    public void UndoPlayedCard()
    {
        cardsPlayedThisTurn = Mathf.Max(0, cardsPlayedThisTurn - 1);
    }

    private void HandleCardDropped(int row, bool isPlayerSlot)
    {
        cardsPlayedThisTurn++;

        Board sourceBoard = isPlayerSlot ? playerBoard : enemyBoard;
        Board oppositeBoard = isPlayerSlot ? enemyBoard : playerBoard;
        int[,] sourceBoardState = isPlayerSlot ? playerBoardState : enemyBoardState;

        if (TryGetPlayedCardValue(sourceBoard, sourceBoardState, row, out int playedValue))
        {
            RemoveMatchingCardsFromOppositeRow(oppositeBoard, row, playedValue, isPlayerSlot);
        }

        RefreshState();
        CheckGameOver();
    }

    private void CheckGameOver()
    {
        if (playerBoard.IsFull() || enemyBoard.IsFull())
        {
            int pScore = ScoreSystem.CalculateTotalScore(playerBoard);
            int eScore = ScoreSystem.CalculateTotalScore(enemyBoard);

            string winner;
            if (pScore > eScore)
                winner = "Spieler 1";
            else if (eScore > pScore)
                winner = "Spieler 2";
            else
                winner = "Unentschieden";

            EventManager.GameOver(winner, pScore, eScore);
        }
    }

    private void HandleCardRemoved(int row, bool isPlayerSlot)
    {
        RefreshState();
    }

    public int[,] GetPlayerBoardState()
    {
        return (int[,])playerBoardState.Clone();
    }

    public int[,] GetEnemyBoardState()
    {
        return (int[,])enemyBoardState.Clone();
    }

    public void RefreshState()
    {
        RefreshScores(playerBoard, playerRowScores);
        RefreshScores(enemyBoard, enemyRowScores);

        RefreshBoardState(playerBoard, playerBoardState);
        RefreshBoardState(enemyBoard, enemyBoardState);
    }

    private static void RefreshScores(Board board, int[] scores)
    {
        if (scores == null)
        {
            return;
        }

        for (int row = 0; row < scores.Length; row++)
        {
            scores[row] = board != null ? ScoreSystem.CalculateRowScore(board, row) : 0;
        }
    }

    private static void RefreshBoardState(Board board, int[,] state)
    {
        if (state == null)
        {
            return;
        }

        int rows = state.GetLength(0);
        int columns = state.GetLength(1);

        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                CardData card = board != null ? board.GetCard(row, column) : null;
                state[row, column] = card != null ? card.value : 0;
            }
        }
    }

    private static bool TryGetPlayedCardValue(Board board, int[,] cachedState, int row, out int playedValue)
    {
        playedValue = 0;

        if (board == null || cachedState == null)
        {
            return false;
        }

        if (row < 0 || row >= cachedState.GetLength(0))
        {
            return false;
        }

        int columns = cachedState.GetLength(1);
        for (int column = 0; column < columns; column++)
        {
            int previousValue = cachedState[row, column];
            CardData currentCard = board.GetCard(row, column);
            int currentValue = currentCard != null ? currentCard.value : 0;

            if (currentValue != 0 && currentValue != previousValue)
            {
                playedValue = currentValue;
                return true;
            }
        }

        return false;
    }

    private void RemoveMatchingCardsFromOppositeRow(Board board, int row, int valueToRemove, bool isPlayerSlot)
    {
        if (board == null || valueToRemove == 0)
        {
            return;
        }

        for (int column = 0; column < 5; column++)
        {
            CardData card = board.GetCard(row, column);
            if (card == null || card.value != valueToRemove)
            {
                continue;
            }

            board.RemoveCard(row, column);
            if (deckManager != null)
            {
                deckManager.AddToDiscard(card.Identity, isPlayerSlot);
            }
            card.gameObject.SetActive(false);
            EventManager.CardRemoved(row, isPlayerSlot);
        }
    }

    public void SetGameActive()
    {
        gameIsActive = true;
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}

