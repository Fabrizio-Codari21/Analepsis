using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AudioMultiSource : MonoBehaviour
{
    [Range(-3f, 3f)] public float generalPitchOverride = 1f;
    [ReadOnly] public List<AudioSource> sources;
    public List<AudioSource> GetSources()
    {
        var s = GetComponents<AudioSource>();
        sources = s.Select(x =>
        {
            x.pitch = generalPitchOverride;
            return x;
        })
        .ToList();
        return sources;
    }
}
