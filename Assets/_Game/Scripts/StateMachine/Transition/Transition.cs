public abstract class Transition {
    public IState To { get; protected set; }
    public abstract bool Evaluate();
}