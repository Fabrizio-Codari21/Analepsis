using System;
using UnityEngine;

public class Controller :  MonoBehaviour,IGravityActor,IMoverActor
{

    [SerializeField] private ControllerStat m_stat;
    
    private ICamera _camera;
    private CCInputHandler  _inputHandler; 
    private HSMStateMachine _movementStateMachine;
    private MoveEngine  _moveEngine;
    
    
    private Vector2 _moveInput;
    private Vector3 _velocity;
    private const float StopEpsilon = 0.02f; 
    
    private void Awake()
    {
        _moveEngine =  GetComponent<MoveEngine>();
        _inputHandler =  GetComponent<CCInputHandler>();

        _camera = GetComponentInChildren<ICamera>();
    }
    
    private void Start()
    {
        BuildStateMachine();
        
    }

    private void Update()
    {
        _movementStateMachine.Update();  // lo que son transitiones se hacen en update
    }

    private void FixedUpdate()
    {
        _movementStateMachine.FixedUpdate(); // fisica se hace en fixed, todos lo de air y ground
        _moveEngine.MovePlayer(Time.fixedDeltaTime,_velocity * Time.fixedDeltaTime );
    }

    private void OnEnable()
    {
        _inputHandler.Move += ReadMoveInput;
    }

    private void OnDisable()
    {
        _moveInput =  Vector2.zero;
        _inputHandler.Move -= ReadMoveInput;
    }


    

    private void ReadMoveInput(Vector2 moveInput)
    {
        _moveInput = moveInput;
    }
    
    private void BuildStateMachine()
    {
        var root = new EmptyState();
        var ground = new GroundState(this);
        var air = new Air(this);
        var groundIdle = new IdleState(this,m_stat);
        var groundMove = new Move(this,m_stat);
        var airIdle = new IdleState(this,m_stat); // por ahora comparte stat, pero puede ser que stat sea SO or class, si llega cambiar algo de movement
        var airMove = new Move(this,m_stat);
        _movementStateMachine = new HSMStateMachine.Builder(root,true)
            .State(ground,root,true)
            .State(air,root)
            
            .State(groundIdle,ground,true)
            .State(groundMove,ground)
            
            .State(airIdle,air,true)
            .State(airMove,air)
            
            .At<Func<bool>>(air,ground, ()=>_moveEngine.GroundedState.StandingOnGround && !_moveEngine.MovingUp(_velocity))
            .At<Func<bool>>(ground,air,()=>_moveEngine.GroundedState.Falling || _moveEngine.GroundedState.Sliding)
            
            .At<Func<bool>>(groundIdle, groundMove,() => _moveInput.sqrMagnitude > 0.001f)
            .At<Func<bool>>(groundMove ,groundIdle,() => _moveInput.sqrMagnitude <= 0.001f)
            
            .At<Func<bool>>(airIdle,airMove,() => _moveInput.sqrMagnitude > 0.001f)
            .At<Func<bool>>(airMove,airIdle,() => _moveInput.sqrMagnitude <= 0.001f)
            
            .Build(root);
    }

    
    
    public void ApplyGravity(Vector3 gravity, float deltaTime)
    {
        _velocity += gravity * deltaTime ;
    }

    public void ResetGravity()
    {
        _velocity.y = 0;
    }

    public Vector3 Move(float targetSpeed, float timeToTargetSpeed)
    {
        Vector3 desiredDir = GetDesiredDirection();
        Vector3 targetVelocity = desiredDir  * targetSpeed;
        float accel = targetSpeed / Mathf.Max(timeToTargetSpeed, 0.001f);
        
        Vector3 horizontal = new Vector3(_velocity.x, 0, _velocity.z);
        horizontal = Vector3.MoveTowards(
            horizontal,
            targetVelocity,
            accel * Time.fixedDeltaTime
        );
        _velocity.x = horizontal.x;
        _velocity.z = horizontal.z;
        return _velocity;
    }
    
    private Vector3 GetDesiredDirection()
    {
        Quaternion horizontalRotation = _camera.HorizontalPlane;
        Vector3 inputDir = new Vector3(_moveInput.x, 0, _moveInput.y);
        Vector3 rotateMovement = horizontalRotation * inputDir;
        return _moveEngine.GetProjectedMovement(rotateMovement);
    }

    public void Stop(float timeToStop)
    {
        if(_velocity is { x: 0, z: 0 }) return;
        Vector3 horizontal = new Vector3(_velocity.x, 0, _velocity.z);
        if (horizontal.sqrMagnitude < StopEpsilon * StopEpsilon)
        {
            _velocity.x = 0f;
            _velocity.z = 0f;
            return;
        }
        if (horizontal.sqrMagnitude < 0.0001f)
            return;

        float deceleration = horizontal.magnitude / Mathf.Max(timeToStop, 0.001f);

        horizontal = Vector3.MoveTowards(
            horizontal,
            Vector3.zero,
            deceleration * Time.fixedDeltaTime
        );

        _velocity.x = horizontal.x;
        _velocity.z = horizontal.z;
    }
}


public interface IGravityActor
{
    public void ApplyGravity(Vector3 gravity, float deltaTime);
    public void ResetGravity();
}

public interface IMoverActor
{
    public Vector3 Move(float targetSpeed,float timeToTargetSpeed);
    public void Stop(float timeToStop);
}
[Serializable]
public class ControllerStat
{
    public float speed;
    [SerializeField,Range(0.1f,2f)] public float moveTimeToMaxSpeed;
    public float timeToStop; // de movimiento a quieto
}

public class EmptyState : IState
{
    
}


public class IdleState : IState
{
    private readonly IMoverActor _actor;
    private readonly ControllerStat _stat;
    public IdleState(IMoverActor actor,ControllerStat stat)
    {
        _actor = actor;
        _stat = stat;
    }
    public void FixedUpdate()
    {
        _actor.Stop(_stat.timeToStop);
    }
}

public class GroundState : IState
{
    private readonly IGravityActor _actor;
    public GroundState(IGravityActor actor)
    {
        _actor = actor;
    }

    public void OnEnter()
    {
        _actor.ResetGravity();
    }
}

public class Air : IState
{
    private readonly IGravityActor _actor;
    public Air(IGravityActor gravityAffecter)
    {
        _actor = gravityAffecter;
    }

    public void FixedUpdate()
    {
        _actor.ApplyGravity(Physics.gravity,Time.fixedDeltaTime);
    }

}

public class Move : IState
{
    
   private readonly IMoverActor _actor;
   
   private readonly ControllerStat _stat;
   
   public Move(IMoverActor actor,ControllerStat stat)
   {
      _actor = actor;
      _stat = stat;
   }


   public void FixedUpdate()
   {
       _actor.Move(_stat.speed,_stat.moveTimeToMaxSpeed);
   }
}