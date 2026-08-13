using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class EventBusUtil
{
    public static IReadOnlyList<Type> EventTypes { get; set; }
    public static IReadOnlyList<Type> EventBusTypes { get; set; }

#if UNITY_EDITOR
    public static PlayModeStateChange PlayModeState {get; set;}

    [InitializeOnLoadMethod]
    public static void InitializedEditor()
    {
        EditorApplication.playModeStateChanged -=  OnPlayModeStateChanged;
        EditorApplication.playModeStateChanged +=  OnPlayModeStateChanged;
    }

    static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        PlayModeState = state;
        if (PlayModeState == PlayModeStateChange.ExitingPlayMode)
        {
            ClearAllBuses();
        }
    }
#endif

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Initialize()
    {
        EventTypes = PredefinedAssemblyUtil.GetTypes(typeof(IEvent));

        EventBusTypes = InitializeAllBuses();

    }

    static List<Type> InitializeAllBuses()
    {
        List<Type> eventBusTypes = new List<Type>();

        var typedef = typeof(EventBus<>);

        foreach (var evType in EventTypes)
        {
            var busType = typedef.MakeGenericType(evType);
            eventBusTypes.Add(evType);
            Debug.Log($"Registered event bus type: {busType}");
        }
        
        
        return eventBusTypes;
    }


    public static void ClearAllBuses()
    {
        Debug.Log($"Clearing all event bus types.");

        foreach (var evType in EventTypes)
        {
            var clearMethod = evType.GetMethod("Clear", BindingFlags.Static | BindingFlags.NonPublic);
            clearMethod?.Invoke(null, null);
        }
    }
}