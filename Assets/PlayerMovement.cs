using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>(); 
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D component not found on Player!");
        }
    }

    void Update()
    {
        
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

       
        Vector2 movement = new Vector2(moveX, moveY).normalized * moveSpeed;

        
        if (rb != null)
        {
            rb.velocity = new Vector2(movement.x, movement.y); 
        }

   
        Vector2 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, -10.49f, 10.51f);
        pos.y = Mathf.Clamp(pos.y, -4.94f, 4.8f);
        transform.position = pos;
    }
}