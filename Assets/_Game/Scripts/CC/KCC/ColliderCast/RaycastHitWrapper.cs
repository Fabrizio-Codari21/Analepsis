using UnityEngine;

public class RaycastHitWrapper : IRaycastHit
{
    /// <summary>
    /// Wrapper for raycast hit object.
    /// </summary>
    private RaycastHit hit;

    /// <summary>
    /// Initialize a raycast hit wrapper for a given raycast hit event.
    /// </summary>
    /// <param name="hit">Raycast hit event.</param>
    public RaycastHitWrapper(RaycastHit hit)
    {
        this.hit = hit;
    }

    /// <inheritdoc/>
    public Collider collider => hit.collider;

    /// <inheritdoc/>
    public Vector3 point { get => hit.point; set => hit.point = value; }

    /// <inheritdoc/>
    public Vector3 normal { get => hit.normal; set => hit.normal = value; }

    /// <inheritdoc/>
    public Vector3 barycentricCoordinate { get => hit.barycentricCoordinate; set => hit.barycentricCoordinate = value; }

    /// <inheritdoc/>
    public float distance { get => hit.distance; set => hit.distance = value; }

    /// <inheritdoc/>
    public int triangleIndex => hit.triangleIndex;

    /// <inheritdoc/>
    public Vector2 textureCoord => hit.textureCoord;

    /// <inheritdoc/>
    public Vector2 textureCoord2 => hit.textureCoord2;

    /// <inheritdoc/>
    public Transform transform => hit.transform;

    /// <inheritdoc/>
    public Rigidbody rigidbody => hit.rigidbody;
}