using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceInFront : MonoBehaviour
{
    public Camera sceneCamera;

    void Start()
    {
        transform.position = sceneCamera.transform.position + sceneCamera.transform.forward * 1f;
    }

    void Update()
    {
        
    }
}
