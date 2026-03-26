using TMPro;
using UnityEngine.UI;
using UnityEngine;
using System.Linq;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance {  get; private set; }

    public GameObject dialogueBox; // Contenedor del dialogo
    public TextMeshProUGUI characterName, dialogText; // Nombre del personaje y contenido del dialogo
    public GameObject responseButtonPrefab; // Prefab para generar botones de respuestas
    public Transform responseButtonContainer; // Contenedor de botones de respuestas

    public Dialogue currentDialogue;

    private void Awake()
    {
        // Solo tendria que haber una instancia de esto.
        if(!instance) instance = this; else Destroy(gameObject);
        HideDialogue();
    }

    // Empieza el dialogo con un cierto nombre de personaje y nodo
    public void StartDialogue(string name, DialogueNode node)
    {
        ShowDialogue();
        UpdateDialogue($"{name}:", node);
    }

    public void UpdateDialogue(string name, DialogueNode node)
    {
        characterName.text = name;
        dialogText.text = "- " + node.dialogueText;

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
            buttonObj.GetComponent<Button>().onClick.AddListener(() => SelectResponse(response, name));
            //buttonObj.GetComponent<Button>().clicked += () => SelectResponse(response, name);
        }
    }

    // Elige una respuesta y activa el proximo nodo
    public void SelectResponse(DialogueResponse response, string title)
    {
        DialogueNode nextNode = GetNodeById(response.nextNodeId);

        // Si hay nodo...
        if (nextNode != null && !nextNode.IsLastNode())
        {
            UpdateDialogue(characterName.text, nextNode); // ...arranca el proximo dialogo
        }
        else
        {
            // Si no, apagamos la UI.
            InputsManager.instance.EnableInputReader((InputsManager.instance.m_inputReader[0], true));
            HideDialogue();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    // Para desactivar y activar la UI de dialogo.
    public void HideDialogue() => dialogueBox.SetActive(false);
    private void ShowDialogue() => dialogueBox.SetActive(true);
    public bool IsDialogueActive() => dialogueBox.activeSelf;

    // Si todos los nodos estan guardados en el dialogo actual, buscamos el que tenga el id correcto.
    DialogueNode GetNodeById(string id) => currentDialogue.dialogueNodes.FirstOrDefault(node => node.id == id);

    public void SetCurrentDialogue(Dialogue dialogue) => currentDialogue = dialogue;

}
