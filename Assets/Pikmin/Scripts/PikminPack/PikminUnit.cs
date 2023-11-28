using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction.Input;
using UnityEngine;

namespace PikminPack
{

    public class PikminUnit : MonoBehaviour
    {
        public PikminState CurrentState; // {private set; get;}
        private PikminUnitManager _manager;
        private Raycaster _raycaster;
        private Animator _animator;
        private bool _inSquad;
        private Vector3 _topJointPositionOffset;

        [SerializeField] private Transform _topJoint;
        [SerializeField] private float _launchSpeed;
        [SerializeField] private float _inLaunchSpinSpeed;

        void Start()
        {
            _animator = GetComponent<Animator>();
            _inSquad = true;
            CurrentState = PikminState.Idle;
            _topJointPositionOffset = _topJoint.position;
        }

        void Update()
        {
            switch(CurrentState)
            {
                case PikminState.Idle:
                    UpdateIdleState();
                    break;

                case PikminState.FollowLeader:
                    UpdateFollowLeaderState();
                    break;

                case PikminState.PreLaunch:
                    UpdatePreLaunchState();
                    break;

                case PikminState.InLaunch:
                    UpdateInLaunchState();
                    break;

                case PikminState.Climb:
                    UpdateClimbState();
                    break;

                case PikminState.ReturnToSquad:
                    break;
            }
        }
        
        public void Init(PikminUnitManager manager, Raycaster raycaster)
        {
            _manager = manager;
            _raycaster = raycaster;
        }

        public void SetState(PikminState state)
        {
            if(state != CurrentState)
            {
                switch(state)
                {
                    case PikminState.Idle:
                        EnterIdleState();
                        break;

                    case PikminState.FollowLeader:
                        EnterFollowLeaderState();
                        break;

                    case PikminState.PreLaunch:
                        EnterPreLaunchState();
                        break;

                    case PikminState.InLaunch:
                        EnterInLaunchState();
                        break;
                    
                    case PikminState.Climb:
                        EnterClimbState();
                        break;

                    case PikminState.ReturnToSquad:
                        break;
                }

                CurrentState = state;
            }
        }

        void EnterIdleState()
        {
            _animator.CrossFade(_manager.Idle, 0, 0);
        }

        void EnterFollowLeaderState()
        {
            _animator.CrossFade(_manager.Run, 0, 0);
        }

        void EnterPreLaunchState()
        {
            _animator.CrossFade(_manager.Fall, 0, 0);
        }

        void EnterInLaunchState()
        {
            _inSquad = false;
            _animator.CrossFade(_manager.Hang, 0, 0);
            StartCoroutine(ProjectileMovement(_raycaster.LaunchPosition, _raycaster.GroundDirectionNorm, _raycaster.V0, _raycaster.LaunchAngle, _raycaster.LaunchDuration));
        }

        void EnterClimbState()
        {
            _animator.CrossFade(_manager.Climb, 0, 0);
        }

        void UpdateIdleState()
        {
            
        }

        void UpdateFollowLeaderState()
        {
            
        }

        void UpdatePreLaunchState()
        {
            transform.position = _raycaster.transform.position - (_topJoint.position - transform.position);
            transform.rotation = Quaternion.LookRotation(-_raycaster.transform.forward, Vector3.up); 
        }

        void UpdateInLaunchState()
        {
        }

        void UpdateClimbState()
        {

        }

        public bool GetInSquad()
        {
            return _inSquad;
        }

        IEnumerator ProjectileMovement(Vector3 launch, Vector3 direction, float v0, float angle, float time)
        {
            float t = 0;
            float rotationX = 0;
            while(t < time)
            {
                transform.position = ProjectileLibrary.GetPositionAtTime(launch, direction, v0, angle, t);
                transform.rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(rotationX, 0, 0);

                rotationX -= Time.deltaTime * _inLaunchSpinSpeed;
                t += Time.deltaTime * _launchSpeed;
                yield return null;
            }
            HandleLanding();
        }
        
        void HandleLanding()
        {
            RaycastHit hit = _raycaster.RaycastHit;
            if(hit.collider.CompareTag("Floor"))
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(_raycaster.GroundDirectionNorm), 360);
                SetState(PikminState.Idle);
            }
            else if (hit.collider.CompareTag("Wall"))
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(-hit.transform.forward, Vector3.up), 360);
                SetState(PikminState.Climb);
            }
        }
    }

}
