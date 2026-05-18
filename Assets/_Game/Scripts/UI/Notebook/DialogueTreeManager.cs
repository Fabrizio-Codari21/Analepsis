using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading;
using UnityEngine.UI;
using TMPro;
using PrimeTween;
using UnityEngine.TextCore.Text;

// Esto despues se unificaria con el notebook manager/view normal cuando saquemos el sistema viejo.
public class DialogueTreeManager : Singleton<DialogueTreeManager>, IActivity
{
    #region Variables

    public NotebookView view;
    [HideInInspector] public Transform _handler;

    public ScrollRect treeScroll;
    public float scrollSpeed = 10f, transitionSpeed = 6f;
    public DialogueTreeUI contentUI;
    public GameObject treeAnchor, normalCanvas;
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

    #endregion

    #region IActivity
    public bool CanPopWithKey()
    {
        return true;
    }

    public void Pause()
    {
        OnPause?.Invoke();
    }

    public void Resume()
    {
        OnResume?.Invoke();       
    }

    public void Stop()
    {
        OnPause?.Invoke();
    }
    #endregion

    #region Controls & Transition

    NotebookManager _manager;
    Vector3 _scrollScale;

    private void Start()
    {
        _manager = NotebookManager.Instance;
        _handler = _manager.handler;
        _scrollScale = treeParent.localScale;
        treeAnchor.gameObject.SetActive(false);
    }

    private void Update()
    {
        // placeholder, obvio
        //if(Input.GetKeyDown(KeyCode.T) && !_manager.m_markingPanel.isMarkingClue)
        //{
        //    _ = ToggleTree();
        //}
        // otro placeholder, para mover el scroll con el teclado
        MoveTreeScroll();
    }
    public async UniTask ToggleTree(bool on = true, NpcIdentity openingCharacter = default)
    {
        AudioManager.Instance.SelectSFX(SFXType.Player, on ? "FlipForwards" : "FlipBackwards");
        if (on)
        {
            //pushEvent?.Raise(this);
            //enableCursor?.Raise(true);
            normalCanvas.gameObject.SetActive(false);
            await RotateHand(true);
            view.m_renderCamera.gameObject.transform.Rotate(0, 0, 90);
            treeScroll.verticalNormalizedPosition = 0;
            treeScroll.horizontalNormalizedPosition = 0;
            treeParent.localScale = _scrollScale;
            treeAnchor.gameObject.SetActive(true);
            
            var returnButton = view.CreateCustomButton("- RETURN -", characterParent, buttonSetting);
            returnButton.transform.localScale = new Vector3(0.6f, 0.9f, 0.9f);
            returnButton.DisableSub();
            returnButton.AddListener(async () =>
            {
                await ToggleTree(false);
            });

            foreach (var character in _manager.FoundCharacters)
            {
                var button = view.CreateCustomButton(
                    character.Key.npcName,
                    characterParent,
                    buttonSetting);

                button.transform.localScale = new Vector3(0.6f,0.9f,0.9f);

                button.DisableSub();
                button.AddListener(async () =>
                {
                    ClearText();
                    DeleteTree();
                    treeScroll.verticalNormalizedPosition = 0;
                    treeScroll.horizontalNormalizedPosition = 0;
                    treeParent.localScale = _scrollScale;
                    await BuildTree(_manager.StartedDialogues
                        [_manager.FoundCharacters.ToList().IndexOf(character)]);
                });
            }
            if(openingCharacter != default)
                await BuildTree(_manager.StartedDialogues
                        [_manager.FoundCharacters.ToList().IndexOf(
                            _manager.FoundCharacters.First(x => x.Key == openingCharacter))]);
        }
        else
        {
            //popEvent?.Raise();
            view.ClearDetail();
            ClearText(); DeleteTree(); ClearButtons();
            treeScroll.verticalNormalizedPosition = 0;
            treeScroll.horizontalNormalizedPosition = 0;
            treeParent.localScale = _scrollScale;
            treeAnchor.gameObject.SetActive(false);
            view.m_renderCamera.gameObject.transform.Rotate(0, 0, -90);
            await RotateHand(false);
            normalCanvas.gameObject.SetActive(true);
            //enableCursor?.Raise(false);
        }

    }
    Sequence _seq = new();
    public async UniTask RotateHand(bool horizontal = true)
    {
        if (_handler == null) return;
        //Tween.StopAll(_handler);
        _seq.Complete();

        _seq = Sequence.Create();

        _ = _seq.Group(Tween.LocalRotation(
            target: _handler,
            endValue: _handler.localRotation.eulerAngles + new Vector3(0, 0, (horizontal ? 80 : -80)),
            duration: 2 / transitionSpeed,
            ease: Ease.OutCirc));

        _ = _seq.Group(Tween.LocalPosition(
            target: _handler,
            endValue: _handler.localPosition 
            + (new Vector3(0.4f,0.4f,-0.4f) / UIManager.Instance.AspectRatioScale(0.0001f)) 
            * (horizontal ? 1 : -1),
            duration: 2 / transitionSpeed,
            ease: Ease.OutCirc));

        await _seq;
    }

    public void MoveTreeScroll()
    {        
        if (treeAnchor.gameObject.activeInHierarchy && !_manager.m_markingPanel.isMarkingClue)
        {
            if (Input.GetKey(KeyCode.W)) treeParent.position -= new Vector3(0,Time.deltaTime,0) * 100 * scrollSpeed;
            else if (Input.GetKey(KeyCode.S)) treeParent.position += new Vector3(0, Time.deltaTime, 0) * 100 * scrollSpeed;
            if (Input.GetKey(KeyCode.D)) treeParent.position -= new Vector3(Time.deltaTime, 0, 0) * 100 * scrollSpeed;
            else if (Input.GetKey(KeyCode.A)) treeParent.position += new Vector3(Time.deltaTime, 0, 0) * 100 * scrollSpeed;

            if(Input.mouseScrollDelta != Vector2.zero)
            {
                if (Input.mouseScrollDelta.y > 0) 
                    treeParent.localScale += new Vector3(Time.deltaTime, Time.deltaTime, 0) * 3 * scrollSpeed;
                else if (Input.mouseScrollDelta.y < 0) 
                    treeParent.localScale -= new Vector3(Time.deltaTime, Time.deltaTime, 0) * 3 * scrollSpeed;
                
                treeParent.localScale = new Vector3(
                Mathf.Clamp(treeParent.localScale.x, _scrollScale.x / 2, _scrollScale.x * 2),
                Mathf.Clamp(treeParent.localScale.y, _scrollScale.y / 2, _scrollScale.y * 2),
                treeParent.localScale.z);
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                treeScroll.verticalNormalizedPosition = 0;
                treeScroll.horizontalNormalizedPosition = 0;
                treeParent.localScale = _scrollScale;
            }
        }
    }

    #endregion

    #region Tree Generation

    Dialogue _currentDialogue;
    List<INode> _unlockedDialogue;

    public void DeleteTree()
    {
        foreach(Transform child in treeParent) Destroy(child.gameObject);
    }    
    public void ClearText() => view.Despawn(textParent);
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
        
        //Debug.Log("Current level: " + currentLevel);
        Debug.Log($"Creating level {currentLevel - 1} from: " + origin.gameObject.name);

        // 2) Creamos un layout por cada grupo de nodos que recibamos y lo posicionamos
        // respecto al anterior (si tiene un layout previo agrega un layout intermedio de flechas).
        HorizontalLayoutGroup arrowLayout = null;
        if (origin != treeParent)
        {
            arrowLayout = SpawnLayout(origin.position - new Vector3(0, buttonLayoutHeight + 150, 0), new(transf.sizeDelta.x, arrowLayoutHeight));
            arrowLayout.gameObject.name = $"ArrowLayer {currentLevel - 1}";
            arrowLayout.spacing = 500 / Mathf.Pow(nodes.Count, (currentLevel == 2 ? currentLevel : currentLevel - 1));
            //arrowLayout.transform.position = origin.position - new Vector3(0, buttonLayoutHeight + 50, 0);


            foreach (var node in nodes)
            {
                var arrow = Instantiate(arrowImage, arrowLayout.transform);
                arrow.gameObject.name = $"Arrow {nodes.IndexOf(node) + 1} - Level {currentLevel - 1}";
                arrow.SetRandomSprite();
                arrow.SetRotationOnGroup(nodes.IndexOf(node), nodes.Count);
                if (!node.PreviousResponse.IsAvailable()) arrow.baseImage.color = _lockColor;
            }
            origin = (RectTransform)arrowLayout.transform;
        }

        var layout = SpawnLayout(origin.position - new Vector3(0, arrowLayoutHeight + 50, 0), new(transf.sizeDelta.x, buttonLayoutHeight));
        layout.gameObject.name = $"TreeLevel {currentLevel}";
        layout.spacing = 500 / Mathf.Pow(nodes.Count, currentLevel - 1);

        //if (origin != treeParent) layout.transform.position = origin.position - new Vector3(0, arrowLayoutHeight - 50, 0);
        bool allNodesSpawned = false; bool compensated = false;
        foreach (var node in nodes)
        {
            // 3) Armamos una nota con la info de este nodo
            var note = new LogNote(
                node.PreviousResponse != null ? node.PreviousResponse.responseText : "Beginning",
                new() { node.dialogueText },
                new() { node.dialogueText },
                new(_currentDialogue, new() { node.doesItProveAnything }));

            // 4) Creamos un boton para ese nodo, que se bloquea si no esta en la lista de desbloqueados.
            // (si la pista tiene condiciones aparece un simbolo mas sutil)
            if (node.PreviousResponse != null && !node.PreviousResponse.IsAvailable())
            {
                var locked = Instantiate(lockImage, layout.transform);
                locked.gameObject.name = $"Lock {nodes.IndexOf(node) + 1} - Level {currentLevel}";
                locked.GetComponentInChildren<Image>().color = _lockColor;
                if (node == nodes.Last()) allNodesSpawned = true;
                continue;
            }
            var unread = !_unlockedDialogue.Contains(node);
            var button = SpawnClueButton(note, layout.transform, unread);

            button.gameObject.name = $"Button {nodes.IndexOf(node) + 1} - Level {currentLevel}";

            if (node == nodes.Last()) allNodesSpawned = true;

            // 5) Si el nodo no tiene dialogo a continuacion o esta bloqueado, no hace nada mas...
            if (node.responses.Count <= 0 || unread) continue;
            else if (node.responses.Count == 1 && node.responses[0]?.nextNode == null) continue;
            
            // 6) ...pero si tiene dialogo, volvemos a llamar el metodo con el proximo grupo de nodos.
            var nextNodes = node.responses
                .Where(r => r.nextNode != null)
                .Select(x => { x.nextNode.PreviousResponse = x; return x.nextNode; })
                .ToList();

            // 7) Cuando el ultimo nodo del layout termine de aparecer (para asegurarnos de que ya
            // esten bien posicionados), recien ahi llamamos al metodo para hacer los proximos layouts.
            this.ExecuteAfterTrue(() => allNodesSpawned, async () =>
            {
                if(nodes.Count > 2 && !compensated)
                {
                    arrowLayout.transform.position -= new Vector3(arrowLayout.spacing * (nodes.Count - 2), 0, 0);
                    layout.transform.position -= new Vector3(layout.spacing * (nodes.Count - 2), 0, 0);
                    compensated = true;
                }

                await AddLevel(nextNodes, button.transform, currentLevel + 1);
            }, 
            cancelCondition: () => !treeAnchor.gameObject.activeInHierarchy);

        }     

    }

    public HorizontalLayoutGroup SpawnLayout(Vector3 position, Vector2 idealScale)
    {
        var layout = Instantiate(buttonLayout, treeParent);
        var transf = (RectTransform)layout.transform;

        layout.transform.position = position;
        transf.sizeDelta = idealScale;
        return layout;
    }

    public ButtonFactoryObject SpawnClueButton(Note cachedNote, Transform parent, bool unread = false)
    {
        var button = view.CreateCustomButton(cachedNote.GetButtonText(), parent, buttonSetting);
        button.transform.localScale = new Vector3(0.6f,0.8f,1f);
        button.transform.Rotate(0, 0, UnityEngine.Random.Range(-5, 5));

        if (unread)
        {
            button.SetInteractable(false);
            button.DisableSub();
            button.SetText("???");
            return button;
        }
        else
        {
            var text = button.GetComponentInChildren<TextMeshProUGUI>();
            text.fontSizeMax = 30; text.fontSizeMin = 26;
        }

        if (_manager.markedClues.ContainsKey(cachedNote.guid))
        {
            button.DisplayMark(true);
        }
        button.AddListener(async () =>
        {
            var newToken = _manager.Cancel();
            ClearText();
            await contentUI.PlayText(new(){cachedNote.GetInfo()}, CancellationToken.None, textParent, 36);
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

    #endregion

}