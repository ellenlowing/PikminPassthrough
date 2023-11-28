using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PikminPack
{
    public enum PikminState
    {
        Idle,
        FollowLeader,
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

        // Private variables
        private PikminUnit _unitToLaunch;
        private bool _isRightIndexFingerPinching;

        // Component References

        // Public members
        public static PikminUnitManager Instance;
        public List<PikminUnit> InSquadPikminUnits { get; private set; } = new List<PikminUnit>();
        public List<PikminUnit> WildPikminUnits { get; private set; } = new List<PikminUnit>();
        public Raycaster Raycaster;
        
        void Awake() => Instance = this;
        
        void Start()
        {   
            for(int i = 0; i < _pikminUnitAmount; i++)
            {
                var pikminUnit = Instantiate(_pikminPrefab);
                pikminUnit.Init(this, Raycaster);
                InSquadPikminUnits.Add(pikminUnit);
            }
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
    }
}