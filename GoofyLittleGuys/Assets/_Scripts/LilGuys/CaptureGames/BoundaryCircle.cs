using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.HableCurve;

[RequireComponent(typeof(LineRenderer))]
public class BoundaryCircle : MonoBehaviour
{
    // -- Variables --
    private int circleRadius;
    private Vector3 circleCenter;
    [SerializeField] private Transform capturingPlayer;

    private LineRenderer lineRenderer;
    private int dashes;

    /*
    public static BoundaryCircle CreateBoundary(Transform capturingPlayer, int circleRadius, int dashes)
    {
        GameObject circle = new GameObject("BoundaryCircle");
        BoundaryCircle boundaryCircle = circle.AddComponent<BoundaryCircle>();

        boundaryCircle.capturingPlayer = capturingPlayer;
        boundaryCircle.circleRadius = circleRadius;
        boundaryCircle.circleCenter = capturingPlayer.position;

        boundaryCircle.dashes = dashes;
        boundaryCircle.lineRenderer = circle.GetComponent<LineRenderer>();
        boundaryCircle.lineRenderer.positionCount = dashes + 1;
        boundaryCircle.lineRenderer.loop = true;
        boundaryCircle.lineRenderer.widthMultiplier = 0.05f;

        boundaryCircle.DrawCircle();

        return boundaryCircle;
    }
    */

    //TEMP
    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = dashes; // +1 to close the circle
        lineRenderer.loop = true; // Enable looping to connect the last and first point
        lineRenderer.widthMultiplier = 1f; // Adjust width as desired
        lineRenderer.sortingOrder = 1;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.material.color = Color.red;
        lineRenderer.SetVertexCount(dashes);

        DrawCircle();
    }

    private void Update()
    {
        foreach(var player in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (player != capturingPlayer.gameObject)
            {
                OutCircle(player.transform);
            }
            else
            {
                InCircle(player.transform);
            }
        }
    }

    private void DrawCircle()
    {
        float angle = 2f * Mathf.PI / dashes;
        Vector3[] points = new Vector3[dashes];

        for (int i = 0; i < points.Length; i++)
        {
            float x = Mathf.Cos(i * angle) * circleRadius;
            float z = Mathf.Sin(i * angle) + circleRadius;
            points[i] = new Vector3(x, 0, z) + circleCenter;
        }

        lineRenderer.SetPositions(points);
    }

    private void InCircle(Transform player)
    {
        Vector3 dirFromCenter = player.position - circleCenter;
        float distFromCenter = dirFromCenter.magnitude;

        if (distFromCenter > circleRadius)
        {
            player.position = circleCenter + dirFromCenter.normalized * circleRadius;
        }
    }

    private void OutCircle(Transform player)
    {
        Vector3 dirFromCenter = player.position - circleCenter;
        float distFromCenter = dirFromCenter.magnitude;

        if (distFromCenter < circleRadius)
        {
            player.position = circleCenter + dirFromCenter.normalized * circleRadius;
        }
    }
}
