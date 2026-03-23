using System;

/// <summary>
/// Represents a predicate that uses a Func delegate to evaluate a condition.
/// </summary>
public class FuncPredicate : IPredicate {
    readonly Func<bool> func;

    public FuncPredicate(Func<bool> func) {
        this.func = func;
    }

    public bool Evaluate() => func.Invoke();
}