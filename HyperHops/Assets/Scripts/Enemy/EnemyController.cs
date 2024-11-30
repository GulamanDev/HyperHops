using UnityEngine;
using Photon.Pun;
using UnityEngine.AI;
using System.Collections.Generic;
using TMPro.Examples;

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

    // Patrol variables
    [SerializeField] public List<Transform> wayPoints;  // Waypoints for the patrol
    private int currentWaypointIndex = 0;  // The current waypoint index

    // NavMesh Agent reference
    private NavMeshAgent navMeshAgent;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        am = GetComponent<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.angularSpeed = 0f;
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
        if (wayPoints.Count == 0) return;

        Transform targetWaypoint = wayPoints[currentWaypointIndex];
        navMeshAgent.SetDestination(targetWaypoint.position);

        if (Vector3.Distance(transform.position, targetWaypoint.position) <= 1f || !navMeshAgent.pathPending && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % wayPoints.Count;
        }

        // Check if the player is within detection range
        if (Vector3.Distance(transform.position, player.position) < detectionRange)
        {
            // Stop the NavMeshAgent if the player is in range
            Stop(speed);
            Debug.Log("NavMeshAgent stopped: " + navMeshAgent.isStopped);
            currentState = State.Chase;  // Switch to Chase state
        }
        else
        {
            // Resume NavMeshAgent if the player is out of detection range
            if (navMeshAgent.isStopped)
            {
                Move(speed);
                navMeshAgent.SetDestination(wayPoints[currentWaypointIndex].position);  // Resume roaming
            }
        }

        Flip();
    }

    private void Flip()
    {
        // Flip the character when it changes direction based on the NavMeshAgent's velocity
        if (navMeshAgent.velocity.x > 0 && !isFacingRight)
        {
            isFacingRight = true;
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x);  // Ensure the scale is positive (facing right)
            transform.localScale = scale;
        }
        else if (navMeshAgent.velocity.x < 0 && isFacingRight)
        {
            isFacingRight = false;
            Vector3 scale = transform.localScale;
            scale.x = -Mathf.Abs(scale.x);  // Flip the scale for leftward facing
            transform.localScale = scale;
        }
    }
    private float outOfRangeTimer = 0f;  // Timer to track how long the player has been out of range
    private float outOfRangeDelay = 0.4f;  // Delay before switching back to Roam state

    private void Chase()
    {
        // Create a raycast from the enemy's position to the player's position
        Vector3 rayDirection = (player.position - transform.position).normalized;

        rayDirection.y = 0;  // Only care about the X and Z axis
        rayDirection.z = 0;

        // Cast a ray in the X-axis direction
        RaycastHit hit;
        if (Physics.Raycast(transform.position, rayDirection, out hit, detectionRange))
        {
            // If the ray hits the player and the distance to the player is within range
            if (hit.transform.CompareTag("Player"))
            {
                navMeshAgent.SetDestination(player.position);  // Continue chasing the player

                // Reset outOfRangeTimer when the player is detected
                outOfRangeTimer = 0f;

                // If within attack range, switch to attack state
                if (Vector3.Distance(transform.position, player.position) < attackRange)
                {
                    //Stop(speed);
                    rb.velocity = Vector3.zero;
                    currentState = State.Attack;  // Switch to attack state
                    Debug.Log("Attacking player");
                }
            }
        }
        else
        {
            // If the player is out of range, start counting time
            outOfRangeTimer += Time.deltaTime;

            // If the player has been out of range for 0.4 seconds, switch to Roam state
            if (outOfRangeTimer >= outOfRangeDelay)
            {
                ResumeNavMeshAgent();  // Switch back to roaming
            }
        }

        // Set animation for movement
        am.SetBool("isMoving", true);
        am.SetBool("isGrounded", false);
        isMoving = true;

        // Flip the character's direction based on movement
        Flip();
    }

    private void IsFalling()
{
    // Check if the character is falling (y velocity is negative)
    if (rb.velocity.y < 0)
    {
        //Debug.Log("FALLING");
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
private void Attack()
{

    // If the enemy is in attack range and is not already jumping or stomping
    if (!isJumping && !isStomping)
    {
        isJumping = true;
        Stop(speed);
        Debug.Log("Is Jumping: " + isJumping);
        EnemyJump();
        am.SetBool("isJumping", true); 
        am.SetBool("isGrounded", false); 
        am.SetBool("isMoving", false); 

            
    }

    // If the enemy is grounded (i.e., the jump finished), reset the animation and restart NavMeshAgent
    if (IsGrounded())
    {
        am.SetBool("isMoving", true); 
        am.SetBool("isJumping", false); 
        if (!navMeshAgent.isStopped)
        {
            
            if (currentState == State.Chase)
            {
                navMeshAgent.SetDestination(player.position);  // Resume chasing player
            }
            else if (currentState == State.Roam)
            {
                navMeshAgent.SetDestination(wayPoints[currentWaypointIndex].position);  // Resume roaming
            }
        }
    }

    // If the enemy is directly above the player and still in the air, perform the stomp
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
    private void EnemyJump()
    {
        if(isJumping)
        {
            Debug.Log("Already Jumping!"); 
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            rb.velocity = new Vector3(directionToPlayer.x * speed, jumpForce, directionToPlayer.z * speed);
        }

        Debug.Log("EnemyJUMP" + rb.velocity);
    }


private bool IsGrounded()
{
    // Simple ground check using a raycast to detect if the enemy is grounded
    Vector3 rayOrigin = transform.position - new Vector3(0, 0.5f, 0);  // Just below the character
    Vector3 rayDirection = Vector3.down;
    float rayLength = 0.5f;  // Length of the raycast

    // Raycast to detect ground
    if (Physics.Raycast(rayOrigin, rayDirection, rayLength))
    {
        {
            Debug.Log("GROUNDED");
            isJumping = false;
            am.SetBool("isJumping", false);  // Stop jump animation
            am.SetBool("isGrounded", true);  // Mark as grounded
            return true;  // The enemy is grounded now
        }
    }

    return false;  // The enemy is still in the air
}

    private void ResumeNavMeshAgent()
    {
        // Re-enable the NavMeshAgent and resume pathfinding after landing
        Move(speed);

        // Set the destination if chasing or roaming
        if (currentState == State.Chase)
        {
            navMeshAgent.SetDestination(player.position);  // Resume chasing the player
        }
        else if (currentState == State.Roam)
        {
            navMeshAgent.SetDestination(wayPoints[currentWaypointIndex].position);  // Resume roaming
        }
    }
    private void Stop(float speed)
    {
        navMeshAgent.isStopped =true;
        navMeshAgent.speed = 0f;
    }
    private void Move(float speed)
    {
        navMeshAgent.isStopped = false;
        navMeshAgent.speed = speed;
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



    public static void SpawnEnemy(Vector3 position)
    {
        PhotonNetwork.Instantiate("EnemyPrefab", position, Quaternion.identity);
    }
}
