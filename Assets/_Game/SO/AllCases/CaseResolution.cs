using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[Serializable]
[CreateAssetMenu(fileName = "New Case", menuName = "Game/CaseResolution/NewCase")]
public class CaseResolution : ScriptableObject
{
    /// <summary>
    /// La lista tiene structs que contienen un caso (con clues asignadas a roles) 
    /// mas su nombre y descripción (un string).
    /// </summary>
    [InfoBox("Make a new case by creating <b>ways to solve it</b>: " +
        "\n\n<b>1)</b> Give your theory a <b>name</b> to identify it. " +
        "\n<b>2)</b> Add a <b>description</b> of what the case would be." +
        "\n<b>3)</b> Select which <b>clues</b> prove each aspect of the case (never use NoProof). " +
        "\n\n(IMPORTANT: make sure the first answer on the list is your '<b>true</b>' answer.)", 
        Icon = SdfIconType.Newspaper), Space(15)]
    public List<CaseAnswer> AllValidAnswers = new();
}

[System.Serializable]
public struct CaseAnswer
{
    public string Name;
    [TextArea(0,30)] public string Description;
    [ShowInInspector, DictionaryDrawerSettings(KeyLabel = "Role", ValueLabel = "Clue")] 
    public SerializedDictionary<Whodunnit, SerializableList<Clue>> Answer;
}
/// <summary>
/// Unity no me deja serializar listas dentro de diccionarios, asi que hice esto.
/// </summary>
/// <typeparam name="T"></typeparam>
[System.Serializable]
public struct SerializableList<T> {  public List<T> Items; }
