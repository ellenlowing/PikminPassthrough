using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceInFront : MonoBehaviour
{
    public Camera sceneCamera;
    public bool activated;

    void Start()
    {
    }

    void Update()
    {
        transform.position = sceneCamera.transform.position + sceneCamera.transform.forward * 1f;
    }

    public void ActivateScale()
    {
        transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        Debug.Log("Activated");
    }

    public void DectivateScale()
    {
        transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        Debug.Log("Deactivated");
    }
}
