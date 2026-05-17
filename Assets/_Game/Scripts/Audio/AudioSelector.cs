using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class AudioSelector : MonoBehaviour
{
    // El nombre esta mas que nada por si te queres acordar de que hace cada uno, podes no asignarlo.
    public string Name;
    public SerializedDictionary<string, AudioSource> selectableSources = new();
    public AudioMultiSource randomSources;
    List<AudioSource> _randomSources = new();

    private void Start()
    {
        if(randomSources) _randomSources = randomSources.GetSources();
    }

    public void PlaySelectedSource(string id = "")
    {
        if(id == "")
        {
            selectableSources.ToList()[UnityEngine.Random.Range(0, selectableSources.Count - 1)].Value.Play();
        }
        else
        {
            selectableSources[id].Play();
        }
    }

    AudioSource _currentSource;
    public void PlayRandomSource(bool continuously = false, Func<bool> cancelIf = default)
    {
        if (cancelIf()) return;

        this.ExecuteAfterTrue(() => !(_currentSource && _currentSource.isPlaying), () =>
        {
            var sound = _randomSources[UnityEngine.Random.Range(0, _randomSources.Count - 1)];
            sound.Play();
            _currentSource = sound;
            if (continuously && cancelIf != default)
            {
                this.ExecuteAfterTrue(() => !sound.isPlaying, () =>
                {
                    PlayRandomSource(continuously, cancelIf);
                },
                cancelCondition: cancelIf);
            }
        },
        cancelCondition: cancelIf);

    }
}
