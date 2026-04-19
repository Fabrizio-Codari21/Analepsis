
public class Transition<T> : Transition where T : IPredicate
{
    public readonly T condition;

    public Transition(IState to, T condition) {
        To = to;
        this.condition = condition;
    }
    public override bool Evaluate()  => condition.Evaluate();
}






