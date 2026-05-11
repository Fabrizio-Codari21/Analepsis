using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

// Esto despues se unificaria con el notebook manager/view normal cuando saquemos el sistema viejo.
public class DialogueTreeManager : PersistentSingleton<DialogueTreeManager>, IActivity
{
    public NotebookView view;
    public Canvas treeCanvas;
    public HorizontalLayoutGroup buttonLayout;
    public int buttonLayoutHeight = 125, arrowLayoutHeight = 225;
    public ButtonSetting buttonSetting;
    public Transform treeParent, textParent, characterParent;
    public ImageSelector arrowImage;
    public Image lockImage;
    Color _lockColor = new(0.4f, 0, 0.1f, 0.1f);

    [SerializeField] private BoolEventChannel enableCursor;
    [SerializeField] private IActivityEvent pushEvent;
    [SerializeField] private EventChannel popEvent;

    public event Action OnResume;
    public event Action OnPause;
    public event Action OnStop;

    int _currentTreeLevel = 1;
    NotebookManager _manager;

    public bool CanPopWithKey()
    {
        return true;
    }

    public void Pause()
    {
        OnPause?.Invoke();
        enableCursor?.Raise(false);
    }

    public void Resume()
    {
        OnResume?.Invoke();
        enableCursor?.Raise(true);
    }

    public void Stop()
    {
        OnPause?.Invoke();
    }

    private void Start()
    {
        _manager = NotebookManager.Instance;
    }

    public void Update()
    {
        // placeholder, obvio
        if(Input.GetKeyDown(KeyCode.T))
        {
            if(!treeCanvas.gameObject.activeInHierarchy) 
            {
                pushEvent?.Raise(this);
                treeCanvas.gameObject.SetActive(true);
                foreach(var character in _manager.FoundCharacters)
                {
                    var button = view.CreateCustomButton(
                        character.Key.npcName, 
                        characterParent, 
                        buttonSetting);

                    button.transform.localScale *= 0.9f;

                    button.DisableSub();
                    button.AddListener(async () =>
                    {
                        ClearText();  DeleteTree();
                        await BuildTree(_manager.StartedDialogues
                            [_manager.FoundCharacters.ToList().IndexOf(character)]);
                    });
                }
            }
            else
            {
                popEvent?.Raise();
                ClearText(); DeleteTree(); ClearButtons();
                treeCanvas.gameObject.SetActive(false);
            }

        }
    }

    Dialogue _currentDialogue;
    List<INode> _unlockedDialogue;
    public void DeleteTree()
    {
        foreach(Transform child in treeParent) Destroy(child.gameObject);        
    }

    public void ClearText()
    {
        view.Despawn(textParent);
    }

    public void ClearButtons() => view.Despawn(characterParent);

    public async UniTask BuildTree(DialogueNote dialogue)
    {
        _currentDialogue = dialogue.GetFullDialogue();
        _unlockedDialogue = dialogue.GetUnlockedDialogue();

        // 1) Inicia la construccion del arbol creando un layout nuevo con el nodo original
        await AddLevel(new(){_currentDialogue.startingNode}, treeParent, 1);
    }

    public async UniTask AddLevel(List<DialogueNode> nodes, Transform origin, int currentLevel = 1)
    {
        var transf = (RectTransform)buttonLayout.transform;

        // 2) Creamos un layout por cada grupo de nodos que recibamos y lo posicionamos
        // respecto al anterior (si tiene un layout previo agrega un layout intermedio de flechas).
        if (origin != treeParent)
        {
            var arrowLayout = SpawnLayout(new(transf.sizeDelta.x, arrowLayoutHeight));
            arrowLayout.transform.position = origin.position - new Vector3(0, buttonLayoutHeight + 50, 0);

            foreach (var node in nodes)
            {
                var arrow = Instantiate(arrowImage, arrowLayout.transform);
                arrow.SetRandomSprite();
                arrow.SetRotationOnGroup(nodes.IndexOf(node), nodes.Count);
                if (!node.PreviousResponse.IsAvailable()) arrow.baseImage.color = _lockColor;
            }
            origin = arrowLayout.transform;
        }

        var layout = SpawnLayout(new(transf.sizeDelta.x, buttonLayoutHeight));

        if (origin != treeParent)
            layout.transform.position = origin.position - new Vector3(0, arrowLayoutHeight - 50, 0);

        foreach (var node in nodes)
        {
            // 3) Armamos una nota con la info de este nodo
            var note = new LogNote(
                node.PreviousResponse != default ? node.PreviousResponse.responseText : "Beginning",
                new() { node.dialogueText },
                new() { node.dialogueText },
                new(_currentDialogue, new() { node.doesItProveAnything }));

            // 4) Creamos un boton para ese nodo, que se bloquea si no esta en la lista de desbloqueados.
            // (si la pista tiene condiciones aparece un simbolo mas sutil)
            if (!node.PreviousResponse.IsAvailable())
            {
                var locked = Instantiate(lockImage, layout.transform);
                locked.GetComponentInChildren<Image>().color = _lockColor;
                continue;
            }
            var unread = !_unlockedDialogue.Contains(node);
            var button = SpawnClueButton(note, layout.transform, unread);

            // 5) Si el nodo no tiene dialogo a continuacion o esta bloqueado, no hace nada mas...
            if ((node.responses.Count <= 1 && node.responses[0]?.nextNode == null) || unread) continue;
            
            // 6) ...pero si tiene dialogo, volvemos a llamar el metodo con el proximo grupo de nodos.
            var nextNodes = node.responses
                .Where(r => r.nextNode != null)
                .Select(x => { x.nextNode.PreviousResponse = x; return x.nextNode; })
                .ToList();
            await AddLevel(nextNodes, button.transform, currentLevel++);
        }     

    }

    public HorizontalLayoutGroup SpawnLayout(Vector2 idealScale)
    {
        var layout = Instantiate(buttonLayout, treeParent);
        var transf = (RectTransform)layout.transform;

        transf.sizeDelta = idealScale;
        return layout;
    }

    public ButtonFactoryObject SpawnClueButton(Note cachedNote, Transform parent, bool unread = false)
    {

        var button = view.CreateCustomButton(cachedNote.GetButtonText(), parent, buttonSetting);
        button.transform.localScale *= 0.6f;

        if(unread)
        {
            button.SetInteractable(false);
            button.DisableSub();
            button.SetText("???");
            return button;
        }

        if (_manager.markedClues.ContainsKey(cachedNote.guid))
        {
            button.DisplayMark(true);
        }
        button.AddListener(async () =>
        {
            var newToken = _manager.Cancel();
            ClearText();
            await view.PlayText(new(){cachedNote.GetInfo()}, new(), textParent, 32);
             _manager.AddDetailButtons(button, view, cachedNote);
        });
        //button.MoveSubToLast();
        button.EnableSub();
        _manager.enableButtonsEvent += (x) =>
        {
            if (button != null) button.EnableSub();
        };
        button.AddListenerToSub(() =>
        {
            if (_manager.markedClues.ContainsKey(cachedNote.guid))
            {
                button.DisplayMark(false);
                _manager.markedClues.Remove(cachedNote.guid);
                return;
            }
            _manager.m_markingPanel.isMarkingClue = true;

            button.DisplayMark(true);
            _manager.ClearMarkEvent();
            _manager.enableMarkEvent += button.DisplayMark;
            _manager.EnableButtons(false);
            _manager.markedClueEvent.Raise(cachedNote);
        });

        return button;
    }
}
