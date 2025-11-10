using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Create BulletAsset")]
public class BulletObject : ScriptableObject
{
    [Header("Bullet Config")]
    public float LifeCycle = 5f;
    public float linearVelocity = 0f;
    public float acceleration = 0f;
    public float angularVelocity = 0f;
    public float angularAcceleration = 0f;
    public float maxVelocity = 10f;
    public float InitRotation = 0f; // 修复: 添加 InitRotation 字段 (Sender.Awake 使用)
    public float LinearVelocity = 0f; // 修复: 添加 LinearVelocity 字段 (Sender.InitBullet 使用)
    public float Acceleration = 0f; // 修复: 添加 Acceleration 字段
    public float AngularAcceleration = 0f; // 修复: 添加 AngularAcceleration 字段 (已存在 angularAcceleration, 添加大写版本)
    public float AngularVelocity = 0f; // 修复: 添加 AngularVelocity 字段 (已存在 angularVelocity, 添加大写版本)
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