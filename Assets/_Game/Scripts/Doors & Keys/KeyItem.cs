using UnityEngine;
using System;

// Este capaz se usa para cada key o se puede hacer que ciertos tipos de key
// lo tengan de referencia (va a ser un SO en ese caso).
[Serializable]
public class KeyItem : ITakeable
{
    public bool isKey = false;
    public void Release()
    {
        
    }

    public void TryTake(Transform takeRoot)
    {
        
    }

}
