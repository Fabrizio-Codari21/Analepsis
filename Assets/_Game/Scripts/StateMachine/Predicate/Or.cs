using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Or : IPredicate {
    [SerializeField] List<IPredicate> rules = new List<IPredicate>();
    public bool Evaluate() => rules.Any(r => r.Evaluate());
}