using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugInteraction : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnHover()
    {
        Debug.Log("Hovering");
    }

    public void OnUnhover()
    {
        Debug.Log("Unhovering");
    }

    public void OnSelect()
    {
        Debug.Log("Selecting");
    }

    public void OnUnselect()
    {
        Debug.Log("Unselecting");
    }
}
