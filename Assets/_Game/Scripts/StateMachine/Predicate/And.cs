using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class And : IPredicate {
    [SerializeField] List<IPredicate> rules = new List<IPredicate>();
    public bool Evaluate() => rules.All(r => r.Evaluate());
}