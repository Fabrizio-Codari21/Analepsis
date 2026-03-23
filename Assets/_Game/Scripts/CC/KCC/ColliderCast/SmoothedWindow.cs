using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SmoothedWindow<E>
{
    /// <summary>
    /// Gets the sample values.
    /// </summary>
    protected E[] Samples { get; private set; }

    /// <summary>
    /// Gets the current selected index.
    /// </summary>
    protected int CurrentIdx { get; private set; }

    /// <summary>
    /// Get values for debugging.
    /// </summary>
    internal IEnumerable<E> Values =>
        Enumerable.Range(0, Count).Select(index => Samples[(CurrentIdx + index) % Count]);

    /// <summary>
    /// Count of 
    /// </summary>
    /// <value></value>
    public int Count { get; protected set; }

    /// <summary>
    /// Create a smoothed window.
    /// </summary>
    /// <param name="size">Size of smoothing window.</param>
    public SmoothedWindow(int size)
    {
        Samples = new E[size];
    }

    /// <summary>
    /// Adds a sample to the smoothing window at the next
    /// available space. Will overwrite data if any data
    /// is there.
    /// </summary>
    /// <param name="value">Value to add.</param>
    /// <returns>Previous value removed.</returns>
    public virtual E AddSample(E value)
    {
        E previous = Samples[CurrentIdx];
        Samples[CurrentIdx] = value;
        Count = Mathf.Max(Count, ++CurrentIdx);
        CurrentIdx %= Samples.Length;
        return previous;
    }
}