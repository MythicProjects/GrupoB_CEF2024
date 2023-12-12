using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    PlayerLocomotion locomotion;
    PlayerInteraction interact;

    [Header("Movement Flags")]
    public bool isGrounded;
    public bool isWallCollision;
    public bool isClimbing;

    [Header("Interaction Flags")]
    public bool isInteracting;
    public bool isOnLadder;
    public bool isGrabbingObject;

    [Header("Input Controller")]
    public float hAxisInput, vAxisInput, axisInputAmount;
    public bool crouchInput;
    public bool runInput;
    public bool jumpInput;
    public bool interactInput;


    private void Awake()
    {
        locomotion = GetComponent<PlayerLocomotion>();
        interact = GetComponent<PlayerInteraction>();

        locomotion.GetLocomotionComponents();
        interact.GetInteractionComponents();

    }
    private void Start()
    {
        locomotion.SetLocomotionComponents();
    }
    private void Update()
    {
        float delta = Time.deltaTime;

        if (isClimbing) //Se encarga la animación
        {
            isInteracting  = isOnLadder = false;
            return;
        }

        InputController();

        //Activate interaction
        if (interactInput) interact.SetCharacterInteraction();
        if (isInteracting)
        {
            interact.UpdateInteraction();
            return;
        }

        locomotion.HandleJump();
    }
    private void FixedUpdate()
    {
        float delta = Time.fixedDeltaTime;

        //Detect collisions
        locomotion.HandleGroundCollision(delta);
        locomotion.HandleWallsCollision();
        locomotion.HandleClimbInteraction();

        if (isClimbing)
            return;

        //Apply Gravity
        locomotion.HandleGravity(delta);

        //Normal Movmenet
        locomotion.HandleMovement(delta);
        locomotion.HandleRotation(delta);
    }


    //Input Controller
    public void InputController()
    {
        GetMovementInput();
        GetActionInputs();
    }
    private void GetMovementInput()
    {
        hAxisInput = Input.GetAxisRaw("Horizontal");
        vAxisInput = Input.GetAxisRaw("Vertical");
        axisInputAmount = Mathf.Clamp01(Mathf.Abs(hAxisInput) + Mathf.Abs(vAxisInput));
    }
    private void GetActionInputs()
    {
        crouchInput = Input.GetKey(KeyCode.LeftControl);
        jumpInput = Input.GetButtonDown("Jump");

        runInput = Input.GetKey(KeyCode.LeftShift);

        interactInput = Input.GetKeyDown(KeyCode.E);
        if (interactInput) isInteracting = !isInteracting;
    }
}
