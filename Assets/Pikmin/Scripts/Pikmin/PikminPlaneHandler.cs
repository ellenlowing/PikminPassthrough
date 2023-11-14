using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(OVRSceneAnchor))]
public class PikminPlaneHandler : MonoBehaviour
{
    private OVRSceneAnchor _sceneAnchor;
    private OVRSemanticClassification _classification;

    void Start()
    {
        _sceneAnchor = GetComponent<OVRSceneAnchor>();
        _classification = GetComponent<OVRSemanticClassification>();
        ClassifyAndTag();
    }

    void ClassifyAndTag()
    {
        var plane = _sceneAnchor.GetComponent<OVRScenePlane>();
        var dimensions = Vector3.one;

        if(_classification && plane)
        {
            dimensions = plane.Dimensions;
            dimensions.z = 1;

            if(_classification.Contains(OVRSceneManager.Classification.Floor))
            {
                gameObject.layer = LayerMask.NameToLayer("Floor");
                gameObject.transform.GetChild(0).gameObject.layer = LayerMask.NameToLayer("Floor");
            }
            else if (_classification.Contains(OVRSceneManager.Classification.Ceiling))
            {
                gameObject.tag = "Ceiling";
            }
            else if (_classification.Contains(OVRSceneManager.Classification.WallFace))
            {
                gameObject.layer = LayerMask.NameToLayer("Wall");
                gameObject.transform.GetChild(0).gameObject.layer = LayerMask.NameToLayer("Wall");
            }
        }
    }
}