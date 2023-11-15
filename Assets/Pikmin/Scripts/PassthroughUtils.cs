using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PassthroughUtils
{
    public static bool CheckSurfaceHit(Vector3 origin, Vector3 direction, float sphereCastRadius, float detectionLength, out RaycastHit frontSurfaceHit, out float surfaceLookAngle, int surfaceLayer = Physics.DefaultRaycastLayers)
    {
        bool surfaceFront = Physics.SphereCast(origin, sphereCastRadius, direction, out frontSurfaceHit, detectionLength, surfaceLayer);
        surfaceLookAngle = Vector3.Angle(direction, -frontSurfaceHit.normal);
        return surfaceFront;
    }
}
