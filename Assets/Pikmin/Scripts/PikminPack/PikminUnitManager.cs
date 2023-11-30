using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PikminPack
{
    public enum PikminState
    {
        Idle,
        FollowLeader,
        GetInFormation,
        PreLaunch,
        InLaunch,
        Climb,
        ReturnToSquad
    }

    public class PikminUnitManager : MonoBehaviour
    {
        // Animation Clips 
        public readonly int Idle = Animator.StringToHash("Idle");
        public readonly int Run = Animator.StringToHash("Run");
        public readonly int Hang = Animator.StringToHash("Hang");
        public readonly int Notice = Animator.StringToHash("Notice");
        public readonly int Fall = Animator.StringToHash("Fall");
        public readonly int Climb = Animator.StringToHash("Climb");

        // Serialized fields
        [SerializeField] private PikminUnit _pikminPrefab;
        [SerializeField] private int _pikminUnitAmount;
        [SerializeField] private OVRHand _rightHand;
        [SerializeField] private Transform _rightHandRayPointer;
        [SerializeField] private bool _debugMode;
        [SerializeField] private float _phylloc;
        [SerializeField] private float _leaderFromPackDistanceThreshold;
        [SerializeField] private float _leaderMoveDistanceThreshold;

        // Private variables
        private PikminUnit _unitToLaunch;
        private bool _isRightIndexFingerPinching;

        // Component References

        // Public members
        public static PikminUnitManager Instance;
        public List<PikminUnit> InSquadPikminUnits { get; private set; } = new List<PikminUnit>();
        public List<PikminUnit> WildPikminUnits { get; private set; } = new List<PikminUnit>();
        public Raycaster Raycaster;
        public GameObject LeaderGhost;
        public Transform LeaderTransform;
        public Vector3 LastGroundedLeaderPosition;
        public Vector3 GroundedLeaderPosition;
        public bool LeaderMoveEnough;
        public float PikminWalkSpeed;
        public float PikminTurnSpeed;
        
        void Awake() 
        {
            if(Instance != null && Instance != this)
            {
                Destroy(this);
            }
            Instance = this;
        }
        
        void Start()
        {   
            for(int i = 0; i < _pikminUnitAmount; i++)
            {
                var pikminUnit = Instantiate(_pikminPrefab);
                pikminUnit.Init(this, Raycaster, GetFormationPositionOffset(i));
                InSquadPikminUnits.Add(pikminUnit);
            }

            LeaderGhost = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            LeaderGhost.transform.position = GetOffsetPositionGrounded(LeaderTransform, Vector3.zero);
            LeaderGhost.transform.rotation = LeaderTransform.rotation;
            LeaderGhost.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            Rigidbody ghostRb = LeaderGhost.AddComponent<Rigidbody>();
            ghostRb.interpolation = RigidbodyInterpolation.Interpolate;
            ghostRb.useGravity = false;
            ghostRb.isKinematic = true;
            Destroy(LeaderGhost.GetComponent<SphereCollider>());
            // Destroy(LeaderGhost.GetComponent<MeshRenderer>());
        }

        void Update()
        {
            // Handle input for Launch Projectile
            if(!_debugMode)
            {
                if(_rightHand.IsTracked)
                {
                    Raycaster.transform.SetPositionAndRotation(_rightHandRayPointer.position, _rightHandRayPointer.rotation);

                    // Check pinching status
                    _isRightIndexFingerPinching = _rightHand.GetFingerIsPinching(OVRHand.HandFinger.Index);
                    if(_isRightIndexFingerPinching && !_unitToLaunch)
                    {
                        HandlePrelaunch();
                    }
                    else if (!_isRightIndexFingerPinching && _unitToLaunch != null && _unitToLaunch.TryGetComponent<PikminUnit>(out PikminUnit unit))
                    {
                        HandleLaunch(unit);
                    }
                }
            }
            else
            {
                // Debug helper
                if(Input.GetKey(KeyCode.Space) && !_unitToLaunch)
                {
                    HandlePrelaunch();
                }
                if(Input.GetKeyUp(KeyCode.Space))
                {
                    if(_unitToLaunch != null && _unitToLaunch.TryGetComponent<PikminUnit>(out PikminUnit unit))
                    {
                        HandleLaunch(unit);
                    }
                }
            }

            // Update logic for Formation
            float step = Time.deltaTime * PikminWalkSpeed;
            float angularStep = Time.deltaTime * PikminTurnSpeed;
            GroundedLeaderPosition = new Vector3(LeaderTransform.position.x, 0, LeaderTransform.position.z);
            bool leaderAwayFromPack = Vector3.Distance(LeaderGhost.transform.position, GroundedLeaderPosition) > _leaderFromPackDistanceThreshold;
            LeaderMoveEnough = Vector3.Distance(GroundedLeaderPosition, new Vector3(LastGroundedLeaderPosition.x, 0, LastGroundedLeaderPosition.z)) > _leaderMoveDistanceThreshold;

            if(leaderAwayFromPack)
            {
                Vector3 newGhostPosition = GetOffsetPositionGrounded(LeaderTransform, Vector3.zero);
                LeaderGhost.transform.position = Vector3.MoveTowards(LeaderGhost.transform.position, newGhostPosition, step);
                
                Quaternion leaderYRotation = Quaternion.identity;
                leaderYRotation.eulerAngles = new Vector3(0, LeaderTransform.rotation.eulerAngles.y, 0);
                LeaderGhost.transform.rotation = Quaternion.RotateTowards(LeaderGhost.transform.rotation, leaderYRotation, angularStep);
            }

            foreach(PikminUnit unit in InSquadPikminUnits)
            {
                unit.DetermineFormationState();
            }
            
            LastGroundedLeaderPosition = GroundedLeaderPosition;
        }

        void HandlePrelaunch()
        {
            if(InSquadPikminUnits.Count > 0)
            {
                _unitToLaunch = InSquadPikminUnits[0];
            }

            if(_unitToLaunch != null && _unitToLaunch.TryGetComponent<PikminUnit>(out PikminUnit unit))
            {
                unit.SetState(PikminState.PreLaunch);
                Raycaster.Prelaunch = true;
            }
        }

        void HandleLaunch(PikminUnit unit)
        {
            unit.SetState(PikminState.InLaunch);
            InSquadPikminUnits.Remove(unit);
            WildPikminUnits.Add(unit);
            _unitToLaunch = null;
            Raycaster.Prelaunch = false;
        }

        public Vector3 GetOffsetPositionGrounded(Transform refTransform, Vector3 positionOffset)
        {
            Vector3 newOffset = refTransform.forward * positionOffset.z + refTransform.right * positionOffset.x;
            Vector3 newPosition = refTransform.position + newOffset;
            // newPosition += newOffset * (_radius - 1f);
            newPosition.y = 0;
            return newPosition;
        }

        Vector3 GetFormationPositionOffset(int index)
        {
            float a = (float)index * 137.5f * Mathf.Deg2Rad;
            float r = _phylloc * Mathf.Sqrt(index);
            float x = r * Mathf.Cos(a);
            float z = r * Mathf.Sin(a);
            return new Vector3(x, 0, z);
        }
    }
}