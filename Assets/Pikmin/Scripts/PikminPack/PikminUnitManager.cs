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
        [SerializeField] private PikminState _state;
        [SerializeField] private OVRInput.RawButton _launchButton;

        // Variables to store temporary information
        private PikminUnit _unitToLaunch;

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
            if(OVRInput.Get(_launchButton) || Input.GetKey(KeyCode.Space))
            {
                _unitToLaunch = PikminUnits[_pikminUnitAmount - 1];
                _unitToLaunch.SetState(PikminState.PreLaunch);
                Raycaster.Prelaunch = true;
            }
            if(OVRInput.GetUp(_launchButton) || Input.GetKeyUp(KeyCode.Space))
            {
                if(_unitToLaunch)
                {
                    // pass projectile params to unit
                    _unitToLaunch.SetState(PikminState.InLaunch);
                    _unitToLaunch = null;
                    Raycaster.Prelaunch = false;
                }
            }
        }
    }

}