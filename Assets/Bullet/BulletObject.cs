using UnityEngine;

[CreateAssetMenu(menuName = "Create BulletAsset")]
public class BulletObject : ScriptableObject
{
    [Header("Bullet Config")]
    public int damage = 1; // 【新增】伤害值
    public float LifeCycle = 5f;
    public float maxVelocity = 10f;

    [Header("Aiming Logic (自机狙设置)")]
    public bool isAimed = false;
    public float aimOffset = 0f;

    [Header("Movement")]
    public float InitRotation = 0f;
    public float LinearVelocity = 0f;
    public float Acceleration = 0f;
    public float AngularAcceleration = 0f;
    public float AngularVelocity = 0f;

    [Header("Sender Config")]
    public float SenderAngularVelocity = 0f;
    public float SenderMaxAngularVelocity = int.MaxValue;
    public float SenderAcceleration = 0f;
    public int Count = 0;
    public float LineAngle = 30;
    public float SendInterval = 0.1f;

    [Header("Prefab")]
    public GameObject prefabs;
}