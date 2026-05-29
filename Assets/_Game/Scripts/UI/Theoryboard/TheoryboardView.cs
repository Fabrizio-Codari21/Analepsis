using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Rendering;
using Cysharp.Threading.Tasks;
using TMPro;
using System;

public class TheoryboardView : MonoBehaviour
{
    [SerializeField] NotebookManager notebookManager;
    [SerializeField] TheoryboardManager manager;
    public Transform markedLogsRoot;
    public Transform markedItemsRoot;
    public Transform markedCharactersRoot;
    public Button previousCharacterButton, nextCharacterButton;
    public ButtonSetting clueButtonSetting;
    public Button solveButton;
    public TextMeshProUGUI solveText;
    public Image solveCanvas;

    public SerializedDictionary<Whodunnit, TheoryPanel> boardRoots = new();


    private IActivity _activity;
    
    void Start()
    {
        solveButton.onClick.AddListener(async () => await TryToSolveCase(solveText));
        solveCanvas.transform.parent.gameObject.SetActive(false);
        previousCharacterButton.onClick.AddListener(() => SwitchCharacter(-1));
        nextCharacterButton.onClick.AddListener(() => SwitchCharacter(1));
        
        _activity = manager.GetComponent<IActivity>();

        _activity.OnStop += () =>
        {
            Despawn(markedLogsRoot);
            Despawn(markedItemsRoot);
        };
    }


    int _currentCharacter = 0;
    public void LoadMarkedClues()
    {

        Despawn(markedLogsRoot);
        Despawn(markedItemsRoot);
        var markedLogs = TheoryMarkingPanel.Instance.MarkedClues.Where(x => x.Value.type == PageType.Character);
        var markedList = markedLogs.ToList();
        if(!markedList.Any()) CreateClueButton("No Logs marked \n(Click the star to mark)", markedLogsRoot, null, true);
        var markedItems = TheoryMarkingPanel.Instance.MarkedClues.Where(x => x.Value.type == PageType.Objects);
        var keyValuePairs = markedItems.ToList();
        if (!keyValuePairs.Any()) CreateClueButton("No Objects marked \n(Click the star to mark)", markedItemsRoot, null, true);
        
        SwitchCharacter();
        
        if ( TheoryMarkingPanel.Instance.MarkedClues.Count <= 0) return;
        
        foreach (var log in markedList) 
        {
            CreateClueButton(log.Value.displayName, markedLogsRoot, log.Value.IsProof);
           
        }
        foreach (var item in keyValuePairs)
        {
           CreateClueButton(item.Value.displayName, markedItemsRoot, item.Value.IsProof);
          
        }

    }

    private void SwitchCharacter(int nextOrPrevious = 0)
    {
        Despawn(markedCharactersRoot);
        if(NotebookManager.Instance.FoundCharacters.Count <= 0)
        {
            CreateClueButton("No characters discovered.", markedCharactersRoot, null, true); 
            return;
        }
        
        var character = NotebookManager.Instance.FoundCharacters.ToList()[_currentCharacter];
        
        switch (nextOrPrevious)
        {
            case > 0:
            {
                _currentCharacter++;
                if (_currentCharacter > NotebookManager.Instance.FoundCharacters.Count - 1) _currentCharacter = 0;
                break;
            }
            case < 0:
            {
                _currentCharacter--;
                if (_currentCharacter < 0) _currentCharacter = NotebookManager.Instance.FoundCharacters.Count - 1;
                break;
            }
        }
        CreateClueButton(character.npcName, markedCharactersRoot, new(character,new List<Whodunnit>(character.possibleRoles)), isCharacter: true);
    }

    public ButtonFactoryObject CreateClueButton(string text, Transform parent, Tuple<Clue,List<Whodunnit>> proof, bool placeholder = false, bool isCharacter = false)
    {
        var button = FlyweightFactory.Instance.Spawn<ButtonFactoryObject>(
            clueButtonSetting,
            Vector3.zero,
            Quaternion.identity,
            parent
        );

        button.SetText(text);
        if (placeholder) 
        {
            button.SetInteractable(false);
            return button;
        }
        button.SetInteractable(true);
        button.SetBoard(boardRoots);
        button.SetView(this);
        button.SetProof(proof);
        button.SetCharacter(isCharacter);
        button.MoveToLast();
        return button;
    }

    private void Despawn(Transform root) 
    {
        foreach (var f in root.GetComponentsInChildren<IFlyweight>())
        {
            FlyweightFactory.Instance.Return(f);
        }
    }


    public async UniTask TryToSolveCase(TextMeshProUGUI solveText)
    {
        CaseAnswer viableAnswer = default;
        List<CaseAnswer> possibleAnswers = new(manager.currentCase.AllValidAnswers);
        if(boardRoots.Any(x => x.Value.droppedClue == null))
        {
            await ShowIncomplete(solveButton.GetComponentInChildren<TextMeshProUGUI>());
            return;
        }
        foreach(var item in boardRoots) 
        { 
            var choice = item.Value.droppedClue; 

            var proof = choice.GetProof();

            if (choice != null && proof.Item2.Contains(item.Key))
            {
                //SerializedList<Clue> l = default;
                possibleAnswers = possibleAnswers
                    .Where(x => x.Answer.TryGetValue(item.Key, out var l) && l.Items.Contains(proof.Item1))
                    .ToList();
                if (possibleAnswers.Count > 0) continue; else
                {
                    await manager.ConsumeAttempt(solveText);
                    return;
                }
            }
            else
            {
                await manager.ConsumeAttempt(solveText);
                return;
            }
        }

        if(possibleAnswers.Count > 1)
        {
            print("Mas de un resultado posible");
            return;
        }

        viableAnswer = possibleAnswers.FirstOrDefault();
        if(!viableAnswer.Equals(null)) 
            await manager.SolveCase(
                manager.currentCase.AllValidAnswers.IndexOf(viableAnswer),
                viableAnswer.Name);
    }

    public async UniTask ShowIncomplete(TextMeshProUGUI solveText)
    {
        var text = solveButton.GetComponentInChildren<TextMeshProUGUI>();
        var oldtext = text.text;
        text.text = "Incomplete";
        await UniTask.Delay(800);
        text.text = oldtext;
    }

    public async UniTask ShowError(TextMeshProUGUI solveText)
    {
        var oldText = solveText.text;

        print("unsolved");
        //solveText.text = "Not quite";
        solveText.text = $"Wait... this doesn't make enough sense. " +
                         $"\n I need to find a better theory, quickly... " +
                         $"\n [{manager.attemptsLeft} attempts left]";
        await DisplayErrorUI();
        solveText.text = oldText;
    }

    public async UniTask DisplayErrorUI()
    {
        solveCanvas.transform.parent.gameObject.SetActive(true);
        while (solveCanvas.color.a < 0.8f)
        {
            solveCanvas.color += new Color(0, 0, 0, 0.06f);
            solveText.color += new Color(0, 0, 0, 0.06f);
            //shadeUI.color += new Color(0, 0, 0, 0.02f * timeToFadeUI / 5);
            await UniTask.Delay(20);
        }

        await UniTask.Delay(2000);

        while (solveCanvas.color.a > 0)
        {
            solveCanvas.color -= new Color(0, 0, 0, 0.03f);
            solveText.color -= new Color(0, 0, 0, 0.03f);
            //shadeUI.color -= new Color(0, 0, 0, 0.015f * timeToFadeUI / 5);
            await UniTask.Delay(20);
        }
        solveCanvas.transform.parent.gameObject.SetActive(false);

    }



}

//[System.Serializable]
//public struct DataTest
//{
//    public TheoryPanel theory;
//    public bool check;
//}
