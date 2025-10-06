using System.Collections.Generic;
using UnityEngine;

public class FlockManager : MonoBehaviour
{
    [Header("Spawn")]
    public GameObject boidPrefab;
    public int boidCount = 50;
    public float spawnRadius = 3f;

    [Header("Movement")]
    public float maxSpeed = 3.5f;
    public float maxForce = 2.0f; // steering 最大加速度（感知到的方向调整量）

    [Header("Perception")]
    public float neighborRadius = 2.0f;
    public float separationRadius = 0.8f;

    [Header("Obstacle Avoidance")]
    public LayerMask obstacleLayer;
    public float avoidDistance = 1.5f;
    public int avoidRays = 5;              // 前方扇形发射射线数量
    public float avoidSpreadAngle = 60f;   // 扇形角度（度）

    [Header("Behavior Weights")]
    public float alignmentWeight = 1.0f;
    public float cohesionWeight = 0.8f;
    public float separationWeight = 1.4f;
    public float avoidanceWeight = 2.0f;
    public float targetWeight = 0.6f;
    public float noiseWeight = 0.2f;

    [Header("Initial Formation")]
    [Tooltip("生成后前几秒对齐增强，避免四散")]
    public float initialAlignStrength = 2.0f;
    public float initialAlignDuration = 3.0f;

    [Header("Optional Target")]
    public Transform target;
    public bool bl;

    [HideInInspector] public List<Boid> boids;

    // internal
    private float spawnTime;

    private void Start()
    {
        if (boidPrefab == null)
        {
            Debug.LogError("[FlockManager2D] boidPrefab 未设置");
            enabled = false;
            return;
        }

        boids = new List<Boid>(boidCount);
        spawnTime = Time.time;

        for (int i = 0; i < boidCount; i++)
        {
            Vector2 localPos = Random.insideUnitCircle * spawnRadius;
            Vector3 worldPos = transform.position + (Vector3)localPos;

            var go = Instantiate(boidPrefab, worldPos, Quaternion.identity, transform);
            var b = go.GetComponent<Boid>();
            if (b == null) b = go.AddComponent<Boid>();

            b.manager = this;

            // 初始速度：朝向 manager.up 为主，加一点随机扰动
            Vector2 initDir = (Vector2)transform.up + Random.insideUnitCircle * 0.2f;
            b.velocity = initDir.normalized * (maxSpeed * 0.5f);

            boids.Add(b);
        }

        foreach (Boid b in boids)
        {
            b.GetComponent<Boid>().DisplaySprite(bl);
        }
    }

    /// <summary>
    /// 用于在 boid 中获取当前的初始对齐增强系数
    /// 在生成后 initialAlignDuration 秒内从 initialAlignStrength 逐渐衰减到 1
    /// </summary>
    public float GetInitialAlignFactor()
    {
        float elapsed = Time.time - spawnTime;
        if (elapsed >= initialAlignDuration) return 1f;
        return Mathf.Lerp(initialAlignStrength, 1f, elapsed / initialAlignDuration);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, neighborRadius);
    }
}
