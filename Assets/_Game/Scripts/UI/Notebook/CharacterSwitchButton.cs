using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CharacterSwitchButton : MonoBehaviour
{
    [SerializeField] private Button m_switchButton;
    [SerializeField] private NpcEvent m_onCharacterSelectedChannel;
    [SerializeField] private Image m_characterImage;
    private NpcIdentity _identity;
    public void Init(NpcIdentity identity)
    {
        _identity = identity;
        // m_characterName.text = identity.npcName;
        m_characterImage.sprite = identity.filePhoto;
        m_switchButton.onClick.RemoveAllListeners();
        m_switchButton.onClick.AddListener(Switch);
    }
    private void Switch()
    {
        if (_identity != null && m_onCharacterSelectedChannel != null)
        {
            m_onCharacterSelectedChannel.Raise(_identity);
        }
    }
    
    public void AddListener(UnityAction listener) => m_switchButton.onClick.AddListener(listener);


    private void OnDestroy()
    {
        m_switchButton.onClick.RemoveAllListeners();
    }
}