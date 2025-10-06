using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Scanner : MonoBehaviour
{
    [Header("Scanner Settings")]
    public float maxRadius = 5f;
    [Range(1f, 180f)] public float scanAngle = 60f;
    [Range(3, 200)] public int segmentCount = 20;
    public Color meshColor;
    [Range(0, 1)] public float meshAlpha;

    [Header("Smooth")]
    public float smoothTime = 0.1f;
    public float maxRotateSpeed = 720f;

    private float currentAngleDeg = 0f;
    private float angleVelocity = 0f;

    [Header("Collider")]
    public PolygonCollider2D scanCollider;
    private Vector2[] colliderPoints;

    [Header("Debug")]
    public bool showGizmos = true;

    private MeshFilter meshFilter;
    private Mesh viewMesh;
    private bool isScan = false;

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        viewMesh = new Mesh { name = "Scanner_ViewMesh" };
        viewMesh.MarkDynamic();
        meshFilter.mesh = viewMesh;

        if(scanCollider ==  null )
            scanCollider = GetComponent<PolygonCollider2D>();
        if (scanCollider == null)
            scanCollider = gameObject.AddComponent<PolygonCollider2D>();

        scanCollider.isTrigger = true;
        scanCollider.enabled = false;
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

    private void Update()
    {
        UpdateScannerMesh();
    }

    #region < 事件订阅 >
    private bool subscribed = false;

    private void TrySubscribeInput()
    {
        if (subscribed) return;
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnScan += HandleScan;
            subscribed = true;
        }
    }

    private void TryUnsubscribeInput()
    {
        if (!subscribed) return;
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnScan -= HandleScan;
        }
        subscribed = false;
    }
    #endregion

    #region < 事件处理 >
    private void HandleScan(bool b) => isScan = b;


    #endregion

    private void UpdateScannerMesh()
    {
        Vector3 originWorld = transform.position;
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(new Vector2(mousePos.x, mousePos.y));

        Vector2 dirWorld = (mouseWorld - originWorld);
        if (dirWorld.sqrMagnitude < 0.000001f) dirWorld = Vector2.up;

        float targetAngleDeg = Mathf.Atan2(dirWorld.y, dirWorld.x) * Mathf.Rad2Deg;
        currentAngleDeg = Mathf.SmoothDampAngle(currentAngleDeg, targetAngleDeg, ref angleVelocity, smoothTime, maxRotateSpeed);

        int vertexCount = segmentCount + 2;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[segmentCount * 3];
        Vector3[] normals = new Vector3[vertexCount];
        Vector2[] uvs = new Vector2[vertexCount];
        Color[] colors = new Color[vertices.Length];

        Transform meshTransform = meshFilter.transform;
        Vector3 originLocal = meshTransform.InverseTransformPoint(originWorld);

        vertices[0] = originLocal;
        normals[0] = Vector3.back;
        uvs[0] = new Vector2(0.5f, 0.5f);

        float halfAngle = scanAngle * 0.5f;

        for (int i = 0; i <= segmentCount; i++)
        {
            float t = (float)i / segmentCount;
            float angleDeg = currentAngleDeg - halfAngle + scanAngle * t;
            float rad = angleDeg * Mathf.Deg2Rad;
            Vector3 pointWorld = originWorld + new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * maxRadius;
            Vector3 pointLocal = meshTransform.InverseTransformPoint(pointWorld);

            vertices[i + 1] = pointLocal;
            normals[i + 1] = Vector3.back;

            Vector2 dir = (pointLocal - originLocal).normalized;
            uvs[i + 1] = new Vector2(0.5f + dir.x * 0.5f, 0.5f + dir.y * 0.5f);
        }

        for (int i = 0; i < segmentCount; i++)
        {
            int t = i * 3;
            triangles[t] = 0;
            triangles[t + 1] = i + 2;
            triangles[t + 2] = i + 1;
        }

        for (int i = 0; i < vertices.Length; i++)
            colors[i] = new Color(meshColor.r, meshColor.g, meshColor.b, isScan ? meshAlpha : 0f);

        viewMesh.Clear(false);
        viewMesh.vertices = vertices;
        viewMesh.triangles = triangles;
        viewMesh.normals = normals;
        viewMesh.uv = uvs;
        viewMesh.colors = colors;
        viewMesh.RecalculateBounds();

        if(isScan)
        {
            scanCollider.enabled = true;

            colliderPoints = new Vector2[segmentCount + 2];
            colliderPoints[0] = originWorld;

            for (int i = 0; i <= segmentCount; i++)
            {
                float t = (float)i / segmentCount;
                float angleDeg = currentAngleDeg - halfAngle + scanAngle * t;
                float rad = angleDeg * Mathf.Deg2Rad;
                Vector2 point = originWorld + new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * maxRadius;
                colliderPoints[i + 1] = point;
            }

            for (int i = 0; i < colliderPoints.Length; i++)
                colliderPoints[i] = transform.InverseTransformPoint(colliderPoints[i]);

            scanCollider.pathCount = 1;
            scanCollider.SetPath(0, colliderPoints);
        }
        else
        {
            if (scanCollider.enabled) scanCollider.enabled = false;
        }
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, maxRadius);
    }
}
