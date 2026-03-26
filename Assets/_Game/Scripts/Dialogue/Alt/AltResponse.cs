

// Lo que decimos en respuesta a un NPC y que dialogo le sigue a esa respuesta.
[System.Serializable]
public class AltResponse
{
    public string responseText;
    public AltNode nextNode; // referencia al identificador del proximo nodo;
}
