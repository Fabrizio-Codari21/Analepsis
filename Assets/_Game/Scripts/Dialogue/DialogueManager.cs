using TMPro;
using UnityEngine.UIElements;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance {  get; private set; }

    public GameObject dialogueBox; // Contenedor del dialogo
    public TextMeshProUGUI characterName, dialogText; // Nombre del personaje y contenido del dialogo
    public GameObject responseButtonPrefab; // Prefab para generar botones de respuestas
    public Transform responseButtonContainer; // Contenedor de botones de respuestas

    private void Awake()
    {
        // Solo tendria que haber una instancia de esto.
        if(!instance) instance = this; else Destroy(gameObject);
    }

    // Empieza el dialogo con un cierto nombre y nodo
    public void StartDialogue(string title, DialogueNode node)
    {
        ShowDialogue();

        characterName.text = title;
        dialogText.text = node.dialogueText;

        // Si hay botones de respuesta los borramos
        foreach (Transform child in responseButtonContainer)
        {
            Destroy(child.gameObject);
        }

        // Creamos botones de respuesta en base al nodo activo
        foreach (DialogueResponse response in node.responses)
        {
            GameObject buttonObj = Instantiate(responseButtonPrefab, responseButtonContainer);
            buttonObj.GetComponentInChildren<TextMeshProUGUI>().text = response.responseText;

            // Hacemos que cada boton llame a SelectResponse para que se encargue de continuar.
            //buttonObj.GetComponent<Button>().onClick.AddListener(() => SelectResponse(response, title));
            buttonObj.GetComponent<Button>().clicked += () => SelectResponse(response, title);
        }
    }

    // Elige una respuesta y activa el proximo nodo
    public void SelectResponse(DialogueResponse response, string title)
    {
        // Si hay nodo...
        if (!response.nextNode.IsLastNode())
        {
            StartDialogue(title, response.nextNode); // ...arranca el proximo dialogo
        }
        else
        {
            // Si no, apagamos la UI.
            HideDialogue();
        }
    }

    // Para desactivar y activar la UI de dialogo.
    public void HideDialogue() => dialogueBox.SetActive(false);
    private void ShowDialogue() => dialogueBox.SetActive(true);
    public bool IsDialogueActive() => dialogueBox.activeSelf;

}
