using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject optionsWindow;

    // Start is called before the first frame update
    void Start()
    {
        this.gameObject.SetActive(false);
        this.optionsWindow.SetActive(false);
    }

    public void ResumeGame()
    {
        GameObject.FindObjectOfType<GameSession>().PauseAndUnpauseGame(1, false, false);
    }

    public void RestartLevel()
    {
        GameObject.FindObjectOfType<GameSession>().PauseAndUnpauseGame(1, false, false);
        GameObject.FindObjectOfType<LevelLoader>().RestartLevel();
    }

    public void ShowOptionsWindow()
    {
        //this.gameObject.SetActive(false);
        this.optionsWindow.SetActive(true);
    }

    public void CloseOptionsWindow()
    {
        //this.gameObject.SetActive(true);
        this.optionsWindow.SetActive(false);
    }

    public void ExitToMainMenu()
    {
        GameObject.FindObjectOfType<LevelLoader>().LoadMainMenu();
    }
}
