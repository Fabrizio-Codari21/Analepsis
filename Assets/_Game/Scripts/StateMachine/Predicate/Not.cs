using UnityEngine;

public class Not : IPredicate {
    [SerializeField] IPredicate rule;
    public bool Evaluate() => !rule.Evaluate();
}