using UnityEngine;

public interface IRaycastHelper
{
    /// <summary>
    /// Do a raycast in a given direction ignoring this object.
    /// </summary>
    /// <param name="source">Source point to check from.</param>
    /// <param name="direction">Direction to search for step.</param>
    /// <param name="distance">Distance to search for step ahead of player.</param>
    /// <param name="stepHit">Information about step hit ahead.</param>
    /// <param name="layerMask">Layer mask for checking which objects to collide with.</param>
    /// <param name="queryTriggerInteraction">Configuration for QueryTriggerInteraction when solving for collisions.</param>
    /// <returns>Is something ahead hit.</returns>
    bool DoRaycastInDirection(Vector3 source, Vector3 direction, float distance, out IRaycastHit stepHit, int layerMask = RaycastHelperConstants.DefaultLayerMask, QueryTriggerInteraction queryTriggerInteraction = RaycastHelperConstants.DefaultQueryTriggerInteraction);
}