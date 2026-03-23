// Copyright (C) 2023 Nicholas Maltbie
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
// associated documentation files (the "Software"), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish, distribute,
// sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
// BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
// CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
// ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
using UnityEngine;

/// <summary>
/// Data structure describing a bounce of the KCC when moving throughout a scene.
/// </summary>
public class KCCBounce
{
    /// <summary>
    /// Initial position before moving.
    /// </summary>
    public Vector3 initialPosition;

    /// <summary>
    /// Final position once finishing this bounce.
    /// </summary>
    public Vector3 finalPosition;

    /// <summary>
    /// Initial momentum when starting the move.
    /// </summary>
    public Vector3 initialMomentum;

    /// <summary>
    /// Remaining momentum after this bounce.
    /// </summary>
    public Vector3 remainingMomentum;

    /// <summary>
    /// Action that ocurred during this bounce.
    /// </summary>
    public ControllerUtil.MovementAction action;

    /// <summary>
    /// Collision data associated with the bounce.
    /// </summary>
    public IRaycastHit hit;

    /// <summary>
    /// Get the movement of a vector (from initial position to final position).
    /// </summary>
    public Vector3 Movement => finalPosition - initialPosition;
    
}