using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlLeader : MonoBehaviour
{
    [SerializeField] private OVRInput.RawButton _launchButton;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // if(Mathf.Abs(Input.GetAxis("Horizontal")) > 0)
        // {
        //     transform.position += new Vector3(Input.GetAxis("Horizontal") * 2f * Time.deltaTime, 0, 0);
        // }
        
        // if(Mathf.Abs(Input.GetAxis("Vertical")) > 0)
        // {
        //     transform.position += new Vector3(0, 0, Input.GetAxis("Vertical") * 2f * Time.deltaTime);
        // }
        // Vector3 screenPoint = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1.0f));
        // transform.rotation = Quaternion.LookRotation(screenPoint - transform.position, Vector3.up);

        if(OVRInput.Get(_launchButton))
        {
            Debug.Log("Pressed");
        }
    }
}
