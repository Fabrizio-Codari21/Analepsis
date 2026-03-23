using System;
using UnityEngine;

public class Controller :  MonoBehaviour,IGravityActor,IMoverActor
{

    [SerializeField] private ControllerStat m_stat;
    private HSMStateMachine _movementStateMachine;
    private MoveEngine  _moveEngine;
    private CCInputHandler  _inputHandler;
    
    private void Awake()
    {
        _moveEngine =  GetComponent<MoveEngine>();
        _inputHandler =  GetComponent<CCInputHandler>();
    }
    
    private void Start()
    {
        BuildStateMachine();
    }
    
    private void BuildStateMachine()
    {
        var root = new EmptyState();
        var ground = new GroundState(this);
        var groundIdle = new IdleState(this,m_stat);
        _movementStateMachine = new HSMStateMachine.Builder(root)
            .State(ground,root,true)
            
            .Build(root);
    }

    public void ApplyGravity(Vector3 gravity, float deltaTime)
    {
        throw new NotImplementedException();
    }

    public void ResetGravity()
    {
        throw new NotImplementedException();
    }

    public Vector3 Move(float targetSpeed, float timeToTargetSpeed)
    {
        throw new NotImplementedException();
    }

    public void Stop(float timeToStop)
    {
        throw new NotImplementedException();
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

public class MoveState : IState
{
    
   private readonly IMoverActor _actor;
    
}