using TMPro;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using PrimeTween;
using Random = UnityEngine.Random;


public class MarkingPanel : MonoBehaviour
{


    #region UI

    [Header("UI")]
    [SerializeField] GameObject mainUI;
    [SerializeField] TMP_InputField inputField;
    [SerializeField] TextMeshProUGUI tipText;
    [SerializeField] Button markClueButton, cancelButton;

    #endregion


    private void Start()
    {
        mainUI.SetActive(false);
    }

    #region Tips Dicionary
    private Dictionary<NoteType, List<string>> _tips = new()
    {
        { NoteType.Log, new List<string>()
        {
            "e.g. 'X claims Y hates Z.'",
            "e.g. 'X heard Y talking with Z.'",
            "e.g. 'X thinks Y is hiding something.'",
            "e.g. 'X said they know about Y.'",
            "e.g. 'X avoided talking about Y.'",
            "e.g. 'X saw Y carrying Z.'",
        }},
        { NoteType.Objects, new List<string>()
        {
            "e.g. 'X was used by Y.'",
            "e.g. 'X thinks this is dangerous.'",
            "e.g. 'X knows about this Y.'",
            "e.g. 'X might belong to Y.'",
            "e.g. 'X was given to Y by Z.'",
            "e.g. 'X gonna give it to Y.'",
        }},
    };
    
    private string RandomTip(NoteType type) => _tips[type][Random.Range(0, _tips[type].Count - 1)];
    
    #endregion

    
    #region Core
    private UniTaskCompletionSource<string> _completionSource;
    public async UniTask<string> RenameAndMarkClue(Note clue)
    {
        _completionSource = new UniTaskCompletionSource<string>();
       
        inputField.text = clue.displayName;
        tipText.text = RandomTip(clue.type);
        markClueButton.interactable = true;
        cancelButton.interactable = true;
        inputField.interactable = true;
        
        mainUI.SetActive(true);
       
        await UnfoldPanel(true);
        markClueButton.onClick.RemoveAllListeners();
        markClueButton.onClick.AddListener(OnConfirm);

        cancelButton.onClick.RemoveAllListeners();
        cancelButton.onClick.AddListener(OnCancel);
        
        return await _completionSource.Task;
    }
    
    private void OnConfirm()
    {
        string finalName = string.IsNullOrEmpty(inputField.text) ? "Unamed Clue" : inputField.text;
        Finish(finalName);
    }

    private void OnCancel()
    {
        Finish(null); 
    }

    private async void Finish(string result)
    {
        markClueButton.interactable = false;
        cancelButton.interactable = false;
        await UnfoldPanel(false);
        mainUI.SetActive(false);
        _completionSource.TrySetResult(result);
    }

    #endregion
    private async UniTask UnfoldPanel(bool isOpening)
    {
        if (mainUI == null) return;
        Tween.StopAll(mainUI.gameObject.transform);

        var seq = Sequence.Create();

        if (isOpening)
        {
            mainUI.gameObject.transform.localScale = new Vector3(0, 1, 1);
            await seq.Group(Tween.ScaleX(mainUI.gameObject.transform, 1f, 0.3f, Ease.OutBack));
        }
        else
        {
            await seq.Group(Tween.ScaleX(mainUI.gameObject.transform, 0f, 0.2f, Ease.InQuad));
        }


        await seq;

    }


}
