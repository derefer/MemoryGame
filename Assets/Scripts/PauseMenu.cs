using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
   public static bool IsGamePaused = false;
   public GameObject PauseMenuUI;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Return)) {
            if (IsGamePaused) {
                Resume();
            } else {
                Pause();
            }
        }
    }

    public void Resume() {
        PauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        IsGamePaused = false;
    }

    void Pause() {
        PauseMenuUI.SetActive(true);
        Time.timeScale = 0f; // Freeze the game?
        IsGamePaused = true;
    }

    public void QuitGame() {
        Debug.Log("Quitting game...");
        Application.Quit();
    }
}
