using UnityEngine;

/// <summary>
/// Smoothed window for a Vector3 value.
/// </summary>
public class SmoothedVector : SmoothedWindow<Vector3>
{
    /// <summary>
    /// Running sum of the values stored in samples.
    /// </summary>
    protected Vector3 sum = Vector3.zero;

    /// <summary>
    /// Create a smoothed vector with a given number of samples..
    /// </summary>
    /// <param name="size">Size of smoothing window.</param>
    public SmoothedVector(int size) : base(size) { }

    /// <summary>
    /// Returns the average of all samples in the window.
    /// </summary>
    public Vector3 Average()
    {
        if (Count == 0)
        {
            return Vector3.zero;
        }

        return sum / Count;
    }

    /// <inheritdoc/>
    public override Vector3 AddSample(Vector3 value)
    {
        Vector3 previous = base.AddSample(value);
        sum += value;
        sum -= previous;
        return previous;
    }
}