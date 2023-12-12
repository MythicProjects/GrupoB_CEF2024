using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class PlayerInteraction : MonoBehaviour
{
    private PlayerController controller;
    private PlayerLocomotion locomotion;

    [Header("Interaction Settings")]
    public LayerMask interactLayer;

    public void GetInteractionComponents()
    {
        controller = GetComponent<PlayerController>();
        locomotion = GetComponent<PlayerLocomotion>();
    }

    //OnlyOneShot
    public void SetCharacterInteraction()
    {
        RaycastHit hit;
        Vector3 interactionPoint = transform.position + Vector3.up;
        Vector3 interactionForward = interactionPoint + transform.forward;
        float interactionDistance = 1;

        bool hitInteractable = Physics.Raycast(interactionPoint, interactionForward, out hit, interactionDistance, interactLayer);
        Debug.DrawRay(interactionPoint, interactionForward);

        if (!hitInteractable)
            return;


        GrabInteraction grabObj;
        LadderInteraction ladder;

        if (grabObj = hit.collider.GetComponent<GrabInteraction>())
        {
            Debug.Log("grab");

            grabObj.ObjectOffset(transform);
            grabObj.ActivateObjectMovement(controller.isInteracting);

            controller.isGrabbingObject = controller.isInteracting; //Debende de isInteracting

        }
        else if (ladder = hit.collider.GetComponent<LadderInteraction>())
        {
            locomotion.RepositionPlayer(ladder.LadderOffset());
            controller.isOnLadder = controller.isInteracting; //Debende de isInteracting??
        }

        locomotion.SetInteractionDirection(hit.transform.position);
    }
    public void UpdateInteraction()
    {

        if (controller.isOnLadder)
        {
            if (controller.isGrounded && controller.vAxisInput < 0)
            {
                controller.isOnLadder = false;
                controller.isInteracting = false;
            }
        }
        else if (controller.isGrabbingObject)
        {
            if (!controller.isGrounded)
            {
                controller.isGrabbingObject = false;
                controller.isInteracting = false;
            }
        }
    }
}
