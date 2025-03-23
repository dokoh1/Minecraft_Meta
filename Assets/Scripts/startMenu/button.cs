using UnityEngine;
//씬 관리 기능 추가
using UnityEngine.SceneManagement;
public class button : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject credits;

    public void GoToSceneTwo(string sceneName)
    {
        SceneManager.LoadScene("Inventory");
    }
    void Start()
    {
        Debug.Log("button start");
    }
}
