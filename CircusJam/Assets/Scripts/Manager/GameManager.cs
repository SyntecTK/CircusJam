using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Board playerBoard;
    [SerializeField] private Board enemyBoard;

    private readonly int[] playerRowScores = new int[3];
    private readonly int[] enemyRowScores = new int[3];
    private readonly int[,] playerBoardState = new int[3, 3];
    private readonly int[,] enemyBoardState = new int[3, 3];

    public int[] PlayerRowScores => (int[])playerRowScores.Clone();
    public int[] EnemyRowScores => (int[])enemyRowScores.Clone();

    private void Start()
    {
        RefreshState();
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

    private void HandleCardDropped(int row, bool isPlayerSlot)
    {
        Board sourceBoard = isPlayerSlot ? playerBoard : enemyBoard;
        Board oppositeBoard = isPlayerSlot ? enemyBoard : playerBoard;
        int[,] sourceBoardState = isPlayerSlot ? playerBoardState : enemyBoardState;

        if (TryGetPlayedCardValue(sourceBoard, sourceBoardState, row, out int playedValue))
        {
            RemoveMatchingCardsFromOppositeRow(oppositeBoard, row, playedValue, !isPlayerSlot);
        }

        RefreshState();
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
                Card card = board != null ? board.GetCard(row, column) : null;
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
            Card currentCard = board.GetCard(row, column);
            int currentValue = currentCard != null ? currentCard.value : 0;

            if (currentValue != 0 && currentValue != previousValue)
            {
                playedValue = currentValue;
                return true;
            }
        }

        return false;
    }

    private static void RemoveMatchingCardsFromOppositeRow(Board board, int row, int valueToRemove, bool isPlayerSlot)
    {
        if (board == null || valueToRemove == 0)
        {
            return;
        }

        for (int column = 0; column < 3; column++)
        {
            Card card = board.GetCard(row, column);
            if (card == null || card.value != valueToRemove)
            {
                continue;
            }

            board.RemoveCard(row, column);
            Object.Destroy(card.gameObject);
            EventManager.CardRemoved(row, isPlayerSlot);
        }
    }
}
