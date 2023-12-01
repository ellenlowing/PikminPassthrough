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
        private Vector3 _formationPositionOffset;
        private IEnumerator _getInFormationCoroutine;

        [SerializeField] private Transform _topJoint;
        [SerializeField] private float _launchSpeed;
        [SerializeField] private float _inLaunchSpinSpeed;
        [SerializeField] private float _distanceFromDestinationThreshold = 0.001f;

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

                case PikminState.GetInFormation:
                    UpdateGetInFormationState();
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
        
        public void Init(PikminUnitManager manager, Raycaster raycaster, Vector3 formationPositionOffset)
        {
            _manager = manager;
            _raycaster = raycaster;
            _formationPositionOffset = formationPositionOffset;
            transform.position = _manager.GetOffsetPositionGrounded(_manager.LeaderTransform, _formationPositionOffset);
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

                    case PikminState.GetInFormation:
                        EnterGetInFormationState();
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
            if(_getInFormationCoroutine != null) StopCoroutine(_getInFormationCoroutine);
            _getInFormationCoroutine = null;
        }

        void EnterGetInFormationState()
        {
            _animator.CrossFade(_manager.Run, 0, 0);
            // Vector3 destination = _manager.GetOffsetPositionGrounded(_manager.LeaderGhost.transform, _formationPositionOffset);
            _getInFormationCoroutine = GetInFormationMovement();
            Debug.Log("start get in formation");
            StartCoroutine(_getInFormationCoroutine);
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
            transform.rotation = Quaternion.LookRotation(_manager.GroundedLeaderPosition - transform.position, Vector3.up);
        }

        void UpdateFollowLeaderState()
        {
            Vector3 newPosition = transform.position + _manager.LeaderGhostMoveDifference; 
            transform.rotation = Quaternion.LookRotation(_manager.LeaderGhostMoveDifference, Vector3.up);
            transform.position = Vector3.MoveTowards(transform.position, newPosition, Time.deltaTime * _manager.PikminWalkSpeed);
        }

        void UpdateGetInFormationState()
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

        public void DetermineFormationState()
        {
            if(CurrentState != PikminState.FollowLeader)
            {
                if(_manager.LeaderAwayFromPack && _manager.LeaderMoveEnough)
                {
                    SetState(PikminState.FollowLeader);
                }
            }
            else
            {
                if(!_manager.LeaderAwayFromPack)
                {
                    Vector3 destination = _manager.GetOffsetPositionGrounded(_manager.LeaderGhost.transform, _formationPositionOffset);
                    bool awayFromDestination = Vector3.Distance(transform.position, destination) > _distanceFromDestinationThreshold;
                    if(awayFromDestination)
                    {

                        SetState(PikminState.GetInFormation);
                    }
                    else
                    {
                        SetState(PikminState.Idle);
                    }
                }
            }
            
        }

        // Add coroutine for get in formation movement to avoid glitching when the pikmin almost arrives at destination
        IEnumerator GetInFormationMovement()
        {
            float distanceFromDestination = 9999f;
            while(distanceFromDestination > _distanceFromDestinationThreshold)
            {
                Vector3 destination = _manager.GetOffsetPositionGrounded(_manager.LeaderGhost.transform, _formationPositionOffset);
                Vector3 newPosition = Vector3.MoveTowards(transform.position, destination, Time.deltaTime * _manager.PikminWalkSpeed);
                transform.rotation = Quaternion.LookRotation(newPosition - transform.position, Vector3.up);
                transform.position = newPosition;

                distanceFromDestination = Vector3.Distance(transform.position, destination);
                // Debug.Log(distanceFromDestination);

                yield return null;
            }
            SetState(PikminState.Idle);
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
