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

        if(moveX != 0f)
        {
            am.SetBool("isMoving", true);
            isMoving = true;
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
        if (!isGrounded && rb.velocity.y > 0)
        {
            am.SetBool("isFalling", true);
            isFalling = true;   
        }


        // Reset the jump states when grounded
        if (IsGrounded() && !Input.GetButton("Jump"))
        {
            if (canJump)  // Allow for jump after cooldown is over
            {
                doubleJump = false;  // Reset double jump when grounded
                am.SetBool("isGrounded", true);
                isGrounded = true;

                am.SetBool("isJumping", false);
                isJumping = false;
            }
        }

        // Start a jump
        if (Input.GetButtonUp("Jump"))
        {
            // Double jump logic
            if (IsGrounded() || !doubleJump)
            {
                // First jump or double jump logic
                Debug.Log("Jump");
                Jumping();
                am.SetBool("isJumping", true);
                isJumping = true;
                am.SetBool("isGrounded", false);
                isGrounded = false;
                am.SetBool("isMoving", false);
                isMoving = false;

                if (!IsGrounded())
                {
                    am.SetBool("isJumping", false);
                    isJumping = false;
                    am.SetBool("isDoubleJumping", true);
                    isDoubleJumping = true;
                    am.SetBool("isMoving", false);
                    isMoving = false;
                    Debug.Log("Double Jump Working");
                    doubleJump = true;  // Enable double jump

                }
            }
        }

        // Handle the jump cooldown timer
        if (!IsGrounded() && !canJump)
        {
            jumpCooldownTimer += Time.deltaTime;
            if (jumpCooldownTimer >= jumpCooldown)
            {
                canJump = true;  // Allow jumping again after cooldown
                jumpCooldownTimer = 0f;  // Reset the timer
            }
        }
    }

    // The Jumping function remains the same
    private void Jumping()
    {
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
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