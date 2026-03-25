using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class VRLaserPointer : MonoBehaviour
{
    [Header("Laser Settings")]
    public float laserLength = 3f;
    public Color laserColor = Color.red;
    
    private LineRenderer lineRenderer;

    void Start()
    {
        // Automatically set up the visual properties of the line
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.startWidth = 0.005f;
        lineRenderer.endWidth = 0.005f;
        
        // Assign a basic material so it isn't pink/invisible
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = laserColor;
        lineRenderer.endColor = laserColor;
    }

    void Update()
    {
        // 1. Start the line at the controller's exact position
        lineRenderer.SetPosition(0, transform.position);

        // 2. Shoot a physics raycast forward to see if we hit the Canvas
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, laserLength))
        {
            // If we hit the canvas collider, stop the laser exactly at the hit point
            lineRenderer.SetPosition(1, hit.point);
        }
        else
        {
            // If we hit nothing, shoot the laser straight out to its max length
            lineRenderer.SetPosition(1, transform.position + (transform.forward * laserLength));
        }
    }
}