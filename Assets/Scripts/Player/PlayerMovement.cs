using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Ground Detection")]
    public LayerMask whatIsGround; //what is considered as ground
    public Transform groundCheck; //position from where to check if the player is grounded

    [Header("Speed Settings")]
    public float acceleration = 300f; //player acceleration
    public float maxSpeed = 30f; //maximum speed
    public float slowDownFactor = 200f; //how much to slow down when there is no input
    public float jumpForce = 700f; //how high to jump
    public float directionChangeSpeed = 20f; //how fast to switch direction of movement
    public ParticleSystem speedParticle;

    [Header("Other")]
    public CameraMovement cameraParent; //the camera 
    public Rigidbody playerBody; //player's rigidbody
    public float threshold = 5f; //the minimum velocity needed before changing the force applied from velocity change to acceleration
    public float gravity = 30f;
    public Vector3 crouchScale; //scale for crouching

    Vector3 playerScale; //scale for standing
    Vector3 down;

    bool jump = false; //if the player can jump
    [HideInInspector] public bool grounded = true; //if the player is on the ground
    bool crouched = false; //if the player is crouched or not
    bool secondJump = false; //if the player has double jumped
    bool canDoubleJump = false; //if the player can double jump

    float timeAtMaxSpeed = 0f; //time at 90% of the maximum speed
    float initialMaxSpeed = 50f; //the initial, unaltered speed limit

    float groundAcceleration = 300f; //amount of acceleration while grounded
    float airAcceleration = 150f; //amount of acceleration while airbourne
    float crouchAcceleration = 100f; //amount of acceleration while crouching

    float playerHeight = 1f; //height of the caracter model

    //movement input
    [HideInInspector] public float lateralInput = 0f;
    [HideInInspector] public float forwardInput = 0f; 
    [HideInInspector] public Vector3 direction; //direction of movement
    void Start()
    {
        playerScale = transform.localScale;
        playerBody = GetComponent<Rigidbody>();
        playerHeight = transform.localScale.y;
        direction = Vector3.zero;
        initialMaxSpeed = maxSpeed;
        groundAcceleration = acceleration;
        airAcceleration = groundAcceleration / 2;
        crouchAcceleration = groundAcceleration / 3;
        down = Vector3.down;
    }
    void Update()
    {
        GetInput();
    }
    void FixedUpdate()
    {
        Move();
    }
    void GetInput()
    {
        lateralInput = Input.GetAxis("Horizontal");
        forwardInput = Input.GetAxis("Vertical");
        direction = lateralInput * transform.right + forwardInput * transform.forward;
        
        CheckGround();

        if (Input.GetButtonDown("Jump")) {
            if (grounded)
                jump = true;
            else if (canDoubleJump)
                secondJump = true;
        }

        if (Input.GetKeyDown(KeyCode.LeftControl))
            Crouch();

        if (Input.GetKeyUp(KeyCode.LeftControl))
            Stand();
    }
    void Move()
    {
        playerBody.AddForce(gravity * Time.fixedDeltaTime * down, ForceMode.Acceleration);

        //cancel the input if the player is above the speed limit, page taken from Dani's book
        if (Mathf.Abs(playerBody.velocity.x) > maxSpeed)
            lateralInput = 0;

        if (Mathf.Abs(playerBody.velocity.z) > maxSpeed)
            forwardInput = 0;

        //apply force according to the acceleration, multiplied by Time.fixedDeltaTime to be independent of framerate
        if ((Mathf.Abs(playerBody.velocity.x) < threshold) && (Mathf.Abs(playerBody.velocity.z) < threshold) && grounded)
            playerBody.velocity = new Vector3(direction.x * threshold, playerBody.velocity.y, direction.z * threshold);
        else
            playerBody.AddForce(acceleration * Time.fixedDeltaTime * direction, ForceMode.VelocityChange);

        if (jump)
            Jump();

        if (secondJump)
            DoubleJump();

        CounterMovement();
        ChangeSpeedLimit();
    }
    void DoubleJump()
    {
        //stop the vertical velocity before jumping again so gravity doesn't cancel out the second jump
        playerBody.velocity = new Vector3(playerBody.velocity.x, 0, playerBody.velocity.z);
        playerBody.AddForce(jumpForce * Time.fixedDeltaTime * transform.up, ForceMode.Impulse);
        canDoubleJump = false;
        secondJump = false;
    }
    void Jump()
    {
        //apply a force upwards when the player wants to jump
        playerBody.AddForce(jumpForce * Time.fixedDeltaTime * transform.up, ForceMode.Impulse);
        jump = false;
    }
    void Stand()
    {
        //check if there is something above the player before standing up
        if (!Physics.CheckSphere(transform.position + transform.up * .5f, .5f, whatIsGround))
        {
            //switch from the crouching scale to the standing scale and adjust the position so the player doesn't clip through the floor
            crouched = false;
            transform.localScale = playerScale;
            transform.position = new Vector3(transform.position.x, transform.position.y + playerHeight / 2, transform.position.z);
        }
    }
    void Crouch()
    {
        //switch from the standing scale to the crouching scale and adjust the position so the player doesn't drop
        crouched = true;
        transform.localScale = crouchScale;
        transform.position = new Vector3(transform.position.x, transform.position.y - playerHeight / 2, transform.position.z);
    }
    void CounterMovement()
    {
        //get velocity relative to the player
        Vector3 localVelocity = transform.InverseTransformDirection(playerBody.velocity);

        float drag = acceleration * slowDownFactor;

        //when there is no input or the player's input is in the opposite direction slow down the player's velocity according to the drag
        if (lateralInput == 0)
            playerBody.AddForce(drag * -localVelocity.x * Time.fixedDeltaTime * transform.right, ForceMode.Acceleration);
        else if ((localVelocity.x > 0 && lateralInput < 0) || (localVelocity.x < 0 && lateralInput > 0))
            playerBody.AddForce(directionChangeSpeed * -localVelocity.x * Time.fixedDeltaTime * transform.right, ForceMode.VelocityChange);

        if (forwardInput == 0) 
            playerBody.AddForce(drag * -localVelocity.z * Time.fixedDeltaTime * transform.forward, ForceMode.Acceleration);
        else if((localVelocity.z > 0 && forwardInput < 0) || (localVelocity.z < 0 && forwardInput > 0))
            playerBody.AddForce(directionChangeSpeed * -localVelocity.z * Time.fixedDeltaTime * transform.forward, ForceMode.VelocityChange);
    }
    void ChangeSpeedLimit()
    {
        //if the player is above 80% of the speed limit for more than 5 seconds increase the maximum speed
        if (playerBody.velocity.magnitude > (maxSpeed * .9f))
        {
            if (speedParticle.isStopped)
                speedParticle.Play();

            timeAtMaxSpeed += Time.fixedDeltaTime;
            if (timeAtMaxSpeed > 5)
            {
                maxSpeed += maxSpeed * .1f;
                timeAtMaxSpeed = 0;
            }
        }
        else
        {
            if (speedParticle.isPlaying)
                speedParticle.Stop();

            timeAtMaxSpeed = 0;

            //if the player is not moving bring the speed limit back down
            if (Mathf.Round(playerBody.velocity.magnitude) == 0)
                maxSpeed = initialMaxSpeed;
        }
    }
    void CheckGround()
    {
        //check if the player is grounded with a checksphere, this doesn't return information about the surface the player is on so we need another raycast to check if the surface is sloped
        if (Physics.CheckSphere(groundCheck.position, .5f, whatIsGround))
        {
            if(crouched)
                acceleration = crouchAcceleration;
            else
                acceleration = groundAcceleration;

            canDoubleJump = true;
            grounded = true;
            CheckSlope();
        }
        else
        {
            acceleration = airAcceleration;
            grounded = false;
        }
    }
    void CheckSlope()
    {
        //throw a raycast from the ground check position to see if the player is on a slope
        Ray downCheck = new Ray(groundCheck.position, Vector3.down);
        RaycastHit slopeCheck;

        if (Physics.Raycast(downCheck, out slopeCheck, .5f, whatIsGround))
        {
            direction = Vector3.ProjectOnPlane(direction, slopeCheck.normal);
            down = -slopeCheck.normal;
        }
        else
        {
            down = Vector3.down;
            acceleration = airAcceleration;
        }
    }
    private void OnDrawGizmos()
    {
        Debug.DrawRay(transform.position, playerBody.velocity);
        Debug.DrawRay(transform.position, direction);
        Debug.DrawRay(groundCheck.position, Vector3.down);
    }
}
