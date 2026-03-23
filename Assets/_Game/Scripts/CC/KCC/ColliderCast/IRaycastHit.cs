using UnityEngine;

public interface IRaycastHit
{
    /// <summary>
    /// The Collider that was hit.
    /// </summary>
    /// <value></value>
    Collider collider { get; }

    /// <summary>
    /// The impact point in world space where the ray hit the collider.
    /// </summary>
    /// <value></value>
    Vector3 point { get; set; }

    /// <summary>
    /// The normal of the surface the ray hit.
    /// </summary>
    /// <value></value>
    Vector3 normal { get; set; }

    /// <summary>
    /// The barycentric coordinate of the triangle we hit.
    /// </summary>
    /// <value></value>
    Vector3 barycentricCoordinate { get; set; }

    /// <summary>
    /// The distance from the ray's origin to the impact point.
    /// </summary>
    /// <value></value>
    float distance { get; set; }

    /// <summary>
    /// The index of the triangle that was hit.
    /// </summary>
    /// <value></value>
    int triangleIndex { get; }

    /// <summary>
    /// The uv texture coordinate at the collision location.
    /// </summary>
    /// <value></value>
    Vector2 textureCoord { get; }

    /// <summary>
    /// The secondary uv texture coordinate at the impact point.
    /// </summary>
    /// <value></value>
    Vector2 textureCoord2 { get; }

    /// <summary>
    /// The Transform of the rigidbody or collider that was hit.
    /// </summary>
    /// <value></value>
    Transform transform { get; }

    /// <summary>
    /// The Rigidbody of the collider that was hit. If the collider is not attached to
    /// a rigidbody then it is null.
    /// </summary>
    /// <value></value>
    Rigidbody rigidbody { get; }
}