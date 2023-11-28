using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class PikminLauncher : MonoBehaviour
{
    public GameObject pikmin;

    public float speed = 2f;

    private List<Vector3> projectilePositions;
    private LineRenderer line;
    public float v0, time, angle, height;
    public Vector3 groundDirectionNorm;

    void Start()
    {
        line = GetComponent<LineRenderer>();
    }

    void Update()
    {   
        if(OVRInput.Get(OVRInput.RawButton.RIndexTrigger) || Input.GetKey(KeyCode.Space))
        {
            DrawProjectile();
            pikmin.transform.position = transform.position;
            pikmin.transform.rotation = transform.rotation;
        }

        if(OVRInput.GetUp(OVRInput.RawButton.RIndexTrigger) ||  Input.GetKeyUp(KeyCode.Space))
        {
            // Throw pikmin
            StopAllCoroutines();
            StartCoroutine(ProjectileMovement(groundDirectionNorm, height, v0, angle, time));
            Debug.Log("Up secondary trigger");

            // Clear line
            ClearLinePositions();
        }
    }

    void ClearLinePositions()
    {
        line.positionCount = 0;
    }

    IEnumerator ProjectileMovement(Vector3 direction, float height, float v0, float angle, float time)
    {
        float t = 0;
        while(t < time)
        {
            pikmin.transform.position = ProjectileLibrary.GetPositionAtTime(transform.position, direction, v0, angle, t);
            Vector3 nextPosition = ProjectileLibrary.GetPositionAtTime(transform.position, direction, v0, angle, t + Time.deltaTime * speed);
            pikmin.transform.rotation = Quaternion.LookRotation(nextPosition - transform.position, Vector3.up);
            t += Time.deltaTime * speed;
            yield return null;
        }
    }

    void DrawProjectile()
    {
        RaycastHit raycastHit;
        Physics.Raycast(transform.position, transform.forward, out raycastHit);

        projectilePositions = GetProjectilePositions(transform.position, raycastHit.point);
        line.positionCount = projectilePositions.Count;
        line.SetPositions(projectilePositions.ToArray());
    }

    List<Vector3> GetProjectilePositions(Vector3 launchPosition, Vector3 targetPosition)
    {
        ProjectileLibrary.CalculatePathFromLaunchToTarget(targetPosition, launchPosition, out groundDirectionNorm, out height, out v0, out time, out angle);
        
        List<Vector3> positions = new List<Vector3>();
        for(float t = 0; t < time; t += 0.02f)
        {
            positions.Add(ProjectileLibrary.GetPositionAtTime(launchPosition, groundDirectionNorm, v0, angle, t));
        }

        return positions;
    }
}
