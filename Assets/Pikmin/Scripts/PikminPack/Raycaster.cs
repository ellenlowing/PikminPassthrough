using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Raycaster : MonoBehaviour
{
    [HideInInspector] public bool Prelaunch = false;
    [HideInInspector] public float V0;
    [HideInInspector] public float LaunchDuration;
    [HideInInspector] public float LaunchAngle;
    [HideInInspector] public float LaunchHeight;
    [HideInInspector] public Vector3 GroundDirectionNorm;
    [HideInInspector] public Vector3 LaunchPosition;
    [HideInInspector] public RaycastHit RaycastHit;

    [SerializeField] private TubeRenderer _tubeRenderer;
    private TubePoint [] _arcPoints;

    public Pose PointerPose;

    void Start()
    {
        Prelaunch = false;
    }

    void Update()
    {
        transform.SetPositionAndRotation(PointerPose.position, PointerPose.rotation);

        // Calculate path
        if(Prelaunch)
        {
            GetRaycastHit();
            LaunchPosition = transform.position;
            ProjectileLibrary.CalculatePathFromLaunchToTarget(RaycastHit.point, LaunchPosition, out GroundDirectionNorm, out LaunchHeight, out V0, out LaunchDuration, out LaunchAngle);
            Vector3 [] projectilePositions = ProjectileLibrary.GetProjectilePositions(LaunchPosition, GroundDirectionNorm, V0, LaunchDuration, LaunchAngle);
            DrawProjectile(projectilePositions);
        }
        else
        {
            _tubeRenderer.Hide();
        }
        
    }

    public void GetRaycastHit()
    {
        Physics.Raycast(transform.position, transform.forward, out RaycastHit);
    }

    public void DrawProjectile(Vector3 [] projectilePositions)
    {
        UpdateProjectilePoints(projectilePositions);
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
