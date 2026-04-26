using UnityEngine;

public class WinManager : MonoBehaviour
{
    //que buen script la puta madre
    public void EndGame()
    {
        print("termino"); Application.Quit();
    }

    public void Retry()
    {
        this.AsyncLoader("SampleScene");
    }
}
