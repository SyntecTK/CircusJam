using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class UIManager : MonoBehaviour
{
    [Header("Score Texts")]
    [SerializeField] private TMP_Text enemyScore01;
    [SerializeField] private TMP_Text enemyScore02;
    [SerializeField] private TMP_Text enemyScore03;
    [SerializeField] private TMP_Text playerScore01;
    [SerializeField] private TMP_Text playerScore02;
    [SerializeField] private TMP_Text playerScore03;
    [SerializeField] private TMP_Text playerScoreTotal;
    [SerializeField] private TMP_Text enemyScoreTotal;

    [Header("Deck Texts")]
    [SerializeField] private TMP_Text playerDeckCountText;

    [Header("Discard Texts")]
    [SerializeField] private TMP_Text playerDiscardCountText;

    [Header("References")]
    [SerializeField] private Board playerBoard;
    [SerializeField] private Board enemyBoard;
    [SerializeField] private DeckManager deckManager;

    private TMP_Text[] playerScoreTexts;
    private TMP_Text[] enemyScoreTexts;

    private void Start()
    {
        playerScoreTexts = new TMP_Text[] { playerScore01, playerScore02, playerScore03 };
        enemyScoreTexts = new TMP_Text[] { enemyScore01, enemyScore02, enemyScore03 };

        RefreshAll();
    }

    private void OnEnable()
    {
        EventManager.OnCardDropped += OnBoardChanged;
        EventManager.OnCardRemoved += OnBoardChanged;
        EventManager.OnTurnEnded += OnTurnEnded;
    }

    private void OnDisable()
    {
        EventManager.OnCardDropped -= OnBoardChanged;
        EventManager.OnCardRemoved -= OnBoardChanged;
        EventManager.OnTurnEnded -= OnTurnEnded;
    }

    private void OnBoardChanged(int row, bool isPlayerSlot)
    {
        RefreshAll();
    }

    private void OnTurnEnded()
    {
        RefreshAll();
    }

    private void RefreshAll()
    {
        RefreshBoardScores(playerBoard, playerScoreTexts);
        RefreshBoardScores(enemyBoard, enemyScoreTexts);
        RefreshDeckCounts();
        playerScoreTotal.text = ScoreSystem.CalculateTotalScore(playerBoard).ToString();
        enemyScoreTotal.text = ScoreSystem.CalculateTotalScore(enemyBoard).ToString();
    }

    private void RefreshBoardScores(Board board, TMP_Text[] scoreTexts)
    {
        if (board == null || scoreTexts == null)
        {
            return;
        }

        int maxRows = Mathf.Min(3, scoreTexts.Length);
        for (int row = 0; row < maxRows; row++)
        {
            int score = ScoreSystem.CalculateRowScore(board, row);
            if (scoreTexts[row] != null)
            {
                scoreTexts[row].text = score.ToString();
            }
        }
    }

    private void RefreshDeckCounts()
    {
        if (deckManager == null)
        {
            return;
        }

        bool isPlayerTurn = GameManager.Instance != null && GameManager.Instance.IsPlayerTurn;

        if (playerDeckCountText != null)
        {
            playerDeckCountText.text = isPlayerTurn ? deckManager.PlayerDrawCount.ToString() : deckManager.EnemyDrawCount.ToString();
        }

        if (playerDiscardCountText != null)
        {
            playerDiscardCountText.text = isPlayerTurn ? deckManager.PlayerDiscardCount.ToString() : deckManager.EnemyDiscardCount.ToString();
        }
    }
}
