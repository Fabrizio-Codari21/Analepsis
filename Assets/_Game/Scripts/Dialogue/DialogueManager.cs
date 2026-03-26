using TMPro;
using UnityEngine.UI;
using UnityEngine;
using System.Linq;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance {  get; private set; }

    [Space(10)]
    [Header("UI ELEMENTS")]
    public GameObject dialogueBox; // Contenedor del dialogo
    public TextMeshProUGUI characterName, dialogueText; // Nombre del personaje y contenido del dialogo
    public Transform dialogueContainer;
    public GameObject responseButtonPrefab; // Prefab para generar botones de respuestas
    public Transform responseButtonContainer; // Contenedor de botones de respuestas

    [Space(20)]
    [Header("PLAYER SPEECH")]
    public Color playerTextColor;
    public float playerTalkingSpeed;
    public float responseDelay;

    Dialogue _currentDialogue;

    private void Awake()
    {
        // Solo tendria que haber una instancia de esto.
        if(!instance) instance = this; else Destroy(gameObject);
        HideDialogue();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Empieza el dialogo con un cierto nombre de personaje y nodo
    public void StartDialogue(string name, DialogueNode node)
    {
        ShowDialogue();
        foreach (Transform child in dialogueContainer) Destroy(child.gameObject); 

        UpdateDialogue($"{name}:", node);
    }

    public void UpdateDialogue(string name, DialogueNode node)
    {
        if(characterName.text != name) characterName.text = name;

        // Sumamos un texto a la UI.
        var dialogue = Instantiate(dialogueText, dialogueContainer);
        dialogue.text = "- "; BuildText(dialogue, node.dialogueText, _currentDialogue.characterTalkingSpeed);
        dialogue.color = _currentDialogue.characterTextColor;

        // Si hay botones de respuesta los borramos
        foreach (Transform child in responseButtonContainer)
        {
            Destroy(child.gameObject);
        }


        this.WaitAndThen(timeToWait: (1 / (_currentDialogue.characterTalkingSpeed * 10)) * (node.dialogueText.Length) + responseDelay, () =>
        {            
            // Creamos botones de respuesta en base al nodo activo
            foreach (DialogueResponse response in node.responses)
            {
                GameObject buttonObj = Instantiate(responseButtonPrefab, responseButtonContainer);
                buttonObj.GetComponentInChildren<TextMeshProUGUI>().text = response.responseText;

                // Hacemos que cada boton llame a SelectResponse para que se encargue de continuar.
                buttonObj.GetComponent<Button>().onClick.AddListener(() => SelectResponse(response));
                //buttonObj.GetComponent<Button>().clicked += () => SelectResponse(response, name);
            }
        },
        cancelCondition: () => !IsDialogueActive());

    }

    // Elige una respuesta y activa el proximo nodo
    public void SelectResponse(DialogueResponse response)
    {
        // Si hay un nodo siguiente, sumamos nuestra respuesta a la UI; si no, cortamos.
        if (response.nextNodeId != "")
        {
            // Si hay botones de respuesta los borramos
            foreach (Transform child in responseButtonContainer)
            {
                Destroy(child.gameObject);
            }

            var answer = Instantiate(dialogueText, dialogueContainer);
            answer.text = "- '"; BuildText(answer, $"'{response.responseText}'");
            answer.color = playerTextColor;
        }
        else
        {
            EndDialogue();
            return;
        }

        this.WaitAndThen(timeToWait: (1/(playerTalkingSpeed*10)) * (response.responseText.Length) + responseDelay, () =>
        {
            DialogueNode nextNode = GetNodeById(response.nextNodeId);

            // Si hay nodo...
            if (nextNode != null && !nextNode.IsLastNode())
            {
                UpdateDialogue(characterName.text, nextNode); // ...arranca el proximo dialogo
            }
            else
            {
                // Si no, se termina el dialogo.
                EndDialogue();
            }
        },
        cancelCondition: () => !IsDialogueActive());

    }

    // Para desactivar y activar la UI de dialogo.
    public void HideDialogue() => dialogueBox.SetActive(false);
    private void ShowDialogue() => dialogueBox.SetActive(true);
    public bool IsDialogueActive() => dialogueBox.activeSelf;

    // Si todos los nodos estan guardados en el dialogo actual, buscamos el que tenga el id correcto.
    DialogueNode GetNodeById(string id) => _currentDialogue.dialogueNodes.FirstOrDefault(node => node.id == id);

    public void SetCurrentDialogue(Dialogue dialogue) => _currentDialogue = dialogue;

    public void BuildText(TextMeshProUGUI dialogue, string text, float speed = 4)
    {
        var charAmount = text.Length;
        int newCharAmount = 0;
        float charSpeed = 1 / (speed * 10);

        //print("Char amount: " + charAmount);
        this.SteppedExecution(duration: charSpeed * charAmount, stepLength: charSpeed, () =>
        {
            if (newCharAmount < charAmount)
            {
                dialogue.text += text[newCharAmount];
                newCharAmount++;
                //print("New Char Amount: " + newCharAmount);
            }
            else return;
        },
        cancelCondition: () => newCharAmount >= charAmount);

    }
    
    public void EndDialogue()
    {
        InputsManager.instance.EnableInputReader((InputsManager.instance.m_inputReader[0], true));
        HideDialogue();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

}
