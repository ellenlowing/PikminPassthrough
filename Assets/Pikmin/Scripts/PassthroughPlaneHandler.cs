using System.Collections;
using System.Collections.Generic;
using PikminPack;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(OVRSceneAnchor))]
public class PassthroughPlaneHandler : MonoBehaviour
{
    private OVRSceneAnchor _sceneAnchor;
    private OVRSemanticClassification _classification;
    private PikminUnitManager _pikminUnitManager;

    void Start()
    {
        _sceneAnchor = GetComponent<OVRSceneAnchor>();
        _classification = GetComponent<OVRSemanticClassification>();
        _pikminUnitManager = PikminUnitManager.Instance;
        ClassifyAndTag();
        _pikminUnitManager.enabled = true;
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
                gameObject.tag = "Floor";
                gameObject.transform.GetChild(0).tag = "Floor";
                _pikminUnitManager.FloorLevel = gameObject.transform.position.y;
                Debug.Log("Found Floor " + gameObject.transform.position.y);
            }
            else if (_classification.Contains(OVRSceneManager.Classification.Ceiling))
            {
            }
            else if (_classification.Contains(OVRSceneManager.Classification.WallFace))
            {
                gameObject.layer = LayerMask.NameToLayer("Wall");
                gameObject.transform.GetChild(0).gameObject.layer = LayerMask.NameToLayer("Wall");
                gameObject.tag = "Wall";
                gameObject.transform.GetChild(0).tag = "Wall";
                Debug.Log("Found wall");

            }
        }
    }
}
