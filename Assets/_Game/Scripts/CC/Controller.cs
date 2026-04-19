using System;
using UnityEngine;
using Sirenix.OdinInspector;

public class Controller :  MonoBehaviour,IGravityActor,IMoverActor
{
    [FoldoutGroup("Controller Setting")]
    [LabelText("Stats")] 
    [SerializeField] private ControllerStat m_stat;
    
    #region StateMachineField
        [Space(15)]
        [FoldoutGroup("StateMachine Debug")]
        [LabelText("Movement StateMachine")] 
        [SerializeField, Tooltip("Transition Chain Debug")] 
         private bool m_transitionChainDebug;
    #endregion
    
    private ICamera _camera;
    private CcInputHandler  _inputHandler; 
    private HSMStateMachine _movementStateMachine;
    private MoveEngine  _moveEngine;
    
    
    private Vector2 _moveInput;
    private Vector3 _velocity;
    private const float StopEpsilon = 0.02f;

   
    private void Awake()
    {
        _moveEngine =  GetComponent<MoveEngine>();
        _inputHandler =  GetComponent<CcInputHandler>();

        _camera = GetComponentInChildren<ICamera>();
    }
    
    private void Start()
    {
        BuildMovementStateMachine();
    }

    private void Update()
    {
        _movementStateMachine.Update(); 
        // la fisica no lo hice por fixed porque internamente estoy controlando el transicion de estado, como leer input por update, para que
        // no haya problema de tick entre camera y movimiento lo hice todos por udpate. pero esta ya esta asegurado el orden de ejecucion , asi que no creo que haya problema.
        _moveEngine.MovePlayer(Time.deltaTime,_velocity * Time.deltaTime ); 
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
    
    private void BuildMovementStateMachine()
    {
        var root = new EmptyState();
        var ground = new GroundState(this);
        var air = new Air(this);
        var groundIdle = new IdleState(this,m_stat);
        var groundMove = new Move(this,m_stat);
        var airIdle = new IdleState(this,m_stat); // por ahora comparte stat, pero puede ser que stat sea SO or class, si llega cambiar algo de movement
        var airMove = new Move(this,m_stat);
        _movementStateMachine = new HSMStateMachine.Builder(root,m_transitionChainDebug)
            .State(ground,root,true)
            .State(air,root)
            
            .State(groundIdle,ground,true)
            .State(groundMove,ground)
            
            .State(airIdle,air,true)
            .State(airMove,air)
            
            .At(air,ground, new FuncPredicate(() =>_moveEngine.GroundedState.StandingOnGround && !_moveEngine.MovingUp(_velocity)))
            .At(ground,air,new FuncPredicate(() =>_moveEngine.GroundedState.Falling || _moveEngine.GroundedState.Sliding))
            
            .At(groundIdle, groundMove, new FuncPredicate(() => _moveInput.sqrMagnitude > 0.001f))
            .At(groundMove ,groundIdle,new FuncPredicate(() => _moveInput.sqrMagnitude <= 0.001f))
            
            .At(airIdle,airMove,new FuncPredicate(() => _moveInput.sqrMagnitude > 0.001f))
            .At(airMove,airIdle,new FuncPredicate(() => _moveInput.sqrMagnitude <= 0.001f))
            
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
    public void Update()
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
    
    

    public void Update()
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


   public void Update()
   {
       _actor.Move(_stat.speed,_stat.moveTimeToMaxSpeed);
   }
}