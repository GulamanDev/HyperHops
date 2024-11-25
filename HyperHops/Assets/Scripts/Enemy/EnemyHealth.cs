using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour

{
    public int health = 100;

    public void TakeDamage(int damage)
    {
        Debug.Log("Health: " + health);
        health -= damage;
        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // Handle enemy death (e.g., destroy the object)
        Destroy(gameObject);
    }
}