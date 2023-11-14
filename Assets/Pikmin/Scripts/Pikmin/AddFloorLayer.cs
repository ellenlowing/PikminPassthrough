using System.Collections;
using System.Collections.Generic;
using Pikmin;
using UnityEngine;

public class AddFloorLayer : MonoBehaviour
{
    public PikminStateManager manager;

    void Start()
    {
        gameObject.layer = LayerMask.NameToLayer("Floor");
    }
}
