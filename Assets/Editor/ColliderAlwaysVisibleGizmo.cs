using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public static class ColliderAlwaysVisibleGizmo
{
    static ColliderAlwaysVisibleGizmo()
    {
        // 在 SceneView 绘制时持续调用
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private static void OnSceneGUI(SceneView sceneView)
    {
        Handles.color = new Color(0f, 1f, 1f, 0.6f); // 青色线条

        // 绘制所有 PolygonCollider2D
        foreach (var collider in Object.FindObjectsByType<PolygonCollider2D>(FindObjectsSortMode.None))
        {
            DrawPolygonCollider(collider);
        }

        foreach (var collider in Object.FindObjectsByType<EdgeCollider2D>(FindObjectsSortMode.None))
        {
            DrawEdgeCollider(collider);
        }
    }

    private static void DrawPolygonCollider(PolygonCollider2D collider)
    {
        if (!collider.enabled) return;

        for (int p = 0; p < collider.pathCount; p++)
        {
            Vector2[] points = collider.GetPath(p);
            for (int i = 0; i < points.Length; i++)
            {
                Vector3 start = collider.transform.TransformPoint(points[i]);
                Vector3 end = collider.transform.TransformPoint(points[(i + 1) % points.Length]);
                Handles.DrawLine(start, end);
            }
        }
    }

    private static void DrawEdgeCollider(EdgeCollider2D collider)
    {
        if (!collider.enabled) return;

        Vector2[] points = collider.points;
        for (int i = 0; i < points.Length - 1; i++)
        {
            Vector3 start = collider.transform.TransformPoint(points[i]);
            Vector3 end = collider.transform.TransformPoint(points[i + 1]);
            Handles.DrawLine(start, end);
        }
    }
}

