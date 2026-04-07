using System;

public interface IFocus
{
    public event Action OnFocus;
    public event Action OnUnfocus;
    public void Focus();
    public void Unfocus();
}