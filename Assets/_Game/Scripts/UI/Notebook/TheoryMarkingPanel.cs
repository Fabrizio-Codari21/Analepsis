using TMPro;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using PrimeTween;
using System;
using Sirenix.OdinInspector;


public class TheoryMarkingPanel : Singleton<TheoryMarkingPanel>, IActivity
{
    [Header("Event")]
    [Header("Core")]
    [SerializeField] private NoteEvent m_markNoteEvent;
    [Header("Activity")]
    [SerializeField] private IActivityEvent m_pushEvent;
    [SerializeField] private EventChannel m_popEvent;

    
    [Header("UI Setting")]
    [SerializeField] private TMP_InputField m_inputField;
    [SerializeField] private TMP_Text m_tipText;
    [SerializeField] private Button m_confirmButton, m_cancelButton;
    
    [Header("Input")]
    [SerializeField] private MarkingInputReader m_inputReader;
    
    [Header("Data")]
    [ReadOnly, ShowInInspector] private readonly Dictionary<SerializableGuid, Note> _markedClues = new();
    public  Dictionary<SerializableGuid, Note> MarkedClues => _markedClues;
    

    #region Core Funtion
    /// <summary>
    /// if return False == Remove,
    /// if return True == Mark
    /// </summary>
    /// <param name="note"></param>
    /// <returns></returns>
    private bool MarkOrRemove(Note note)
    {
        if (_markedClues.ContainsKey(note.guid))
        {
            Remove(note);
            return false;
        }
        Mark(note);
        return true;
    }
    public void Mark(Note note)
    {
        if(!_markedClues.TryAdd(note.guid, note)) return;
    }

    private void Remove(Note note)
    {
        _markedClues.Remove(note.guid);
    }
    
    

    #endregion

    #region  Activity

    public event Action OnResume;
    public event Action OnPause;
    public event Action OnStop;
    public void Resume()
    {
        
    }

    public void Pause()
    {
      
    }

    public void Stop()
    {
       
    }

    public bool CanPopWithKey()
    {
        return false;
    }
    #endregion
}