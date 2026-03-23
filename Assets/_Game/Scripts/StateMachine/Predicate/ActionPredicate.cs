using System;

/// <summary>
/// Represents a predicate that encapsulates an action and evaluates to true once the action has been invoked.
/// </summary>
public class ActionPredicate : IPredicate {
    public bool flag;

    public ActionPredicate(ref Action eventReaction) => eventReaction += () => { flag = true; };

    public bool Evaluate() {
        bool result = flag;
        flag = false;
        return result;
    }
}