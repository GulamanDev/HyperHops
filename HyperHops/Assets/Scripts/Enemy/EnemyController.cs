using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    enum State { Roam, Chase, Attack }
    private State currentState = State.Roam;

    public Transform player;  // The player's transform to track
    public float detectionRange = 10f;  // How far the enemy can detect the player
    public float attackRange = 3f;  // How close the enemy has to get to attack
    public float speed = 3f;  // Movement speed
    public float jumpForce = 10f;  // Jump force when attacking
    public float jumpCooldown = 3f;  // Time between jump attacks
    public float missTimeLimit = 1.0f;  // Time before the enemy decides to stop stomping if it misses

    private float timeSinceLastJumpAttack = 0f;  // Time since last jump attack
    private bool isJumping = false;  // Is the enemy in the air or jumping
    private bool isStomping = false;  // Is the enemy in the stomping state
    private float stompStartTime = 0f;  // Time when stomp was initiated
    private Rigidbody rb;  // Reference to the Rigidbody component

    private void Start()
    {
        rb = GetComponent<Rigidbody>();  // Get the Rigidbody component
    }

    private void Update()
    {
        timeSinceLastJumpAttack += Time.deltaTime;  // Increment the time since the last jump attack

        switch (currentState)
        {
            case State.Roam:
                Roam();
                break;

            case State.Chase:
                Chase();
                break;

            case State.Attack:
                Attack();
                break;
        }

        // If the enemy is stomping, check if it missed and continue chasing if necessary
        if (isStomping && Time.time - stompStartTime > missTimeLimit)
        {
            // If stomp has been going for too long (i.e., the enemy missed), return to chasing
            currentState = State.Chase;
            isStomping = false;
            isJumping = false;
        }
    }

    private void Roam()
    {
        // Random roaming behavior (can also use predefined points if needed)
        float roamSpeed = speed * 0.5f;  // Slower roaming speed
        transform.Translate(Vector3.forward * roamSpeed * Time.deltaTime, Space.Self);

        // Check if the player is within detection range
        if (Vector3.Distance(transform.position, player.position) < detectionRange)
        {
            currentState = State.Chase;
        }
    }

    private void Chase()
    {
        // Move towards the player
        Vector3 direction = (player.position - transform.position).normalized;
        transform.Translate(direction * speed * Time.deltaTime, Space.World);

        // Check if the enemy is close enough to attack
        if (Vector3.Distance(transform.position, player.position) < attackRange && timeSinceLastJumpAttack >= jumpCooldown)
        {
            currentState = State.Attack;
        }
        else if (Vector3.Distance(transform.position, player.position) > detectionRange)
        {
            currentState = State.Roam;
        }
    }

    private void Attack()
    {
        // Perform a jump attack if the enemy is not already jumping
        if (!isJumping && !isStomping)
        {
            JumpOnPlayer();
        }

        // Check if the player is directly below the enemy and if we're aligned on the X and Z axes
        if (isJumping && IsDirectlyAbovePlayer())
        {
            PerformStomp();
        }
    }

    private void JumpOnPlayer()
    {
        isJumping = true;  // Set the enemy as jumping
        timeSinceLastJumpAttack = 0f;  // Reset jump attack cooldown

        // Calculate direction to the player
        Vector3 directionToPlayer = (player.position - transform.position).normalized;

        // Apply a force to jump upwards and forward (towards the player)
        rb.velocity = new Vector3(directionToPlayer.x * speed, jumpForce, directionToPlayer.z * speed);
    }

    private bool IsDirectlyAbovePlayer()
    {
        // Check if the enemy is directly above the player along the X and Z axes
        // We can define a small tolerance to account for minor floating-point differences
        float tolerance = 0.5f;  // Adjust this based on how precise you want the "directly above" check to be
        return Mathf.Abs(transform.position.x - player.position.x) < tolerance &&
               Mathf.Abs(transform.position.z - player.position.z) < tolerance;
    }

    private void PerformStomp()
    {
        if (!isStomping)
        {
            isStomping = true;  // Set stomp state
            isJumping = false;  // Stop normal jumping
            stompStartTime = Time.time;  // Record the time when the stomp started

            // Immediately apply a very strong downward force to simulate the stomp impact
            rb.velocity = new Vector3(rb.velocity.x, -20f, rb.velocity.z);  // Strong downward velocity
        }
    }

    private bool IsGrounded()
    {
        // Simple ground check using raycasting (or use a collider's `isGrounded` property)
        return Physics.Raycast(transform.position, Vector3.down, 0.1f);
    }

    private void DealDamageToPlayer()
    {
        // Assuming the player has a health system
        // Here you would call a method on the player to deal damage or trigger an effect
        Debug.Log("Enemy stomped on the player!");
        // Example: player.TakeDamage(10);  // Replace with actual damage logic
    }
}