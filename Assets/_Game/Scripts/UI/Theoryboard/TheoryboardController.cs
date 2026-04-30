using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TheoryboardController : MonoBehaviour, IActivity
{
    [SerializeField] TheoryboardView view;
    
    [SerializeField] int maxSolveAttempts;
    public int attemptsLeft { get; private set; }

    [SerializeField] private EventChannel m_openTheoryBoardChannel;
    [SerializeField] private BoolEventChannel enableCursor;
    [SerializeField] private BoardInputReader inputReaderBoard;
    private readonly Model _model = new Model();
    private class Model
    {

        #region NpcIdentity       
        private readonly SelectionList<NpcIdentity> _suspiciousNpcs = new SelectionList<NpcIdentity>();
        public NpcIdentity CurrentSuspicious => _suspiciousNpcs.CurrentItem;
        public void AddSuspiciousNpc(NpcIdentity npc) => _suspiciousNpcs.Add(npc);
        public void RemoveSuspiciousNpc(NpcIdentity npc) => _suspiciousNpcs.Remove(npc);
        public void NextNpc() => _suspiciousNpcs.Next();
        public void PreviousNpc() => _suspiciousNpcs.Previous();

         #endregion
        #region ProofTarget
        private readonly SelectionList<ProofTarget> _validatedTargets = new SelectionList<ProofTarget>();
        public ProofTarget CurrentTarget => _validatedTargets.CurrentItem;
        public void AddValidatedTarget(ProofTarget target) => _validatedTargets.Add(target);
        public void RemoveValidatedTarget(ProofTarget target) => _validatedTargets.Remove(target);
        public void NextTarget() => _validatedTargets.Next();
        public void PreviousTarget() => _validatedTargets.Previous();
        #endregion
    }
    
    #region Utility
    private class SelectionList<T> where T : class
    {
        private readonly List<T> _items = new List<T>();
        private int _currentIndex = -1;

        public T CurrentItem => (_currentIndex >= 0 && _currentIndex < _items.Count) ? _items[_currentIndex] : null;
        public int Count => _items.Count;

        public void Add(T item)
        {
            if (item == null || _items.Contains(item)) return;
            _items.Add(item);
            if (_currentIndex == -1) MoveTo(0);
        }

        public void Remove(T item)
        {
            int index = _items.IndexOf(item);
            if (index == -1) return;

            _items.RemoveAt(index);

            if (_items.Count == 0)
            {
                _currentIndex = -1;
            }
            else
            {
                if (index > _currentIndex) return;
                _currentIndex = _items.Count > 0 ? (_currentIndex >= _items.Count ? 0 : _currentIndex) : -1;
                
                if (index < _currentIndex) _currentIndex--;
                else if (index == _currentIndex && _currentIndex >= _items.Count) _currentIndex = 0;
            }
        }

        public void Next()
        {
            if (_items.Count == 0) return;
            MoveTo((_currentIndex + 1) % _items.Count);
        }

        public void Previous()
        {
            if (_items.Count == 0) return;
            MoveTo((_currentIndex - 1 + _items.Count) % _items.Count);
        }

        private void MoveTo(int index)
        {
            if (index >= 0 && index < _items.Count) _currentIndex = index;
        }
    }
    #endregion

    #region Proof Managment
    private readonly ProofGarage _proofGarage = new();
    private void AddProof(IProof proof,ProofConnection connection) => _proofGarage.Add(proof, connection);
    private void RemoveProof(IProof proof, ProofConnection connection) => _proofGarage.Remove(proof, connection);
    private void ClearProof() => _proofGarage.Clear();
    private bool ValidateProof(Proof proofType, IProof proof)
    {
        return _proofGarage.CheckProof(_model.CurrentSuspicious,_model.CurrentTarget,proofType, proof);
    }
    #endregion
    
    #region Screen Event
    [Header("Screen Event")]
    [SerializeField] private IActivityEvent pushEvent;
    [SerializeField] private EventChannel popEvent;
    #endregion

    #region Unity Life
    private void Start()
    {
        m_openTheoryBoardChannel.OnEventRaised += Open;
        m_openTheoryBoardChannel.OnEventRaised += view.LoadMarkedClues;
        inputReaderBoard.Close += Close;
        attemptsLeft = maxSolveAttempts;
        view = Instantiate(view, transform);
        view.OnRequestSolved += TrySolveCase;
    }


    private void OnDestroy()
    {
        view.OnRequestSolved -=  TrySolveCase;
    }
    #endregion

    #region IActivity
    public event Action OnResume;
    public event Action OnPause;
    public event Action OnStop;

    public bool CanPopWithKey()
    {
        return true;
    }

    public void Pause()
    {
        OnPause?.Invoke();
        inputReaderBoard.SetEnable(false);
        enableCursor.Raise(false);
        
    }

    public void Resume()
    {
        OnResume?.Invoke();
        inputReaderBoard.SetEnable();
        enableCursor.Raise(true);
    }

    public void Stop()
    {
        OnStop?.Invoke();
        Pause();
    }
    
    private void Open() => pushEvent.Raise(this);
    private void Close() => popEvent.Raise();
    #endregion

    private void TrySolveCase()
    {
        attemptsLeft--;
        
        var result = CheckCase();
        
        if (result)
        {
            SolveCase();  // si esta aprobado
            return;
        }

        if (attemptsLeft <= 0)
        {
            FailCase();  // si no esta aprobado y no queda chances
            return;
        }
        
        TryAgain();
        
    }
    
    private bool CheckCase()
    {
        return true;
    }

    
    private void SolveCase()
    {
        
        this.AsyncLoader("WinScene");
    }
    private void FailCase()
    {
      
        this.AsyncLoader("LoseScene");
    }


    private void TryAgain()
    {
        Debug.Log("Show Erro canva");
    }
  
}


public class ProofGarage
{
    private readonly Dictionary<IProof,List<ProofConnection>>  _data = new ();

    public void Add(IProof proof, ProofConnection connection)
    {
        if (!_data.TryGetValue(proof, out var connections))
        {
            connections = new List<ProofConnection>();
            _data.Add(proof, connections);
        }
        connections.Add(connection);
    }
    public void Remove(IProof proof, ProofConnection connection)
    {
        if (!_data.TryGetValue(proof, out var connections)) return;
        
        connections.Remove(connection);
        
        if (connections.Count == 0) { _data.Remove(proof); }
    }
    public void Clear()
    {
        _data.Clear();
    }

    public bool CheckProof(NpcIdentity subject, ProofTarget proofTarget, Proof proofType, IProof proof)
    {
        if (!_data.TryGetValue(proof, out var connections)) { return false; } // check de que si este proof(item, dialogue) puede ser un clue o no, si no hay si significa que no descubrio nada para este proof
        
        if(subject == proofTarget) { return false; } // si son iguales 
        return connections.Any(c =>     // checkeo si alguno de la lista cumple que sujeto objeto y si esta relacionado a proof que pide, si cumple con todos return true
            c.Subject == subject && 
            c.Object == proofTarget && 
            c.ProofRelative == proofType);
    }
}

public enum Proof
{
    NoProof,
    Victim,
    Killer,
    Motive,
    Weapon,
}


