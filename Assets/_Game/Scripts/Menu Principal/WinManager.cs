using Sirenix.OdinInspector;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
public class WinManager : PersistentSingleton<WinManager>
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

    // obviamente esto despues se cambia cuando nuestra pantalla de victoria sea mas que un texto.
    public TextMeshProUGUI conclusionText;
    [TextArea(0,30), InfoBox("Make sure to match the order of the corresponding answers on your CaseResolution.")]
    public List<string> conclusions;

    public void SetConclusion(int answer)
    {
        conclusionText.text = conclusions[answer];
    }
}
