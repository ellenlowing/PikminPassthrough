using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] public Transform launchPoint;
    [SerializeField] public Transform targetTransform;
    public float speed = 0.3f;
    Vector3 launchPos;

    public void Launch(Transform _targetTransform, Transform _launchPoint)
    {
        targetTransform = _targetTransform;
        launchPoint = _launchPoint;
        launchPos = launchPoint.position;
        float v0;
        float time;
        float angle;
        float height;
        Vector3 groundDirectionNorm;

        ProjectileLibrary.CalculatePathFromLaunchToTarget(targetTransform.position, launchPos, out groundDirectionNorm, out height, out v0, out time, out angle);
        StopAllCoroutines();
        StartCoroutine(ProjectileMovement(groundDirectionNorm, height, v0, angle, time));
    }

    IEnumerator ProjectileMovement(Vector3 direction, float height, float v0, float angle, float time)
    {
        float t = 0;
        while(t < time)
        {
            transform.position = ProjectileLibrary.GetPositionAtTime(launchPos, direction, v0, angle, t);
            Vector3 nextPosition = ProjectileLibrary.GetPositionAtTime(launchPos, direction, v0, angle, t + Time.deltaTime * speed);
            transform.rotation = Quaternion.LookRotation(nextPosition - transform.position, Vector3.up);
            t += Time.deltaTime * speed;
            yield return null;
        }
    }
}
