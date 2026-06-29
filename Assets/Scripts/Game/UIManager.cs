using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameConfig gameConfig;

    [Header("Assets")]
    [Header("HUD")]
    [SerializeField] private Canvas HUDCanvas;
    [SerializeField] private TextMeshProUGUI heightText;
    [SerializeField] private TextMeshProUGUI perfectText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI highscoreText;
    [SerializeField] private Canvas livesCanvas;
    [SerializeField] private GameObject livesPrefab;
    [Header("Game Over")]
    [SerializeField] private Canvas gameOverCanvas;
    [SerializeField] private TextMeshProUGUI statsText;
    [Header("Pause")]
    [SerializeField] private Canvas pauseCanvas;
    [Header("Settings")]
    [SerializeField] private Canvas settingsCanvas;


    [Header("Listener Events")]
    [SerializeField] private IntEventChannel UpdateFloorsEvent;
    [SerializeField] private IntEventChannel UpdateStreakEvent;
    [SerializeField] private IntEventChannel UpdateScoreEvent;
    [SerializeField] private IntEventChannel UpdateHighscoreEvent;
    [SerializeField] private IntEventChannel UpdateLivesEvent;
    [SerializeField] private StatsEventChannel GameOverEvent;
    [SerializeField] private EventChannel PauseGameEvent;
    [SerializeField] private EventChannel UnpauseGameEvent;

    private Stack<GameObject> LiveStack;

    private void Awake()
    {
        heightText.text = "Floors: 0";
        perfectText.gameObject.SetActive(false);
        scoreText.text = "0";

        LiveStack = new Stack<GameObject>();
        for (int i = 0; i < gameConfig.InitialLives; i++)
        {
            LiveStack.Push(Instantiate(livesPrefab, livesCanvas.transform));
        }
    }

    private void OnEnable()
    {
        UpdateFloorsEvent.OnEventTriggered += UpdateFloorsUI;
        UpdateStreakEvent.OnEventTriggered += UpdateStreakUI;
        UpdateScoreEvent.OnEventTriggered += UpdateScoreUI;
        UpdateHighscoreEvent.OnEventTriggered += UpdateHighscoreUI;
        UpdateLivesEvent.OnEventTriggered += UpdateLivesUI;
        GameOverEvent.OnEventTriggered += GameOver;
        PauseGameEvent.OnEventTriggered += ShowPauseMenu;
        UnpauseGameEvent.OnEventTriggered += HidePauseMenu;
    }

    private void OnDisable()
    {
        UpdateFloorsEvent.OnEventTriggered -= UpdateFloorsUI;
        UpdateStreakEvent.OnEventTriggered -= UpdateStreakUI;
        UpdateScoreEvent.OnEventTriggered -= UpdateScoreUI;
        UpdateHighscoreEvent.OnEventTriggered -= UpdateHighscoreUI;
        UpdateLivesEvent.OnEventTriggered -= UpdateLivesUI;
        GameOverEvent.OnEventTriggered -= GameOver;
        PauseGameEvent.OnEventTriggered -= ShowPauseMenu;
        UnpauseGameEvent.OnEventTriggered -= HidePauseMenu;

    }

    private void UpdateFloorsUI(int floors)
    {
        heightText.text = "Floors: " + floors;
    }

    private void UpdateStreakUI(int streak)
    {
        if (streak > 0)
        {
            perfectText.gameObject.SetActive(true);
            perfectText.text = "Perfect! x" + streak;
        }
        else
            perfectText.gameObject.SetActive(false);
    }

    private void UpdateScoreUI(int score)
    {
        scoreText.text = score.ToString();
    }
    private void UpdateHighscoreUI(int highscore)
    {
        highscoreText.text = highscore.ToString();
    }
    private void UpdateLivesUI(int lives)
    {
        // Warning: la condicion del while incluye LiveStack.Count > 0, por lo que si por algun motivo hubiera que SUBIR de 0 a N vidas el bucle nunca arranca. Para el flujo actual (solo se pierden vidas) funciona, pero la rama 'if (lives > LiveStack.Count) Push' queda como codigo muerto/inalcanzable en ese caso.
        while (LiveStack.Count != lives && LiveStack.Count > 0)
        {
            if (lives > LiveStack.Count)
                LiveStack.Push(Instantiate(livesPrefab, livesCanvas.transform));
            else
                Destroy(LiveStack.Pop().gameObject);
        }
    }

    private void GameOver(Statistics stats)
    {
        HUDCanvas.gameObject.SetActive(false);
        statsText.text = "Highest Streak: " + stats.highestStreak + "\nTower Height: " + stats.towerHeight + "\nScore: " + stats.score;
        gameOverCanvas.gameObject.SetActive(true);
    }

    private void ShowPauseMenu()
    {
        pauseCanvas.gameObject.SetActive(true);
    }

    private void HidePauseMenu()
    {
        pauseCanvas.gameObject.SetActive(false);
        settingsCanvas.gameObject.SetActive(false);
    }

    public void ShowSettings()
    {
        HidePauseMenu();
        settingsCanvas.gameObject.SetActive(true);
    }

    public void HideSettings()
    {
        settingsCanvas.gameObject.SetActive(false);
        ShowPauseMenu();
    }
}
