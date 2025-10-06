using UnityEngine;
using UnityEngine.Rendering;

public class Boid : MonoBehaviour
{
    [HideInInspector] public FlockManager manager;
    [HideInInspector] public Vector2 velocity;

    // 当速度接近 0 时填充一个小随机速度
    private const float minSpeedEpsilon = 0.01f;

    private SpriteRenderer spriteRenderer;

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("ScannerTrigger"))
        {
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 1);
        }
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0);
    }

    public void DisplaySprite(bool bl)
    {
        if(bl) spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 1);
        else spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0);
    }

    void Update()
    {
        if (manager == null) return;

        Vector2 accel = Vector2.zero;

        // 三大规则 + 避障 + 目标 + 噪声
        accel += Alignment() * manager.alignmentWeight * manager.GetInitialAlignFactor();
        accel += Cohesion() * manager.cohesionWeight;
        accel += Separation() * manager.separationWeight;
        accel += AvoidObstacles() * manager.avoidanceWeight;
        accel += TargetAttraction() * manager.targetWeight;

        // 随机扰动
        accel += Random.insideUnitCircle * manager.noiseWeight;

        // 应用加速度
        velocity += accel * Time.deltaTime;

        // 限速
        velocity = Vector2.ClampMagnitude(velocity, manager.maxSpeed);

        // 如果速度太小，赋予少量初始速度，避免静止
        if (velocity.sqrMagnitude < minSpeedEpsilon)
            velocity = Random.insideUnitCircle.normalized * manager.maxSpeed * 0.2f;

        // 移动
        transform.position += (Vector3)(velocity * Time.deltaTime);

        // 朝向（up 朝向速度方向）
        if (velocity.sqrMagnitude > 0.0001f)
            transform.up = velocity.normalized;

        if (manager.target != null) 
        {
        float dis = Vector2.Distance(transform.position, manager.target.position);
        float radius = 20f;
        var c = spriteRenderer.color;
            if (c.a != 0 && dis > radius)
            {
                c.a = Mathf.Max(0, c.a - Time.deltaTime * 0.2f);
                spriteRenderer.color = c;
            }
        }
    }

    #region Behaviors

    Vector2 Alignment()
    {
        Vector2 sum = Vector2.zero;
        int count = 0;
        float r = manager.neighborRadius;
        float r2 = r * r;

        foreach (var other in manager.boids)
        {
            if (other == this) continue;
            float d2 = ((Vector2)other.transform.position - (Vector2)transform.position).sqrMagnitude;
            if (d2 <= r2)
            {
                sum += other.velocity;
                count++;
            }
        }

        if (count == 0) return Vector2.zero;

        Vector2 avg = sum / count;
        Vector2 desired = avg.normalized * manager.maxSpeed;
        Vector2 steer = desired - velocity;
        return Vector2.ClampMagnitude(steer, manager.maxForce);
    }

    Vector2 Cohesion()
    {
        Vector2 center = Vector2.zero;
        int count = 0;
        float r = manager.neighborRadius;
        float r2 = r * r;

        foreach (var other in manager.boids)
        {
            if (other == this) continue;
            float d2 = ((Vector2)other.transform.position - (Vector2)transform.position).sqrMagnitude;
            if (d2 <= r2)
            {
                center += (Vector2)other.transform.position;
                count++;
            }
        }

        if (count == 0) return Vector2.zero;

        center /= count;
        return SteerTowards(center - (Vector2)transform.position);
    }

    Vector2 Separation()
    {
        Vector2 steer = Vector2.zero;
        int count = 0;
        float r = manager.separationRadius;
        float r2 = r * r;

        foreach (var other in manager.boids)
        {
            if (other == this) continue;
            Vector2 toOther = (Vector2)transform.position - (Vector2)other.transform.position;
            float d2 = toOther.sqrMagnitude;
            if (d2 <= r2 && d2 > 0.00001f)
            {
                // 距离越近，排斥越强（倒数平方）
                steer += toOther.normalized / Mathf.Sqrt(d2);
                count++;
            }
        }

        if (count == 0) return Vector2.zero;
        steer /= count;
        steer = steer.normalized * manager.maxSpeed - velocity;
        return Vector2.ClampMagnitude(steer, manager.maxForce);
    }

    Vector2 AvoidObstacles()
    {
        // 在前方扇形发射若干射线，若命中则向命中点法线方向避让
        int rays = Mathf.Max(1, manager.avoidRays);
        float half = manager.avoidSpreadAngle * 0.5f;
        Vector2 origin = transform.position;
        Vector2 forward = velocity.sqrMagnitude > 0.0001f ? velocity.normalized : transform.up;

        Vector2 avoid = Vector2.zero;
        for (int i = 0; i < rays; i++)
        {
            float t = (rays == 1) ? 0f : (float)i / (rays - 1);
            float ang = -half + t * (half * 2f); // 从 -half 到 +half
            float rad = Mathf.Deg2Rad * ang;
            Vector2 dir = Rotate(forward, rad);

            RaycastHit2D hit = Physics2D.Raycast(origin, dir, manager.avoidDistance, manager.obstacleLayer);
            Debug.DrawRay(origin, dir * manager.avoidDistance, hit.collider ? Color.red : Color.gray, 0.02f);

            if (hit.collider != null)
            {
                // 使用表面法线来避免，法线加权累积
                Vector2 away = (Vector2)transform.position - hit.point;
                Vector2 n = hit.normal;
                avoid += n.normalized * (manager.avoidDistance - hit.distance) / manager.avoidDistance;
            }
        }

        if (avoid == Vector2.zero) return Vector2.zero;
        Vector2 desired = avoid.normalized * manager.maxSpeed;
        Vector2 steer = desired - velocity;
        return Vector2.ClampMagnitude(steer, manager.maxForce);
    }

    Vector2 TargetAttraction()
    {
        if (manager.target == null) return Vector2.zero;
        Vector2 dir = (Vector2)manager.target.position - (Vector2)transform.position;
        return SteerTowards(dir) * 0.5f;
    }

    #endregion

    #region Helpers

    Vector2 SteerTowards(Vector2 vec)
    {
        if (vec.sqrMagnitude < 0.0001f) return Vector2.zero;
        Vector2 desired = vec.normalized * manager.maxSpeed;
        Vector2 steer = desired - velocity;
        return Vector2.ClampMagnitude(steer, manager.maxForce);
    }

    static Vector2 Rotate(Vector2 v, float rad)
    {
        float c = Mathf.Cos(rad);
        float s = Mathf.Sin(rad);
        return new Vector2(v.x * c - v.y * s, v.x * s + v.y * c);
    }

    #endregion

    #region Sprite
    public void RadarScan()
    {
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 1);
    }

    #endregion
}

