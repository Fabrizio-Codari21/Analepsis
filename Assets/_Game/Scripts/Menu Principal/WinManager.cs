using UnityEngine;
using UnityEngine.SceneManagement;
public class WinManager : MonoBehaviour
{
    //que buen script la puta madre
    public void EndGame()
    {
        print("termino"); Application.Quit();
    }

    public void Retry()
    {
       SceneManager.LoadScene("SampleScene");
    }
}
