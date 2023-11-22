using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reticle : MonoBehaviour
{
    public Transform launchTransform;
    void Update()
    {
        RaycastHit raycastHit;
        Physics.Raycast(launchTransform.position, launchTransform.forward, out raycastHit);

        transform.position = raycastHit.point;
        transform.rotation = Quaternion.LookRotation(raycastHit.normal, Vector3.up);
    }
}
