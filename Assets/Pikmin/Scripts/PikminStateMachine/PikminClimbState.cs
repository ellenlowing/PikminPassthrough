using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pikmin
{
    public class PikminClimbState : BaseState<PikminStateManager.PikminState>
    {
        public PikminClimbState(PikminStateManager.PikminState key) : base(key) {}
        protected PikminStateManager stateManager;
        
        public float timeStep = 5f;
        public float detectionLength;
        public float sphereCastRadius;
        public float maxClimbLookAngle;

        private bool movingWalls;
        private Vector3 wallEndPosition;
        private Quaternion faceWallQuaternion;
        private Vector3 wallStartPosition;
        private Quaternion wallStartQuaternion;
        private Quaternion wallEndQuaternion;
        private float wallInterpolationRatio = 0f;

        public override void Initialize(StateManager<PikminStateManager.PikminState> _stateManager)
        {
            stateManager = _stateManager.GetComponent<PikminStateManager>();
            timeStep = stateManager.timeStep;
            detectionLength = stateManager.climbDetectionLength;
            sphereCastRadius = stateManager.climbSphereCastRadius;
            maxClimbLookAngle = stateManager.maxClimbLookAngle;
            Debug.Log("floor layer: " + stateManager.floorLayer);
        }

        public override void EnterState()
        {
            stateManager.animator.SetInteger("state", (int)this.StateKey);
            faceWallQuaternion = stateManager.transform.rotation;
            movingWalls = false;
        }

        public override void ExitState()
        {
            stateManager.animator.speed = 1f;
        }

        public override void UpdateState() 
        {
            if(movingWalls)
            {
                stateManager.animator.speed = 1f;
                stateManager.transform.position = Vector3.Slerp(wallStartPosition, wallEndPosition, wallInterpolationRatio);
                stateManager.transform.rotation = Quaternion.Slerp(wallStartQuaternion, wallEndQuaternion, wallInterpolationRatio);
                wallInterpolationRatio += Time.deltaTime;
            }
            else
            {
                float joystickMagnitude = stateManager.joystickInput.magnitude;
            
                if(joystickMagnitude > 0)
                {   
                    stateManager.animator.speed = 1f;
                    Vector3 targetRotation = new Vector3(0, 0, -Mathf.Atan2(stateManager.joystickInput.x, stateManager.joystickInput.y) * Mathf.Rad2Deg);
                    Quaternion targetQuaternion = Quaternion.identity;
                    targetQuaternion.eulerAngles = targetRotation;
                    
                    stateManager.transform.rotation = Quaternion.Slerp(stateManager.transform.rotation, faceWallQuaternion * targetQuaternion, Time.deltaTime * timeStep);
                    stateManager.transform.position += stateManager.transform.up * joystickMagnitude * stateManager.climbingSpeed * Time.deltaTime;
                }
                else
                {
                    stateManager.animator.speed = 0f;
                }
            }
        }

        public override PikminStateManager.PikminState GetNextState() 
        {
            RaycastHit surfaceHit;
            float floorLookAngle;
            bool floorFront = PassthroughUtils.CheckSurfaceHit(stateManager.transform.position, stateManager.transform.up, sphereCastRadius, detectionLength, out surfaceHit, out floorLookAngle, stateManager.floorLayer);
            if(floorFront && floorLookAngle <= maxClimbLookAngle)
            {
                stateManager.movingWallToFloor = true;
                stateManager.wallStartPosition = new Vector3(stateManager.transform.position.x, stateManager.transform.position.y, stateManager.transform.position.z);
                stateManager.wallStartQuaternion = new Quaternion(stateManager.transform.rotation.x, stateManager.transform.rotation.y, stateManager.transform.rotation.z, stateManager.transform.rotation.w);
                stateManager.wallEndPosition = surfaceHit.point - stateManager.transform.forward * 0.05f;
                stateManager.wallEndQuaternion = Quaternion.LookRotation(-stateManager.transform.forward, surfaceHit.normal);
                
                return PikminStateManager.PikminState.Idle;
            }
            else
            {
                float wallLookAngle;
                bool wallFront = PassthroughUtils.CheckSurfaceHit(stateManager.transform.position, stateManager.transform.up, sphereCastRadius, detectionLength, out surfaceHit, out wallLookAngle, stateManager.wallLayer);
                if(!movingWalls && wallFront && wallLookAngle <= maxClimbLookAngle)
                {
                    // keep climbing on another wall
                    faceWallQuaternion = Quaternion.LookRotation(stateManager.isQuest ? -surfaceHit.transform.forward :-surfaceHit.transform.up, Vector3.up);

                    movingWalls = true;
                    wallStartPosition = new Vector3(stateManager.transform.position.x, stateManager.transform.position.y, stateManager.transform.position.z);
                    wallStartQuaternion = new Quaternion(stateManager.transform.rotation.x, stateManager.transform.rotation.y, stateManager.transform.rotation.z, stateManager.transform.rotation.w);
                    wallEndPosition = surfaceHit.point;
                    Vector3 targetRotation = new Vector3(0, 0, -Mathf.Atan2(stateManager.joystickInput.x, stateManager.joystickInput.y) * Mathf.Rad2Deg);
                    Quaternion targetQuaternion = Quaternion.identity;
                    targetQuaternion.eulerAngles = targetRotation;
                    wallEndQuaternion = faceWallQuaternion * targetQuaternion;

                    wallInterpolationRatio = 0f;
                }
            }

            if(movingWalls && wallInterpolationRatio >= 1)
            {
                movingWalls = false;
            }
            

            // Debug.Log(stateManager.transform.position + " " + floorFront + " " + floorLookAngle + " " + wallFront + " " + wallLookAngle);

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

