using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
using UnityEngine;

namespace PikminPack
{
    public enum RaycastState
    {
        Idle,
        PreLaunch,
        InLaunch
    }

    public class Raycaster : MonoBehaviour
    {
        [HideInInspector] public RaycastState CurrentState = RaycastState.Idle;
        [HideInInspector] public float V0;
        [HideInInspector] public float LaunchDuration;
        [HideInInspector] public float LaunchAngle;
        [HideInInspector] public float LaunchHeight;
        [HideInInspector] public Vector3 GroundDirectionNorm;
        [HideInInspector] public Vector3 LaunchPosition;
        [HideInInspector] public RaycastHit RaycastHit;
        [HideInInspector] public Pose PointerPose;

        [SerializeField] private TubeRenderer _tubeRenderer;
        [SerializeField] private float _tubeTrailLength = 2f;
        [SerializeField] private float _tubeTrailStep = 0.01f;
        [SerializeField] private Gradient _prelaunchGradient;
        [SerializeField] private Gradient _inlaunchGradient;
        private TubePoint [] _arcPoints;


        void Start()
        {
            CurrentState = RaycastState.Idle;
        }

        void Update()
        {
            transform.SetPositionAndRotation(PointerPose.position, PointerPose.rotation);

            switch(CurrentState)
            {
                case RaycastState.Idle:
                    UpdateIdleState();
                    break;
                
                case RaycastState.PreLaunch:
                    UpdatePreLaunchState();
                    break;

                case RaycastState.InLaunch:
                    UpdateInLaunchState();
                    break;
            }            
        }

        public void SetState(RaycastState state)
        {
            if(state != CurrentState)
            {
                switch(state)
                {
                    case RaycastState.Idle:
                        EnterIdleState();
                        break;

                    case RaycastState.PreLaunch:
                        EnterPreLaunchState();
                        break;

                    case RaycastState.InLaunch:
                        EnterInLaunchState();
                        break;
                }

                CurrentState = state;
            }
        }

        private void EnterIdleState()
        {
            _tubeRenderer.Hide();
        }

        private void EnterPreLaunchState()
        {
            _tubeRenderer.Gradient = _prelaunchGradient;
        } 

        private void EnterInLaunchState()
        {
            _tubeRenderer.Gradient = _inlaunchGradient;
            StartCoroutine(InLaunchProjectileTrail());
        }

        private void UpdateIdleState()
        {

        }

        private void UpdatePreLaunchState()
        {
            GetRaycastHit();
            LaunchPosition = transform.position;
            ProjectileLibrary.CalculatePathFromLaunchToTarget(RaycastHit.point, LaunchPosition, out GroundDirectionNorm, out LaunchHeight, out V0, out LaunchDuration, out LaunchAngle);
            Vector3 [] projectilePositions = ProjectileLibrary.GetProjectilePositions(LaunchPosition, GroundDirectionNorm, V0, LaunchDuration, LaunchAngle);
            DrawProjectile(projectilePositions);
        }

        private void UpdateInLaunchState()
        {

        }

        public void GetRaycastHit()
        {
            Physics.Raycast(transform.position, transform.forward, out RaycastHit);
        }

        public IEnumerator InLaunchProjectileTrail()
        {
            _tubeRenderer.StartFadeThresold = -_tubeTrailLength;
            _tubeRenderer.EndFadeThresold = _tubeRenderer.TotalLength;
            while(_tubeRenderer.StartFadeThresold < (_tubeRenderer.TotalLength - _tubeTrailLength))
            {
                _tubeRenderer.StartFadeThresold += _tubeTrailStep;
                _tubeRenderer.EndFadeThresold -= _tubeTrailStep;
                _tubeRenderer.RenderTube(_arcPoints, Space.World);
                yield return null;
            }
        }

        public void DrawProjectile(Vector3 [] projectilePositions)
        {
            UpdateProjectilePoints(projectilePositions);
            _tubeRenderer.StartFadeThresold = 0f;
            _tubeRenderer.EndFadeThresold = 0f;
            _tubeRenderer.RenderTube(_arcPoints, Space.World);
        }

        private void UpdateProjectilePoints(Vector3 [] projectilePositions)
        {
            _arcPoints = new TubePoint[projectilePositions.Length-1];
            float totalDistance = 0f;
            for(int i = 0; i < _arcPoints.Length; i++)
            {
                Vector3 position = projectilePositions[i+1];
                Vector3 difference = position - projectilePositions[i];
                totalDistance += difference.magnitude;

                _arcPoints[i].position = position;
                _arcPoints[i].rotation = Quaternion.LookRotation(difference.normalized);
            }

            for(int i = 1; i < _arcPoints.Length; i++)
            {
                float segmentLength = (_arcPoints[i - 1].position - _arcPoints[i].position).magnitude;
                _arcPoints[i].relativeLength = _arcPoints[i - 1].relativeLength + (segmentLength / totalDistance);
            }
        }

    }

}