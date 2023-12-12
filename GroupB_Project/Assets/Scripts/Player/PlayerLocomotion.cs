using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLocomotion : MonoBehaviour
{
    Rigidbody rb;

    PlayerController controller;

    [Header("Camera Settings")]
    private Transform cameraObject;

    [Header("Horizontal Movement Settings")]
    private Vector3 moveDirection;
    [SerializeField] float crouchSpeed = 2;
    [SerializeField] float walkSpeed = 4;
    [SerializeField] float sprintSpeed = 7;
    private float actualSpeed;
    [SerializeField] float crouchRotationSpeed = 10;
    [SerializeField] float normalRotationSpeed = 20;
    private float actualRotationSpeed;

    [Header("Vertical Movement Settings")]
    public float jumpForce = 8;
    public float fallingSpeed = -30;
    private float verticalSpeed;

    [Header("Interaction Settings")]
    private Vector3 interactionDirection;
    [Header("Grabbing Movement Settings")]
    [SerializeField] float grabObjectSpeed = 6;

    [Header("Ladder Movement Settings")]
    [SerializeField] float ladderSpeed = 6;
    private Vector3 ladderPosition;

    [Header("Ground Collision Settings")]
    public LayerMask groundLayer;
    [SerializeField] float groundCheckRadius = 0.2f;

    [Header("Step Collision Settings")]
    [SerializeField] float stepRayDistance = 1.2f;
    [SerializeField] float maxStepHeight = 0.3f;
    [SerializeField] float stepUpSpeed = 10;

    [Header("Wall Collision Settings")]
    [SerializeField] float wallRayDistance = 0.6f;
    [SerializeField] float wallRaysAngle = 0.2f;

    [Header("Climb Settings")]
    [SerializeField] float climbHeight = 1;
    [SerializeField] float climblRayDistance = 1;
    Vector3 climbTargetPoint;

    private float climbAnimationTimer; //Change for animation
    public Transform targetClimbTest;//Delete


    public void GetLocomotionComponents()
    {
        rb = GetComponent<Rigidbody>();
        controller = GetComponent<PlayerController>();

    }
    public void SetLocomotionComponents()
    {
        cameraObject = Camera.main.transform;
    }

    public void HandleMovement(float delta)
    {
        //Set Inputs
        float xAxis = controller.hAxisInput;
        float zAxis = controller.vAxisInput;
        float inputValue = controller.axisInputAmount;

        //Handle direction
        Vector3 targetDirection = cameraObject.forward * zAxis + cameraObject.right * xAxis;

        if (!controller.isGrabbingObject) moveDirection = Vector3.Slerp(moveDirection, targetDirection, delta * actualRotationSpeed);
        else moveDirection = targetDirection;

        moveDirection.Normalize();
        moveDirection.y = 0;

        //Get X/Z and Y velocity
        Vector3 verticalVelocity;
        Vector3 horizontalVelocity;

        if (controller.isOnLadder)
        {
            verticalVelocity = Vector3.up * ladderSpeed * zAxis;
            horizontalVelocity = Vector3.zero;
        }
        else if (controller.isGrabbingObject)
        {
            actualSpeed = grabObjectSpeed;

            verticalVelocity = Vector3.up * verticalSpeed;
            horizontalVelocity = moveDirection * actualSpeed * inputValue;
        }
        else 
        {
            if (controller.crouchInput && controller.isGrounded)
            {
                actualSpeed = crouchSpeed;
            }
            else if (controller.runInput && !controller.crouchInput)
            {
                actualSpeed = sprintSpeed;
            }
            else
            {
                actualSpeed = walkSpeed;
            }

            if (controller.isWallCollision) actualSpeed = 0;

            verticalVelocity = Vector3.up * verticalSpeed;
            horizontalVelocity = moveDirection * actualSpeed * inputValue;
        }

        //Apply all velocity
        Vector3 movementVelocity = horizontalVelocity + verticalVelocity;
        rb.position += movementVelocity * delta;
    }
    public void SetInteractionDirection(Vector3 newInteractionObject)
    {
        interactionDirection = newInteractionObject - transform.position;
    }
    public void RepositionPlayer(Vector3 newPosition)
    {
        transform.position = newPosition;
    }
    public void HandleRotation(float delta)
    {
        //Control rotation speed
        if (controller.crouchInput && controller.isGrounded) actualRotationSpeed = crouchRotationSpeed;
        else actualRotationSpeed = normalRotationSpeed;

        //Control direction
        Vector3 targetDir;
        if (controller.isInteracting )
        {
            targetDir = interactionDirection;
        }
        else
        {
            targetDir = moveDirection;
            if (targetDir == Vector3.zero) targetDir = transform.forward;

        }

        Quaternion lookRotation = Quaternion.LookRotation(targetDir);
        Quaternion targetRotation = Quaternion.Slerp(transform.rotation, lookRotation, actualRotationSpeed * delta);

        rb.rotation = targetRotation;
    }

    public void HandleGravity(float delta)
    {

        if (controller.isGrounded && verticalSpeed < 0.1f)
        {
            verticalSpeed = 0;
        }
        else if (!controller.isGrounded)
        {
            verticalSpeed += fallingSpeed * delta;
        }

        verticalSpeed = Mathf.Clamp(verticalSpeed, fallingSpeed, jumpForce);
    }

    public void HandleJump()
    {
        if (controller.jumpInput && controller.isOnLadder)
        {
            verticalSpeed = jumpForce;
            //ladderJump = true;
            controller.isOnLadder = controller.isInteracting = false;
            return;
        }
        if (controller.jumpInput && controller.isGrounded)
        {
            verticalSpeed = jumpForce;
        }
    }
    public void HandleGroundCollision(float delta)
    {
        Vector3 groundPos = transform.position;
        Vector3 centerPos = transform.position + Vector3.up;

        bool detectGround = Physics.CheckSphere(groundPos, groundCheckRadius, groundLayer, QueryTriggerInteraction.Ignore);


        controller.isGrounded = detectGround;

        if (detectGround)
        {
            RaycastHit hit;
            bool hitGround;

            Vector3 rayOrigin;
            if (controller.axisInputAmount > 0.1f)
            {
                rayOrigin = centerPos + transform.forward * 0.3f;
            }
            else
            {
                rayOrigin = centerPos;
            }

            hitGround = Physics.Raycast(rayOrigin, -Vector3.up, out hit, stepRayDistance, groundLayer);
            Debug.DrawRay(rayOrigin, -Vector3.up * stepRayDistance, Color.blue, 0, false);


            if (hitGround && verticalSpeed < 0.1f)
            {
                Vector3 targetPosition = transform.position;
                targetPosition.y = hit.point.y;

                float stepHeightDifference = hit.point.y - transform.position.y;

                if (Mathf.Abs(stepHeightDifference) < maxStepHeight)
                {
                    rb.position = Vector3.Slerp(transform.position, targetPosition, delta * stepUpSpeed);
                }
            }
        }
    }
    public void HandleWallsCollision()
    {
        Vector3 groundPos = transform.position;
        if (controller.isGrounded) groundPos = transform.position + Vector3.up * 0.5f;
        Vector3 centerPos = transform.position + Vector3.up;

        Vector3 forward = transform.forward * wallRayDistance;
        Vector3 forwardLeft = transform.forward * wallRayDistance - transform.right * wallRaysAngle;
        Vector3 forwardRight = transform.forward * wallRayDistance + transform.right * wallRaysAngle;

        Vector3[] rayDirection = { forward, forwardLeft, forwardRight };

        bool wallRaycast = false;

        for (int i = 0; i < rayDirection.Length; i++)
        {
            bool center = Physics.Raycast(centerPos, rayDirection[i], wallRayDistance, groundLayer);
            bool ground = Physics.Raycast(groundPos, rayDirection[i], wallRayDistance, groundLayer);

            Debug.DrawRay(groundPos, rayDirection[i] * wallRayDistance, Color.green, 0, false);
            Debug.DrawRay(centerPos, rayDirection[i] * wallRayDistance, Color.green, 0, false);

            if (center || ground)
            {
                wallRaycast = true;
                Debug.DrawRay(groundPos, rayDirection[i] * wallRayDistance, Color.red, 0, false);
                Debug.DrawRay(centerPos, rayDirection[i] * wallRayDistance, Color.red, 0, false);
            }
        }

        if (wallRaycast)
        {
            controller.isWallCollision = true;
        }
        else
        {
            controller.isWallCollision = false;
        }
    }

    public void HandleClimbInteraction()
    {
        if (controller.isClimbing)
        {
            float distance = Vector3.Distance(transform.position, climbTargetPoint);

            //StopMovement
            verticalSpeed = 0;

            //Change for animation
            rb.position = Vector3.MoveTowards(transform.position, climbTargetPoint, Time.deltaTime * 10);

            if (distance < 0.1f)
            {
                controller.isClimbing = false;
            }
        }

        if (controller.isWallCollision && !controller.isGrounded)
        {
            Vector3 overheadRayPosition = transform.position + Vector3.up * climbHeight;
            Vector3 landRayPosition = overheadRayPosition + transform.forward * climblRayDistance;
            
            bool climbCheck = Physics.Raycast(overheadRayPosition, transform.forward, climblRayDistance, groundLayer);
            Debug.DrawRay(overheadRayPosition, transform.forward * climblRayDistance, Color.magenta);

            if (!climbCheck)
            {
                RaycastHit hit;
                bool landDetect = Physics.Raycast(landRayPosition, Vector3.down, out hit, 2 * climbHeight, groundLayer);
                Debug.DrawRay(landRayPosition, Vector3.down *2 * climbHeight, Color.magenta);

                if (landDetect && hit.point.y > transform.position.y)
                {
                    climbTargetPoint = hit.point;
                    controller.isClimbing = true;
                    //Delete
                    targetClimbTest.position = hit.point;
                    Debug.Log(hit.point);
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

        if (controller == null) Gizmos.color = transparentGreen;
        else
        {
            if (controller.isGrounded) Gizmos.color = transparentGreen;
            else Gizmos.color = transparentRed;
        }

        Vector3 spherePosition = transform.position;
        Gizmos.DrawSphere(spherePosition, groundCheckRadius);
    }

}
