using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Raycaster : MonoBehaviour
{
    private LineRenderer _lineRenderer;
    public bool Prelaunch = false;

    public float V0;
    public float LaunchDuration;
    public float LaunchAngle;
    public float LaunchHeight;
    public Vector3 GroundDirectionNorm;
    public Vector3 LaunchPosition;

    void Start()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        Prelaunch = false;
    }

    void Update()
    {
        // Calculate path
        if(Prelaunch)
        {
            LaunchPosition = transform.position;
            ProjectileLibrary.CalculatePathFromLaunchToTarget(GetRaycastHitpoint(), LaunchPosition, out GroundDirectionNorm, out LaunchHeight, out V0, out LaunchDuration, out LaunchAngle);
            Vector3 [] projectilePositions = ProjectileLibrary.GetProjectilePositions(LaunchPosition, GroundDirectionNorm, V0, LaunchDuration, LaunchAngle);
            DrawProjectile(projectilePositions);
        }
        
    }

    public Vector3 GetRaycastHitpoint()
    {
        RaycastHit raycastHit;
        Physics.Raycast(transform.position, transform.forward, out raycastHit);

        return raycastHit.point;
    }

    public void DrawProjectile(Vector3 [] projectilePositions)
    {
        _lineRenderer.positionCount = projectilePositions.Length;
        _lineRenderer.SetPositions(projectilePositions);
    }
}
