using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pikmin
{
    public class PikminStateManager : StateManager<PikminStateManager.PikminState>
    {
        public enum PikminState
        {
            Idle,
            Walk,
            Climb
        }

        [Header("Platform")]
        public bool isQuest;

        [Header("Time")]
        public float timeStep = 5f;

        [Header("WalkDetection")]
        public float runningSpeed = 5f;
        public float walkDetectionLength;
        public float walkSphereCastRadius;
        public float maxWalkLookAngle;

        [Header("ClimbDetection")]
        public float climbingSpeed = .1f;
        public float climbDetectionLength;
        public float climbSphereCastRadius;
        public float maxClimbLookAngle;

        [Header("Transitions")]
        [HideInInspector] public bool movingWallToFloor;
        [HideInInspector] public Vector3 wallStartPosition;
        [HideInInspector] public Vector3 wallEndPosition;
        [HideInInspector] public Quaternion wallStartQuaternion;
        [HideInInspector] public Quaternion wallEndQuaternion;

        [Header("References")]
        [HideInInspector] public Animator animator;
        public LayerMask wallLayer;
        public LayerMask floorLayer;
        [HideInInspector] public Vector2 joystickInput;

        private void Start()
        {
            animator = GetComponent<Animator>();

            States.Add(PikminState.Idle, new PikminIdleState(PikminState.Idle));
            States.Add(PikminState.Walk, new PikminWalkState(PikminState.Walk));
            States.Add(PikminState.Climb, new PikminClimbState(PikminState.Climb));

            foreach(KeyValuePair<PikminState, BaseState<PikminState>> entry in States)
            {
                entry.Value.Initialize(this);
            }

            CurrentState = States[PikminState.Idle];
            CurrentState.EnterState();
        }

        public void Initialize()
        {
            CurrentState = States[PikminState.Idle];
            CurrentState.EnterState();
        }

        private void Update()
        {
            PikminState nextStateKey = CurrentState.GetNextState();

            if(nextStateKey.Equals(CurrentState.StateKey))
            {
                CurrentState.UpdateState();
            }
            else
            {
                TransitionToState(nextStateKey);
            }
        }

        private void OnTriggerEnter(Collider other) {
            CurrentState.OnTriggerEnter(other);
        }

        private void OnTriggerStay(Collider other) {
            CurrentState.OnTriggerStay(other);
        }

        private void OnTriggerExit(Collider other) {
            CurrentState.OnTriggerExit(other);
        }

    }
}

