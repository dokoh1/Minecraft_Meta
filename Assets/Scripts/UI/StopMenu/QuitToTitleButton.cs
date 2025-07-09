using UnityEngine;
using UnityEngine.SceneManagement;
public class QuitToTitleButton : MonoBehaviour
{
    public void GoToSceneTwo()
    {
        SceneManager.LoadScene("StartMenu");
    }
}
