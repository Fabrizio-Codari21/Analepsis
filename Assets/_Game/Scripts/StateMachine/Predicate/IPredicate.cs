public interface IPredicate {
    bool Evaluate();
}

public interface IPredicate<in TContext>
{
    bool Evaluate(TContext context);
}