using UnityEngine;

public class EnemyDamage : MonoBehaviour
{
    public int damage = 1; // 每次碰撞造成的伤害

    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Collision detected with: " + collision.gameObject.name);
        PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            Debug.Log("PlayerHealth component found. Applying damage: " + damage);
            playerHealth.TakeDamage(damage);
        }
        else
        {
            Debug.Log("No PlayerHealth component found on " + collision.gameObject.name);
        }
    }
}