using UnityEngine;
using UnityEngine.UI;

public class CharacterNotebookButton : MonoBehaviour
{
    
    [SerializeField] private Button m_button;

    private void OnDestroy()
    {
        m_button.onClick.RemoveAllListeners();
    }
}