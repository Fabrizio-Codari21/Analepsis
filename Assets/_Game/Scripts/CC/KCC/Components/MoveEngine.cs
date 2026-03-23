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
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class MoveEngine : MonoBehaviour,IKCCConfig
{
    public const int DefaultMaxBounces = 5;
    
    public float stepHeight = 0.35f;
    public const float DefaultStepUpDepth = 0.5f;
    public const float DefaultAnglePower = 2.0f;
    public const float DefaultMaxPushSpeed = 100.0f;
    [Tooltip("Skin width for player collisions.")]
    public float skinWidth = 0.01f;
    public LayerMask layerMask = RaycastHelperConstants.DefaultLayerMask;
    #region IKCCConfig

    
         public virtual int MaxBounces => DefaultMaxBounces;

        /// <inheritdoc/>
        public virtual float VerticalSnapUp => stepHeight;

        /// <inheritdoc/>
        public virtual float StepUpDepth => DefaultStepUpDepth;

        /// <inheritdoc/>
        public virtual float AnglePower => DefaultAnglePower;

        /// <inheritdoc/>
        public virtual float SkinWidth => skinWidth;
        
        public bool CanSnapUp => GroundedState.OnGround;
        
        public KCCGroundedState GroundedState { get; protected set; }
    
        public virtual Vector3 Up => Vector3.up;
        public virtual IColliderCast ColliderCast => _colliderCast  ?? GetComponent<IColliderCast>();
        /// <inheritdoc/>
        public virtual LayerMask LayerMask => layerMask;
    #endregion
    
    
    public float MaxPushSpeed => DefaultMaxPushSpeed;
    private Vector3 _previousPosition = Vector3.zero;
  
    public SmoothedVector worldVelocity = new SmoothedVector(10);
    public RelativeParentConfig RelativeParent { get; protected set; }= new RelativeParentConfig();
    public IColliderCast _colliderCast;
    
    public const float DefaultGroundCheckDistance = 0.25f;
    public virtual float GroundCheckDistance => DefaultGroundCheckDistance;
    public const float DefaultGroundedDistance = 0.05f;
    public virtual float GroundedDistance => DefaultGroundedDistance;
    public virtual float MaxWalkAngle => maxWalkAngle;
    
    public virtual float SnapDown => stepHeight * 2f;
    public virtual float MaxSnapDownSpeed => 15.0f;
    public float maxWalkAngle = 60.0f;
    

    private void Awake()
    {
        _previousPosition = transform.position;
    }

    private void Update()
    {
        RelativeParent.FollowGround(transform);
    }
    
    public virtual IEnumerable<KCCBounce> GetMovement(Vector3 movement)
    {
        foreach (KCCBounce bounce in ControllerUtil.GetBounces(transform.position, movement, transform.rotation, this))
        {
            if (bounce.action == ControllerUtil.MovementAction.Stop)
            {
                transform.position = bounce.finalPosition;
            }

            yield return bounce;
        }
    }
    public virtual KCCBounce[] MovePlayer(float deltaTime,params Vector3[] moves )
    {
        RelativeParent.FollowGround(transform);
        Vector3 previousVelocity = (transform.position - _previousPosition) / deltaTime;
        worldVelocity.AddSample(previousVelocity);
        
        Vector3 start = transform.position;

        if (ColliderCast == null)
        {
            Debug.LogWarning("No ColliderCast found");
            return null;
        }
        
        transform.position += ColliderCast.PushOutOverlapping(transform.position, transform.rotation, MaxPushSpeed * Time.fixedDeltaTime, layerMask, QueryTriggerInteraction.Ignore, SkinWidth / 2);
        
        KCCBounce[] bounces = moves.SelectMany(GetMovement).ToArray();
        bool snappedUp = bounces.Any(bounce => bounce.action == ControllerUtil.MovementAction.SnapUp);
        bool snappedDown = ShouldSnapDown(snappedUp, moves);
        if (snappedDown) SnapPlayerDown();
        CheckGrounded(snappedUp, snappedDown);

        Vector3 delta = transform.position - start;
        transform.position += RelativeParent.UpdateMovingGround(start, transform.rotation, GroundedState, delta, Time.fixedDeltaTime);
        _previousPosition = transform.position;
        return bounces;
    }
    protected virtual bool ShouldSnapDown(bool snappedUp, IEnumerable<Vector3> moves)
    {
        return !snappedUp &&
               GroundedState.StandingOnGround &&
               !GroundedState.Sliding &&
               !moves.Any(MovingUp);
    }
    public bool MovingUp(Vector3 move)
    {
        return Vector3.Dot(move, Up) > 0;
    }
    
  
    protected virtual void SnapPlayerDown()
    {
        Vector3 delta = ControllerUtil.GetSnapDelta(
            transform.position,
            transform.rotation,
            -Up,
            SnapDown,
            ColliderCast,
            LayerMask,
            skinWidth);
        transform.position += Vector3.ClampMagnitude(delta, MaxSnapDownSpeed * Time.fixedDeltaTime);
    }
    protected virtual KCCGroundedState CheckGrounded(bool snappedUp, bool snappedDown)
        {
            Vector3 groundCheckPos = transform.position;

            // If snapped up, use the snapped position to check grounded
            if (snappedUp || snappedDown)
            {
                Vector3 snapDelta = ControllerUtil.GetSnapDelta(
                    transform.position,
                    transform.rotation,
                    -Up,
                    SnapDown,
                    ColliderCast,
                    LayerMask,
                    SkinWidth);
                groundCheckPos += snapDelta;
            }

            bool didHit = ColliderCast.CastSelf(
                groundCheckPos,
                transform.rotation,
                -Up,
                GroundCheckDistance,
                out IRaycastHit hit,
                layerMask,
                skinWidth: SkinWidth);

            Vector3 normal = hit.normal;
            if (snappedUp)
            {
                normal = GroundedState.SurfaceNormal;
            }
            else if (!snappedUp && snappedDown)
            {
                // Check if we're walking down stairs
                bool overrideNormal = ColliderCast.DoRaycastInDirection(
                    transform.position + skinWidth * Up,
                    -Up,
                    GroundCheckDistance + skinWidth,
                    out IRaycastHit stepHit,
                    layerMask);
                if (overrideNormal)
                {
                    normal = stepHit.normal;
                }
            }

            GroundedState = new KCCGroundedState(
                distanceToGround: hit.distance,
                onGround: didHit,
                angle: Vector3.Angle(normal, Up),
                surfaceNormal: normal,
                groundHitPosition: hit.point,
                floor: hit.collider?.gameObject,
                groundedDistance: GroundedDistance,
                maxWalkAngle: MaxWalkAngle);

            return GroundedState;
        }
    
    public Vector3 GetProjectedMovement(Vector3 movement)
    {
        if (GroundedState is not { StandingOnGround: true, Sliding: false })
            return movement;

        Vector3 projected = Vector3.ProjectOnPlane(
            movement,
            GroundedState.SurfaceNormal
        );

      
        if (projected.magnitude + ControllerUtil.Epsilon >= movement.magnitude)
        {
            movement = projected;
        }

        return movement;
    }
    
    
    
    
    public const float DefaultMaxLaunchVelocity = 2.0f;
    public virtual float MaxDefaultLaunchVelocity => DefaultMaxLaunchVelocity;  
    
    public virtual Vector3 GetGroundVelocity()
    {
        Vector3 groundVelocity = Vector3.zero;
        IMovingGround movingGround = GroundedState.Floor?.GetComponent<IMovingGround>();
        Rigidbody rb = GroundedState.Floor?.GetComponent<Rigidbody>();
        if (movingGround != null)
        {
            if (movingGround.AvoidTransferMomentum())
            {
                return Vector3.zero;
            }

            // Weight movement of ground by ground movement weight
            groundVelocity = movingGround.GetVelocityAtPoint(GroundedState.GroundHitPosition);
            float velocityWeight =
                movingGround.GetMovementWeight(GroundedState.GroundHitPosition, groundVelocity);
            float transferWeight =
                movingGround.GetTransferMomentumWeight(GroundedState.GroundHitPosition, groundVelocity);
            groundVelocity *= velocityWeight;
            groundVelocity *= transferWeight;
        }
        else if (rb != null && !rb.isKinematic)
        {
            Vector3 groundVel = rb.GetPointVelocity(GroundedState.GroundHitPosition);
            float velocity = Mathf.Min(groundVel.magnitude, MaxDefaultLaunchVelocity);
            groundVelocity = groundVel.normalized * velocity;
        }
        else if (GroundedState.StandingOnGround)
        {
            Vector3 avgVel = worldVelocity.Average();
            float velocity = Mathf.Min(avgVel.magnitude, MaxDefaultLaunchVelocity);
            groundVelocity = avgVel.normalized * velocity;
        }

        return groundVelocity;
    }

 
}

