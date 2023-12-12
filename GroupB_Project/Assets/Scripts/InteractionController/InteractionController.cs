using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InteractionController : MonoBehaviour
{
    private void Update()
    {
        ObjectBehaviour();
    }
    protected virtual void ObjectBehaviour(){ }
}
