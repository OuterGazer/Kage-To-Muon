using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class LevelLoader : MonoBehaviour
{
    [SerializeField] float loadTimeDelay = 0.5f;

    [SerializeField] GameObject mainMenu;
    [SerializeField] GameObject optionsWindow;
    [SerializeField] GameObject characterSelect;
    [SerializeField] GameObject creditsWindow;
    [SerializeField] PlayerSelection player;

    private bool isCheckpointReached = false;
    public bool IsCheckpointReached => this.isCheckpointReached;

    private void Awake()
    {
        if (SceneManager.GetActiveScene().buildIndex == 2)
            GameObject.Destroy(GameObject.Find("Tutorial Popup"));
    }

    private void Start()
    {
        if (this.optionsWindow != false)
            this.optionsWindow.SetActive(false);

        if (this.creditsWindow != false)
            this.creditsWindow.SetActive(false);

        if (this.characterSelect != false)
            this.characterSelect.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (this.gameObject.CompareTag("Interactables"))
            this.StartCoroutine(this.LoadNextLevel());
        else if (this.gameObject.CompareTag("Checkpoint") && collision.gameObject.CompareTag("Player"))
            this.isCheckpointReached = true;
    }

    //method purely for playtesting
    public void LoadLevel()
    {
        this.StartCoroutine(this.LoadNextLevel());
    }

    private IEnumerator LoadNextLevel()
    {
        yield return new WaitForSecondsRealtime(this.loadTimeDelay);

        GameObject.Destroy(GameObject.FindObjectOfType<ScenePersist>().gameObject);

        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        if (currentSceneIndex == 2)
            GameObject.FindObjectOfType<GameSession>().PlayLevelMusic(1);
        else if(currentSceneIndex != 4)
            GameObject.FindObjectOfType<GameSession>().PlayLevelMusic(currentSceneIndex + 1);
        else if(currentSceneIndex == 4)
            GameObject.FindObjectOfType<GameSession>().PlayLevelMusic(0);

        SceneManager.LoadScene(currentSceneIndex + 1);
    }

    public void LoadMainMenu()
    {
        GameObject.FindObjectOfType<GameSession>().PauseAndUnpauseGame(1, false, false);

        GameObject.Destroy(GameObject.FindObjectOfType<GameSession>().gameObject);

        if(GameObject.FindObjectOfType<ScenePersist>() != null)
            GameObject.Destroy(GameObject.FindObjectOfType<ScenePersist>().gameObject);

        GameObject.Destroy(GameObject.FindObjectOfType<PlayerSelection>().gameObject);
        GameObject.Destroy(GameObject.FindObjectOfType<MusicPlayer>().gameObject);

        SceneManager.LoadScene(0);
    }

    public void ChooseCharacter()
    {
        this.mainMenu.SetActive(false);
        this.characterSelect.SetActive(true);
    }

    public void StartGameWithKage()
    {
        this.player.SetPlayer("Player Ninja");
        
        SceneManager.LoadScene(1);
    }

    public void StartGameWithMuon()
    {
        this.player.SetPlayer("Player Kunoichi");
        
        SceneManager.LoadScene(1);
    }

    public void RestartLevel()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        SceneManager.LoadScene(currentSceneIndex);
    }

    public void ShowOptionsMenu()
    {
        this.mainMenu.SetActive(false);
        this.optionsWindow.SetActive(true);
    }

    public void ShowCredits()
    {
        this.mainMenu.SetActive(false);
        this.creditsWindow.SetActive(true);
    }

    public void CloseOptionsMenu()
    {
        this.mainMenu.SetActive(true);
        this.optionsWindow.SetActive(false);
    }

    public void CloseCredits()
    {
        this.mainMenu.SetActive(true);
        this.creditsWindow.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
