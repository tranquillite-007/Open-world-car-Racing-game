using UnityEngine;
using UnityEngine.SceneManagement; // Required for scene management

public class MenuController : MonoBehaviour
{
    // This method will be called when the Start button is clicked
    public void StartGame()
    {
       
        SceneManager.LoadScene(2);
    }
}