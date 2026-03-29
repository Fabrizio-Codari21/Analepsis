using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

    public static Func<bool> AsyncLoader(this MonoBehaviour x, string sceneName)
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);

        IEnumerator load = x.ExecuteUntilTrue(() => op.isDone, () =>
        {
            Debug.Log($"Loading {sceneName}: {Mathf.Clamp01(op.progress) * 100}%");
        });

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

    #endregion

}





