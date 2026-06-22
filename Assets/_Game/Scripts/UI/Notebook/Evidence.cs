using System;

[Serializable]
public class Evidence
{
    public SerializableGuid guid;
    public string displayName;
    public Whodunnit whodunnits; // puede ser list y puede ser para varios
    public IClue representerClue;
    protected Evidence(string displayName,SerializableGuid guid,Whodunnit proofs,IClue representerClue)
    {
        this.displayName = displayName;
        this.guid = guid;
        this.representerClue = representerClue;
        whodunnits = proofs;
    }
    
    public virtual string GetInfo()
    {
        return string.Empty;
    }
}

public class ItemEvidence : Evidence
{
    public readonly Item item;
    public ItemEvidence(string displayName, SerializableGuid guid, Whodunnit proofs, Item item) : base(displayName, guid, proofs, item)
    {
        
    }
}