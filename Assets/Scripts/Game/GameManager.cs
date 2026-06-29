using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] GameConfig config;


    [Header("Broadcast Events")]
    [SerializeField] private EventChannel BlockCreateEvent;
    [SerializeField] private EventChannel BlockDropEvent;
    [SerializeField] private IntEventChannel UpdateFloorsEvent;
    [SerializeField] private IntEventChannel UpdateStreakEvent;
    [SerializeField] private IntEventChannel UpdateScoreEvent;
    [SerializeField] private IntEventChannel UpdateHighcoreEvent;
    [SerializeField] private IntEventChannel UpdateLivesEvent;
    [SerializeField] private StatsEventChannel GameOverEvent;
    [SerializeField] private EventChannel PauseGameEvent;
    [SerializeField] private EventChannel UnpauseGameEvent;

    [Header("Listener Events")]
    [SerializeField] private BlockEventChannel BlockSuccessfulLandEvent;
    [SerializeField] private BlockEventChannel BlockPerfectLandEvent;
    [SerializeField] private BlockEventChannel BlockFailedLandEvent;
    [SerializeField] private BlockEventChannel BlockMissedLandEvent;

    private int towerHeight = 0;
    private int perfectStreak = 0;
    private int highestStreak = 0;
    private int score = 0;
    private int lives;
    private int highScore = 0;

    bool isPlaying = false;
    bool isPaused = false;
    private void Awake()
    {
        lives = config.InitialLives;
        isPlaying = true;
        isPaused = false;
        Time.timeScale = 1;
        // Warning: PlayerPrefs.GetInt sin valor por defecto; conviene GetInt("Highscore", 0) y centralizar la clave "Highscore" en una constante para evitar errores de tipeo entre Awake y GameOver.
        highScore = PlayerPrefs.GetInt("Highscore");
    }
    void Start()
    {
        BlockCreateEvent.RaiseEvent();
        UpdateHighcoreEvent.RaiseEvent(highScore);
    }

    private void OnEnable()
    {
        BlockSuccessfulLandEvent.OnEventTriggered += OnBlockLand;
        BlockPerfectLandEvent.OnEventTriggered += OnBlockPerfectLand;
        BlockFailedLandEvent.OnEventTriggered += OnBlockFail;
        BlockMissedLandEvent.OnEventTriggered += OnBlockMiss;
    }
    private void OnDisable()
    {
        BlockSuccessfulLandEvent.OnEventTriggered -= OnBlockLand;
        BlockPerfectLandEvent.OnEventTriggered -= OnBlockPerfectLand;
        BlockFailedLandEvent.OnEventTriggered -= OnBlockFail;
        BlockMissedLandEvent.OnEventTriggered -= OnBlockMiss;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position + new Vector3(config.TowerMaxWobble, 0, 0), 0.30f);
        Gizmos.DrawSphere(transform.position + new Vector3(-config.TowerMaxWobble, 0, 0), 0.30f);
    }
    void Update()
    {
        // Sugestion: las teclas (Space, Escape) estan hardcodeadas; convendria exponerlas como KeyCode serializado o usar el nuevo Input System para permitir rebinding.
        if (Input.GetKeyDown(KeyCode.Space))
            BlockDropEvent.RaiseEvent();
        if (Input.GetKeyDown(KeyCode.Escape) && isPlaying)
        {
            if (isPaused)
                UnpauseGame();
            else
                PauseGame();
        }
    }

    private void OnBlockLand(Block b)
    {
        UpdateHeight(1);
        UpdateScore(config.ScoreForFloor);
        RemoveStreak();
    }
    private void OnBlockPerfectLand(Block b)
    {
        UpdateHeight(1);
        UpdateScore(config.ScoreForPerfect);
        perfectStreak += 1;
        if (perfectStreak > highestStreak)
            highestStreak = perfectStreak;
        UpdateStreakEvent.RaiseEvent(perfectStreak);
    }

    private void OnBlockFail(Block b)
    {
        if (towerHeight > 1)
            UpdateHeight(-1);
        RemoveStreak();
        LoseLife();
    }

    private void OnBlockMiss(Block b)
    {
        RemoveStreak();
        LoseLife();
    }

    private void UpdateHeight(int addition)
    {
        towerHeight += addition;
        UpdateFloorsEvent.RaiseEvent(towerHeight);
    }

    private void UpdateScore(int addition)
    {
        score += addition;
        UpdateScoreEvent.RaiseEvent(score);

        if (score > highScore)
        {
            highScore = score;
            UpdateHighcoreEvent.RaiseEvent(highScore);
        }
    }

    private void RemoveStreak()
    {
        perfectStreak = 0;
        UpdateStreakEvent.RaiseEvent(perfectStreak);
    }

    private void LoseLife()
    {
        // Error: hay un off-by-one. Con InitialLives=3, al fallar se baja a 2,1,0 sin terminar; el GameOver recién dispara en el CUARTO fallo (cuando lives ya es 0 y entra al else). El jugador obtiene una vida extra respecto a lo configurado. Deberías decrementar y luego chequear: lives--; if (lives <= 0) GameOver();
        if (lives > 0)
            lives--;
        else
            GameOver();
        UpdateLivesEvent.RaiseEvent(lives);
    }

    public void ResetGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    private void GameOver()
    {
        Time.timeScale = 0;
        isPlaying = false;
        PlayerPrefs.SetInt("Highscore", highScore);
        PlayerPrefs.Save();
        GameOverEvent.RaiseEvent(new Statistics(towerHeight, highestStreak, score));
    }

    public void PauseGame()
    {
        Time.timeScale = 0;
        isPaused = true;
        PauseGameEvent.RaiseEvent();
    }

    public void UnpauseGame()
    {
        Time.timeScale = 1;
        isPaused = false;
        UnpauseGameEvent.RaiseEvent();
    }
}
