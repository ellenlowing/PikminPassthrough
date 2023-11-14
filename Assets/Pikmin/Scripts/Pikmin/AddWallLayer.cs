using System.Collections;
using System.Collections.Generic;
using Pikmin;
using UnityEngine;

public class AddWallLayer : MonoBehaviour
{
    public PikminStateManager manager;

    void Start()
    {
        gameObject.layer = LayerMask.NameToLayer("Wall");
    }
}
