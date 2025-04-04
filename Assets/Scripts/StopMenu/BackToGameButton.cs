using UnityEngine;

public class BackToGameButton : MonoBehaviour
{
    public GameObject pauseMenu;
    public static bool isPaused;
    public PlayerMove player;

    public void Start()
    {
        pauseMenu.SetActive(false);
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && player._mouseLockHide) 
        {
            PauseGame();
        }
    }
    
    public void PauseGame()
    {
        pauseMenu.SetActive(true);
        player._inventoryLock = false;
        player._mouseLockHide = false;
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void ResumeGame()
    {
        pauseMenu.SetActive(false);
        player._inventoryLock = true;
        player._mouseLockHide = true;
        Time.timeScale = 1f;
        isPaused = false;
    }
}
