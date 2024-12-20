using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int health = 100;
    public int damage = 20;

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Triggered by " + other.gameObject.name);
        if(other.CompareTag("PlayerHitBox"))
        {
            TakeDamage(damage);
        }
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        Debug.Log("Getting Hit: ENEMY HEALTH: " + health);

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