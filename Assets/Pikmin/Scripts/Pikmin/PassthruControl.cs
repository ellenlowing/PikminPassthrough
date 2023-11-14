using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace Pikmin
{
    public class PassthruControl : MonoBehaviour
    {
        public Camera sceneCamera;

        public OVRSceneManager sceneManager;

        private PikminStateManager stateManager;

        void Start()
        {
            stateManager = GetComponent<PikminStateManager>();

            Vector3 inFrontOfUser = sceneCamera.transform.position + sceneCamera.transform.forward * 1.0f;
            transform.position = inFrontOfUser;

            sceneManager.SceneModelLoadedSuccessfully += PlaceOnGround;
            sceneManager.NoSceneModelToLoad += OnNoSceneModelToLoad;
        }

        void Update()
        {
            Vector2 secondaryThumbstick = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);
            stateManager.joystickInput = secondaryThumbstick;
        }

        void OnNoSceneModelToLoad()
        {
            Debug.Log("Requesting screen capture");
            sceneManager.RequestSceneCapture();
        }

        void PlaceOnGround()
        {
            Debug.Log("PIKMIN: Firing PlaceOnGround");
            GameObject floor = GameObject.FindGameObjectWithTag("Floor");
            if(floor) Debug.Log("PIKMIN: Floor found!");
            Vector3 inFrontOfUser = sceneCamera.transform.position + sceneCamera.transform.forward * 0.5f;
            inFrontOfUser.y = floor.transform.position.y;
            transform.position = inFrontOfUser;
            transform.rotation = Quaternion.LookRotation(-sceneCamera.transform.forward, floor.transform.forward);
        }
    }
}