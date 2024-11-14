using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 5f;
    public Rigidbody rb;
    private Vector3 movement;
    public float groundDistance = 1.1f;
    public float velocity;

    //doublejump
    private bool doubleJump;

    //dashvariables
    private bool canDash = true;
    private bool isDashing;
    [SerializeField] private float Dashingpower = 20.0f;
    [SerializeField] private float DashingTime = 0.2f;
    [SerializeField] private float DashCooldown = 1f;
    [SerializeField] TrailRenderer tr;


    void Update()
    {
        if (isDashing)
        {
            return;
        }
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        movement = new Vector3(moveX, 0f, moveZ).normalized * moveSpeed;
        
        //checker
        if (IsGrounded() && !Input.GetButton("Jump"))
        {
            doubleJump = false;
        }

        //double jump
        if (Input.GetButtonDown("Jump"))
        {
            if (IsGrounded() || doubleJump)
            {
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                doubleJump = !doubleJump;
            }
        }
        
        if (Input.GetButtonDown("Jump") && IsGrounded())
        {
            
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
        
        //dashingmovement
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
        {
            StartCoroutine(Dash());
        }
    }

    void FixedUpdate()
    {
        Vector3 newPosition = rb.position + movement * Time.fixedDeltaTime;
        rb.MovePosition(newPosition);

        if(isDashing)
        {
            return; 
        }    
    }

    bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, groundDistance);
    }

    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        rb.velocity = new Vector3(transform.localScale.x * Dashingpower, 0f);
        tr.emitting = true;
        yield return new WaitForSeconds(DashingTime);
        tr.emitting = false;
        isDashing = false;
        yield return new WaitForSeconds(DashCooldown);
        canDash = true;

    }
}
