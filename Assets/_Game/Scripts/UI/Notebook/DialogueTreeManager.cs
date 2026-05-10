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
    public Transform treeParent, textParent;
    public ImageSelector arrowImage;

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
        treeCanvas.gameObject.SetActive(false);
        popEvent?.Raise();
    }

    public void Resume()
    {
        OnResume?.Invoke();
        enableCursor?.Raise(true);
        treeCanvas.gameObject.SetActive(true);
    }

    public void Stop()
    {
        throw new NotImplementedException();
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
            pushEvent?.Raise(this);
            if(_manager.StartedDialogues.Count > 0) _ = BuildTree(_manager.StartedDialogues[0]);
        }
    }

    Dialogue _currentDialogue;
    List<INode> _unlockedDialogue;
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
            arrowLayout.transform.position = origin.position - new Vector3(0, buttonLayoutHeight + 20, 0);
            foreach (var node in nodes)
            {
                var arrow = Instantiate(arrowImage, arrowLayout.transform);
                arrow.SetRandomSprite();
            }
            origin = arrowLayout.transform;
        }

        var layout = SpawnLayout(new(transf.sizeDelta.x, buttonLayoutHeight));

        if (origin != treeParent)
            layout.transform.position = origin.transform.position - new Vector3(0, arrowLayoutHeight + 20, 0);

        foreach (var node in nodes)
        {
            // 3) Armamos una nota con la info de este nodo
            var note = new LogNote(
                node.tag,
                new() { node.dialogueText },
                new() { node.dialogueText },
                new(_currentDialogue, new() { node.doesItProveAnything }));

            // 4) Creamos un boton para ese nodo, que se bloquea si no esta en la lista de desbloqueados.
            var locked = !_unlockedDialogue.Contains(node);
            var button = SpawnClueButton(note, layout.transform, locked);

            // 5) Si el nodo no tiene dialogo a continuacion o esta bloqueado, no hace nada mas...
            if ((node.responses.Count <= 1 && node.responses[0].nextNode == null) || locked) continue;
            
            // 6) ...pero si tiene dialogo, volvemos a llamar el metodo con el proximo grupo de nodos.
            var nextNodes = node.responses.Select(x => x.nextNode).ToList();
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

    public ButtonFactoryObject SpawnClueButton(Note cachedNote, Transform parent, bool locked = false)
    {
        var button = view.CreateCustomButton(cachedNote.GetButtonText(), parent, buttonSetting);
        button.transform.localScale *= 0.6f;

        if (_manager.markedClues.ContainsKey(cachedNote.guid))
        {
            button.DisplayMark(true);
        }
        button.AddListener(() =>
        {
            var newToken = _manager.Cancel();
            view.ClearDetail();
            _ = _manager.SelectNote(button, cachedNote, newToken.Token);
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
