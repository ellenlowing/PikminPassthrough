using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pikmin
{
    public class PikminWalkState : BaseState<PikminStateManager.PikminState>
    {
        public PikminWalkState(PikminStateManager.PikminState key) : base(key) {}
        protected PikminStateManager stateManager;

        public float runningSpeed = 0.5f;
        public float timeStep = 5f;
        public float detectionLength;
        public float sphereCastRadius;
        public float maxWalkLookAngle;

        public override void Initialize(StateManager<PikminStateManager.PikminState> _stateManager)
        {
            stateManager = _stateManager.GetComponent<PikminStateManager>();
            runningSpeed = stateManager.runningSpeed;
            timeStep = stateManager.timeStep;
            detectionLength = stateManager.walkDetectionLength;
            sphereCastRadius = stateManager.walkSphereCastRadius;
            maxWalkLookAngle = stateManager.maxWalkLookAngle;
        }

        public override void EnterState()
        {
            stateManager.animator.SetInteger("state", (int)this.StateKey);
        }

        public override void ExitState()
        {
            
        }

        public override void UpdateState() 
        {
            Vector3 targetRotation = new Vector3(0, Mathf.Atan2(stateManager.joystickInput.x, stateManager.joystickInput.y) * Mathf.Rad2Deg, 0);
            Quaternion targetQuaternion = Quaternion.identity;
            targetQuaternion.eulerAngles = targetRotation;
            stateManager.transform.rotation = Quaternion.Slerp(stateManager.transform.rotation, targetQuaternion, Time.deltaTime * timeStep);

            stateManager.transform.position += stateManager.joystickInput.magnitude * runningSpeed * Time.deltaTime * stateManager.transform.forward;
        }
        
        public override PikminStateManager.PikminState GetNextState() 
        {
            if(stateManager.joystickInput.magnitude == 0)
            {
                return PikminStateManager.PikminState.Idle;
            }

            RaycastHit frontWallHit;
            float wallLookAngle;
            bool wallFront = PassthroughUtils.CheckSurfaceHit(stateManager.transform.position, stateManager.transform.forward, sphereCastRadius, detectionLength, out frontWallHit, out wallLookAngle, stateManager.wallLayer);
            if(wallFront && wallLookAngle <= maxWalkLookAngle)
            {
                stateManager.transform.rotation = Quaternion.LookRotation(stateManager.isQuest ? -frontWallHit.transform.forward : -frontWallHit.transform.up, Vector3.up);
                return PikminStateManager.PikminState.Climb;
            }
            // Debug.Log(wallFront);

            return this.StateKey;
        }

        public override void OnTriggerEnter(Collider other) 
        {

        }
        public override void OnTriggerStay(Collider other) 
        {

        }
        public override void OnTriggerExit(Collider other) 
        {

        }
    }
}

