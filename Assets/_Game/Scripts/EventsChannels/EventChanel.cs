using UnityEngine;
using System;

[CreateAssetMenu(fileName = "SO_Event", menuName = "Game/EventChanel/Void")]
public class EventChanel : ScriptableObject
{
    [Tooltip("Description to organize")]
    [SerializeField,TextArea] private string m_description;
    public event Action Event;
    public void Invoke() => Event?.Invoke();
}