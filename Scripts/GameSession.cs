using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameSession : MonoBehaviour
{
    [SerializeField] int playerLives = 3;
    private int shurikenAmount = 0;
    public int ShurikenAmount
    {
        get { return this.shurikenAmount; }
        set { this.shurikenAmount = value; }
    }

    private float playerHealth = 0;
    public float PlayerHealth
    {
        get { return this.playerHealth; }
        set { this.playerHealth = value; }
    }
    private int score = 0;
    [SerializeField] float loadTimeDelay = 1.50f;

    [SerializeField] Slider healthSlider;
    [SerializeField] TextMeshProUGUI livesText;
    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] TextMeshProUGUI shurikenText;

    [SerializeField] PauseMenu pauseMenu;
    [SerializeField] GameObject OpsWindow;

    private MusicPlayer musicPlayer;


    private bool isGamePaused = false;
    public bool IsGamePaused => this.isGamePaused;

    private bool isShurikenUpdated = false;
    public bool IsShurikenUpdated => this.isShurikenUpdated;

    private bool isHealthUpdated = false;
    public bool IsHealthUpdated
    {
        get { return this.isHealthUpdated; }
        set { this.isHealthUpdated = value; }
    }

    private void Awake()
    {
        int gameSessionAmount = GameObject.FindObjectsOfType<GameSession>().Length;

        if(gameSessionAmount > 1)
        {
            this.gameObject.SetActive(false);
            GameObject.Destroy(this.gameObject);
        }
        else
        {
            GameObject.DontDestroyOnLoad(this.gameObject);
        }

        this.musicPlayer = GameObject.FindObjectOfType<MusicPlayer>();

        if(this.gameObject.activeSelf)
            this.PlayLevelMusic(1);
    }

    private void Start()
    {
        this.UpdateUIText();
    }

    private void Update()
    {
        if (!this.OpsWindow.activeSelf)
        {
            if ((Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P)) && !this.isGamePaused)
            {
                this.PauseAndUnpauseGame(0, true, true);
            }
            else if ((Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P)) && this.isGamePaused)
            {
                this.PauseAndUnpauseGame(1, false, false);
            }
        }
    }

    public void PlayLevelMusic(int sceneIndex)
    {
        this.musicPlayer.PlaysceneMusic(sceneIndex);
    }

    public void PauseAndUnpauseGame(int timeScale, bool isWindowActive, bool isGamePaused)
    {
        Time.timeScale = timeScale;
        this.pauseMenu.gameObject.SetActive(isWindowActive);
        this.isGamePaused = isGamePaused;
    }

    private void UpdateUIText()
    {
        this.livesText.text = this.playerLives.ToString();
        this.scoreText.text = this.score.ToString();

        if(GameObject.FindObjectOfType<Player>().GetComponent<Health>().CharHealth >= 0)
            this.healthSlider.value = (1);
        else
            this.healthSlider.value = (GameObject.FindObjectOfType<Player>().GetComponent<Health>().CharHealth /
                                       GameObject.FindObjectOfType<Player>().GetComponent<Health>().MaxHealth);
    }

    public void UpdatePlayerHealthBar()
    {
        this.healthSlider.value = (GameObject.FindObjectOfType<Player>().GetComponent<Health>().CharHealth /
                                   GameObject.FindObjectOfType<Player>().GetComponent<Health>().MaxHealth);
    }

    public void ProcessPlayerDeath()
    {
        if (this.playerLives > 0)
            this.SubtractLife();
        else
            this.StartCoroutine(this.ResetGameSession());

    }

    private void SubtractLife()
    {
        this.playerLives--;
        this.StartCoroutine(this.ReloadLevel());
    }

    private IEnumerator ReloadLevel()
    {
        yield return new WaitForSecondsRealtime(this.loadTimeDelay);

        GameObject.FindObjectOfType<LevelLoader>().RestartLevel();

        this.UpdateUIText();
    }

    private IEnumerator ResetGameSession()
    {
        yield return new WaitForSecondsRealtime(this.loadTimeDelay);

        GameObject.Destroy(GameObject.FindObjectOfType<ScenePersist>().gameObject);
        GameObject.Destroy(GameObject.FindObjectOfType<PlayerSelection>().gameObject);
        GameObject.Destroy(this.gameObject);

        GameObject.FindObjectOfType<LevelLoader>().LoadMainMenu();
        GameObject.Destroy(this.gameObject);
    }

    public void AddScore(int amount)
    {
        this.score += amount;
        this.scoreText.text = this.score.ToString();

        if (this.score % 100 == 0) //Every 10 coins we give the player a life
        {
            this.playerLives++;
            this.livesText.text = this.playerLives.ToString();
        }
            
    }

    public void ChangeShurikenAmount(int amount)
    {
        this.shurikenAmount = amount;
        this.shurikenText.text = amount.ToString();
        this.isShurikenUpdated = true;
    }

    public void UpdateHealthInfo()
    {
        this.playerHealth = GameObject.FindObjectOfType<Player>().GetComponent<Health>().MaxHealth;
        this.isHealthUpdated = true;
    }

    public void UpdatePlayerInfoAfterDeath()
    {
        this.UpdateUIText();
    }
}
