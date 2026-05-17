using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AudioMultiSource : MonoBehaviour
{
    [ReadOnly] public List<AudioSource> sources;
    public List<AudioSource> GetSources()
    {
        var s = GetComponents<AudioSource>();
        sources = s.ToList();
        return sources;
    }
}
