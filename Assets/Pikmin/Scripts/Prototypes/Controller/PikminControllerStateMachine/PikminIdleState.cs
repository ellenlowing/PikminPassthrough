using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pikmin
{
    public class PikminIdleState : BaseState<PikminStateManager.PikminState>
    {
        public PikminIdleState(PikminStateManager.PikminState key) : base(key) {}
        protected PikminStateManager stateManager;

        float wallInterpolationRatio = 0f;

        public override void Initialize(StateManager<PikminStateManager.PikminState> _stateManager)
        {
            stateManager = _stateManager.GetComponent<PikminStateManager>();
        }

        public override void EnterState()
        {
            stateManager.animator.SetInteger("state", (int)this.StateKey);
            stateManager.animator.speed = 1f;
        }

        public override void ExitState()
        {
            
        }

        public override void UpdateState() 
        {
            if(stateManager.movingWallToFloor)
            {
                stateManager.transform.position = Vector3.Slerp(stateManager.wallStartPosition, stateManager.wallEndPosition, wallInterpolationRatio);
                stateManager.transform.rotation = Quaternion.Slerp(stateManager.wallStartQuaternion, stateManager.wallEndQuaternion, wallInterpolationRatio);
                wallInterpolationRatio += Time.deltaTime;

                if(wallInterpolationRatio >= 1)
                {
                    stateManager.movingWallToFloor = false;
                }
            }
        }

        public override PikminStateManager.PikminState GetNextState() 
        {
            if(!stateManager.movingWallToFloor && stateManager.joystickInput.magnitude > 0)
            {
                return PikminStateManager.PikminState.Walk;
            }
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

