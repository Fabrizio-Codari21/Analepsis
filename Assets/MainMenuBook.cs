using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuBook : MonoBehaviour
{
    [SerializeField] private Button m_firstLevelButton;// ahora si tenemos 1 solo nivel
    [SerializeField] private Button m_newLevelButton;
    [SerializeField] private string firstSceneName;
    [SerializeField] private string newSceneName;

    private IActivity _activity;


    private void Start()
    {
        _activity = GetComponentInParent<IActivity>();

        _activity.OnPause += () => { enabled = false; };
        _activity.OnResume += () => { enabled = true; };
        
        enabled = false;
    }

    private void OnEnable()
    {
        m_firstLevelButton.onClick.AddListener(PlayFirstLevel);
        m_firstLevelButton.interactable = true;
        m_newLevelButton.onClick.AddListener(PlayNewLevel);
        m_newLevelButton.interactable = true;
    }

    private void PlayFirstLevel()
    {
        _ = this.AsyncLoader(firstSceneName);
    }
    private void PlayNewLevel()
    {
        _ = this.AsyncLoader(newSceneName);
    }

    private void OnDisable()
    {
        m_newLevelButton.onClick.RemoveListener(PlayFirstLevel);
        m_newLevelButton.interactable = false;
        m_newLevelButton.onClick.RemoveListener(PlayNewLevel);
        m_newLevelButton.interactable = false;
    }
}