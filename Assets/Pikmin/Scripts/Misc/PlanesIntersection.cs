using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanesIntersection : MonoBehaviour
{
    public GameObject plane0;
    public GameObject plane1;

    public int positionCount = 100;
    public float lineResolution = 0.1f;

    private LineRenderer visualizer;

    void Start()
    {
        visualizer = GetComponent<LineRenderer>();
    }

    void Update()
    {
        Vector3 linePoint;
        Vector3 lineVec;

        planePlaneIntersection(out linePoint, out lineVec, plane0, plane1);

        visualizer.positionCount = positionCount;
        for(int i = 0; i < positionCount; i++)
        {
            float increment = (float)i * lineResolution;
            Vector3 position = linePoint + increment * lineVec;
            visualizer.SetPosition(i, position);
        }
    }

    void planePlaneIntersection(out Vector3 linePoint, out Vector3 lineVec, GameObject plane1, GameObject plane2){
 
        linePoint = Vector3.zero;
        lineVec = Vector3.zero;
    
        //Get the normals of the planes.
        Vector3 plane1Normal = plane1.transform.up;
        Vector3 plane2Normal = plane2.transform.up;
    
        //We can get the direction of the line of intersection of the two planes by calculating the
        //cross product of the normals of the two planes. Note that this is just a direction and the line
        //is not fixed in space yet.
        lineVec = Vector3.Cross(plane1Normal, plane2Normal);
    
        //Next is to calculate a point on the line to fix it's position. This is done by finding a vector from
        //the plane2 location, moving parallel to it's plane, and intersecting plane1. To prevent rounding
        //errors, this vector also has to be perpendicular to lineDirection. To get this vector, calculate
        //the cross product of the normal of plane2 and the lineDirection.      
        Vector3 ldir = Vector3.Cross(plane2Normal, lineVec);      
    
        float numerator = Vector3.Dot(plane1Normal, ldir);
    
        //Prevent divide by zero.
        if(Mathf.Abs(numerator) > 0.000001f){
        
            Vector3 plane1ToPlane2 = plane1.transform.position - plane2.transform.position;
            float t = Vector3.Dot(plane1Normal, plane1ToPlane2) / numerator;
            linePoint = plane2.transform.position + t * ldir;
        }
    }
}
