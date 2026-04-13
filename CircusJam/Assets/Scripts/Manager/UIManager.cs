using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject combinations;
    [Header("Start Screen")]
    [SerializeField] private GameObject startScreen;
    [SerializeField] private TMP_InputField player1NameInput;
    [SerializeField] private TMP_InputField player2NameInput;
    [SerializeField] private Image p1TicketCut;
    [SerializeField] private Image p2TicketCut;

    [Header("Score Texts")]
    [SerializeField] private TMP_Text currentPlayerDisplay;
    [SerializeField] private TMP_Text enemyScore01;
    [SerializeField] private TMP_Text enemyScore02;
    [SerializeField] private TMP_Text enemyScore03;
    [SerializeField] private TMP_Text playerScore01;
    [SerializeField] private TMP_Text playerScore02;
    [SerializeField] private TMP_Text playerScore03;
    [SerializeField] private TMP_Text playerScoreTotal;
    [SerializeField] private TMP_Text enemyScoreTotal;

    [Header("Deck Components")]
    [SerializeField] private TMP_Text playerDeckCountText;
    [SerializeField] private TMP_Text playerDiscardCountText;
    [SerializeField] private Image deckImage;
    [SerializeField] private Image discardImage;
    [SerializeField] private Sprite playerBackSprite;
    [SerializeField] private Sprite enemyBackSprite;

    [Header("References")]
    [SerializeField] private Board playerBoard;
    [SerializeField] private Board enemyBoard;
    [SerializeField] private DeckManager deckManager;

    [Header("WinText")]
    [SerializeField] private GameObject winScreen;
    [SerializeField] private TMP_Text winText;
    [SerializeField] private TMP_Text p1PointsText;
    [SerializeField] private TMP_Text p2PointsText;

    [Header("PlayerCard")]
    [SerializeField] private GameObject playerCardDisplay;
    [SerializeField] private TMP_Text playerCardText;
    [SerializeField] private TMP_Text turnAroundText;


    private TMP_Text[] playerScoreTexts;
    private TMP_Text[] enemyScoreTexts;

    private string player1Name;
    private string player2Name;

    private bool p1IsReady = false;
    private bool p2IsReady = false;

    private void Start()
    {
        playerScoreTexts = new TMP_Text[] { playerScore01, playerScore02, playerScore03 };
        enemyScoreTexts = new TMP_Text[] { enemyScore01, enemyScore02, enemyScore03 };

        currentPlayerDisplay.enabled = false;

        RefreshAll();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Tab))
        {
            combinations.SetActive(!combinations.activeSelf);
        }
    }

    private void OnEnable()
    {
        EventManager.OnCardDropped += OnBoardChanged;
        EventManager.OnCardRemoved += OnBoardChanged;
        EventManager.OnTurnEnded += OnTurnEnded;
        EventManager.OnPassTurn += OnTurnPassed;
        EventManager.OnGameOver += OnGameOver;

        player1NameInput.onSubmit.AddListener(OnInputEnd);
        player2NameInput.onSubmit.AddListener(OnInputEnd);
    }

    private void OnDisable()
    {
        EventManager.OnCardDropped -= OnBoardChanged;
        EventManager.OnCardRemoved -= OnBoardChanged;
        EventManager.OnTurnEnded -= OnTurnEnded;
        EventManager.OnPassTurn -= OnTurnPassed;
        EventManager.OnGameOver -= OnGameOver;
    }

    private void OnInputEnd(string input)
    {
        if (player1NameInput.isFocused)
        {
            p1TicketCut.GetComponent<Animator>().Play("TicketRip");
            p1TicketCut.GetComponent<AudioSource>().Play();
            p1IsReady = true;
        }
        else if (player2NameInput.isFocused)
        {
            p2TicketCut.GetComponent<Animator>().Play("TicketRip");
            p2TicketCut.GetComponent<AudioSource>().Play();
            p2IsReady = true;
        }

        if (p1IsReady && p2IsReady)
        {
            StartCoroutine(StartGameDelayed(2f));
        }
    }

    private IEnumerator StartGameDelayed(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        StartGame();
    }

    private void OnBoardChanged(int row, bool isPlayerSlot)
    {
        RefreshAll();
    }

    private void OnTurnEnded()
    {
        RefreshAll();
        ShowPlayerCard();
        SwapCardBacks(GameManager.Instance.IsPlayerTurn);
        DisplayCurrentPlayer();
    }

    private void DisplayCurrentPlayer()
    {
        currentPlayerDisplay.text = GameManager.Instance.IsPlayerTurn ? 
                                                    player1Name + "'s Turn!" :
                                                    player2Name + "'s Turn!";
    }

    private void OnGameOver(string winnerName, int playerScore, int enemyScore)
    {
        if (winScreen != null)
        {
            winScreen.SetActive(true);
        }
        string displayName = winnerName == "Spieler 1" ? player1Name
                           : winnerName == "Spieler 2" ? player2Name
                           : winnerName;

        winText.text = winnerName == "Unentschieden"
            ? "Unentschieden!"
            : $"{displayName} gewinnt!";

        p1PointsText.text = $"{player1Name}: {playerScore} Punkte";
        p2PointsText.text = $"{player2Name}: {enemyScore} Punkte";
        currentPlayerDisplay.enabled = false;
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

    public void RestartGame()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }

        if (winScreen != null)
        {
            winScreen.SetActive(false);
        }

        RefreshAll();
    }

    private void ShowPlayerCard()
    {
        StartCoroutine(FadeOut(playerCardDisplay.GetComponent<CanvasGroup>()));
    }

    private void OnTurnPassed()
    {
        StartCoroutine(FadePlayerCardDisplay());
    }

    private IEnumerator FadePlayerCardDisplay()
    {
        if (playerCardDisplay == null)
        {
            yield break;
        }

        if (GameManager.Instance.IsPlayerTurn)
        {
            playerCardText.text = player2Name + "'s turn!";
            turnAroundText.text = player1Name + " please turn around!";
        }
        else
        {
            playerCardText.text = player1Name + "'s turn!";
            turnAroundText.text = player2Name + " please turn around!";
        }

        playerCardDisplay.SetActive(true);
        CanvasGroup canvasGroup = playerCardDisplay.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            float elapsedTime = 0f;
            float fadeDuration = 0.5f;
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                canvasGroup.alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
                yield return null;
            }
            canvasGroup.alpha = 1f;
        }
    }

    private IEnumerator FadeOut(CanvasGroup targetCanvasGroup)
    {
        if (targetCanvasGroup != null)
        {
            float fadeOutTime = 0f;
            float fadeOutDuration = 0.5f;
            while (fadeOutTime < fadeOutDuration)
            {
                fadeOutTime += Time.deltaTime;
                targetCanvasGroup.alpha = Mathf.Clamp01(1f - (fadeOutTime / fadeOutDuration));
                yield return null;
            }
            targetCanvasGroup.alpha = 0f;
        }

        playerCardDisplay.SetActive(false);
    }

    private void SwapCardBacks(bool isPlayerTurn)
    {
        if (isPlayerTurn)
        {
            if (deckImage != null)
            {
                deckImage.sprite = playerBackSprite;
            }
            if (discardImage != null)
            {
                discardImage.sprite = playerBackSprite;
            }
        }
        else
        {
            if (deckImage != null)
            {
                deckImage.sprite = enemyBackSprite;
            }
            if (discardImage != null)
            {
                discardImage.sprite = enemyBackSprite;
            }
        }
    }

    public void StartGame()
    {
        player1Name = string.IsNullOrEmpty(player1NameInput.text) ? "Spieler 1" : player1NameInput.text;
        player2Name = string.IsNullOrEmpty(player2NameInput.text) ? "Spieler 2" : player2NameInput.text;

        currentPlayerDisplay.text = player1Name + "'s Turn!";

        currentPlayerDisplay.enabled = true;
        startScreen.SetActive(false);
        StartCoroutine(ShowInitialPlayerCard());
        StartCoroutine(TextAnimation(currentPlayerDisplay));
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private IEnumerator ShowInitialPlayerCard()
    {
        playerCardText.text = player1Name + "'s turn!";
        turnAroundText.text = player2Name + " please turn around!";

        playerCardDisplay.SetActive(true);
        CanvasGroup canvasGroup = playerCardDisplay.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            float elapsedTime = 0f;
            float fadeDuration = 0.5f;
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                canvasGroup.alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
                yield return null;
            }
            canvasGroup.alpha = 1f;
        }

        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));

        yield return StartCoroutine(FadeOut(playerCardDisplay.GetComponent<CanvasGroup>()));

        GameManager.Instance.SetGameActive();
        GameManager.Instance.StartFirstTurn();
    }

    private IEnumerator TextAnimation(TMP_Text text, float amplitude = 5f, float speed = 1f)
    {
        float baseSize = text.fontSize;
        while (true)
        {
            text.fontSize = baseSize + amplitude * Mathf.Sin(Time.time * speed);
            yield return null;
        }
    }
}
