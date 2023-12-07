using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PikminPack
{
    public enum PikminState
    {
        WaitForPluck,
        InPluck,
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
        public readonly int Somersault = Animator.StringToHash("Somersault");

        // Serialized fields
        [SerializeField] private PikminUnit _bluePikminPrefab;
        [SerializeField] private PikminUnit _redPikminPrefab;
        [SerializeField] private PikminUnit _yellowPikminPrefab;
        [SerializeField] private int _bluePikminUnitAmount;
        [SerializeField] private int _redPikminUnitAmount;
        [SerializeField] private int _yellowPikminUnitAmount;
        [SerializeField] private OVRHand _rightHand;
        [SerializeField] private Transform _rightHandRayPointer;
        [SerializeField] private bool _debugMode;
        [SerializeField] private float _phylloc;
        [SerializeField] private float _leaderFromPackDistanceThreshold;
        [SerializeField] private float _leaderMoveDistanceThreshold;

        // Private variables
        private PikminUnit _unitToLaunch;
        public bool IsRightIndexFingerPinching;

        // Component References

        // Public members
        public static PikminUnitManager Instance;
        public List<PikminUnit> InSquadPikminUnits { get; private set; } = new List<PikminUnit>();
        public List<PikminUnit> WildPikminUnits { get; private set; } = new List<PikminUnit>();
        public Raycaster Raycaster;
        [HideInInspector] public GameObject LeaderGhost;
        public Transform LeaderTransform;
        [HideInInspector] public Vector3 LastGroundedLeaderPosition;
        [HideInInspector] public Vector3 GroundedLeaderPosition;
        public bool LeaderMoveEnough;
        public bool LeaderAwayFromPack;
        [HideInInspector] public Vector3 LeaderGhostMoveDifference;
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
            
            LeaderGhost = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            LeaderGhost.transform.position = GetOffsetPositionGrounded(LeaderTransform, Vector3.zero);
            LeaderGhost.transform.rotation = LeaderTransform.rotation;
            LeaderGhost.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            Rigidbody ghostRb = LeaderGhost.AddComponent<Rigidbody>();
            ghostRb.interpolation = RigidbodyInterpolation.Interpolate;
            ghostRb.useGravity = false;
            ghostRb.isKinematic = true;
            Destroy(LeaderGhost.GetComponent<SphereCollider>());
            Destroy(LeaderGhost.GetComponent<MeshRenderer>());

            LeaderMoveEnough = false;
            GroundedLeaderPosition = new Vector3(LeaderTransform.position.x, 0, LeaderTransform.position.z);
            LastGroundedLeaderPosition = GroundedLeaderPosition;
            
            int pikminUnitCount = 0;
            for(int i = 0; i < _bluePikminUnitAmount; i++)
            {
                Vector3 formationPositionOffset = GetFormationPositionOffset(pikminUnitCount);
                var pikminUnit = Instantiate(_bluePikminPrefab, GetOffsetPositionGrounded(LeaderTransform, formationPositionOffset), Quaternion.identity);
                pikminUnit.Init(this, Raycaster, formationPositionOffset, PikminState.Idle, true);
                InSquadPikminUnits.Add(pikminUnit);
                pikminUnitCount++;
            }
            for(int i = 0; i < _redPikminUnitAmount; i++)
            {
                Vector3 formationPositionOffset = GetFormationPositionOffset(pikminUnitCount);
                var pikminUnit = Instantiate(_redPikminPrefab, GetOffsetPositionGrounded(LeaderTransform, formationPositionOffset), Quaternion.identity);
                pikminUnit.Init(this, Raycaster, formationPositionOffset, PikminState.Idle, true);
                InSquadPikminUnits.Add(pikminUnit);
                pikminUnitCount++;
            }
            for(int i = 0; i < _yellowPikminUnitAmount; i++)
            {
                Vector3 formationPositionOffset = GetFormationPositionOffset(pikminUnitCount);
                var pikminUnit = Instantiate(_yellowPikminPrefab, GetOffsetPositionGrounded(LeaderTransform, formationPositionOffset), Quaternion.identity);
                pikminUnit.Init(this, Raycaster, formationPositionOffset, PikminState.Idle, true);
                InSquadPikminUnits.Add(pikminUnit);
                pikminUnitCount++;
            }

            // instantiate pikmin seeds
            for(int i = 0; i < 3; i++)
            {
                Vector3 formationPositionOffset = GetFormationPositionOffset(pikminUnitCount);
                var pikminUnit = Instantiate(_yellowPikminPrefab, GetOffsetPositionGrounded(LeaderTransform, formationPositionOffset), Quaternion.identity);
                pikminUnit.Init(this, Raycaster, formationPositionOffset, PikminState.WaitForPluck, false);
                WildPikminUnits.Add(pikminUnit);
                pikminUnitCount++;
            }
        }

        void Update()
        {
            // Handle input for Launch Projectile
            if(!_debugMode)
            {
                if(_rightHand.IsTracked)
                {
                    Raycaster.PointerPose = new Pose(_rightHandRayPointer.position, _rightHandRayPointer.rotation);

                    // Check pinching status
                    // TODO check if finger is touching pikmin
                    // 1. if touching a pikmin seed (waiting to be plucked), pikmin will be plucked
                    // 2. if touching an active pikmin, pikmin will be grabbed
                    // 3. if it's not touching, pikmin throw a projectile
                    IsRightIndexFingerPinching = _rightHand.GetFingerIsPinching(OVRHand.HandFinger.Index);
                    if(IsRightIndexFingerPinching && !_unitToLaunch)
                    {
                        HandlePrelaunch();
                    }
                    else if (!IsRightIndexFingerPinching && _unitToLaunch != null && _unitToLaunch.TryGetComponent<PikminUnit>(out PikminUnit unit))
                    {
                        HandleLaunch(unit);
                    }
                }
            }
            else
            {
                // Debug helper
                IsRightIndexFingerPinching = Input.GetKey(KeyCode.B);
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

                Pose pointerPose = Raycaster.PointerPose;
                if(Mathf.Abs(Input.GetAxis("Horizontal")) > 0)
                {
                    pointerPose.position += new Vector3(Input.GetAxis("Horizontal") * 2f * Time.deltaTime, 0, 0);
                }
                if(Mathf.Abs(Input.GetAxis("Vertical")) > 0)
                {
                    pointerPose.position += new Vector3(0, 0, Input.GetAxis("Vertical") * 2f * Time.deltaTime);
                }
                Vector3 screenPoint = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1.0f));
                pointerPose.rotation = Quaternion.LookRotation(screenPoint - pointerPose.position, Vector3.up);
                Raycaster.PointerPose = pointerPose;
            }

            // Update logic for Formation
            float step = Time.deltaTime * PikminWalkSpeed;
            float angularStep = Time.deltaTime * PikminTurnSpeed;
            GroundedLeaderPosition = new Vector3(LeaderTransform.position.x, 0, LeaderTransform.position.z);
            LeaderAwayFromPack = Vector3.Distance(LeaderGhost.transform.position, GroundedLeaderPosition) > _leaderFromPackDistanceThreshold;
            LeaderMoveEnough = Vector3.Distance(GroundedLeaderPosition, new Vector3(LastGroundedLeaderPosition.x, 0, LastGroundedLeaderPosition.z)) > _leaderMoveDistanceThreshold;

            if(LeaderAwayFromPack)
            {
                Vector3 newGhostPosition = GetOffsetPositionGrounded(LeaderTransform, Vector3.zero);
                LeaderGhostMoveDifference = newGhostPosition - LeaderGhost.transform.position;
                LeaderGhost.transform.position = Vector3.MoveTowards(LeaderGhost.transform.position, newGhostPosition, step);
                
                // Quaternion leaderYRotation = Quaternion.identity;
                // leaderYRotation.eulerAngles = new Vector3(0, LeaderTransform.rotation.eulerAngles.y, 0);
                // LeaderGhost.transform.rotation = Quaternion.RotateTowards(LeaderGhost.transform.rotation, leaderYRotation, angularStep);
            } 

            foreach(PikminUnit unit in InSquadPikminUnits)
            {
                unit.DetermineFormationState();
            }

            for(int i = WildPikminUnits.Count - 1; i >= 0; i--)
            {
                var unit = WildPikminUnits[i];
                if(unit.InSquad)
                {
                    InSquadPikminUnits.Add(unit);
                    WildPikminUnits.Remove(unit);
                }
            }
            
            LastGroundedLeaderPosition = GroundedLeaderPosition;
        }

        void HandlePrelaunch()
        {
            if(InSquadPikminUnits.Count > 0)
            {
                _unitToLaunch = InSquadPikminUnits[InSquadPikminUnits.Count-1];
            }

            if(_unitToLaunch != null)
            {
                _unitToLaunch.SetState(PikminState.PreLaunch);
                Raycaster.SetState(RaycastState.PreLaunch);
            }
        }

        void HandleLaunch(PikminUnit unit)
        {
            unit.SetState(PikminState.InLaunch);
            InSquadPikminUnits.Remove(unit);
            WildPikminUnits.Add(unit);
            _unitToLaunch = null;
            Raycaster.SetState(RaycastState.InLaunch);
        }

        public Vector3 GetOffsetPositionGrounded(Transform refTransform, Vector3 positionOffset)
        {
            Vector3 newOffset = refTransform.forward * positionOffset.z + refTransform.right * positionOffset.x;
            Vector3 newPosition = refTransform.position + newOffset;
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