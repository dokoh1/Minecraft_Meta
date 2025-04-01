using UnityEngine;

public class GameEndButton : MonoBehaviour
{
    //Application.Quit();
    

    public void Quit()
    {
        Application.Quit();
        Debug.Log("Quit");
    }
}
