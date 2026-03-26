using System.Collections.Generic;

// Lo que pueden decir los NPC y como podemos responder a eso.
[System.Serializable]
public class AltNode
{
    public string dialogueText;
    public List<AltResponse> responses;

    internal bool IsLastNode()
    {
        return responses.Count <= 0;
    }
}
