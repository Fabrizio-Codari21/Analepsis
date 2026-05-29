using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSwitchButton : MonoBehaviour
{
    [SerializeField] private Button m_switchButton;
    [SerializeField] private NpcEvent m_onCharacterSelectedChannel;
    [SerializeField] private TMP_Text m_characterName;
    private NpcIdentity _identity;
    public void Init(NpcIdentity identity)
    {
        _identity = identity;
        m_characterName.text = identity.npcName;
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
}