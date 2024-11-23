using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f; // Speed of the player
    [SerializeField] private float jumpForce = 5f; // Force of the jump
    [SerializeField] private float dashPower = 20f; // Power of the dash
    [SerializeField] private float dashDuration = 0.2f; // Duration of the dash
    [SerializeField] private float dashCooldown = 1f; // Cooldown time for dashing
    [SerializeField] private TrailRenderer trailRenderer; // Trail effect for dashing and check if its working

    private Rigidbody rb;
    private Vector3 movement;
    
    //Dash
    private bool isDashing = false;
    private bool canDash = true;
    
    //doublejump
    private bool doubleJump = false;
    private float groundDistance = 1.1f;
    private bool canJump = true;  // New flag to control jump timing
    private float jumpCooldown = 0.2f; // Time to wait before jump is allowed again
    private float jumpCooldownTimer = 0f;

    //flip
    private bool isFacingRight = true;

    //animate
    private Animator am;
    private bool isJumping;
    private bool isGrounded;
    private bool isDoubleJumping;
    private bool isLanding;
    private bool isMoving;
    private bool isFalling;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        am = GetComponent<Animator>();

    }

    void Update()
    {
        if (isDashing) return;

        Movement();
        Jump();
        HandleDash();


    }

    void FixedUpdate()
    {
        if (!isDashing)
        {
            Move();
        }
    }

    private void Movement()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        movement = new Vector3(moveX, 0f, moveZ).normalized * moveSpeed;

        if(moveX != 0f && IsGrounded())
        {
            Debug.Log("Running");
            am.SetBool("isMoving", true);
            isMoving = true;
            am.SetBool("isJumping", false);
            isJumping = false;
        }
        else
        {
            am.SetBool("isMoving", false);
            isMoving = false;
        }


        // Check if the object is moving left or right
        if (moveX > 0 && !isFacingRight)
        {
            Flip();  // Flip the object to the right
        }
        else if (moveX < 0 && isFacingRight)
        {
             Flip();  // Flip the object to the left
        }
        
    }

    private void Move()
    {
        Vector3 newPosition = rb.position + movement * Time.fixedDeltaTime;
        rb.MovePosition(newPosition);
    }

    private void Jump()
{
    // Handle the falling animation
    if (!isGrounded && rb.velocity.y < 0)
    {
        if (!isFalling)
        {
            am.SetBool("isFalling", true);
            isFalling = true;
        }
    }

    // Reset jump states when grounded
    if (IsGrounded())
    {
        if (!Input.GetButton("Jump"))
        {
            if (!isGrounded) // Only reset states when newly grounded
            {
                Debug.Log("Landed");
                doubleJump = false; // Reset double jump
                am.SetBool("isGrounded", true);
                am.SetBool("isFalling", false);
                am.SetBool("isJumping", false);
                am.SetBool("isDoubleJumping", false);

                // Reset states
                isGrounded = true;
                isFalling = false;
                isJumping = false;
                isDoubleJumping = false;
                canJump = true; // Reset jump cooldown
                jumpCooldownTimer = 0f; // Reset cooldown timer
            }
        }
    }
    else
    {
        isGrounded = false; // Update grounded state when not grounded
        am.SetBool("isGrounded", false);
    }

    // Handle jumping logic
    if (Input.GetButtonUp("Jump") && canJump)
    {
        if (IsGrounded()) // First jump
        {
            Debug.Log("Jump");
            PerformJump();
            am.SetBool("isJumping", true);
            isJumping = true;
            isGrounded = false;
        }
        else if (!doubleJump) // Double jump logic
        {
            Debug.Log("Second Jump Working");
            PerformJump();
            am.SetBool("isDoubleJumping", true);
            isDoubleJumping = true;
            doubleJump = true; // Mark double jump as used
        }

        // Start jump cooldown
        canJump = false;
    }

    // Handle the jump cooldown timer
    if (!canJump)
    {
        jumpCooldownTimer += Time.deltaTime;
        if (jumpCooldownTimer >= jumpCooldown)
        {
            canJump = true; // Allow jumping again after cooldown
            jumpCooldownTimer = 0f; // Reset the timer
        }
    }
}

// Helper function to handle the jump action
private void PerformJump()
{
    rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z); // Apply upward force
    am.SetBool("isFalling", false); // Reset falling animation
    isFalling = false;
}


    private void HandleDash()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
        {
            am.SetBool("isDashing", true);
            isDashing = true;
            StartCoroutine(Dash());
        }
        else
        {
            am.SetBool("isDashing", false);
            isDashing = false;
        }
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, groundDistance);
    }

    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;

        // Get the current movement direction
        Vector3 dashDirection = movement.normalized;
        if (dashDirection.magnitude < 0.1f) // If not moving, dash forward
        {
            dashDirection = transform.forward;
        }
        //deactivates gravity
        rb.useGravity = false;
        Debug.Log("DASHING");
        // Apply dash velocity
        rb.velocity = dashDirection * dashPower;

        // Enable trail effect
        if (trailRenderer != null)
        {
            trailRenderer.emitting = true;
        }

        yield return new WaitForSeconds(dashDuration);

        // Disable trail effect
        if (trailRenderer != null)
        {
            trailRenderer.emitting = false;
        }

        rb.useGravity=true; //activate gravity again
        isDashing = false;

        // Wait for cooldown
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private void Flip()
    {
        // Invert the object's local scale on the x-axis
        isFacingRight = !isFacingRight;  // Toggle the facing direction
        Vector3 scale = transform.localScale;
        scale.x *= -1;  // Flip the object by changing its scale on the x-axis
        transform.localScale = scale;
    }

}