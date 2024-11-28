using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
public class PlayerMovement : MonoBehaviourPun
{
    [SerializeField] private float moveSpeed = 5f; // Speed of the player
    [SerializeField] private float jumpForce = 5f; // Force of the jump
    [SerializeField] private float dashPower = 20f; // Power of the dash
    [SerializeField] private float dashDuration = 0.2f; // Duration of the dash
    [SerializeField] private float dashCooldown = 1f; // Cooldown time for dashing
    [SerializeField] private float stompRange = 1.5f; // Range of the stomp attack
    [SerializeField] private int stompDamage = 10; // Damage dealt by the stomp
    [SerializeField] private TrailRenderer trailRenderer; // Trail effect for dashing and check if its working
    [SerializeField] private LayerMask enemyLayer; // The layer that contains enemies

    private Rigidbody rb;
    private Vector3 movement;

    // Dash variables
    private bool isDashing = false;
    private bool canDash = true;

    // Double Jump variables
    private bool doubleJump = false;
    private float groundDistance = 1.1f;
    private bool canJump = true;
    private float jumpCooldown = 0.2f;
    private float jumpCooldownTimer = 0f;

    // Flip variables
    private bool isFacingRight = true;

    // Animation variables
    private Animator am;
    private bool isJumping;
    private bool isGrounded;
    private bool isDoubleJumping;
    private bool isLanding;
    private bool isMoving;
    private bool isFalling;

    // Attack variables
    private bool isAttacking = false; // Prevent multiple attacks at once

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        am = GetComponent<Animator>();

        if (PhotonNetwork.IsConnected && PhotonView.Get(this).IsMine)
        {
            // Assign the local player's Transform to the TagObject
            PhotonNetwork.LocalPlayer.TagObject = this.transform;
        }
    }

    void Awake()
    {
        if (am == null)
        {
            am = GetComponent<Animator>();
        }
    }

    void Update()
    {
        if (photonView.IsMine)
        {
            if (isDashing) return;

            Movement();
            Jump();
            HandleDash();
            HandleAttack(); // Call the attack function

            SyncAnimationState();
        }
    }

    void FixedUpdate()
    {
        if (photonView.IsMine && !isDashing) // Only move if this is the local player's object
        {
            Move();
        }
    }

    private void Movement()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        movement = new Vector3(moveX, 0f, moveZ).normalized * moveSpeed;

        if (moveX != 0f && IsGrounded())
        {
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

        if (moveX > 0 && !isFacingRight)
        {
            Flip();
        }
        else if (moveX < 0 && isFacingRight)
        {
            Flip();
        }

    }

    private void Move()
    {
        Vector3 newPosition = rb.position + movement * Time.fixedDeltaTime;
        rb.MovePosition(newPosition);
    }

    private void Jump()
    {
        if (!isGrounded && rb.velocity.y < 0)
        {
            if (!isFalling)
            {
                am.SetBool("isFalling", true);
                isFalling = true;
            }
        }

        if (IsGrounded())
        {
            if (!Input.GetButton("Jump"))
            {
                if (!isGrounded)
                {
                    Debug.Log("Landed");
                    doubleJump = false;
                    am.SetBool("isGrounded", true);
                    am.SetBool("isFalling", false);
                    am.SetBool("isJumping", false);
                    am.SetBool("isDoubleJumping", false);

                    isGrounded = true;
                    isFalling = false;
                    isJumping = false;
                    isDoubleJumping = false;
                    canJump = true;
                    jumpCooldownTimer = 0f;
                }
            }
        }
        else
        {
            isGrounded = false;
            am.SetBool("isGrounded", false);
        }

        if (Input.GetButtonUp("Jump") && canJump)
        {
            if (IsGrounded())
            {
                Debug.Log("Jump");
                PerformJump();
                am.SetBool("isJumping", true);
                isJumping = true;
                isGrounded = false;
            }
            else if (!doubleJump)
            {
                Debug.Log("Second Jump Working");
                PerformJump();
                am.SetBool("isDoubleJumping", true);
                isDoubleJumping = true;
                doubleJump = true;
                canJump = false;
            }
        }

        if (!canJump)
        {
            jumpCooldownTimer += Time.deltaTime;
            if (jumpCooldownTimer >= jumpCooldown)
            {
                canJump = true;
                jumpCooldownTimer = 0f;
            }
        }
    }

    private void PerformJump()
    {
        rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
        am.SetBool("isFalling", false);
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
        int groundLayerMask = LayerMask.GetMask("Ground"); // Make sure "Ground" layer exists and includes the ground surfaces
        return Physics.Raycast(transform.position, Vector3.down, groundDistance, groundLayerMask);
    }

    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;

        Vector3 dashDirection = movement.normalized;
        if (dashDirection.magnitude < 0.1f)
        {
            dashDirection = transform.forward;
        }

        rb.useGravity = false;
        Debug.Log("DASHING");
        rb.velocity = dashDirection * dashPower;

        if (trailRenderer != null)
        {
            trailRenderer.emitting = true;
        }

        yield return new WaitForSeconds(dashDuration);

        if (trailRenderer != null)
        {
            trailRenderer.emitting = false;
        }

        rb.useGravity = true;
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private void Flip()
    {
        // Only flip for the owning player
        if (photonView.IsMine)
        {
            isFacingRight = !isFacingRight;
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;

            // Notify other players to flip via RPC
            photonView.RPC("SyncFlip", RpcTarget.OthersBuffered, isFacingRight);
        }
    }

    [PunRPC]
    private void SyncFlip(bool flip)
    {
        isFacingRight = flip;
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (flip ? 1 : -1);
        transform.localScale = scale;
    }

    // Attack

    private void HandleAttack()
    {
        if (Input.GetKeyDown(KeyCode.F) && !IsGrounded())
        {
            PerformStomp();
        }
    }


    private void PerformStomp()
    {
        rb.velocity = new Vector3(rb.velocity.x, -20f, rb.velocity.z);
    }

    private void SyncAnimationState()
    {
        bool isJumpingState = am.GetBool("isJumping");
        bool isFallingState = am.GetBool("isFalling");
        bool isMovingState = am.GetBool("isMoving");

        // Notify others of the animation state changes
        photonView.RPC("SyncAnimation", RpcTarget.OthersBuffered, isJumpingState, isFallingState, isMovingState);
    }

    [PunRPC]
    private void SyncAnimation(bool isJumping, bool isFalling, bool isMoving)
    {
        Debug.Log("Syncing Animation - Jumping: " + isJumping + ", Falling: " + isFalling + ", Moving: " + isMoving);
        // Update animation parameters on remote clients
        am.SetBool("isJumping", isJumping);
        am.SetBool("isFalling", isFalling);
        am.SetBool("isMoving", isMoving);
    }

}