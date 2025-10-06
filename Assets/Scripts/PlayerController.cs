using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Physics")]
    public float thrust;
    public float maxSpeed;
    public float linearDrag;
    public float angularDrag;

    [Header("Input Smoothing")]
    public float inputSmoothTime;
    private Vector2 smoothedInput = Vector2.zero;
    private Vector2 inputSmoothVelocity = Vector2.zero;

    [Header("Buoyancy & Stabilization")]
    public bool applyBuoyancy;
    public float buoyancyForce = 0.5f;

    [Header("Limits")]
    public bool useYLimits = true;
    public float minY = -10f;
    public float maxY = 10f;

    [Header("Sonar")]
    public SonarVisual sonarVisual;

    [Header("Permission")]
    public bool canFastMove = false;
    public float fastMoveThrust;
    public float fastMoveMaxSpeed;
    private bool isFastMove = false;

    [Header("Debug")]
    public bool showDebugGizmos = true;

    private Rigidbody2D rb;
    private Vector2 targetInput = Vector2.zero;
    private bool subscribed = false;

    public Vector2 SmoothedInput => smoothedInput;
    public Vector2 LinearVelocity => rb != null ? rb.linearVelocity : Vector2.zero;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.linearDamping = linearDrag;
        rb.angularDamping = angularDrag;
    }

    private void OnEnable()
    {
        TrySubscribeInput();
    }

    private void Start()
    {
        TrySubscribeInput();
    }

    private void OnDisable()
    {
        TryUnsubscribeInput();
    }

    private void FixedUpdate()
    {
        UpdatePhysics();
    }

    #region < 事件订阅 >
    private void TrySubscribeInput()
    {
        if (subscribed) return;
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnMove += HandleMoveEvent;
            InputManager.Instance.OnFastMove += HandleFastMoveEvent;
            InputManager.Instance.OnPing += HandlePingEvent;
            subscribed = true;
        }
    }

    private void TryUnsubscribeInput()
    {
        if (!subscribed) return;
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnMove -= HandleMoveEvent;
            InputManager.Instance.OnFastMove -= HandleFastMoveEvent;
            InputManager.Instance.OnPing -= HandlePingEvent;
        }
        subscribed = false;
    }
    #endregion

    #region < 事件处理 >
    private void HandleMoveEvent(Vector2Int v)
    {
        Vector2 raw = new Vector2(v.x, v.y);
        if (raw.sqrMagnitude > 1f) raw.Normalize();
        targetInput = raw;
    }

    private void HandleFastMoveEvent(bool b)
    {
        isFastMove = b;
    }

    private void HandlePingEvent()
    {
        sonarVisual.StartSonar();
    }
    #endregion

    #region < 物理更新 >
    private void UpdatePhysics()
    {
        smoothedInput = Vector2.SmoothDamp(smoothedInput, targetInput, ref inputSmoothVelocity, inputSmoothTime);
        if (applyBuoyancy) rb.AddForce(Vector2.up * buoyancyForce * rb.mass);
        Vector2 force = new Vector2(smoothedInput.x * (isFastMove ? fastMoveThrust : thrust), smoothedInput.y * (isFastMove ? fastMoveThrust : thrust));
        rb.AddForce(force);
        if (rb.linearVelocity.magnitude > (isFastMove ? fastMoveMaxSpeed : maxSpeed)) rb.linearVelocity = rb.linearVelocity.normalized * (isFastMove ? fastMoveMaxSpeed : maxSpeed);

        if (useYLimits)
        {
            Vector2 pos = rb.position;
            float clampedY = Mathf.Clamp(pos.y, minY, maxY);
            if (!Mathf.Approximately(pos.y, clampedY))
            {
                pos.y = clampedY;
                rb.position = pos;
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            }
        }
    }
    #endregion

    #region < 调试方法 >
    private void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)(rb != null ? rb.linearVelocity : Vector2.zero));
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, smoothedInput * 1.0f);
    }

    #endregion
}
