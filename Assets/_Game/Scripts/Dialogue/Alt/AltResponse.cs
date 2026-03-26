

// Lo que decimos en respuesta a un NPC y que dialogo le sigue a esa respuesta.

using UnityEngine;

[System.Serializable]
public class AltResponse
{
    public string responseText;
    [SerializeReference]public AltNode nextNode; // referencia al identificador del proximo nodo;
}
