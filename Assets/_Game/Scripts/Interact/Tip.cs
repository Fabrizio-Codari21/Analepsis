using System;
using UnityEngine;

[Serializable]
public class Tip
{ 
    public string tip;
    public TipOrder order;
    public Tip(string tip, TipOrder order)
    {
        this.tip = tip;
        this.order = order;
    }
}

public enum TipOrder
{
    Name,
    InteractionType,
    ActionCost,
    
}