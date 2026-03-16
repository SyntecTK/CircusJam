using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TMP_Text enemyScore01;
    [SerializeField] private TMP_Text enemyScore02;
    [SerializeField] private TMP_Text enemyScore03;
    [SerializeField] private TMP_Text playerScore01;
    [SerializeField] private TMP_Text playerScore02;
    [SerializeField] private TMP_Text playerScore03;
    [SerializeField] private Board playerBoard;
    [SerializeField] private Board enemyBoard;

    private TMP_Text[] playerScoreTexts;
    private TMP_Text[] enemyScoreTexts;

    private void Start()
    {
        playerScoreTexts = new TMP_Text[] { playerScore01, playerScore02, playerScore03 };
        enemyScoreTexts = new TMP_Text[] { enemyScore01, enemyScore02, enemyScore03 };

        RefreshBoardScores(playerBoard, playerScoreTexts);
        RefreshBoardScores(enemyBoard, enemyScoreTexts);
    }

    private void OnEnable()
    {
        EventManager.OnCardDropped += UpdateScores;
        EventManager.OnCardRemoved += UpdateScores;
    }

    private void OnDisable()
    {
        EventManager.OnCardDropped -= UpdateScores;
        EventManager.OnCardRemoved -= UpdateScores;
    }

    private void UpdateScores(int row, bool isPlayerSlot)
    {
        RefreshBoardScores(playerBoard, playerScoreTexts);
        RefreshBoardScores(enemyBoard, enemyScoreTexts);
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
}
