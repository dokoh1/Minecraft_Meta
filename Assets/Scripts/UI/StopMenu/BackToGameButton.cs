using UnityEngine;

public class BackToGameButton : MonoBehaviour
{
    public GameObject pauseMenu;
    public static bool isPaused;
    public PlayerController player;

    public void Start()
    {
        pauseMenu.SetActive(false);
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && player.mouseLockHide) 
        {
            PauseGame();
        }
    }
    
    public void PauseGame()
    {
        pauseMenu.SetActive(true);
        player.inventoryLock = false;
        player.mouseLockHide = false;
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void ResumeGame()
    {
        pauseMenu.SetActive(false);
        player.inventoryLock = true;
        player.mouseLockHide = true;
        Time.timeScale = 1f;
        isPaused = false;
    }
}
