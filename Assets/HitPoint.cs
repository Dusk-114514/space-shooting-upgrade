using UnityEngine;

public class HitPoint : MonoBehaviour
{
    private PlayerHealth playerHealth;

    void Start()
    {
        playerHealth = GetComponentInParent<PlayerHealth>();
        Debug.Log("HitPoint initialized. PlayerHealth found: " + (playerHealth != null));
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("HitPoint triggered by: " + other.gameObject.name);
        if (other.CompareTag("EnemyBullet"))
        {
            if (playerHealth != null)
            {
                Debug.Log("HitPoint applying damage: 1");
                playerHealth.TakeDamage(1);
                Destroy(other.gameObject); // Ïú»ÙµÐ·½×Óµ¯
            }
        }
    }
}