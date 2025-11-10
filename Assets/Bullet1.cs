using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Bullet1 : MonoBehaviour
{
    public float linearVelocity = 0f;
    public float acceleration = 0f;
    public float angularVelocity = 0f;
    public float angularAcceleration = 0f;
    public float maxVelocity = 10f;
    public float LifeCycle = 5;

    private void FixedUpdate()
    {
        linearVelocity += Mathf.Clamp(acceleration * Time.fixedDeltaTime, -maxVelocity, maxVelocity);
        angularVelocity += angularAcceleration * Time.fixedDeltaTime;
        transform.Translate(linearVelocity * Vector2.right * Time.fixedDeltaTime, Space.Self);
        transform.rotation *= Quaternion.Euler(0, 0, angularVelocity * Time.fixedDeltaTime);
        LifeCycle -= Time.fixedDeltaTime;
        if (LifeCycle <= 0)
        {
            Destroy(gameObject);
        }
    }
}