public class NpcEvidence : Evidence
{
    public readonly NpcIdentity NpcSource;

    public NpcEvidence(string displayName, SerializableGuid guid, Whodunnit proofs, NpcIdentity representerClue) : base(displayName, guid, proofs, representerClue)
    {
        NpcSource = representerClue;
    }
    
    public override string GetInfo()
    {
        return NpcSource != null ? NpcSource.characterInfo : string.Empty;
    }
}