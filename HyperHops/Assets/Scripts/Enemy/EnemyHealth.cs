using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int health = 100;
    public int damage = 20;
    public GameObject enemyChild; // Reference to the child GameObject (e.g., "EnemyChild")
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();  // Access the Rigidbody attached to the parent (or child if needed)

        // Optionally, get the child GameObject dynamically (if not set in Inspector)
        if (enemyChild == null)
        {
            enemyChild = transform.Find("EnemyChild").gameObject;  // Find the child by name
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("PlayerHitBox"))
        {
            TakeDamage(damage);
        }
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        Debug.Log("Getting Hit: CURRENT HEALTH: " + health);

        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Enemy died");
        Destroy(gameObject); // Destroy the child GameObject when health reaches zero
    }
}