using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuBook : MonoBehaviour
{
    [SerializeField] private Button m_toGameButton;// ahora si tenemos 1 solo nivel
    [SerializeField] private string gameSceneName;

    private IActivity _activity;


    private void Start()
    {
        _activity = GetComponentInParent<IActivity>();

        _activity.OnPause += () => { enabled = false; };
        _activity.OnResume += () => { enabled = true; };
    }

    private void OnEnable()
    {
        m_toGameButton.onClick.AddListener(StartGame);
        m_toGameButton.interactable = true;
    }

    private void StartGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    private void OnDisable()
    {
        m_toGameButton.onClick.RemoveListener(StartGame);
        m_toGameButton.interactable = false;
    }
}