using System;
using UnityEngine;

public abstract class BaseState<EState> where EState : Enum
{
    public BaseState(EState key)
    {
        StateKey = key;
    }

    public EState StateKey {get; private set; }
    private StateManager<EState> stateManager;

    public abstract void Initialize(StateManager<EState> _stateManager);
    public abstract void EnterState();
    public abstract void ExitState();
    public abstract void UpdateState();
    public abstract EState GetNextState();
    public abstract void OnTriggerEnter(Collider other);
    public abstract void OnTriggerStay(Collider other);
    public abstract void OnTriggerExit(Collider other);

    public virtual bool CheckSurfaceHit(Vector3 origin, Vector3 direction, float sphereCastRadius, float detectionLength, LayerMask surfaceLayer, out RaycastHit frontSurfaceHit, out float surfaceLookAngle)
    {
        bool surfaceFront = Physics.SphereCast(origin, sphereCastRadius, direction, out frontSurfaceHit, detectionLength, surfaceLayer);
        surfaceLookAngle = Vector3.Angle(direction, -frontSurfaceHit.normal);
        return surfaceFront;
    }
}
