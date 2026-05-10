using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class CustomExtensions
{

    #region TIME UTILITIES

    // Devuelve una lista de WaitForSeconds que deberia permitir realizar acciones a lo largo del tiempo en una corrutina.
    //
    // Su uso seria algo asi como:
    // foreach (var step in BuildTimeSpan(2, 0.2f))
    // {
    //   // lo que sea que quieras hacer
    //   yield return step;
    // }
    public static List<WaitForSeconds> BuildTimeSpan(float seconds, float stepLength = 0)
    {
        var remaining = seconds;

        List<WaitForSeconds> stepList = new();

        int watchdog = 1000000;
        while (remaining > 0)
        {
            if (watchdog <= 0) break;

            if (stepLength != 0) 
            { 
                stepList.Add(new WaitForSeconds(remaining > stepLength ? stepLength : remaining)); 
                remaining -= stepLength; 
            }
            else 
            { 
                stepList.Add(null); 
                remaining -= Time.fixedUnscaledDeltaTime;
            }

            watchdog--;
        }

        return stepList;
    }

    
    // Se llama cuando queremos realizar una accion repetida cada X tiempo.
    public static void SteppedExecution(this MonoBehaviour starter, float duration, float stepLength, Action ExecuteOnEachStep, Func<bool> cancelCondition = default) 
        => starter.StartCoroutine(SteppedExecution(duration, stepLength, ExecuteOnEachStep));

    // Ejecuta una accion hasta que pase X tiempo.
    public static void ExecuteUntil(this MonoBehaviour starter, float timeLimit, Action Exec, Func<bool> cancelCondition = default)
        => starter.StartCoroutine(SteppedExecution(timeLimit * 4, 0, Exec, cancelCondition));

    public static IEnumerator SteppedExecution(float duration, float stepLength, Action ExecuteOnEachStep, Func<bool> cancelCondition = default)
    {
        if (cancelCondition == default) cancelCondition = () => false;

        var stepList = BuildTimeSpan(duration, stepLength);

        foreach (var step in stepList)
        {
            if (cancelCondition()) break;

            ExecuteOnEachStep();
            yield return step;
        }
    }

    // Se llama cuando queremos realizar una serie de acciones separadas por un intervalo de X tiempo.
    public static void MultiSteppedExecution(this MonoBehaviour starter, float duration, float stepLength, Action[] ListOfSteppedExecutions, Func<bool> cancelCondition = default)
        => starter.StartCoroutine(MultiSteppedExecution(duration, stepLength, ListOfSteppedExecutions));
   
    public static IEnumerator MultiSteppedExecution(float duration, float stepLength, Action[] ListOfSteppedExecutions, Func<bool> cancelCondition = default)
    {
        if (cancelCondition == default) cancelCondition = () => false;

        var stepList = BuildTimeSpan(duration, stepLength);

        foreach (var step in stepList)
        {
            if (cancelCondition()) break;

            ListOfSteppedExecutions[stepList.IndexOf(step)]();
            yield return step;
        }
    }
 
    // Ejecuta una accion despues de X tiempo.
    public static void WaitAndThen(this MonoBehaviour starter, float timeToWait, Action ExecuteAfterwards, Func<bool> cancelCondition = default)
    {
        starter.StartCoroutine(ExecuteAfter(starter, timeToWait, ExecuteAfterwards, false, cancelCondition));

    }

    // Ejecuta una accion cada X segundos (puede cancelarse si se le da una condicion).
    public static void ExecuteLooping(this MonoBehaviour starter, float timeUntilLoop, Action Exec, Func<bool> cancelCondition = default)
    {
        Exec();
        if(cancelCondition == default) starter.StartCoroutine(ExecuteAfter(starter, timeUntilLoop, Exec, true));
        else starter.StartCoroutine(ExecuteAfter(starter, timeUntilLoop, Exec, true, cancelCondition));
    }

    public static IEnumerator ExecuteAfter(MonoBehaviour starter, float timeToWait, Action ExecuteAfterwards, bool loop = false, Func<bool> cancelCondition = default)
    {
        if (cancelCondition == default) cancelCondition = () => false;
        
        yield return new WaitForSeconds(timeToWait);

        if (!cancelCondition())
        {
            if (loop) ExecuteLooping(starter, timeToWait, ExecuteAfterwards, cancelCondition);
            else ExecuteAfterwards();
        }
    }

    // Ejecuta hasta que una condicion se cumpla.
    public static IEnumerator ExecuteUntilTrue(this MonoBehaviour starter, Func<bool> condition, Action Exec)
    {
        IEnumerator exec = ExecuteByCondition(starter, condition, Exec, false);
        starter.StartCoroutine(exec);
        return exec;
    }
        

    // Ejecuta despues de que una condicion se haya cumplido.
    public static void ExecuteAfterTrue(this MonoBehaviour starter, Func<bool> condition, Action Exec, Func<bool> cancelCondition = default)
        => starter.StartCoroutine(ExecuteByCondition(starter, condition, Exec, true,999,false,cancelCondition));

    // Si la condicion se cumple dentro del tiempo estipulado, se realiza la accion.
    public static void QuickTimeEvent(this MonoBehaviour starter, float timeLimit, Func<bool> doneWithinTime, Action Exec)
        => starter.StartCoroutine(ExecuteByCondition(starter, doneWithinTime, Exec, true, timeLimit));

    // Ejecuta cada vez que la condicion se cumpla
    public static void ExecuteWhenever(this MonoBehaviour starter, Func<bool> condition, Action Exec, Func<bool> cancelCondition = default)
    {
        starter.StartCoroutine(ExecuteByCondition(starter,condition,Exec,true,999,true,cancelCondition));
    }

    public static IEnumerator ExecuteByCondition(MonoBehaviour starter, Func<bool> condition, Action Exec, bool requireCondition = true, float span = 999, bool repeat = false, Func<bool> cancelCondition = default)
    {
        if (cancelCondition == default) cancelCondition = () => false;
        var timeSpan = BuildTimeSpan(span);

        if (requireCondition) 
        foreach (var frame in timeSpan)
        {
            if (!condition())
            {
                if (cancelCondition()) break;
                else yield return frame;
            }
            else if (!cancelCondition())
            {
                Exec();

                if (repeat)
                { 
                    starter.StartCoroutine(ExecuteByCondition(starter, 
                                                            condition, 
                                                            Exec, 
                                                            requireCondition, 
                                                            span, 
                                                            repeat, 
                                                            cancelCondition));
                }

                break;
            }

        }
        else 
        foreach (var frame in timeSpan)
        {
            if (condition()) break;
            else
            {
                Exec();
                yield return frame;

            }
        }
    }

    public static bool ExecuteIfCancelled(this MonoBehaviour x, bool cancelCondition, Action Exec)
    {
        if (cancelCondition) Exec(); return cancelCondition;
    }

    #endregion

    #region LEVEL UTILITIES

    public static async UniTask<Func<bool>> AsyncLoader(this MonoBehaviour x, string sceneName)
    {
        UnityEngine.AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);

        IEnumerator load = x.ExecuteUntilTrue(() => op.isDone, () =>
        {
            Debug.Log($"Loading {sceneName}: {Mathf.Clamp01(op.progress) * 100}%");
        });

        await op;
        return () => op.isDone;
    }

    // Simplifica la creacion de instancias estaticas de MonoBehaviours.
    public static MonoBehaviour ToSingleton(this MonoBehaviour x, MonoBehaviour instance)
    {
        if (instance == default) return x; 
        else 
        {
            if(x.gameObject) GameObject.Destroy(x.gameObject); 
            else x.enabled = false;        
        }

        return default;
    }

    // Devuelve una referencia al mismo transform rotado para mirar a un target.
    public static Transform WhenLookingAt(this Transform x, Transform target = default, Vector3 targetPos = default)
    {
        Transform transform = x.transform;
        if(target) transform.LookAt(target.position);
        else transform.LookAt(targetPos);
        return transform;
    }

    #endregion

    #region STRING UTILITIES

    // Convierte una lista de strings en un string largo, segmentado o no.
    public static string AsString(this ICollection<string> strings, bool segmented = false, int segmentSpace = 1)
    {
        var str = string.Empty;
        foreach (string s in strings) str += (segmented ? "\n".Times(segmentSpace) : "") + s;
        return str;
    }

    // Devuelve tu lista de strings segmentada si se cumple X condicion.
    public static List<string> Segmented(this ICollection<string> list, Func<bool> segmentIf = default, int segmentSpace = 1)
    {
        if (segmentIf == default) segmentIf = () => true;
        List<string> result = new List<string>();
        foreach (string s in list)
        {
            result.Add((segmentIf() ? "\n".Times(segmentSpace) : "") + s);
        }
        return result;
    }

    // Devuelve un texto duplicado X cantidad de veces.
    public static string Times(this string s, int timesToCopy = 1)
    {
        string str = string.Empty;
        for (int i = 0; i < timesToCopy; i++) str += s;
        return str;
    }

    // Agrega el posesivo en ingles ajustado a si la palabra termina en "s" o no.
    public static string Possessive(this string s)
    {
        return s + (s.Last() == 's' ? "'" : "'s"); 
    }

    // Agrega el plural en ingles cuando X se cumple, basado en ciertas reglas gramaticales.
    public static string Plural(this string s, Func<bool> pluralize = default, string irregularPlural = "")
    {
        if (pluralize == default) pluralize = () => true;
        if (irregularPlural == "")
        {
            if (pluralize() == false) return s;
            else switch (s.Last())
            {
                case 'y': return s[s.Length - 2] == 'e' 
                            ? s.Remove(s.Length - 2, 2) + "ies" 
                            : s.Remove(s.Length - 1) + "ies";
                case 's': return s + "es";
                case 'z': return s + "es";
                case 'i': return s + "es";
                default: return s + "s";
            }
        }
        else return pluralize() ? irregularPlural : s;
    }

    #endregion

    #region UI UTILITIES

    public static void ModifyColor(this Color color, float r = 0f, float g = 0f, float b = 0f, float a = 0f)
    {      
        color = color + new Color(r, g, b, a);
    }

    public static void SetAlphaToZero(this Color color)
    {
        color = color - new Color(0f, 0f, 0f, color.a);
    }

    #endregion

}

#region STRUCTS ET AL.

// Struct que contiene cualquier tipo de variable, ideal para uso generico.
// (se puede hacer con mas T si hace falta)
[System.Serializable]
public struct AnyVariable<T>
{
    // ir agregando mas tipos a medida que se necesiten
    bool _bool; public bool Bool { get => _bool; set => _bool = value; }
    int _int; public int Int { get => _int; set => _int = value; }
    float _float; public float Float { get => _float; set => _float = value; }
    char _char; public char Char { get => _char; set => _char = value; }
    string _string; public string String { get => _string; set => _string = value; }
    T _t; public T CustomType { get => _t; set => _t = value; }

    // Permite hacer un enum usando el tipo asignado como id.
    Dictionary<string,List<T>> _fakeEnums; 
    public int FakeEnum(Tuple<string,T> id, Tuple<string,List<T>> newEnum = default) 
    { 
        if (newEnum != default) _fakeEnums.Add(newEnum.Item1, newEnum.Item2);
        if (id != default)
        {
            if (_fakeEnums.ContainsKey(id.Item1) && _fakeEnums[id.Item1].Contains(id.Item2))
                return _fakeEnums[id.Item1].IndexOf(id.Item2);
            else
            {
                Debug.LogWarning(!_fakeEnums.ContainsKey(id.Item1)
                    ? $"There is no FakeEnum with the name {id.Item1}"
                    : $"The FakeEnum {id.Item1} does not contain an index named {id.Item2}");
                return default;
            }
        }
        else
        {
            Debug.LogWarning("FakeEnum ID not provided.");
            return default;
        }
    }

    public List<object> AllVariables() => new() { Bool, Int, Float, Char, String, CustomType };
    public List<object> UsedVariables() => AllVariables().Where(x => !x.Equals(default) && !x.Equals(null)).ToList();

}

// Lo mismo pero sin usar tipos genericos.
[System.Serializable]
public struct AnyVariable
{
    // ir agregando mas tipos a medida que se necesiten
    bool _bool; public bool Bool { get => _bool; set => _bool = value; }
    int _int; public int Int { get => _int; set => _int = value; }
    float _float; public float Float { get => _float; set => _float = value; }
    char _char; public char Char { get => _char; set => _char = value; }
    string _string; public string String { get => _string; set => _string = value; }

    // En vez de usar tipos genericos, directamente usamos un string como id.
    Dictionary<string, List<string>> _fakeEnums;
    public int FakeEnum(Tuple<string, string> id, Tuple<string, List<string>> newEnum = default)
    {
        if (newEnum != default) _fakeEnums.Add(newEnum.Item1, newEnum.Item2);
        if (id != default)
        {
            if (_fakeEnums.ContainsKey(id.Item1) && _fakeEnums[id.Item1].Contains(id.Item2))
                return _fakeEnums[id.Item1].IndexOf(id.Item2);
            else
            {
                Debug.LogWarning(!_fakeEnums.ContainsKey(id.Item1)
                    ? $"There is no FakeEnum with the name {id.Item1}"
                    : $"The FakeEnum {id.Item1} does not contain an index named {id.Item2}");
                return default;
            }
        }
        else
        {
            Debug.LogWarning("FakeEnum ID not provided.");
            return default;
        }
    }

    public List<object> AllVariables() => new() { Bool, Int, Float, Char, String };
    public List<object> UsedVariables() => AllVariables().Where(x => !x.Equals(default) && !x.Equals(null)).ToList();
}

/// <summary>
/// Unity no me deja serializar listas dentro de diccionarios, asi que hice esto.
/// </summary>
/// <typeparam name="T"></typeparam>
[System.Serializable]
public class SerializedList<T> : List<T> {}

#endregion




