using System;
using System.Collections.Generic;

public class EvidenceDataBase : Singleton<EvidenceDataBase>
{ 
    private readonly Dictionary<SerializableGuid,Evidence> _evidencesSaved = new Dictionary<SerializableGuid,Evidence>();
    
    public T GetOrCreate<T>(SerializableGuid guid, Func<T> creator) where T : Evidence
    {
        if (_evidencesSaved.TryGetValue(guid, out var evidence))
        {
            return (T)evidence;
        }
        var newEvidence = creator();

        _evidencesSaved.Add(guid, newEvidence);

        return newEvidence;
    }
    
    public bool TryGet(SerializableGuid guid, out Evidence evidence)
    {
        return _evidencesSaved.TryGetValue(guid, out evidence);
    }
}