using Cysharp.Threading.Tasks;
using PrimeTween;
using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;

public class AudioManager : PersistentSingleton<AudioManager>
{
    public SerializedDictionary<SFXType, AudioSelector> SFX = new();
    public SerializedDictionary<MusicState, AudioSource> Music = new();
    public float musicTransitionSpeed = 5f;

    protected override void Awake()
    {
        base.Awake();
        foreach (var item in Music) item.Value.volume = 0f;
        _ = ChangeMusicState(MusicState.Default, true);
    }

    public void SelectSFX(SFXType type, string id = "") 
        => SFX[type]?.PlaySelectedSource(id);
    public void RandomSFX(SFXType type, bool continuously = false, Func<bool> cancelIf = default)
        => SFX[type]?.PlayRandomSource(continuously, cancelIf);


    MusicState _currentMusicState = default;
    Sequence _currentSeq = new();
    public async UniTask ChangeMusicState(MusicState state, bool instant = false)
    {
        if (!instant)
        {
            while (_currentSeq.isAlive)
            {
                await UniTask.NextFrame();
            }
        }
        var oldState = _currentMusicState;
        Tween.StopAll();
        //Music[oldState].volume = _currentSeq.progress;
        //Music[state].volume = Mathf.Lerp(1, 0, _currentSeq.progress);
        var seq = Sequence.Create();

        // fade out de la musica actual
        _ = seq.Group(Tween.Custom(0, 1, (instant ? 0 : 10 / musicTransitionSpeed), (x) =>
        {
            Music[oldState].volume = Mathf.Lerp(1, 0, x);
            Music[state].volume = x;
        }, 
        Ease.InOutExpo));

        //// fade in de la musica siguiente
        //_ = seq.Group(Tween.Custom(
        //0, 
        //1,
        //(instant ? 0 : 10 / musicTransitionSpeed), 
        //(x) =>
        //{
        //    Music[state].volume = x;
        //},
        //Ease.InOutExpo));
        
        _currentMusicState = state;
        _currentSeq = seq;
        await seq;
    }
    
}

public enum SFXType
{
    None,
    Walking,
    Notebook,
    Menu,
    Ambient,
}

public enum MusicState
{
    Default,
    Dialogue,
    Flashback,
    Notebook,
    Solving,
    Menu,
    Climax,
}
