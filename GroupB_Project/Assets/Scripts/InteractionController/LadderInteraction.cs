using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LadderInteraction : InteractionController
{
    public float offset = 0.3f;

    public Vector3 LadderOffset()
    {
        return transform.position - transform.forward * offset;
    }
}
