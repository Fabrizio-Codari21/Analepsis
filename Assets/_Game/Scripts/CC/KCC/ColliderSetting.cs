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
using System;
using UnityEngine;

[Serializable]
public class ColliderSetting
{
    [Header("Settings")] public float capsuleHeight = 2f;
    public float capsuleThickness = 1f;
    [Range(0f,1f)]public float stepHeightRatio = 0.1f;
    public Vector3 colliderOffset = Vector3.zero;

    public void SetupCollider(CapsuleCollider collider)
    {
        collider.height = capsuleHeight * (1f - stepHeightRatio);
        collider.radius = capsuleThickness / 2f;
        collider.center = colliderOffset * capsuleHeight + new Vector3(0f, stepHeightRatio * capsuleHeight / 2f, 0f);

        if (collider.height / 2f < collider.radius)
        {
            collider.radius = collider.height / 2f;
        }
    }
}