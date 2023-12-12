using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabInteraction : InteractionController
{
    private bool interactionActive;
    public Vector3 offsetWithPlayer;
    public Transform playerTransform;


    private void Update()
    {
        if (interactionActive)
        {
            transform.position = playerTransform.position + offsetWithPlayer;
        }
    }
    public void ObjectOffset(Transform player)
    {
        playerTransform = player;
        offsetWithPlayer = transform.position - playerTransform.position;
    }

    public void ActivateObjectMovement(bool setState)
    {
        interactionActive = setState;
    }
}
