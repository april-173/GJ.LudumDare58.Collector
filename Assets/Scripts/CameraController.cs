using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Player Follow")]
    public Transform player;
    public bool canFollow = true;

    [Header("Smooth Settings")]
    [Range(0.01f, 1f)] public float smoothTime = 0.15f;
    public float zOffset = -10f;

    [Header("Drift Effect (Water Float)")]
    public bool enableDrift = true;
    public float driftStrength = 0.05f;
    public float driftSpeed = 0.1f;

    private Vector3 velocity = Vector3.zero;
    private Vector3 basePosition;
    private float driftOffsetX;
    private float driftOffsetY;

    private void Start()
    {
        // 给漂浮噪声一个随机相位，避免每次运行都相同
        driftOffsetX = Random.Range(0f, 1000f);
        driftOffsetY = Random.Range(0f, 1000f);
    }

    private void FixedUpdate()
    {
        PlayerFollow();
    }

    private void PlayerFollow()
    {
        if (player == null || !canFollow) return;

        Vector3 targetPos = new Vector3(player.position.x, player.position.y, zOffset);
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime);

        if(enableDrift)
        {
            float time = Time.time * driftSpeed;

            float driftX = (Mathf.PerlinNoise(driftOffsetX, time) - 0.5f) * 2f * driftStrength;
            float driftY = (Mathf.PerlinNoise(driftOffsetY, time) - 0.5f) * 2f * driftStrength;

            transform.position += new Vector3(driftX, driftY, 0f);
        }
    }
}
