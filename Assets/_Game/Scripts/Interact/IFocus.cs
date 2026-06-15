using System;

public interface IFocus
{
    public event Action OnFocus;
    public event Action OnUnfocus;
    public event Action<float> OnUpdateDistance;
    public void Focus();
    public void Unfocus();
    public void UpdateDistance(float dist);
}