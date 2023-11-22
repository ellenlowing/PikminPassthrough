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
        public List<PikminUnit> PikminUnits { get; private set; } = new List<PikminUnit>();
        public Raycaster Raycaster;
        
        void Awake() => Instance = this;
        
        void Start()
        {   
            for(int i = 0; i < _pikminUnitAmount; i++)
            {
                var pikminUnit = Instantiate(_pikminPrefab);
                pikminUnit.Init(this, Raycaster);
                PikminUnits.Add(pikminUnit);
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
                    if(_isRightIndexFingerPinching)
                    {
                        HandlePrelaunch();
                    }
                    else if (!_isRightIndexFingerPinching && _unitToLaunch)
                    {
                        HandleLaunch();
                    }
                }
            }
            else
            {
                // Debug helper
                if(Input.GetKey(KeyCode.Space))
                {
                    HandlePrelaunch();
                }
                if(Input.GetKeyUp(KeyCode.Space))
                {
                    if(_unitToLaunch)
                    {
                        HandleLaunch();
                    }
                }
            }
            
        }

        void HandlePrelaunch()
        {
            _unitToLaunch = PikminUnits[_pikminUnitAmount - 1];
            _unitToLaunch.SetState(PikminState.PreLaunch);
            Raycaster.Prelaunch = true;
        }

        void HandleLaunch()
        {
            _unitToLaunch.SetState(PikminState.InLaunch);
            _unitToLaunch = null;
            Raycaster.Prelaunch = false;
        }
    }
}