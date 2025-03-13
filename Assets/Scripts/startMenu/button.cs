using UnityEngine;
//씬 관리 기능 추가
using UnityEngine.SceneManagement;
public class button : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
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

    // Update is called once per frame
    void Update()
    {
        
    }
}
