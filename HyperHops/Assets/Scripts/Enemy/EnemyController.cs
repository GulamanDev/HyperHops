using UnityEngine;
using Photon.Pun;

public class EnemyController : MonoBehaviour
{
    enum State { Roam, Chase, Attack, Flee }
    private State currentState = State.Roam;

    public Transform player;  // The player's transform to track
    public float detectionRange = 10f;  // How far the enemy can detect the player
    public float attackRange = 3f;  // How close the enemy has to get to attack
    public float fleeRange = 2f;  // How close the player has to get for the enemy to flee
    public float speed = 3f;  // Movement speed
    public float jumpForce = 10f;  // Jump force when attacking
    public float jumpCooldown = 3f;  // Time between jump attacks
    public float missTimeLimit = 1.0f;  // Time before the enemy decides to stop stomping if it misses

    private float timeSinceLastJumpAttack = 0f;  // Time since last jump attack
    private bool isJumping = false;  // Is the enemy in the air or jumping
    private bool isStomping = false;  // Is the enemy in the stomping state
    private float stompStartTime = 0f;  // Time when stomp was initiated
    private Rigidbody rb;  // Reference to the Rigidbody component

    // Animation variables
    private Animator am;
    private bool isGrounded;
    private bool isDoubleJumping;
    private bool isLanding;
    private bool isMoving;
    private bool isFalling;
    private bool isDamage;


    // Flip variables
    private bool isFacingRight = true;


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        am = GetComponent<Animator>();
    }

    private void Update()
    {
        if(!PhotonNetwork.IsMasterClient) return; // Only the master client controls AI

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

            case State.Flee:
                Flee();
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

        // Check if the player is directly above the enemy to trigger the flee state
        if (IsDirectlyAbovePlayer() && IsPlayerAboveThreshold())
        {
            currentState = State.Flee;
        }

        IsFalling();

    }

    private void AssignNearestPlayer()
    {
        // Find the nearest player on the network
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        float closestDistance = Mathf.Infinity;

        foreach (GameObject Player in players)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                player = player.transform;
            }
        }
    }


    private void Roam()
    {

        // Random roaming behavior (can also use predefined points if needed)
        float roamSpeed = speed * 0.5f;  // Slower roaming speed

        Vector3 forwardMovement = new Vector3(transform.forward.x, 0, 0) * roamSpeed * Time.deltaTime; 
        transform.Translate(forwardMovement, Space.Self);


        // Check if the player is within detection range
        if (Vector3.Distance(transform.position, player.position) < detectionRange)
        {
            currentState = State.Chase;

        }



        Flip();
    }

    private void IsFalling()
    {
        // Check if the character is falling (y velocity is negative)
        if (rb.velocity.y < 0)
        {
            Debug.Log("FALLING");
            am.SetBool("isFalling", true);
            am.SetBool("isJumping", false);
            am.SetBool("isGrounded", false);
            am.SetBool("isMoving", false);
            isGrounded = false;
        }
        else
        {
            am.SetBool("isFalling", false);
            am.SetBool("isGrounded", true);
        }

    }
    private void Chase()
    {
        // Move towards the player
        Vector3 direction = (player.position - transform.position).normalized;

        direction.z = 0; // no movement z axis
        transform.Translate(direction * speed * Time.deltaTime, Space.World);

        am.SetBool("isMoving", true);
        am.SetBool("isGrounded", false);
        isMoving = true;

        Flip();
        // Check if the enemy is close enough to attack (within attack range)
        if (Vector3.Distance(transform.position, player.position) < attackRange)
        {
            rb.velocity = Vector3.zero; 
            currentState = State.Attack; 
        }
        else if (Vector3.Distance(transform.position, player.position) > detectionRange)
        {
            currentState = State.Roam; 
        }
    }

    private void Attack()
    {
        // If the enemy is in attack range and is not already jumping or stomping
        if (!isJumping && !isStomping)
        {
            JumpOnPlayer();
            am.SetBool("isJumping", true);
            am.SetBool("isGrounded", false);
            am.SetBool("isMoving", false);
            isGrounded= false;
        }
        if (IsGrounded())
        {
            am.SetBool("isMoving", true);         
            am.SetBool("isJumping", false);

        }

        // Check if the player is directly below the enemy
        if (isJumping && IsDirectlyAbovePlayer())
        {
            PerformStomp(); 
            am.SetBool("isStomping", true);
            am.SetBool("isJumping", false);
            am.SetBool("isFalling", false);
            am.SetBool("isGrounded", false);
            isStomping = true;
        }
        else
        {
            am.SetBool("isStomping", false);
            am.SetBool("isMoving", true);
            isStomping = false;
        }
    }

    private void Flee()
    {
        // Move away from the player
        Vector3 fleeDirection = (transform.position - player.position).normalized; 
        transform.Translate(fleeDirection * speed * Time.deltaTime, Space.World);

        // If the enemy gets far enough from the player, change state fleeing to roaming
        if (Vector3.Distance(transform.position, player.position) > fleeRange)
        {
            currentState = State.Roam;
        }
    }

    private void JumpOnPlayer()
    {
        isJumping = true;  // Set the enemy as jumping
        timeSinceLastJumpAttack = 0f;  // Resets Jump Attack

        // Calculate direction to the player
        Vector3 directionToPlayer = (player.position - transform.position).normalized;

        // Apply a force to jump upwards and towards the player
        rb.velocity = new Vector3(directionToPlayer.x * speed, jumpForce, directionToPlayer.z * speed);


    }

    private bool IsDirectlyAbovePlayer()
    {
        // Check if the enemy is directly above the player along the X and Z axes
        // Tolerance is how directly above the checker
        float tolerance = 0.5f; 
        return Mathf.Abs(transform.position.x - player.position.x) < tolerance &&
               Mathf.Abs(transform.position.z - player.position.z) < tolerance;
    }

    private bool IsPlayerAboveThreshold()
    {
        // Check if the player is significantly above the enemy (in the Y axis)
        return player.position.y > transform.position.y + 1.0f;  // Player is above by at least 1 unit
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
        Debug.Log("Detects the ground ");

        // Set the raycast origin just below the enemy's position (use the collider's bounds center minus half the height)
        Vector3 rayOrigin = transform.position - new Vector3(0, 0.5f, 0);  // Adjust to cast just below the enemy
        Vector3 rayDirection = Vector3.down;

        // Length of the raycast
        float rayLength = 0.5f;

        // Draw the ray in the Scene view in red (for debugging)
        Debug.DrawRay(rayOrigin, rayDirection * rayLength, Color.red);

        // If the enemy is jumping, return false to prevent false ground detection while in the air
        if (isJumping || isStomping)
        {
            return false;
        }

        // Simple ground check using raycasting
        return Physics.Raycast(rayOrigin, rayDirection, rayLength);
    }

    private void Flip()
    {
        // Flip the character when it changes direction
        if (rb.velocity.x > 0 && !isFacingRight)
        {
            isFacingRight = true;
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }
        else if (rb.velocity.x < 0 && isFacingRight)
        {
            isFacingRight = false;
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }
    }

    public static void SpawnEnemy(Vector3 position)
    {
        PhotonNetwork.Instantiate("EnemyPrefab", position, Quaternion.identity);
    }
}
