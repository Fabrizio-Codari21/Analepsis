using UnityEngine;
using Unity.Cinemachine;
using System;
using System.Collections.Generic;

public class CameraManager : PersistentSingleton<CameraManager>
{
    private Dictionary<ICinemachineCamera, Action> _enterActions = new();
    private Dictionary<ICinemachineCamera, Action> _exitActions = new();
    private Dictionary<ICinemachineCamera, Action> _fullyBlendedActions = new();

    [SerializeField] private CinemachineBrainEvents brainEvents;

    [SerializeField] private CinemachineBrain brain;

    
    private ICinemachineCamera _lastCamera;


    private void Start()
    {
        brainEvents.CameraActivatedEvent.AddListener(OnCameraActivated);
        brainEvents.BlendFinishedEvent.AddListener(OnBlendFinished);
        brainEvents.CameraCutEvent.AddListener(OnCameraCut);
    }

    private void OnDestroy()
    {
        if (brainEvents == null) return;

        brainEvents.CameraActivatedEvent.RemoveListener(OnCameraActivated);
        brainEvents.BlendFinishedEvent.RemoveListener(OnBlendFinished);
        brainEvents.CameraCutEvent.RemoveListener(OnCameraCut);
    }

    private void OnCameraActivated(ICinemachineMixer _, ICinemachineCamera to)
    {
        
        if (!brain) return;

        if (_lastCamera != null)
        {
            Invoke(_exitActions, _lastCamera);
        }
  
        _lastCamera = to;
        Invoke(_enterActions, to);
    }
    
    private void OnBlendFinished( ICinemachineMixer mixer, ICinemachineCamera cam)
    {
        Invoke(_fullyBlendedActions, cam);
    }
    
    private void OnCameraCut(ICinemachineMixer mixer, ICinemachineCamera cam)
    {
        Invoke(_fullyBlendedActions, cam);
        _lastCamera = cam;
    }

    #region Public API

    public void AddEnter(ICinemachineCamera cam, Action action) => Add(_enterActions, cam, action);

    public void AddExit(ICinemachineCamera cam, Action action)
    {
        Add(_exitActions, cam, action);
    } 

    public void AddFullyBlended(ICinemachineCamera cam, Action action) => Add(_fullyBlendedActions, cam, action);

    public void RemoveEnter(ICinemachineCamera cam, Action action) => Remove(_enterActions, cam, action);

    public void RemoveExit(ICinemachineCamera cam, Action action) => Remove(_exitActions, cam, action);

    public void RemoveFullyBlended(ICinemachineCamera cam, Action action) => Remove(_fullyBlendedActions, cam, action);
    

    public void ClearEnter(ICinemachineCamera cam) => Clear(_enterActions, cam);
    public void ClearExit(ICinemachineCamera cam) => Clear(_exitActions, cam);
    public void ClearFullyBlended(ICinemachineCamera cam) => Clear(_fullyBlendedActions, cam);

    public void ClearAll(ICinemachineCamera cam)
    {
        ClearEnter(cam);
        ClearExit(cam);
        ClearFullyBlended(cam);
    }

    #endregion

    #region Internal Helpers

    private void Add(Dictionary<ICinemachineCamera, Action> map, ICinemachineCamera cam, Action action)
    {
        if (cam == null || action == null) return;

        if (!map.TryAdd(cam, action))
            map[cam] += action;
    }

    private void Remove(Dictionary<ICinemachineCamera, Action> map, ICinemachineCamera cam, Action action)
    {
        if (cam == null || action == null) return;

        if (!map.TryGetValue(cam, out var existing)) return;

        existing -= action;

        if (existing == null) map.Remove(cam);
        else map[cam] = existing;
    }

    private void Clear(Dictionary<ICinemachineCamera, Action> map, ICinemachineCamera cam)
    {
        if (cam == null ) return;
        map.Remove(cam);
    }

    private void Invoke(Dictionary<ICinemachineCamera, Action> map, ICinemachineCamera cam)
    {
        if (cam == null) return;
      

        if (!map.TryGetValue(cam, out var action)) return;
        

        action?.Invoke();

    }

    #endregion
}