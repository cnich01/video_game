using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerMovement : MonoBehaviour
{
    #region Player Movement
    [Header("Movement")]
    private float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;

    public float groundDrag;
    public float gravity;

    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplyer;
    bool readyToJump;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    public float startYScale;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;
    #endregion

    #region Respawn
    [Header("Respawn")]
    public Vector3 respawnPoint;
    public float belowMapPoint;
    #endregion

    #region Player States
    [Header("Player States")]
    public MovementState movement;
    public enum MovementState
    {
        walking,
        sprinting,
        crouching,
        air
    }
    #endregion

    #region Item Interaction
    [Header("Item Interaction")]
   
    [SerializeField]
    private LayerMask pickableLayerMask;
    [SerializeField]
    private Transform playerCameraTransform;
    public GameObject inHandObject;
    [SerializeField]
    public Transform pickUpSlot;
    [SerializeField]
    private GameObject pickUpUI;
    [SerializeField]
    [Min(1)]
    private float hitRange = 3;
    private RaycastHit hit;
    private float dropCooldown = 0.25f;
    bool readyToDrop;
    #endregion

    #region Controls
    [Header("Keyboard/Mouse Inputs")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;
    public KeyCode interactKey = KeyCode.E;
    public KeyCode gearWheelKey = KeyCode.G;
    public KeyCode inventoryKey = KeyCode.Tab;
    public KeyCode dropItemKey = KeyCode.Q;
    public KeyCode skillsKey = KeyCode.I;
    public KeyCode jounralKey = KeyCode.J;
    public KeyCode mapKey = KeyCode.M;
    public KeyCode menuKey = KeyCode.Escape;
    public KeyCode talkKey = KeyCode.Return;
    public KeyCode primaryActionKey = KeyCode.Mouse0;
    public KeyCode secondaryActionKey = KeyCode.Mouse1;
    public KeyCode hotbar1Key = KeyCode.Alpha1;
    public KeyCode hotbar2Key = KeyCode.Alpha2;
    public KeyCode hotbar3Key = KeyCode.Alpha3;
    public KeyCode hotbar4Key = KeyCode.Alpha4;
    public KeyCode hotbar5Key = KeyCode.Alpha5;
    public KeyCode hotbar6Key = KeyCode.Alpha6;
    public KeyCode hotbar7Key = KeyCode.Alpha7;
    public KeyCode hotbar8Key = KeyCode.Alpha8;
    public KeyCode hotbar9Key = KeyCode.Alpha9;
    public KeyCode hotbar0Key = KeyCode.Alpha0;
    #endregion

    float horizontalInput;
    float verticalInput;
    float scrollInput;

    Vector3 moveDirection;

    Rigidbody rb;

    InventorySystem inventorySystem;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        inventorySystem = GetComponent<InventorySystem>();

        readyToJump = true;
        readyToDrop = true;

        startYScale = transform.localScale.y;

        pickUpUI.SetActive(false);
    }

    void Update()
    {
        //ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f);

        CheckInputs(); //Check for user inputs
        SpeedControl(); //Limit player speed to max speed
        StateHandler(); //Control player speed based off of current state
        CheckInteractions(); //Check if there are any interactable objects in front of the player

        //set drag
        if (grounded)
        {
            rb.drag = groundDrag;
        }
        else
        {
            rb.drag = 0;
            rb.AddForce(Vector3.down * gravity, ForceMode.Force);
        }

    }

    void FixedUpdate()
    {
        //calculate movement direction
        moveDirection = transform.forward * verticalInput + transform.right * horizontalInput;

        //on slope
        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection() * moveSpeed * 20f, ForceMode.Force);

            if(rb.velocity.y > 0)
            {
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
        }

        //on ground
        if (grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }
        else
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplyer, ForceMode.Force);
        }

        //turn off gravity while on slope
        rb.useGravity = !OnSlope();

        if(transform.position.y < belowMapPoint)
        {
            transform.position = respawnPoint;
        }
        
    }

    private void CheckInputs()
    {
        //get movement inputs
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        //check if jumping
        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;
            exitingSlope = true;

            //reset y velocity
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);

            Invoke(nameof(ResetJump), jumpCooldown);
        }

        //start crouch
        if(Input.GetKeyDown(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }

        //stop crouch
        if (Input.GetKeyUp(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }

        //get object interaction inputs
        if(hit.collider != null && Input.GetKey(interactKey) && hit.collider.TryGetComponent<ItemObject>(out ItemObject item))
        {
            if (inventorySystem.GetNextSlot() > -1)
            {
                hit.collider.gameObject.transform.SetParent(pickUpSlot);
                hit.collider.gameObject.GetComponent<Rigidbody>().isKinematic = true; ;
                hit.collider.gameObject.GetComponent<Collider>().enabled = false;

                item.OnHandlePickupItem(hit.collider.gameObject);
                inventorySystem.SetCurrentSlot(inventorySystem.GetCurrentSlot());
            }
            else{
                //do something to indicate inventory is full
            }

        }

        if (inHandObject != null)
        {
            inHandObject.transform.position = pickUpSlot.position;
            inHandObject.transform.rotation = pickUpSlot.rotation;


            if (Input.GetKey(dropItemKey) && inHandObject.TryGetComponent<ItemObject>(out ItemObject currentItem) && readyToDrop)
            {
                readyToDrop = false;
                inHandObject.transform.SetParent(null);
                inHandObject.GetComponent<Rigidbody>().isKinematic = false;
                inHandObject.GetComponent<Collider>().enabled = true;
                inHandObject.GetComponent<Rigidbody>().AddForce(inHandObject.transform.forward * 5, ForceMode.Impulse);
                currentItem.OnHandleDropItem();
                inventorySystem.SetCurrentSlot(inventorySystem.GetCurrentSlot());
                Invoke(nameof(ResetDrop), dropCooldown);
            }
        }
        #region Hotbar Selection
        //get scroll wheel input
        scrollInput = Input.GetAxis("Mouse ScrollWheel");

        if(scrollInput < 0) //Scroll Right
        {
            if (inventorySystem.GetCurrentSlot() < 9)
            {
                inventorySystem.SetCurrentSlot(inventorySystem.GetCurrentSlot() + 1);
            }
            else
            {
                inventorySystem.SetCurrentSlot(0);
            }
        }
        else if(scrollInput > 0) //Scroll Left
        {
            if (inventorySystem.GetCurrentSlot() > 0)
            {
                inventorySystem.SetCurrentSlot(inventorySystem.GetCurrentSlot() - 1);
            }
            else
            {
                inventorySystem.SetCurrentSlot(9);
            }
        }

        if (Input.GetKey(hotbar1Key)){
            inventorySystem.SetCurrentSlot(0);
        }
        else if (Input.GetKey(hotbar2Key))
        {
            inventorySystem.SetCurrentSlot(1);
        }
        else if (Input.GetKey(hotbar3Key))
        {
            inventorySystem.SetCurrentSlot(2);
        }
        else if (Input.GetKey(hotbar4Key))
        {
            inventorySystem.SetCurrentSlot(3);
        }
        else if (Input.GetKey(hotbar5Key))
        {
            inventorySystem.SetCurrentSlot(4);
        }
        else if (Input.GetKey(hotbar6Key))
        {
            inventorySystem.SetCurrentSlot(5);
        }
        else if (Input.GetKey(hotbar7Key))
        {
            inventorySystem.SetCurrentSlot(6);
        }
        else if (Input.GetKey(hotbar8Key))
        {
            inventorySystem.SetCurrentSlot(7);
        }
        else if (Input.GetKey(hotbar9Key))
        {
            inventorySystem.SetCurrentSlot(8);
        }
        else if (Input.GetKey(hotbar0Key))
        {
            inventorySystem.SetCurrentSlot(9);
        }
        #endregion
    }

    private void StateHandler()
    {
        if (Input.GetKey(crouchKey)) 
        {
            movement = MovementState.crouching;
            moveSpeed = crouchSpeed;
        }
        if (grounded && Input.GetKey(sprintKey)) 
        {
            movement = MovementState.sprinting;
            moveSpeed = sprintSpeed;
        }
        else if(grounded) 
        {
            movement = MovementState.walking;
            moveSpeed = walkSpeed;
        }
        else 
        {
            movement = MovementState.air; 
        }
    }

    private void SpeedControl()
    {
        //limiting speed on slope
        if (OnSlope() && !exitingSlope)
        {
            if (rb.velocity.magnitude > moveSpeed)
            {
                rb.velocity = rb.velocity.normalized * moveSpeed;
            }
        }

        //limiting speed on ground or in air
        else
        {
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }
    }

    private void CheckInteractions()
    {
        if (hit.collider != null)
        {
            //hit.collider.GetComponent<Highlight>()?.ToggleHighlight(false);
            pickUpUI.SetActive(false);
        }
        if (Physics.Raycast(playerCameraTransform.position, playerCameraTransform.forward, out hit, hitRange, pickableLayerMask))
        {
            //hit.collider.GetComponent<Highlight>()?.ToggleHighlight(true);
            pickUpUI.SetActive(true);
        }
    }

    private void ResetJump()
    {
        readyToJump = true;
        exitingSlope = false;
    }

    private void ResetDrop()
    {
        readyToDrop = true;
    }

    private bool OnSlope()
    {
        if(Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }
        return false;
    }

    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }
}