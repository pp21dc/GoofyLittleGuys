using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class BoundaryCircle : MonoBehaviour
{
    // -- Variables --
    private readonly int circleRadius = 5;
    private Vector3 circleCenter;
    [SerializeField] private Transform capturingPlayer;

    private LineRenderer lineRenderer;
    private readonly int dashes = 50;

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
        lineRenderer.positionCount = dashes;
        lineRenderer.loop = true;
        lineRenderer.widthMultiplier = 0.05f;
        lineRenderer.sortingOrder = 1;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"))
        {
            color = Color.red
        };
        circleCenter = capturingPlayer.localPosition;

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
            float z = Mathf.Sin(i * angle) * circleRadius;
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
