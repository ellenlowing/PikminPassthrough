using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class DrawProjectile : MonoBehaviour
{
    [SerializeField] public Transform launchTransform;
    [SerializeField] public Transform targetTransform;

    private List<Vector3> projectilePositions;
    private LineRenderer line;

    void Start()
    {
        line = GetComponent<LineRenderer>();
    }

    void Update()
    {
        RaycastHit raycastHit;
        Physics.Raycast(launchTransform.position, launchTransform.forward, out raycastHit);

        projectilePositions = GetProjectilePositions(launchTransform.position, raycastHit.point);
        line.positionCount = projectilePositions.Count;
        line.SetPositions(projectilePositions.ToArray());
    }

    List<Vector3> GetProjectilePositions(Vector3 launchPosition, Vector3 targetPosition)
    {
        float v0, time, angle, height;
        Vector3 groundDirectionNorm;
        ProjectileLibrary.CalculatePathFromLaunchToTarget(targetPosition, launchPosition, out groundDirectionNorm, out height, out v0, out time, out angle);
        
        List<Vector3> positions = new List<Vector3>();
        for(float t = 0; t < time; t += 0.02f)
        {
            positions.Add(ProjectileLibrary.GetPositionAtTime(launchPosition, groundDirectionNorm, v0, angle, t));
        }

        return positions;
    }
}
